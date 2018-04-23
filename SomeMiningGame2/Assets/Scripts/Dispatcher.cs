using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;
using PathFind;

public class Dispatcher : MonoBehaviour {

	public bool path_through_units = true;
	public bool update_grid_between_unit_moves = true;

	private bool started = false;

	private Tile[, ,] tiles;
	private bool[,] grid_mask;
	private List<Worker> workers = new List<Worker>();

	private Vector2 last_hover_pos = new Vector2(-1, -1);


	private SimplePriorityQueue<Job> job_queue;
	private List<Job> in_progress_jobs;

	private TilePrefabs tile_prefabs;

	private MapGenerator map_generator;

	private ItemYieldStats item_yield_stats;

	private PauseGame pause_game_control;

	private InfoPanelManager info_panel_manager;

	// Use this for initialization
	void Start () {
		job_queue = new SimplePriorityQueue<Job>();
		in_progress_jobs = new List<Job>();
		item_yield_stats = GetComponent<ItemYieldStats>();

		var pauser = GameObject.Find("GamePauser");
		pause_game_control = pauser.GetComponent<PauseGame>();

		var info_panel = GameObject.Find("InfoPanel");
		info_panel_manager = info_panel.GetComponent<InfoPanelManager>();

		GameObject map_gen_game_object = GameObject.Find("MapGenerator");
		if(map_gen_game_object != null){
			map_generator = map_gen_game_object.GetComponent<MapGenerator>();
		}
	}
	
	// Update is called once per frame
	void Update () {



		if(!started){
			CheckIfStarted();
		}
		else{
			HandleStep();
		}
	}

	private void CheckIfStarted(){
		if(map_generator != null){
			started = map_generator.IsFinishedGenerating();

			if(started){
				tiles = map_generator.GetTiles();
				workers = map_generator.GetWorkers();

				tile_prefabs = map_generator.GetComponent<TilePrefabs>();

				foreach(var worker in workers){
					worker.SetDispatcher(this);
				}

			}
		}
	}

	private void HandleStep(){
		if(!pause_game_control.paused){
			UpdatePathfindingGrid();
		}

		HandleInput();

		if(!pause_game_control.paused){
			MonitorOngoingJobs();

			AssignJobsToIdlers();
		}

	}

	private void MonitorOngoingJobs(){
		// check the status of in-progress jobs
		// clean up completed jobs

		foreach(var worker in workers){
			var job = worker.GetJob();

			if(job != null){

				if(job.done){
					in_progress_jobs.Remove(job);
					worker.ClearJob();

					Target tgt = job.GetTarget();

					if(job is GatherItemJob){
						// we need to remove the resouce deposit
						Destroy(tiles[1, tgt.x, tgt.y].gameObject);
						Destroy(tiles[1, tgt.x, tgt.y]);

					}

					//We need to remove the job marker
					if(tiles[4, tgt.x, tgt.y] != null){
						Destroy(tiles[4, tgt.x, tgt.y].gameObject);
						Destroy(tiles[4, tgt.x, tgt.y]);
					}
				}
				else if(job.aborted){

					// handle abort here.
					worker.ClearJob();
					in_progress_jobs.Remove(job);

					if(job.abort_reason != null){
						info_panel_manager.AddAlert(worker.name + ": Job Aborted: " + job.abort_reason);
					}
				}
				else if(job.postponed){
					// handle postpone here.
					worker.ClearJob();
					in_progress_jobs.Remove(job);

					if(job.postpone_reason != null){
						info_panel_manager.AddAlert(worker.name + ": Job Postponed: " + job.postpone_reason);
					}

					job_queue.Enqueue(job, job.priority);
				}
			}
		}
	}

	private void AssignJobsToIdlers(){
		// assign jobs to idle workers

		foreach(var worker in workers){
			//assign a worker a job if they don't have one
			if(worker.GetJob() == null){

				// first see if there are jobs availiable, otherwise 
				// 	assign a random idle job
				
				if(job_queue.Count == 0){
					// assign an idle task

					var idle_job_types = new List<string>(){
						//"WaitingAround",
						"Wandering"
					};

					int selection = (int)UnityEngine.Random.Range(0f, idle_job_types.Count);

					var t = idle_job_types[selection];

					Job new_job = null; 

					if( t == "WaitingAround"){
						new_job = new WaitingAround(worker);
					}
					else if(t == "Wandering"){
						new_job = new Wandering(worker);
					}
					else{
						// if for some odd reason we get here
						// give up on this unit for this tick
						continue; 
					}

					worker.AssignJob(new_job);

					in_progress_jobs.Add(new_job);

				}
				else{
					var new_job = job_queue.Dequeue();

					worker.AssignJob(new_job);

					in_progress_jobs.Add(new_job);
				}

			}
		}
	}

	private void UpdatePathfindingGrid(){
		
		bool[,] layer_1_mask = MaskOperations.DiscreteMaskOps.GenerateLayerMask(GetTilesLayer(1)); // Item layer
		bool[,] layer_2_mask = MaskOperations.DiscreteMaskOps.GenerateLayerMask(GetTilesLayer(2)); // Building Layer
		bool[,] layer_3_mask = MaskOperations.DiscreteMaskOps.GenerateLayerMask(GetTilesLayer(3)); // Unit Layer

		bool[,] merged_mask = MaskOperations.DiscreteMaskOps.AndLayerMasks(layer_1_mask, layer_2_mask);


		if(!path_through_units){
			merged_mask = MaskOperations.DiscreteMaskOps.AndLayerMasks(merged_mask, layer_3_mask);
		}

		grid_mask = merged_mask;

	}

