using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpawnGroup : Spawner {
	public bool RandomSelection;
	public bool SpawnSingle;
	public float DelayBetweenSpawns;

	public Spawner[] SpawnPoints;

	public override void Spawn () {
		StartCoroutine(StartSpawn());
	}

	private IEnumerator StartSpawn() {
		IEnumerable<Spawner> spawns = RandomSelection ? Shuffle(SpawnPoints.ToList()) : SpawnPoints;

		foreach(var point in spawns) {
			point.Spawn();
			if(SpawnSingle) break;
			if(DelayBetweenSpawns > 0)
				yield return new WaitForSeconds(DelayBetweenSpawns);
		}
	}

	private static IList<T> Shuffle<T>(IList<T> list) {  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = Random.Range(0, n+1);  
			T value = list[k];  
			list[k] = list[n];  
			list[n] = value;  
		}  
		return list;
	}
}
