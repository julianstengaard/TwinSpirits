using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;

public class TriggerForSpiritImmortal : MonoBehaviour
{
	public GameObject BeforeLandingParticle;
	public GameObject AfterLandingParticle;

	private AudioClip _syncSound;
	private float _damageRadius = 3f;
	private float _pullRadius = 6f;
	private float _syncSlowAmount = 2f;

	private float _damagePerInterval = 10f;
	private float _damageInterval = 0.5f;
	private float _damageIntervalTimer = 0f;
	private CharacterController[] enemies;
	private GameObject pullSphere;

    private Vector3 _originPosition;
    private Vector3 _targetPosition;
    private float _explosionRadius;
    //private float _damageOnExplosion;
    private bool _isActivated;
    private bool _doneFlying;
    private bool _exploded;

    public Color TriggerColorCurrent = new Color(1f, 1f, 0f, 0.0f);
    public Color TriggerColorBase = new Color(1f, 1f, 0f, 0.0f);
    public Color TriggerColorReady = new Color(0f, 1f, 0f, 1f);
    private Color _p1Color;
    private Color _p2Color;

    private float _timeToDestination = 1f;
    private float _expiresAfter = 10f;
    private float _startTime;
    private float _expiresTimer;
	private bool _hasPulled = false;

    private Hero _triggerer;
    private SpiritImmortal _powerSource;

    // Use this for initialization
    void Start()
    {
		_syncSound = (AudioClip)Resources.Load("Audio/Whirl pool");
        gameObject.collider.enabled = false;
    }
	/*
    void OnTriggerEnter (Collider other) {
        Hero hero = other.gameObject.GetComponent<Hero>();
        if (hero != null && hero == _triggerer) {
            UpdateReady(true);
            TriggerColorCurrent = TriggerColorReady;
            UpdateColor();
        }
    }
    void OnTriggerExit (Collider other)
    {
        Hero hero = other.gameObject.GetComponent<Hero>();
        if (hero != null && hero == _triggerer)
        {
            UpdateReady(false);
            TriggerColorCurrent = TriggerColorBase;
            UpdateColor();
        }
    } */

    // Update is called once per frame
    void Update() {
        //Move towards target
        if (_isActivated) {
            if (!_doneFlying) {
                MoveTrigger();
            }
			else if (!_hasPulled){
                //gameObject.collider.enabled = true;
				_hasPulled = true;
				BeforeLandingParticle.SetActive(false);
				AfterLandingParticle.SetActive(true);
				StartCoroutine(PullEnemies());
            }
        }
		if (_hasPulled) {
			_damageIntervalTimer += Time.deltaTime;
			if (_damageIntervalTimer >= _damageInterval) {
				_damageIntervalTimer = 0f;
				DoAOEDamage();
			}
		}

        //UpdateColor();
        if (_expiresTimer < Time.time) {
            Destroy(gameObject);
        }
    }

    private void UpdateReady(bool ready)
    {
        if (_triggerer.PlayerSlot == Hero.Player.One)
            _powerSource.Player1Triggered = ready;
        else if (_triggerer.PlayerSlot == Hero.Player.Two)
            _powerSource.Player2Triggered = ready;
    }

    private void MoveTrigger()
    {
        float height = 2f;
        float pct = (Time.time - _startTime) / _timeToDestination;
        float heightLerp = Mathf.Lerp(0f, height, Mathf.Sin((Mathf.PI) * pct));
        gameObject.transform.position = Vector3.Lerp(_originPosition, _targetPosition, pct);
        gameObject.transform.position += new Vector3(0f, heightLerp, 0f);

        if (pct >= 0.95f)
        {
            _doneFlying = true;
        }
    }

