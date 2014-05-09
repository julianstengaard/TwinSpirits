using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateLevel : MonoBehaviour {

	public int mazeWidth = 1;
	public int mazeHeight = 1;
	public int mazeLength = 2;
	public int maxPathIterationLenght = 2;

	public float cellSpacing = 1f;
	public float cellSize;

	public GameObject StartCell;
	public GameObject BossCell;
	private bool BossCellInstansiated;

	public GameObject[] IslandsWithBridgeOpenings;

	private GameObject[] mazeCellPrefabs0000;
	private GameObject[] mazeCellPrefabs0001;
	private GameObject[] mazeCellPrefabs0010;
	private GameObject[] mazeCellPrefabs0011;

	private GameObject[] mazeCellPrefabs0100;
	private GameObject[] mazeCellPrefabs0101;
	private GameObject[] mazeCellPrefabs0110;
	private GameObject[] mazeCellPrefabs0111;

	private GameObject[] mazeCellPrefabs1000;
	private GameObject[] mazeCellPrefabs1001;
	private GameObject[] mazeCellPrefabs1010;
	private GameObject[] mazeCellPrefabs1011;

	private GameObject[] mazeCellPrefabs1100;
	private GameObject[] mazeCellPrefabs1101;
	private GameObject[] mazeCellPrefabs1110;
	private GameObject[] mazeCellPrefabs1111;

	public GameObject bridgeSpanPrefab;
	public GameObject bridgeEndPrefab;

	private GameObject[,] mazeCellInstances;
	private ArrayList mazeBridgeInstances = new ArrayList();

	private Dictionary<int, GameObject[]> mazeCellPrefabs = new Dictionary<int, GameObject[]>();
	private Maze level;

	private int mazeFromX;
	private int mazeFromY;

	// Use this for initialization
	void Start () 
	{
		//Search for menu settings
		GameObject levelInfo = GameObject.Find("LevelCreationInfo");

		if (levelInfo != null) {
			mazeLength = levelInfo.GetComponent<LevelCreationInfo>().levelLength;
		}


		mazeFromX = Mathf.FloorToInt(mazeWidth/2);
		mazeFromY = mazeHeight-1;


		CreateMaze();
	}

	void Update ()
	{
//		if (Input.GetKeyDown(KeyCode.Space))
//		{
//			DestroyMaze();
//			CreateMaze();
//		}
	}

	private void CreateMaze()
	{
		level = new Maze(mazeWidth, mazeHeight);
		PopulateIslandArrays();
		ImportCellPrefabs();

		level.GenerateMazeFromCell(mazeFromX, mazeFromY, mazeLength, maxPathIterationLenght);

        //Send the maze to the minimap
        var miniMap = GameObject.FindGameObjectWithTag("MiniMap").GetComponent<MiniMap>();
        miniMap.CreateMiniMap(level);

		InstantiateMaze(level);
		InstantiateMazeBridges(level);
	}

	private void DestroyMaze ()
	{
		for (int i = 0; i < mazeCellInstances.GetLength(0); i++)
		{
			for (int j = 0; j < mazeCellInstances.GetLength(1); j++)
			{
				Destroy((Object) mazeCellInstances[i,j]);
			}
		}
		foreach (GameObject[] bridge in mazeBridgeInstances)
		{
			for (int i = 0; i < bridge.Length; i++)
			{
				Destroy((Object) bridge[i]);
			}
		}
	}

	private void InstantiateMaze(Maze maze)
	{
        mazeCellInstances = new GameObject[maze.width, maze.height];

		for (int y = 0; y < maze.height; y++)
		{
			for (int x = 0; x < maze.width; x++)
			{
				int dictionaryKey = ConvertDoorsToDictionaryKey(maze.GetCell(x, y).doors);

				//If start cell
				if(x == mazeFromX && y == mazeFromY) {
					Vector3 randomizedCellPosition = GetRandomCellDisplacement(x * cellSpacing, -y * cellSpacing, cellSpacing);
					mazeCellInstances[x,y] = (GameObject)GameObject.Instantiate(StartCell, randomizedCellPosition, Quaternion.identity);
				} 
				else if ( !maze.GetCell(x, y).HasNoDoors() ) 
				{
					Vector3 randomizedCellPosition = GetRandomCellDisplacement(x * cellSpacing, -y * cellSpacing, cellSpacing);
					//print ("ins: " + dictionaryKey);
					//var cells = (GameObject[]) mazeCellPrefabs[dictionaryKey];
					var validIslands = (GameObject[]) mazeCellPrefabs[dictionaryKey];
					mazeCellInstances[x,y] = InstantiateCell(validIslands , Random.Range (0, validIslands.Length), randomizedCellPosition, maze, x, y);
				}

			    if (mazeCellInstances[x, y] != null) {
			        mazeCellInstances[x, y].AddComponent<MazeInstance>().represents = maze.GetCell(x, y);
			    }
			}
		}
	}

	private void PopulateIslandArrays() {
		mazeCellPrefabs0000 = FindIslandsWithDoors(false, false, false, false);
		mazeCellPrefabs0001 = FindIslandsWithDoors(false, false, false, true);
		mazeCellPrefabs0010 = FindIslandsWithDoors(false, false, true, false);
		mazeCellPrefabs0011 = FindIslandsWithDoors(false, false, true, true);
		
		mazeCellPrefabs0100 = FindIslandsWithDoors(false, true, false, false);
		mazeCellPrefabs0101 = FindIslandsWithDoors(false, true, false, true);
		mazeCellPrefabs0110 = FindIslandsWithDoors(false, true, true, false);
		mazeCellPrefabs0111 = FindIslandsWithDoors(false, true, true, true);
		
		mazeCellPrefabs1000 = FindIslandsWithDoors(true, false, false, false);
		mazeCellPrefabs1001 = FindIslandsWithDoors(true, false, false, true);
		mazeCellPrefabs1010 = FindIslandsWithDoors(true, false, true, false);
		mazeCellPrefabs1011 = FindIslandsWithDoors(true, false, true, true);
		
		mazeCellPrefabs1100 = FindIslandsWithDoors(true, true, false, false);
		mazeCellPrefabs1101 = FindIslandsWithDoors(true, true, false, true);
		mazeCellPrefabs1110 = FindIslandsWithDoors(true, true, true, false);
		mazeCellPrefabs1111 = FindIslandsWithDoors(true, true, true, true);
	}

	private GameObject[] FindIslandsWithDoors(bool left, bool top, bool right, bool bot) {
		ArrayList validIslands = new ArrayList();

		foreach (GameObject island in IslandsWithBridgeOpenings) {
			bool[] foundOpenings = {false, false, false, false};
			bool[] forcedOpenings = {false, false, false, false};

			BridgeOpening[] openings = island.transform.GetComponentsInChildren<BridgeOpening>(true);
			//if (openings.Count() > 0)
				//print(island.name);

			//Are some openings forced?
			foreach (BridgeOpening bridgeOpening in openings) {
				if (bridgeOpening.opening == Opening.LEFT && bridgeOpening.forced)
					forcedOpenings[0] = true;
				if (bridgeOpening.opening == Opening.TOP && bridgeOpening.forced)
					forcedOpenings[1] = true;
				if (bridgeOpening.opening == Opening.RIGHT && bridgeOpening.forced)
					forcedOpenings[2] = true;
				if (bridgeOpening.opening == Opening.BOT && bridgeOpening.forced)
					forcedOpenings[3] = true;
			}

			foreach (BridgeOpening bridgeOpening in openings) {
				if ((!left && !forcedOpenings[0]) || (left && bridgeOpening.opening == Opening.LEFT))
					foundOpenings[0] = true;
				if ((!top && !forcedOpenings[1]) || (top && bridgeOpening.opening == Opening.TOP))
					foundOpenings[1] = true;
				if ((!right && !forcedOpenings[2]) || (right && bridgeOpening.opening == Opening.RIGHT))
					foundOpenings[2] = true;
				if ((!bot && !forcedOpenings[3]) || (bot && bridgeOpening.opening == Opening.BOT))
					foundOpenings[3] = true;
			}

			if (foundOpenings[0] && foundOpenings[1] && foundOpenings[2] && foundOpenings[3])
				validIslands.Add(island);
		}

		//Create the island array
		GameObject[] returnIslands = new GameObject[validIslands.Count];

		int i = 0;
		foreach (GameObject validIsland in validIslands) {
			returnIslands[i] = (GameObject) validIsland;
			i++;
		}
		//print ("returning " + returnIslands.Length + " islands for " + left + "" + top + "" + right + "" + bot);
		return returnIslands;
	}

	private void InstantiateMazeBridges(Maze maze)
	{ 
		for (int y = 0; y < maze.height; y++)
		{
			for (int x = 0; x < maze.width; x++)
			{
				bool[] doors = maze.GetCell(x, y).doors;

				GameObject island1Anchor;
				GameObject island2Anchor;

				//Only instantiate bridges right & down (to avoid double bridges)
				if (doors[2]) {
					island1Anchor = FindBridgeAnchor(mazeCellInstances[x, y], Opening.RIGHT);
					island2Anchor = FindBridgeAnchor(mazeCellInstances[x+1, y], Opening.LEFT);

					mazeBridgeInstances.Add(InstantiateSpanningBridge(
						bridgeSpanPrefab, bridgeEndPrefab, mazeCellInstances[x, y], mazeCellInstances[x+1, y], island1Anchor, island2Anchor));
				}
				if (doors[3]) {
					island1Anchor = FindBridgeAnchor(mazeCellInstances[x, y], Opening.BOT);
					island2Anchor = FindBridgeAnchor(mazeCellInstances[x, y+1], Opening.TOP);

					mazeBridgeInstances.Add( InstantiateSpanningBridge(
						bridgeSpanPrefab, bridgeEndPrefab, mazeCellInstances[x, y], mazeCellInstances[x, y+1], island1Anchor, island2Anchor));
				}
			}
		}
	}

	private GameObject FindBridgeAnchor(GameObject island, Opening opening) {
		BridgeOpening[] islandAnchors;
		islandAnchors = island.GetComponentsInChildren<BridgeOpening>();

		foreach (BridgeOpening anchor in islandAnchors) {
			if (anchor.opening == opening)
				return anchor.gameObject;
		}
		Debug.LogError("Couldn't find bridge anchor for island: " + island.name);
		return null;
	}

	private Vector3 GetRandomCellDisplacement(float positionX, float positionY, float cellDistance)
	{
		return new Vector3 (positionX + ((cellDistance-cellSize)/2f) * Random.Range(-0.75f, 0.75f),
							0,
		                    positionY + ((cellDistance-cellSize)/2f) * Random.Range(-0.75f, 0.75f));
	}

	private int ConvertDoorsToDictionaryKey(bool[] doors)
	{
		int key = 0;
		if(doors[0])
			key += 1000;
		if(doors[1])
			key += 100;
		if(doors[2])
			key += 10;
		if(doors[3])
			key += 1;

		return key;

	}

	private void ImportCellPrefabs()
	{
		mazeCellPrefabs.Add(0000, mazeCellPrefabs0000);
		mazeCellPrefabs.Add(0001, mazeCellPrefabs0001);
		mazeCellPrefabs.Add(0010, mazeCellPrefabs0010);
		mazeCellPrefabs.Add(0011, mazeCellPrefabs0011);
		mazeCellPrefabs.Add(0100, mazeCellPrefabs0100);
		mazeCellPrefabs.Add(0101, mazeCellPrefabs0101);
		mazeCellPrefabs.Add(0110, mazeCellPrefabs0110);
		mazeCellPrefabs.Add(0111, mazeCellPrefabs0111);
		mazeCellPrefabs.Add(1000, mazeCellPrefabs1000);
		mazeCellPrefabs.Add(1001, mazeCellPrefabs1001);
		mazeCellPrefabs.Add(1010, mazeCellPrefabs1010);
		mazeCellPrefabs.Add(1011, mazeCellPrefabs1011);
		mazeCellPrefabs.Add(1100, mazeCellPrefabs1100);
		mazeCellPrefabs.Add(1101, mazeCellPrefabs1101);
		mazeCellPrefabs.Add(1110, mazeCellPrefabs1110);
		mazeCellPrefabs.Add(1111, mazeCellPrefabs1111);
	}

	private GameObject InstantiateCell(GameObject[] mazeCellPrefab, int prefabNumber, Vector3 position, Maze maze, int x, int y)
	{
		var fab = mazeCellPrefab[prefabNumber];
		var mCell = maze.GetCell(x, y);
		GameObject cell;
		if(!BossCellInstansiated && mCell.stepsFromOrigin == maze.MaxSteps) {
			cell = (GameObject) Instantiate(BossCell, position, BossCell.transform.rotation);
			BossCellInstansiated = true;
		} else
			cell = (GameObject) Instantiate(fab, position, fab.transform.rotation);

		cell.name = "Distance" + mCell.stepsFromOrigin + "Generation" + mCell.generationNumber;
		return cell;
	}

	private GameObject[] InstantiateSpanningBridge(GameObject bridgeMiddlePrefab, GameObject bridgeEndPrefab, GameObject island1, GameObject island2, GameObject island1Anchor, GameObject island2Anchor)
	{
		GameObject[] bridge = new GameObject[3];

		Vector3 position1 = island1Anchor.transform.position;
		Vector3 position2 = island2Anchor.transform.position;

		//Create end pieces
		//Vector3 bridgeStartPosition = Vector3.Lerp(position1, position2, 0.45f * cellSize / (position1 - position2).magnitude);
		//Vector3 bridgeEndPosition = Vector3.Lerp(position2, position1, 0.45f * cellSize / (position2 - position1).magnitude);

		//Find height to set bridge gates at
		/*
		RaycastHit 	hitIsland1;
		RaycastHit	hitIsland2;
		Vector3		positionIsland1 = Vector3.zero;
		Vector3 	positionIsland2 = Vector3.zero;

		if (Physics.Raycast(bridgeStartPosition + Vector3.up * 20f, Vector3.down, out hitIsland1, 100.0F))
			positionIsland1 = hitIsland1.point;
		if (Physics.Raycast(bridgeEndPosition 	+ Vector3.up * 20f, Vector3.down, out hitIsland2, 100.0F))
			positionIsland2 = hitIsland2.point;*/

		Vector3 midPoint = (position1 + position2) * 0.5f;
		
		//Calculate scale/rotation
		float spanningScale = Vector3.Distance(position1, position2);
		Quaternion rotation0 = Quaternion.LookRotation(position2 - position1, Vector3.up);
		Quaternion rotation1 = Quaternion.LookRotation(position1 - position2, Vector3.up);

		bridge[0] = (GameObject) Instantiate(bridgeEndPrefab, position1, rotation0); 
		bridge[1] = (GameObject) Instantiate(bridgeEndPrefab, position2, rotation1); 

		//Add the bridge gates to the activator
		SpawnActivator island1Activator = island1.GetComponentInChildren<SpawnActivator>();
		if (island1Activator)
			island1Activator.BridgeGates.Add(bridge[0]);

		SpawnActivator island2Activator = island2.GetComponentInChildren<SpawnActivator>();
		if (island2Activator)
			island2Activator.BridgeGates.Add(bridge[1]);

		//Create at default position
		bridge[2] = (GameObject) Instantiate(bridgeMiddlePrefab, midPoint, Quaternion.identity); 

		//Scale and rotate it
		var s = bridge[2].transform.localScale;
		s.z = spanningScale;
		bridge[2].transform.localScale = s;
		bridge[2].transform.localRotation = rotation0;

		return bridge;
	}

    public Maze GetMaze()
    {
        return level;
    }

    public GameObject[,] GetMazeCellInstances()
    {
        return mazeCellInstances;
    }
}
