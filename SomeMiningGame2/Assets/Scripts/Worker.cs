using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PathFind;

public class Worker : Tile {

	public int unit_action_speed = 10;
	private int unit_action_counter = 0;

	public string name;

	private Job job;
	private Dispatcher dispatcher;
	private List<PathFind.Point> path;

	private PathFind.Grid pathfinding_grid;

	private Dictionary<Item, int> inventory = new Dictionary<Item, int>();

	public GameObject worker_action_prefab;

	private GameObject worker_action_instance;
	private WorkerActionLine worker_action_line;	
	public int action_line_lifetime = 4;
	private int action_line_counter = 0;

	private PauseGame pause_game_control;


	// Use this for initialization
	void Start () {
		unit_action_counter = (int)Random.Range(0f, unit_action_speed);

		var pauser = GameObject.Find("GamePauser");
		pause_game_control = pauser.GetComponent<PauseGame>();

		// pick random name
		int index = (int)Random.Range(0f, (float)WorkerNames.names.Count);
		name = WorkerNames.names[index];
		name = char.ToUpper(name[0]) + name.Substring(1);
	}
	
	// Update is called once per frame
	void Update () {

		if(pause_game_control.paused){
			return;
		}

		CleanUpActionLine();

		if(unit_action_counter < unit_action_speed){
			unit_action_counter += 1;
			return;
		}
		else{
			unit_action_counter = 0;
		}



		// If we have a job do something about it
		if(job != null){
			// If we don't have a path we are either at the job
			// or we need to compute an new path	
			if(!IsPathing()){

				if(IsByJob()){
					// we are at the target, do work! 
					path = null;
					WorkOnJob();
				}
				else{
					// We arent't near the target, path there
					var pos = GetPos();
					var tgt = job.GetTarget();

					bool[,] d_grid_mask = dispatcher.GetGridMask();
					bool[,] grid_mask = new bool[d_grid_mask.GetLength(0), d_grid_mask.GetLength(1)];

					for(int i=0; i<d_grid_mask.GetLength(0); i++){
						for(int j=0; j<d_grid_mask.GetLength(1); j++){
							grid_mask[i, j] = d_grid_mask[i, j];
						}
					}

					grid_mask[tgt.x, tgt.y] = true;

					if(pathfinding_grid == null){
						pathfinding_grid = new PathFind.Grid(grid_mask);
					}
					else{
						pathfinding_grid.UpdateGrid(grid_mask);
					}

					var _from = new PathFind.Point((int)pos.x, (int)pos.y);
					var _to = new PathFind.Point(tgt.x, tgt.y);
					path = PathFind.Pathfinding.FindPath(pathfinding_grid, _from, _to);

					if(path.Count == 0){
						// we were not able to path to location or. Abort useful tasks, cancel idle tasks;

						if(job is IdleJob){
							job.Cancel();
						}
						else {
							job.Abort("Could not find path to job.");
						}
					}
					else if(path.Count > 1){
						// pop off the last action so we don't move onto the tile.
						path.RemoveAt(path.Count-1);
					}
				}
			}

			// explicitly separate if so that we can move this turn
			// if we needed to compute a new path
			if(IsPathing()){
				
				// get the next point on the path and move there.
				var move_to = path[0];
				path.RemoveAt(0);

				var new_vec = new Vector3((float)move_to.x, (float)move_to.y, transform.position.z);
				transform.position = new_vec;

				var pos = GetPos();
				dispatcher.AcknowledgeUnitMove(pos, new_vec);
			}
		}
		else{
			// if we have no job, we can clear our pathing
			path = null;
		}
		
	}

	private void CleanUpActionLine(){
		if(worker_action_instance != null){
			if(action_line_counter >= action_line_lifetime){
				Destroy(worker_action_instance);
				Destroy(worker_action_line);
			}
			else{
				action_line_counter += 1;
			}
		}
	}

	private void WorkOnJob(){

		if(job.show_work_animation){
			Target tgt = job.GetTarget();
			var pos = GetPos();

			Vector3 position = new Vector3(0f, 0f, 0f);

			worker_action_instance = Instantiate(worker_action_prefab, position, Quaternion.identity) as GameObject;

			worker_action_line = worker_action_instance.GetComponent<WorkerActionLine>();
			worker_action_line.SetPoints(
				new Vector2(pos.x, pos.y),
				new Vector2((float)tgt.x, (float)tgt.y)
			);
			action_line_counter = 0;
		}

		GenericResult result = job.Do();

		if(result is ItemYieldResult){
			var _result = (ItemYieldResult)result;	
			if(!inventory.ContainsKey(_result.item_type)){
				inventory[_result.item_type] = 0;
			}
			inventory[_result.item_type] += _result.yield;

			Debug.Log("Added " + _result.yield + " of " + _result.item_type.name + " to inventory.");
		}
	}


	public void AssignJob(Job job){
		this.job = job;
	}

	public Job GetJob(){
		return job;
	}

	public void ClearJob(){
		job = null;
	}

	public void SetDispatcher(Dispatcher dispatcher){
		this.dispatcher = dispatcher;
	}

	public Vector3 GetPos(){
		return transform.position;
	}

	public bool IsPathing(){
		bool result = path != null && path.Count > 0;
		return result;
	}

	public bool IsByJob(){

		var pos = GetPos();
		var tgt = job.GetTarget();
		var distance_x = Mathf.Abs(pos.x - tgt.x);
		var distance_y = Mathf.Abs(pos.y - tgt.y);


        return (distance_x <= 1f && distance_y <= 1f);
	}
	
}
