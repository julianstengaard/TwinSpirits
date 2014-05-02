using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SpawnGroup : Spawner {
	public bool RandomSelection;
	public bool SpawnSingle;
	public float DelayBetweenSpawns;

	public Spawner[] SpawnPoints;
    private int _maxPossibleSpawns;

    public override void Spawn () {
        StartCoroutine(StartSpawn());
	}

    private IEnumerator StartSpawn() {
		IEnumerable<Spawner> spawns = RandomSelection ? Shuffle(SpawnPoints.ToList()) : SpawnPoints;
        
        int enemiesRemaining = _enemiesToSpawn;
        int changes = 0;
        while (enemiesRemaining > 0) {
            changes = 0;
            foreach (var spawner in spawns) {
                if (spawner.AddEnemiesToSpawn(1)) {
                    enemiesRemaining -= 1;
                    changes++;
                    if (enemiesRemaining <= 0)
                        break;
                }
            }
            if (changes <= 0)
                break;
        }
        int spawned = 0;
        foreach (var s in spawns) {
            if (spawned < _enemiesToSpawn) {
                s.Spawn();
                if (SpawnSingle) break;
                if (DelayBetweenSpawns > 0)
                    yield return new WaitForSeconds(DelayBetweenSpawns);
            } else {
                break;
            }
        }
	}

    public static IList<T> Shuffle<T>(IList<T> list) {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public override bool SetEnemiesToSpawn(int enemies) {
        if (enemies <= SpawnPoints.Length) {
            _enemiesToSpawn = enemies;
            return true;
        }
        return false;
    }
    public override bool AddEnemiesToSpawn(int enemies) {
        if (_enemiesToSpawn + enemies <= SpawnPoints.Length) {
            _enemiesToSpawn += enemies;
            return true;
        }
        return false;
    }
}
