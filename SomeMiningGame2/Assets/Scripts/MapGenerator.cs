using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

	public int generator_iterations = 10;

	/* Depth Layers
		0: Grass/BG
		1: Item Layer
		2: Building/Clipping Layer
		3: Unit Layer
		4: Job Marker Layer
	 */
	private int depth = 5; 
	public int rows = 100;
	public int cols = 100;

	public int starting_units = 3;
	private int spawned_units = 0;
	private List<Worker> workers = new List<Worker>();

	public Vector3 start_position_top_left = new Vector3(22, 22, 0);
	public Vector3 start_position_bottom_right = new Vector3(27, 27, 0);

	public float iron_spawn_chance = 0.025f;
	public float coal_spawn_chance = 0.025f;
	public float copper_spawn_chance = 0.025f;
	public float stone_spawn_chance = 0.025f;

	private bool is_finished_generating = false;

	private TilePrefabs tile_prefabs;

	private Tile[, ,] tiles;
	private GameObject board_holder;

	// Use this for initialization
	void Start () {
		board_holder = new GameObject("BoardHolder");

		tile_prefabs = GetComponent<TilePrefabs>();

		SetUpTilesArray();

		GenerateGrass();
		
		GenerateItemLayer();

		is_finished_generating = true;
	}

	// Update.GetType() == called once per frame
	void Update () {
		
	}

	private void SetUpTilesArray(){
		tiles =  new Tile[depth, cols, rows];
	}

	private void GenerateGrass(){

		for(int i=0; i < cols; i++){
			for(int j=0; j < rows; j++){
				tiles[0, i, j] = CreateTile<GrassTile>(tile_prefabs.grass_prefab, i, j, 0) as Tile;
			}
		}
	}

	private void GenerateItemLayer(){

		for(int s=0; s < generator_iterations; s++){
			for(int i=0; i < cols; i++){
				for(int j=0; j < rows; j++){

					if( i >= start_position_top_left.x && 
						i <= start_position_bottom_right.x &&
						j >= start_position_top_left.y &&
						j <= start_position_bottom_right.y){

							if(spawned_units < starting_units){
								SpawnUnit(i, j);
								spawned_units += 1;
							}

							continue; //Don't try to add any deposits here
						}

					if(!tiles[1, i, j]){
						GenerateCoalDeposits(i, j, s);	
					}

					if(!tiles[1, i, j]){
						GenerateCopperDeposits(i, j, s);	
					}

					if(!tiles[1, i, j]){
						GenerateIronDeposits(i, j, s);	
					}

					if(!tiles[1, i, j]){
						GenerateStoneDeposits(i, j, s);	
					}

				}
			}
		}
	}

	private void GenerateCoalDeposits(int x, int y, int s){
		bool should_create_tile = RandomDispersal<CoalDepositTile>(x, y, s, coal_spawn_chance);

		if(should_create_tile)
			tiles[1, x, y] = CreateTile<CoalDepositTile>(tile_prefabs.coal_deposit_prefab, x, y, 1);
	}


	private void GenerateCopperDeposits(int x, int y, int s){
		bool should_create_tile = RandomDispersal<CopperDepositTile>(x, y, s, copper_spawn_chance);

		if(should_create_tile)
			tiles[1, x, y] = CreateTile<CopperDepositTile>(tile_prefabs.copper_deposit_prefab, x, y, 1);
	}

	private void GenerateIronDeposits(int x, int y, int s){
		bool should_create_tile = RandomDispersal<IronDepositTile>(x, y, s, iron_spawn_chance);

		if(should_create_tile)
			tiles[1, x, y] = CreateTile<IronDepositTile>(tile_prefabs.iron_deposit_prefab, x, y, 1);
	}

	private void GenerateStoneDeposits(int x, int y, int s){
		bool should_create_tile = RandomDispersal<StoneDepositTile>(x, y, s, stone_spawn_chance);

		if(should_create_tile)
			tiles[1, x, y] = CreateTile<StoneDepositTile>(tile_prefabs.stone_deposit_prefab, x, y, 1);
	}

	private bool RandomDispersal<T>(int x, int y, int s, float probability){
		bool solo = true;

		// above
		int above = y-1;
		if( above>=0 && tiles[1, x, above] != null && tiles[1, x, above].GetType() == typeof(T) ){
			probability += 0.05f;
			solo = false;
		}

		// below
		int below = y+1;
		if( below<rows && tiles[1, x, below] != null && tiles[1, x, below].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}

		// below
		int left = x-1;
		if( left>=0 && tiles[1, left, y] != null && tiles[1, left, y].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}

		// right 
		int right = x+1;
		if( right<cols && tiles[1, right, y] != null && tiles[1, right, y].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}

		// above right
		if( above>=0 && right<cols && tiles[1, right, above] != null && tiles[1, right, above].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}
		
		// above left 
		if( above>=0 && left>=0 && tiles[1, left, above] != null && tiles[1, left, above].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}

		// below left 
		if( below<rows && left>=0 && tiles[1, left, below] != null && tiles[1, left, below].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}

		// below right 
		if( below<rows && right<cols && tiles[1, right, below] != null && tiles[1, right, below].GetType() == typeof(T)){
			probability += 0.05f;
			solo = false;
		}

		if(solo){
			probability -= 0.0001f * s;
		}

		if(probability > Random.Range(0f, 1f)){
			return true; 
		}
		else
		{
			return false;
		}
	}

	private T CreateTile<T>(GameObject prefab, float x, float y, float z){

		Vector3 position = new Vector3(x, y, -z);

		// Create entity
		GameObject tile_instance = Instantiate(prefab, position, Quaternion.identity) as GameObject;

		// Set the tile's parent to board holder
		tile_instance.transform.parent = board_holder.transform;

		return tile_instance.GetComponent<T>();
	}

	private void SpawnUnit(int x, int y){

		GameObject[] unit_options = new GameObject[]{
			tile_prefabs.unit_1,
			tile_prefabs.unit_2,
			tile_prefabs.unit_3
		};

		int idx = (int)Random.Range(0f, unit_options.Length);

		GameObject unit_type = unit_options[idx];

		Vector3 position = new Vector3(x, y, -3);

		// Create entity
		GameObject unit_instance = Instantiate(unit_type, position, Quaternion.identity) as GameObject;

		// Set the tile's parent to board holder
		unit_instance.transform.parent = board_holder.transform;

		Worker worker = unit_instance.GetComponent<Worker>();

		tiles[3, x, y] = worker;

		workers.Add(worker);
	}

	public bool IsFinishedGenerating(){
		return is_finished_generating;
	}

	public Tile[,,] GetTiles(){
		return tiles;
	}

	public List<Worker> GetWorkers(){
		return workers;
	}

}

