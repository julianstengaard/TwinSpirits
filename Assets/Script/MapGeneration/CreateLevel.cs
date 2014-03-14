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

	public GameObject[] mazeCellPrefabs0000;
	public GameObject[] mazeCellPrefabs0001;
	public GameObject[] mazeCellPrefabs0010;
	public GameObject[] mazeCellPrefabs0011;

	public GameObject[] mazeCellPrefabs0100;
	public GameObject[] mazeCellPrefabs0101;
	public GameObject[] mazeCellPrefabs0110;
	public GameObject[] mazeCellPrefabs0111;

	public GameObject[] mazeCellPrefabs1000;
	public GameObject[] mazeCellPrefabs1001;
	public GameObject[] mazeCellPrefabs1010;
	public GameObject[] mazeCellPrefabs1011;

	public GameObject[] mazeCellPrefabs1100;
	public GameObject[] mazeCellPrefabs1101;
	public GameObject[] mazeCellPrefabs1110;
	public GameObject[] mazeCellPrefabs1111;

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
		ImportCellPrefabs();
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
		level.GenerateMazeFromCell(mazeFromX, mazeFromY, mazeLength, maxPathIterationLenght);
		
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
				if(x == mazeFromX && y == mazeFromY) {
					Vector3 randomizedCellPosition = GetRandomCellDisplacement(x * cellSpacing, -y * cellSpacing, cellSpacing);
					mazeCellInstances[x,y] = (GameObject)GameObject.Instantiate(StartCell, randomizedCellPosition, Quaternion.identity);
				} 
				else if ( !maze.GetCell(x, y).HasNoDoors() ) 
				{
					Vector3 randomizedCellPosition = GetRandomCellDisplacement(x * cellSpacing, -y * cellSpacing, cellSpacing);
					var cells = (GameObject[]) mazeCellPrefabs[dictionaryKey];
					mazeCellInstances[x,y] = InstantiateCell(cells , Random.Range (0, cells.Length), randomizedCellPosition, maze, x, y);
				}
			}
		}
	}

	private void InstantiateMazeBridges(Maze maze)
	{ 
		for (int y = 0; y < maze.height; y++)
		{
			for (int x = 0; x < maze.width; x++)
			{
				bool[] doors = maze.GetCell(x, y).doors;

				//Only instantiate bridges right & down (to avoid double bridges)
				if (doors[2])
				{
					mazeBridgeInstances.Add( InstantiateSpanningBridge(bridgeSpanPrefab, bridgeEndPrefab, 
					    mazeCellInstances[x, y], mazeCellInstances[x+1, y]));
				}
				if (doors[3])
				{
					mazeBridgeInstances.Add( InstantiateSpanningBridge(bridgeSpanPrefab, bridgeEndPrefab, 
						mazeCellInstances[x, y], mazeCellInstances[x, y+1]));
				}
			}
		}
	}

	private Vector3 GetRandomCellDisplacement(float positionX, float positionY, float cellDistance)
	{
		return new Vector3 (positionX + ((cellDistance-cellSize)/2f) * Random.Range(-1f, 1f),
							0,
		                    positionY + ((cellDistance-cellSize)/2f) * Random.Range(-1f, 1f));
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
		GameObject cell = (GameObject) Instantiate(fab, position, fab.transform.rotation);
		cell.name = "Distance" + maze.GetCell(x, y).stepsFromOrigin + "Generation" + maze.GetCell(x, y).generationNumber;
		return cell;
	}

	private GameObject[] InstantiateSpanningBridge(GameObject bridgeMiddlePrefab, GameObject bridgeEndPrefab, GameObject island1, GameObject island2)
	{
		GameObject[] bridge = new GameObject[3];

		Vector3 position1 = island1.transform.position;
		Vector3 position2 = island2.transform.position;

		//Create end pieces
		Vector3 bridgeStartPosition = Vector3.Lerp(position1, position2, 0.45f * cellSize / (position1 - position2).magnitude);
		Vector3 bridgeEndPosition = Vector3.Lerp(position2, position1, 0.45f * cellSize / (position2 - position1).magnitude);

		//Find height to set bridge gates at
		RaycastHit 	hitIsland1;
		RaycastHit	hitIsland2;
		Vector3		positionIsland1 = Vector3.zero;
		Vector3 	positionIsland2 = Vector3.zero;

		if (Physics.Raycast(bridgeStartPosition + Vector3.up * 20f, Vector3.down, out hitIsland1, 100.0F))
			positionIsland1 = hitIsland1.point;
		if (Physics.Raycast(bridgeEndPosition 	+ Vector3.up * 20f, Vector3.down, out hitIsland2, 100.0F))
			positionIsland2 = hitIsland2.point;

		Vector3 midPoint = (positionIsland1 + positionIsland2) * 0.5f;
		
		//Calculate scale/rotation
		float spanningScale = Vector3.Distance(positionIsland1, positionIsland2);
		Quaternion rotation = Quaternion.LookRotation(positionIsland2 - positionIsland1, Vector3.up);

		bridge[0] = (GameObject) Instantiate(bridgeEndPrefab, positionIsland1, rotation); 
		bridge[1] = (GameObject) Instantiate(bridgeEndPrefab, positionIsland2, rotation); 

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
		bridge[2].transform.localRotation = rotation;

		return bridge;
	}
}
