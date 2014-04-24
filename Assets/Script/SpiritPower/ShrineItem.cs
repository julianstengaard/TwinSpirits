using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class ShrineItem : Activatable {
	public GameObject[] Items;
    public AudioClip EmergeSound;
    public AudioClip ItemPickupSound;
	
	private Collectable _attachedItem;
	private SphereCollider _pickUpSphere;

	[SerializeField]
	private bool _active;
	private float buryDepth = 1.6f;
	
	// Use this for initialization
	void Start () {
		transform.position += Vector3.down * buryDepth;
		if(_active) Activate();
	}
	
	void OnTriggerEnter(Collider other) {
		if (_active && other.tag == "Player") {
            gameObject.audio.Stop();
            gameObject.audio.clip = ItemPickupSound;
            gameObject.audio.Play();

			var collector = other.GetComponent<Hero>();
			_attachedItem.Collected(collector);
			GameObject.Destroy(_attachedItem.gameObject);
			_active = false;
		}
	}
	
	public override void Activate() {
        gameObject.audio.clip = EmergeSound;
        gameObject.audio.Play();
		StartCoroutine(AnimateReveal());
	}
	private IEnumerator AnimateReveal() {
		float time = 3f;
		
		CreateSphereCollider();
		var randomItem = Items[Random.Range(0, Items.Length)];
		var itemGO = (GameObject) GameObject.Instantiate(randomItem, transform.position + new Vector3(0f, 1.6f, 0f), Quaternion.LookRotation(Vector3.right));
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
