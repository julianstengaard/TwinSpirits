using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Weapon : MonoBehaviour {
	public GameObject Body;
	//public float Damage;
	public string[] ImmuneTags;
	public List<Effect> AttackEffects = new List<Effect>();
//	private Animator _anim;

	[SerializeField]
	private bool isDangerous = true; // NOT USED YET


	void Start() {
//		if(Body == null)
//			return;
//
//		_anim = Body.GetComponent<Animator>();
	}

	public void MakeDangerous() {
		isDangerous = true;
	}

	public void MakeInert() {
		isDangerous = false;
	}

	public void AddEffect(Effect e) {
		AttackEffects.Add(e);
	}

	void OnTriggerEnter(Collider other) {
//		if(_anim == null) {
//			Start ();
//			return;
//		}

		if(isDangerous) {
			var unit = other.GetComponent<BaseUnit>();
			if(unit != null) {
				var initPosition = Body.transform.position;
				unit.EvaluateAttacks(Body, initPosition, AttackEffects, ImmuneTags);
			}
		}
	}
}
