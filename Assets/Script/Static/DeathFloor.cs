using UnityEngine;
using System;
using System.Collections.Generic;

public class DeathFloor : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
	    if (other.GetComponent<Hero>() != null) {
	        var heroes = GameObject.FindGameObjectsWithTag("Player");
	        foreach (var heroGO in heroes) {
                var hero = heroGO.GetComponent<Hero>();
	            hero.TakeDamage(10000f, gameObject);
                hero.UseGravity(false);
	        }
	    } else {
	        other.gameObject.SendMessage("TakeDamage", 10000f);
	        GameObject.Destroy(other.gameObject);
	    }
	}
}