	private Tile[,] GetTilesLayer(int layer){
		Tile [,] subset = new Tile[map_generator.cols, map_generator.rows];

		for(int i=0; i<map_generator.cols; i++){
			for(int j=0; j<map_generator.rows; j++){
				subset[i, j] = tiles[layer, i, j];
			}
		}

		return subset;
	}

	public bool[,] GetGridMask(){
		return grid_mask;
	}

	public void AcknowledgeUnitMove(Vector3 old_pos, Vector3 new_pos){
		if(update_grid_between_unit_moves){
			grid_mask[(int)old_pos.x, (int)old_pos.y] = true;
			grid_mask[(int)new_pos.x, (int)new_pos.y] = false;
		}
	}

	private void HandleInput(){

		HandleSelection();

	}


	private void HandleSelection(){
		// Remove last add job marker if one is at last hover
		if (last_hover_pos.x != -1 && 
			tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y] != null &&
			((SelectionBox)tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y]).selection_box_type == SelectionBox.SelectionBoxType.add_new_job_marker){
			Destroy(tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y].gameObject);
			Destroy(tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y]);
			tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y] = null;
		}

		// remove hover job marker if there is one left from last update
		if( last_hover_pos.x != -1 &&
			tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y] != null &&
			((SelectionBox)tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y]).selection_box_type == SelectionBox.SelectionBoxType.hover_job_marker){
			int _x = (int)last_hover_pos.x;
			int _y = (int)last_hover_pos.y;

			// remove idle job marker
			Destroy(tiles[4, _x, _y].gameObject);
			Destroy(tiles[4, _x, _y]);
			tiles[4, _x, _y] = null;


			Vector3 position = new Vector3((float)_x, (float)_y, -3f);
			GameObject tile_instance = Instantiate(tile_prefabs.idle_job_marker, position, Quaternion.identity) as GameObject;
			
			var tile = tile_instance.GetComponent<SelectionBox>();
			tile.selection_box_type = SelectionBox.SelectionBoxType.idle_job_marker;
			tiles[4, _x, _y] = tile;
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
		if(hit.collider != null){

			int x = (int)hit.collider.gameObject.transform.position.x;
			int y = (int)hit.collider.gameObject.transform.position.y;


			if(tiles[1, x, y] is SelectableTile){


				if(tiles[4, x, y] == null){ // see if the object is not seleted

					Vector3 position = new Vector3((float)x, (float)y, -3f);

					// Create selector
					GameObject tile_instance = Instantiate(tile_prefabs.add_new_job_marker, position, Quaternion.identity) as GameObject;


					// add add_new_job marker
					var tile = tile_instance.GetComponent<SelectionBox>();
					tile.selection_box_type = SelectionBox.SelectionBoxType.add_new_job_marker;
					tiles[4, x, y] = tile;
					last_hover_pos = new Vector2(x, y);

				}
				else if(((SelectionBox)tiles[4, x, y]).selection_box_type == SelectionBox.SelectionBoxType.idle_job_marker) {

					// remove idle job marker
					Destroy(tiles[4, x, y].gameObject);
					Destroy(tiles[4, x, y]);
					tiles[4, x, y] = null;

					Vector3 position = new Vector3((float)x, (float)y, -3f);
					GameObject tile_instance = Instantiate(tile_prefabs.hover_job_marker, position, Quaternion.identity) as GameObject;
					
					var tile = tile_instance.GetComponent<SelectionBox>();
					tile.selection_box_type = SelectionBox.SelectionBoxType.hover_job_marker;
					tiles[4, x, y] = tile;
					last_hover_pos = new Vector2(x, y);
				}

				if(Input.GetMouseButtonDown(0)){

					if (last_hover_pos.x != -1 && tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y] != null){
						Destroy(tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y].gameObject);
						Destroy(tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y]);
						tiles[4, (int)last_hover_pos.x, (int)last_hover_pos.y] = null;
					}

					Vector3 position = new Vector3((float)x, (float)y, -3f);

					GameObject tile_instance = Instantiate(tile_prefabs.idle_job_marker, position, Quaternion.identity) as GameObject;

					var tile = tile_instance.GetComponent<SelectionBox>();
					tile.selection_box_type = SelectionBox.SelectionBoxType.idle_job_marker;
					tiles[4, x, y] = tile;

					last_hover_pos = new Vector2(-1, -1);

					CreateNewJobAtLocation(tiles[1, x, y]);

				}


			}


		}

	}

	void CreateNewJobAtLocation(Tile tile){

		if(tile is ResourceDeposit){
			// we need to create a gather item job

			GatherItemJob new_gather_item_job = null;

			var pos = tile.transform.position;
			Target target = new Target((int)pos.x, (int)pos.y, (int)pos.z);

			if(tile is CoalDepositTile){
				new_gather_item_job = new GatherItemJob(target, new Coal(), item_yield_stats.coal_deposit_yield);
			}
			else if(tile is CopperDepositTile){
				new_gather_item_job = new GatherItemJob(target, new CopperOre(), item_yield_stats.copper_deposit_yield);
			}
			else if(tile is IronDepositTile){
				new_gather_item_job = new GatherItemJob(target, new IronOre(), item_yield_stats.iron_deposit_yield);
			}
			else if(tile is StoneDepositTile){
				new_gather_item_job = new GatherItemJob(target, new Stone(), item_yield_stats.stone_deposit_yield);
			}

			if(new_gather_item_job != null){
				job_queue.Enqueue(new_gather_item_job, new_gather_item_job.priority);
			}
		} 
	}
}
