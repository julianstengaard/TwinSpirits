using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpawnActivator : MonoBehaviour {
	public float SpawnDelayInSeconds = 3f;
	public GameObject GateBlocker;
	public Spawner[] Spawners;
	public List<GameObject> BridgeGates = new List<GameObject>();
	private List<GameObject> _activeBlockers = new List<GameObject>();
	private bool hasSpawned = false;
	private bool lockdownTimeoutExpired;
	private float lockdownTimer = 8;
	private int activePlayersThreshold = 2;
	private int activePlayersWithin = 0;

	void Update() {
		if(!lockdownTimeoutExpired) return;

		var enemies = GameObject.FindGameObjectsWithTag("Enemy");

		if(enemies.Length == 0)
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
	}

	private void deactivateLockdown() {
		foreach(var blocker in _activeBlockers) {
			GameObject.Destroy(blocker);
		}
		_activeBlockers.Clear();
	}

	void OnTriggerEnter() {
		if(hasSpawned) return;

		
		GenerateMesh();
		activePlayersWithin++;
		if(activePlayersWithin != activePlayersThreshold) return;

		hasSpawned = true;
		Debug.Log("Starting island");
		StartEverything();
	}

	void OnTriggerExit() {
		activePlayersWithin--;
	}
}
