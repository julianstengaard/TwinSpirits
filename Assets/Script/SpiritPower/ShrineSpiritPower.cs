using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ShrineSpiritPower : Activatable {
	public GameObject[] SpiritPowers;

	private CollectableSpiritPower _attachedSpiritPower;
	//private Vector3 _attachedSpiritPowerPosition;
	private SphereCollider _pickUpSphere;

	private bool _active;
	private bool _transferring;

	private float buryDepth = 1.6f;

	// Use this for initialization
	void Start () {
		transform.position += Vector3.down * buryDepth;
		//Activate();
	}
	
	// Update is called once per frame
	void Update () {
		if (_active && !_transferring) {
			//Rotate the pickUp
			var currentRotation = _attachedSpiritPower.transform.rotation.eulerAngles;
			_attachedSpiritPower.transform.position = AttachedSpiritPowerPosition();
			_attachedSpiritPower.transform.rotation = Quaternion.Euler(currentRotation + new Vector3(0f, 1f, 0f));
		}
	}

	public Vector3 AttachedSpiritPowerPosition() {
		//return transform.position + new Vector3(-0.35f, 1.2f, -0f);
		return transform.position + new Vector3(0f, 1.2f, 0f);
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

		TweenParms collectorParms = new TweenParms().Prop(
			"position", AttachedSpiritPowerPosition()).Prop(
			"localScale", new Vector3(0.8f, 0.8f, 0.8f)).Prop(
			"rotation", Quaternion.Euler(45f, 0f, 45f)).Ease(
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
		StartCoroutine(AnimateReveal());
	}
	private IEnumerator AnimateReveal() {
		float time = 3f;

		CreateSphereCollider();
		var randomPower = SpiritPowers[Random.Range(0, SpiritPowers.Length)];
		var spiritPowerGO = (GameObject) GameObject.Instantiate(randomPower, AttachedSpiritPowerPosition(), Quaternion.Euler(45f, 0f, 45f));
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

	private void CreateSphereCollider() {
		_pickUpSphere = (SphereCollider) gameObject.AddComponent<SphereCollider>();
		_pickUpSphere.radius = 2.5f;
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
