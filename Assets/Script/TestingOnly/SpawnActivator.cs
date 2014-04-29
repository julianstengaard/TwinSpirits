using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnActivator : MonoBehaviour {
	public float SpawnDelayInSeconds = 3f;
	public GameObject GateBlocker;
	public Spawner[] Spawners;
	public List<GameObject> BridgeGates = new List<GameObject>();
	public Activatable[] Shrines;

	private List<GameObject> _activeBlockers = new List<GameObject>();
	private bool hasSpawned = false;
	private bool lockdownTimeoutExpired;
	private float lockdownTimer = 8;
	private int activePlayersWithin = 0;
	private bool isActive;
	private float ShrineChance = 0.3f;

    public bool SelfActivated = true;
    public bool GameOverIsland = false;
    private MiniMap _miniMap;

	void Start() {
		Shrines = transform.parent.GetComponentsInChildren<Activatable>();
		//print(Shrines.Length);
	    _miniMap = GameObject.FindGameObjectWithTag("MiniMap").GetComponent<MiniMap>();
	}

	void Update() {
		if(!lockdownTimeoutExpired) return;

		var enemies = GameObject.FindGameObjectsWithTag("Enemy");

		if(enemies.Length == 0 && isActive)
			deactivateLockdown ();
	}

	public void StartEverything() {
		activateLockdown();
		StartCoroutine(delayedSpawn());
		StartCoroutine(startLockdownTimer());
	}

	public void GenerateMesh() {
		GetComponentInChildren<CalcNavigation>().GenerateMesh();
	}

	private IEnumerator delayedSpawn() {
		yield return new WaitForSeconds(SpawnDelayInSeconds);
		foreach(var s in Spawners)
			s.Spawn();
	}

	private IEnumerator startLockdownTimer() {
		yield return new WaitForSeconds(lockdownTimer + SpawnDelayInSeconds);
		lockdownTimeoutExpired = true;
	}

	private void activateLockdown() {
		foreach(var gate in BridgeGates) {
			var blocker = (GameObject)GameObject.Instantiate(GateBlocker, gate.transform.position, gate.transform.rotation);
			_activeBlockers.Add(blocker);
		}
        isActive = true;
        _miniMap.SetNeighborsDiscovered(gameObject.transform.root.gameObject);
        _miniMap.SetCellDangerous(gameObject.transform.root.gameObject);
	}

	private void deactivateLockdown() {
		foreach(var blocker in _activeBlockers) {
			GameObject.Destroy(blocker);
		}
		_activeBlockers.Clear();

		foreach(var shrine in Shrines)
			if(Random.Range (0.0f, 1.0f) <= ShrineChance)
				shrine.Activate();
		isActive = false;

        _miniMap.SetCellDone(gameObject.transform.root.gameObject);
	    if (GameOverIsland) {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>().SetGameOver(true);
	    }
	}

	private int getActivePlayersThreshold() {
		var heroes = GameObject.FindObjectsOfType<Hero>();
		var res = 0;
		foreach(var hero in heroes)
			if(!hero.dead)
				res++;
		return res;
	}

	void OnTriggerEnter() {
        _miniMap.SetPlayerPosition(gameObject.transform.root.gameObject);
        activePlayersWithin++;

		if(hasSpawned) return;

	    if (SelfActivated) {
	        GenerateMesh();
	        if (activePlayersWithin != getActivePlayersThreshold()) return;

	        hasSpawned = true;
	        StartEverything();
	    }
	}

    public void RemoteActivate() {
        GenerateMesh();
        hasSpawned = true;
        StartEverything();
    }

    void OnTriggerExit() {
		activePlayersWithin--;
	}
}
