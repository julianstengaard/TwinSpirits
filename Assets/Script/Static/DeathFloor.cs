using UnityEngine;

public class DeathFloor : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
        Hero triggerHero = other.GetComponent<Hero>();
        if (triggerHero != null) {
            //Find respawn point
            var spawnPoints = GameObject.FindGameObjectsWithTag("RespawnPoint");
	        if (spawnPoints != null && spawnPoints.Length > 0) {
                GameObject closestSpawnPoint = spawnPoints[0];
                float minDistanceSqr = Vector3.SqrMagnitude(other.transform.position - spawnPoints[0].transform.position);
                for (int i = 1; i < spawnPoints.Length; i++) {
                    float distanceSqr = Vector3.SqrMagnitude(other.transform.position - spawnPoints[i].transform.position);
                    if (distanceSqr < minDistanceSqr) {
                        closestSpawnPoint = spawnPoints[i];
                        minDistanceSqr = distanceSqr;
                    }
                }
                triggerHero.TakeDamage(10000f, gameObject, true);
                //Are both dead now?
	            int dead = 0;
                var heroes = GameObject.FindGameObjectsWithTag("Player");
                foreach (var heroGO in heroes) {
                    dead += heroGO.GetComponent<Hero>().dead ? 1 : 0;
                }
                //Only respawn if other player is alive
	            if (dead < 2) {
	                triggerHero.transform.position = closestSpawnPoint.transform.position + Vector3.up*2f;
	            }

	        } else {
                print("No spawn points for resetting player");
                var heroes = GameObject.FindGameObjectsWithTag("Player");
                foreach (var heroGO in heroes) {
                    var hero = heroGO.GetComponent<Hero>();
                    hero.TakeDamage(10000f, gameObject, true);
                    hero.UseGravity(false);
                }
	        }
	    } else {
	        other.gameObject.SendMessage("TakeDamage", 10000f);
	        GameObject.Destroy(other.gameObject);
	    }
	}
}
