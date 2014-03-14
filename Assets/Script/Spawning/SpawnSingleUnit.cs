using UnityEngine;
using System.Collections;

public class SpawnSingleUnit : Spawner {
	public Transform FacePoint;
	public BaseUnit[] SpawnableUnits;

	public override void Spawn() {
		var unitType = SpawnableUnits[Random.Range(0, SpawnableUnits.Length-1)];
		var unit = (BaseUnit)GameObject.Instantiate(unitType, transform.position, Quaternion.identity);
		if(FacePoint != null)
			unit.transform.LookAt(FacePoint);
	}
}
