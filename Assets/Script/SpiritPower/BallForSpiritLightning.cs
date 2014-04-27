using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class BallForSpiritLightning : MonoBehaviour {
    public AudioClip SpawnSound;
    public AudioClip ExplodeSound;

	private Vector3 _originPosition;
	private Vector3 _targetPosition;
	private float _explosionRadius;
	private float _damageOnExplosion;
	private bool _isActivated;
	private bool _doneFlying;
	private bool _exploded;
	public Color BallColor;

	private float _timeToDestination;
	private float _startTime;

	// Use this for initialization
	void Start () {
		BallColor = new Color(0.85f, 1f, 1f, 0.7f);
	    gameObject.audio.clip = SpawnSound;
        gameObject.audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
		//Move towards target
		if (_isActivated) {
			if (!_doneFlying) {
				MoveBall();
			} else if (!_exploded) {
				_exploded = true;
				StartCoroutine(Explode());
			}
			UpdateColor();
		}
	}

	private IEnumerator Explode() {
        gameObject.audio.Stop();
        gameObject.audio.clip = ExplodeSound;
        gameObject.audio.Play();

		//Tween Color
		BallColor = new Color(0.85f, 1f, 1f, 1f);
		Color finalColor = new Color(0.95f, 1f, 1f, 0f);
		TweenParms tweenColorParms = new TweenParms().Prop(
			"BallColor", finalColor).Ease(
			EaseType.EaseInCirc).Delay(0f);
		HOTween.To(this, 0.3f, tweenColorParms);

		//Tween in explosion
		gameObject.transform.localScale *= 0f;
		Vector3 finalSize = Vector3.one * _explosionRadius * 2f;
		TweenParms tweenParms = new TweenParms().Prop(
			"localScale", finalSize).Ease(
			EaseType.EaseInCirc).Delay(0f);
		HOTween.To(gameObject.transform, 0.3f, tweenParms);

		yield return new WaitForSeconds(0.2f);
		Collider[] hits = Physics.OverlapSphere(gameObject.transform.position, _explosionRadius, 1 << 8);
		foreach (var other in hits) {
			if (other.tag == "Enemy") {
				other.gameObject.GetComponent<BaseUnit>().TakeDamage(_damageOnExplosion, gameObject);
			}
		}
		yield return new WaitForSeconds(1f);
		GameObject.Destroy(gameObject);
	}

	private void MoveBall() {
		float height = 2f;
		float pct = (Time.time - _startTime) / _timeToDestination;
		float heightLerp = Mathf.Lerp(0f, height, Mathf.Sin((Mathf.PI)*pct));
		gameObject.transform.position = Vector3.Lerp(_originPosition, _targetPosition, pct);
		gameObject.transform.position += new Vector3(0f, heightLerp, 0f);

		if (pct >= 0.95f)
			_doneFlying = true;
	}

	public void ActivateBall(Vector3 target, float explosionRadius, float damageOnExplosion, float time) {
		_isActivated = true;
		_originPosition = transform.position;
		_targetPosition = target;
		_explosionRadius = explosionRadius;
		_damageOnExplosion = damageOnExplosion;
		_timeToDestination = time;
		_startTime = Time.time;
	}

	private void UpdateColor() {
		gameObject.renderer.material.SetColor("_Color", BallColor);
	}
}
