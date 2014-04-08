using UnityEngine;
using System;
using System.Collections.Generic;

public class ProjectileWeapon : MonoBehaviour {
	public GameObject ProjectilePrefab;
	public GameObject Target;
	public GameObject Aim;
	public float AttackDelay;
	private float lastAttackTime = 0;
	
	void Update () {
		if(Time.time - lastAttackTime > AttackDelay) {
			var g = (GameObject)GameObject.Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
			var bullet = g.GetComponent<Projectile>();
			bullet.AddEffect(new Damage(2));
			bullet.Body = gameObject;
			bullet.SetPoints(transform.position, Aim.transform.position, Target.transform.position);
			lastAttackTime = Time.time;
		}
		var a = Vector3.Lerp(transform.position, Target.transform.position, 0.5f);
		a.y = (Vector3.up * Mathf.Max(1, Target.transform.position.y - transform.position.y) * 1.5f).y + transform.position.y;
		Aim.transform.position = a;
	}
}
