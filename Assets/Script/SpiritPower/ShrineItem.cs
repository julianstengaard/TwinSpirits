﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ShrineItem : MonoBehaviour {
	public GameObject[] Items;
	
	private Collectable _attachedItem;
	private SphereCollider _pickUpSphere;
	
	private bool _active;
	private float buryDepth = 1.5f;
	
	// Use this for initialization
	void Start () {
		transform.position += Vector3.down * buryDepth;
		Activate();
	}
	
	void OnTriggerEnter(Collider other) {

		if (_active && other.tag == "Player") {
			var collector = other.GetComponent<Hero>();
			_attachedItem.Collected(collector);
			GameObject.Destroy(_attachedItem.gameObject);
			_active = false;
		}
	}
	
	public void Activate() {
		StartCoroutine(AnimateReveal());
	}
	private IEnumerator AnimateReveal() {
		float time = 3f;
		
		CreateSphereCollider();
		var randomItem = Items[Random.Range(0, Items.Length)];
		var itemGO = (GameObject) GameObject.Instantiate(randomItem, transform.position + new Vector3(-0.35f, 1.2f, -0f), Quaternion.identity);
		_attachedItem = itemGO.GetComponent<Collectable>();
		itemGO.transform.parent = gameObject.transform;
		
		//Move up
		TweenParms revealTween = new TweenParms().Prop(
			"position", transform.position + Vector3.up * buryDepth).Ease(
			EaseType.Linear).Delay(0f);
		HOTween.To(gameObject.transform, time, revealTween);
		
		//Scale power
		var targetScale = itemGO.transform.localScale;
		itemGO.transform.localScale = new Vector3(0f, 0f, 0f);
		
		TweenParms scaleTween = new TweenParms().Prop(
			"localScale", targetScale).Ease(
			EaseType.EaseInOutElastic).Delay(2.5f);
		HOTween.To(itemGO.transform, 1f, scaleTween);
		
		yield return new WaitForSeconds(time);
		_active = true;
	}
	
	private void CreateSphereCollider() {
		_pickUpSphere = (SphereCollider) gameObject.AddComponent<SphereCollider>();
		_pickUpSphere.radius = 1.5f;
		_pickUpSphere.isTrigger = true;
	}
}