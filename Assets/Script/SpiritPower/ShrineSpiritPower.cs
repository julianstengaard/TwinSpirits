﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ShrineSpiritPower : Activatable {
	public GameObject[] SpiritPowers;
    public AudioClip EmergeSound;
    public AudioClip SpiritPowerSound;

    public bool Rotates = true;
    public bool RotateAt45Degrees = false;

	private CollectableSpiritPower _attachedSpiritPower;
	private SphereCollider _pickUpSphere;

	[SerializeField]
	private bool _active;
	private bool _transferring;

	private float buryDepth = 1.6f;

	private ParticleSystem[] particles;

	// Use this for initialization
	void Start () {
		particles = GetComponentsInChildren<ParticleSystem>();
		foreach(var particle in particles)
			particle.Stop();

        transform.position += Vector3.down * buryDepth;
		if(_active) Activate();
	}
	
	// Update is called once per frame
	void Update () {
		if (_attachedSpiritPower == null)
			return;

		if (_active && !_transferring) {
			//Rotate the pickUp
		    if (Rotates) {
		        var currentRotation = _attachedSpiritPower.transform.rotation.eulerAngles;
		        _attachedSpiritPower.transform.position = AttachedSpiritPowerPosition();
		        _attachedSpiritPower.transform.rotation = Quaternion.Euler(currentRotation + new Vector3(0f, 1f, 0f));
		    }
		}
	}

	public Vector3 AttachedSpiritPowerPosition() {
		//return transform.position + new Vector3(-0.35f, 1.2f, -0f);
		return transform.position + new Vector3(0f, 1.6f, 0f);
	}

	void OnTriggerEnter(Collider other) {
		if (!_transferring && other.tag == "Player") {
			var collector = other.GetComponent<Hero>();
			if (!collector.ExchangingSpiritPower && !_attachedSpiritPower.SpiritPowerEquals(collector.currentSpiritPower)) {
				StartCoroutine(InitiatePowerTransfer(collector));
			}
		}
	}

	private IEnumerator InitiatePowerTransfer(Hero collector) {
        gameObject.audio.Stop();
        gameObject.audio.clip = SpiritPowerSound;
        gameObject.audio.Play();

		_transferring = true;
		collector.ExchangingSpiritPower = true;
		float time = 0.5f;
		
		GameObject shrinePower = _attachedSpiritPower.gameObject;
		Vector3 shrineOrigin = AttachedSpiritPowerPosition();
		Vector3 collectorOrigin = collector.transform.position;
		Vector3 midPoint = (collectorOrigin + shrineOrigin) * 0.5f;
		Vector3 localScalePower = shrinePower.transform.localScale;

		TweenParms shrineParmsFirst = new TweenParms().Prop(
			"position", midPoint).Prop(
			"localScale", localScalePower * 0.7f).Ease(
			EaseType.EaseInExpo).Delay(0f);
		HOTween.To(shrinePower.transform, time * 0.5f, shrineParmsFirst);

		yield return new WaitForSeconds(time * 0.5f);

		//Set power as child of target
		shrinePower.transform.parent = collector.transform;

		TweenParms shrineParmsSecond = new TweenParms().Prop(
			"localPosition", new Vector3(0f, 0f, 0f)).Prop(
			"localScale", localScalePower * 0f).Ease(
			EaseType.EaseOutExpo).Delay(0f);
		HOTween.To(shrinePower.transform, time * 0.5f, shrineParmsSecond);


		yield return new WaitForSeconds(time);
		//Destroy recieved object
		Destroy(shrinePower);

		//Send old back
		collectorOrigin = collector.transform.position + Vector3.up;
		shrinePower = (GameObject) Instantiate(FindSpiritPowerGO(collector.currentSpiritPower), collectorOrigin, Quaternion.identity);
		shrinePower.transform.localScale = new Vector3(0f, 0f, 0f);

        var rotation = RotateAt45Degrees ? Quaternion.Euler(45f, 0f, 45f) : Quaternion.identity;

		TweenParms collectorParms = new TweenParms().Prop(
			"position", AttachedSpiritPowerPosition()).Prop(
			"localScale", new Vector3(0.8f, 0.8f, 0.8f)).Prop(
            "rotation", rotation).Ease(
			EaseType.EaseInOutExpo).Delay(0f);
		HOTween.To(shrinePower.transform, time, collectorParms);

		//Recieve new
		_attachedSpiritPower.Collected(collector);

		//Transfer complete
		_attachedSpiritPower = shrinePower.GetComponent<CollectableSpiritPower>();

		yield return new WaitForSeconds(time);
		collector.ExchangingSpiritPower = false;
		_transferring = false;
	}

	public override void Activate() {
		if (EmergeSound != null) {
		    gameObject.audio.clip = EmergeSound;
	        gameObject.audio.Play();
		}

		foreach(var particle in particles)
			particle.Play();
		StartCoroutine(AnimateReveal());
	}
	private IEnumerator AnimateReveal() {
		float time = 3f;

		CreateSphereCollider();
		GameObject randomPower = null;

		while (randomPower == null) {
			randomPower = GetRandomSpiritPower();
			yield return new WaitForSeconds(1f);
		}
	    var rotation = RotateAt45Degrees ? Quaternion.Euler(45f, 0f, 45f) : Quaternion.identity;
        var spiritPowerGO = (GameObject)GameObject.Instantiate(randomPower, AttachedSpiritPowerPosition(), rotation);
		_attachedSpiritPower = spiritPowerGO.GetComponent<CollectableSpiritPower>();
		spiritPowerGO.transform.parent = gameObject.transform;

		//Move up
		TweenParms revealTween = new TweenParms().Prop(
			"position", transform.position + Vector3.up * buryDepth).Ease(
			EaseType.Linear).Delay(0f);
		HOTween.To(gameObject.transform, time, revealTween);

		//Scale power
		var targetScale = spiritPowerGO.transform.localScale;
		spiritPowerGO.transform.localScale = new Vector3(0f, 0f, 0f);

		TweenParms scaleTween = new TweenParms().Prop(
			"localScale", targetScale).Ease(
			EaseType.EaseInOutElastic).Delay(2.5f);
		HOTween.To(spiritPowerGO.transform, 1f, scaleTween);

		yield return new WaitForSeconds(time);
		_active = true;
	}

    private GameObject GetRandomSpiritPower() {
        var heroes = GameObject.FindObjectsOfType<Hero>();
        if (heroes.Length == 2 && heroes[0].currentSpiritPower.GetType() == heroes[1].currentSpiritPower.GetType()) {
            while (true) {
                GameObject power = SpiritPowers[Random.Range(0, SpiritPowers.Length)];
                if (!power.GetComponent<CollectableSpiritPower>().SpiritPowerEquals(heroes[0].currentSpiritPower)) {
                    return power;
                }
            }
        } else {
            return SpiritPowers[Random.Range(0, SpiritPowers.Length)];
        }
    }

	private void CreateSphereCollider() {
		_pickUpSphere = (SphereCollider) gameObject.AddComponent<SphereCollider>();
		_pickUpSphere.radius = 1.5f;
		_pickUpSphere.isTrigger = true;
	}

	private GameObject FindSpiritPowerGO(SpiritPower power) {
		foreach (var spiritPower in SpiritPowers) {
			var collectable = spiritPower.GetComponent<CollectableSpiritPower>();
			if (collectable.SpiritPowerEquals(power)) {
				return spiritPower;
			}
		}
		return null;
	}
}