    public void ActivateTrigger(Vector3 target, Hero triggerer, SpiritImmortal powerSource)
    {
        _isActivated = true;
        _originPosition = transform.position;
        _targetPosition = target;
        _triggerer = triggerer;
        _powerSource = powerSource;
        _startTime = Time.time;
        _expiresTimer = _startTime + _expiresAfter;
        TriggerColorBase = triggerer.GetComponentInChildren<Projector>().material.GetColor("_Color");
        TriggerColorCurrent = TriggerColorBase;

        //Tween in landing
        Vector3 targetScale = gameObject.transform.localScale;
        gameObject.transform.localScale *= 0.5f;
        TweenParms tweenParms = new TweenParms().Prop(
            "localScale", targetScale).Ease(
            EaseType.EaseInCirc).Delay(0f);
        HOTween.To(gameObject.transform, _timeToDestination, tweenParms);
    }

	IEnumerator PullEnemies() {
		//Find enemies to do effect on
		GameObject[] enemiesObjects = GameObject.FindGameObjectsWithTag("Enemy");
		
		enemies = new CharacterController[enemiesObjects.Length];
		int i = 0;
		foreach (GameObject enemy in enemiesObjects)
		{
			enemies[i] = enemy.GetComponent<CharacterController>();
			i++;
		}

		float pullRadiusSqr = _pullRadius * _pullRadius;
		
		//Effect
		/*
		pullSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		var audioSrc = pullSphere.AddComponent<AudioSource>();
		audioSrc.clip = _syncSound;
		audioSrc.rolloffMode = AudioRolloffMode.Linear;
		audioSrc.dopplerLevel = 0;
		audioSrc.Play();

		pullSphere.transform.position = transform.position;
		
		pullSphere.transform.localScale = Vector3.one * (_pullRadius * 2f);
		pullSphere.collider.enabled = false;
		pullSphere.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		pullSphere.renderer.material.SetColor("_Color", new Color(0f, 1f, 1f, 0.5f));
		*/
		yield return new WaitForFixedUpdate();
		
		//Try to pull enemies over n ticks
		int ticks = 120;
		List<BaseUnit> slowedUnits = new List<BaseUnit>();
		for (int j = 0; j < ticks; j++) {
			//pullSphere.transform.localScale = Vector3.Lerp(Vector3.one * (_pullRadius * 2f), Vector3.one, j*6 / (float)ticks);
			foreach (CharacterController enemy in enemies) {
				if (enemy == null)
					continue;
				
				if (j == 0) {
					var enemyUnit = enemy.gameObject.GetComponent<BaseUnit>();
					if (enemyUnit != null && !slowedUnits.Contains(enemyUnit)) {
						slowedUnits.Add(enemyUnit);
					}
				}
				var offset = transform.position - enemy.transform.position;
				var sqrMagnitude = offset.sqrMagnitude;
				if (sqrMagnitude > pullRadiusSqr)
				{
					continue;
				}
				if(sqrMagnitude > 1f) {
					offset = offset/12f;
					enemy.Move(offset);
				}
			}
			yield return new WaitForFixedUpdate();
		}
		//GameObject.Destroy(pullSphere);
		
		//Slow enemies
		foreach (var enemy in slowedUnits) {
			if (enemy != null)
				enemy.SetMovementSpeedBuff(-_syncSlowAmount);
		}
		
		//Give speed back
		yield return new WaitForSeconds(_expiresTimer - 2f);
		foreach (var enemy in slowedUnits) {
			if (enemy != null)
				enemy.SetMovementSpeedBuff(_syncSlowAmount);
		}
		yield return null;
	}

	private void DoAOEDamage() {
		Collider[] hits = Physics.OverlapSphere(transform.position, _damageRadius, 1 << 8);
		foreach (var other in hits) {
			if (other.tag == "Enemy") {
				other.gameObject.GetComponent<BaseUnit>().TakeDamage(_damagePerInterval, gameObject);
			}
		}
	}

    private void UpdateColor()
    {
        gameObject.renderer.material.SetColor("_Color", TriggerColorCurrent);
    }
}
