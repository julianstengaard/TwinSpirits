using UnityEngine;
using System.Collections;

public abstract class Spawner : MonoBehaviour {
    protected int _enemiesToSpawn;

	public abstract void Spawn();

    public abstract bool SetEnemiesToSpawn(int enemies);
    public abstract bool AddEnemiesToSpawn(int enemies);
}
