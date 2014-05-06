using UnityEngine;
using System.Collections;

public class SpawnSingleUnit : Spawner {
	public Transform FacePoint;
	public BaseUnit[] SpawnableUnits;

	private GameObject particles;

	public void Start() {
		var particlesType = Resources.Load<GameObject>("particle_enemy_spawn");

		particles = (GameObject) GameObject.Instantiate(particlesType, transform.position, Quaternion.identity);
		if(particles == null)
			print ("Blow it out your ass...");
	}

	public override void Spawn() {
		if (_enemiesToSpawn > 0) {
			foreach(var particle in particles.GetComponentsInChildren<ParticleSystem>())
				particle.Play();
			StartCoroutine(DelaySpawn());
	    }
	}

	public IEnumerator DelaySpawn() {
		yield return new WaitForSeconds(2);

		var unitType = SpawnableUnits[Random.Range(0, SpawnableUnits.Length - 1)];
		var unit = (BaseUnit) GameObject.Instantiate(unitType, transform.position, Quaternion.identity);
		//print("spawned a: " + unit.name);
		if (FacePoint != null)
			unit.transform.LookAt(FacePoint);
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
