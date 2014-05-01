using UnityEngine;

public class SpawnSingleUnit : Spawner {
	public Transform FacePoint;
	public BaseUnit[] SpawnableUnits;

	public override void Spawn() {
	    if (_enemiesToSpawn > 0) {
	        var unitType = SpawnableUnits[Random.Range(0, SpawnableUnits.Length - 1)];
	        var unit = (BaseUnit) GameObject.Instantiate(unitType, transform.position, Quaternion.identity);
            //print("spawned a: " + unit.name);
	        if (FacePoint != null)
	            unit.transform.LookAt(FacePoint);
	    }
	}

    public override bool SetEnemiesToSpawn(int enemies) {
        if (enemies <= 1) {
            _enemiesToSpawn = enemies;
            return true;
        }
        return false;
    }
    public override bool AddEnemiesToSpawn(int enemies) {
        if (_enemiesToSpawn + enemies <= 1) {
            _enemiesToSpawn += enemies;
            return true;
        }
        return false;
    }
}
