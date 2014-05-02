using Holoville.HOTween;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpiritImmortal : SpiritPower {
    private AudioClip _syncSound;

    private GameObject _shield;
	private GameObject _shieldPrefab;
    private bool _shieldActive;
	private float _shieldTimer;
	private float _shieldDuration = 5f;

	private GameObject pullSphere;

	private CharacterController[] enemies;
	private Vector3 center;

    private float _throwRange = 5f;
	private float _syncSlowAmount = 2f;
	private float _syncSlowDuration = 10f;

    private GameObject _triggerPrefab;
    public bool Player1Triggered = false;
    public bool Player2Triggered = false;

    private TriggerForSpiritImmortal triggerP1;
    private TriggerForSpiritImmortal triggerP2;
    private bool readyToPull = false;

	private Hero _othHero;
	
	void Start() {
		costActivate 		=  20f;
		costPerSecond 		=  0f;
		costActivateSync 	= 30f;
        _shieldPrefab = (GameObject) Resources.Load("SpiritShield");
        _triggerPrefab = (GameObject) Resources.Load("SpiritImmortalTrigger");
        _syncSound = (AudioClip)Resources.Load("Audio/Whirl pool");
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Activating" + this.GetType());
		_shieldTimer = 0f;
		_shieldActive = true;

		if (_shield != null) {
			GameObject.Destroy(_shield);
		}
        _othHero = otherHero;

	    _shield = (GameObject) Instantiate(_shieldPrefab, otherHero.transform.position, otherHero.transform.rotation);
        _shield.GetComponent<TimedShieldBlink>().Activate(_shieldDuration);
        _shield.transform.rotation = Quaternion.LookRotation(otherHero.transform.TransformDirection(Vector3.right), Vector3.up);
        //Tween in the shield
        _shield.transform.localScale *= 0f;
        TweenParms tweenParms = new TweenParms().Prop(
            "localScale", new Vector3(1f, 1f, 1f)).Ease(
            EaseType.EaseOutBounce).Delay(0f);
        HOTween.To(_shield.transform, 0.5f, tweenParms);

	    otherHero.SpiritShieldActive = true;
		return null;
	}

    public bool GetShieldStatus()
    {
        return _shieldActive;
    }

    public static GameObject CreateShieldMesh(Color color, Transform trans) {
		var shieldMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		shieldMesh.transform.position = trans.position + Vector3.up;
		shieldMesh.transform.localScale *= 2f;
		shieldMesh.collider.enabled = false;
		shieldMesh.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		shieldMesh.renderer.material.SetColor("_Color", color);

		return shieldMesh;
	}

	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	public void UpdateShield(Hero otherHero) {
	    if (_shield == null) {
	        _shieldActive = false;
            return;
	    }
		_shield.transform.position = otherHero.transform.position + otherHero.transform.TransformDirection(Vector3.forward);
		_shield.transform.rotation = Quaternion.LookRotation(otherHero.transform.TransformDirection(Vector3.right), Vector3.up);
	}
	 
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero, bool onDestroy)
	{
		if (onDestroy && _shield != null) {
			GameObject.Destroy(_shield);
		    _shieldActive = false;
		}

		//Debug.Log("Deactivating" + this.GetType());
		return null;
	}

	/* END REGULAR POWER */
	
	/* BEGIN SYNC POWER */
	public override bool OnPotentialSync (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Potential" + this.GetType() + " sync power!");
		potentialSyncTime = Time.time;
		
		//If other Hero has pressed already
		if (Mathf.Abs(potentialSyncTime - otherHero.currentSpiritPower.potentialSyncTime) < timeWindowForSync)
		{
			syncActive = true;
			OnActivateSync(sourceHero, otherHero);
			return true;
		}
		else
		{
			return false;
		}
		
	}
	public override IEnumerator OnActivateSync (Hero sourceHero, Hero otherHero, bool secondSync = false)
	{
		//Debug.Log("Activating" + this.GetType() + " SYNC POWER!");
	    if (triggerP1 != null) {
	        Destroy(triggerP1.gameObject);
	    }
        if (triggerP2 != null) {
            Destroy(triggerP2.gameObject);
        }

	    //This cant be used twice
		if (secondSync && otherHero.currentSpiritPower.GetType() == typeof(SpiritImmortal)) 
            return null;

		if (!secondSync) {
			//Stop other Heros effect
			otherHero.SwitchToSyncPower();
			
			//Pay for activation
			sourceHero.ChangeSpiritAmount(-costActivateSync);
			otherHero.ChangeSpiritAmount(-costActivateSync);
		}

	    readyToPull = true;
	    ThrowGravityTriggers(sourceHero, otherHero);
		return null;
	}

    void Update()
    {
		if (_shieldActive && _shieldTimer < _shieldDuration) {
			_shieldTimer += Time.deltaTime;
			UpdateShield(_othHero);
		} else if (_shieldActive) {
			if (_shield != null) {
				GameObject.Destroy(_shield);
			}
			_othHero.SpiritShieldActive = false;
			_shieldActive = false;
		}

        if (readyToPull && Player1Triggered && Player2Triggered)
        {
            readyToPull = false;
            Player1Triggered = false;
            Player2Triggered = false;

            //Find enemies to do effect on
            GameObject[] enemiesObjects = GameObject.FindGameObjectsWithTag("Enemy");

            enemies = new CharacterController[enemiesObjects.Length];
            int i = 0;
            foreach (GameObject enemy in enemiesObjects)
            {
                enemies[i] = enemy.GetComponent<CharacterController>();
                i++;
            }

            center = (triggerP1.transform.position + triggerP2.transform.position) * 0.5f;
            StartCoroutine(PullEnemies());
        }
    }

    private void ThrowGravityTriggers(Hero sourceHero, Hero otherHero)
    {
        GameObject triggerOtherGO = (GameObject) GameObject.Instantiate(_triggerPrefab, sourceHero.transform.position, Quaternion.identity);
        GameObject triggerSourceGO = (GameObject) GameObject.Instantiate(_triggerPrefab, otherHero.transform.position, Quaternion.identity);
        triggerP1 = triggerOtherGO.GetComponent<TriggerForSpiritImmortal>();
        triggerP2 = triggerSourceGO.GetComponent<TriggerForSpiritImmortal>();
        Vector3 throwDirectionOther = otherHero.transform.TransformDirection(Vector3.forward) * _throwRange;
        Vector3 throwDirectionSource = sourceHero.transform.TransformDirection(Vector3.forward) * _throwRange;
        triggerP1.ActivateTrigger(sourceHero.transform.position + throwDirectionSource, otherHero, this);
        triggerP2.ActivateTrigger(otherHero.transform.position + throwDirectionOther, sourceHero, this);
    }

    public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	IEnumerator PullEnemies() {
		float radius = (center - triggerP1.transform.position).magnitude * 2f;
		float radiusSqr = radius * radius;
		//Destroy triggers
        if (triggerP1 != null)
            GameObject.Destroy(triggerP1.gameObject);
        if (triggerP2 != null)
            GameObject.Destroy(triggerP2.gameObject);

		//Effect
		pullSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
	    var audioSrc = pullSphere.AddComponent<AudioSource>();
	    audioSrc.clip = _syncSound;
        audioSrc.rolloffMode = AudioRolloffMode.Linear;
	    audioSrc.dopplerLevel = 0;
	    audioSrc.Play();

		pullSphere.transform.position = center;
	    
        pullSphere.transform.localScale = Vector3.one * (radius * 2f);
		pullSphere.collider.enabled = false;
		pullSphere.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		pullSphere.renderer.material.SetColor("_Color", new Color(0f, 1f, 1f, 0.5f));

		yield return new WaitForFixedUpdate();

		//Try to pull enemies over n ticks
	    int ticks = 120;
		List<BaseUnit> slowedUnits = new List<BaseUnit>();
		for (int i = 0; i < ticks; i++) {
            pullSphere.transform.localScale = Vector3.Lerp(Vector3.one * (radius * 2f), Vector3.one, i*6 / (float)ticks);
			foreach (CharacterController enemy in enemies) {
                if (i == 0) {
                    var enemyUnit = enemy.gameObject.GetComponent<BaseUnit>();
                    if (enemyUnit != null && !slowedUnits.Contains(enemyUnit)) {
                        slowedUnits.Add(enemyUnit);
                    }
                }
				var offset = center - enemy.transform.position;
				var sqrMagnitude = offset.sqrMagnitude;
                if (sqrMagnitude > radiusSqr)
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
		GameObject.Destroy(pullSphere);

		//Slow enemies
		foreach (var enemy in slowedUnits) {
			if (enemy != null)
                enemy.SetMovementSpeedBuff(-_syncSlowAmount);
		}

		//Give speed back
		yield return new WaitForSeconds(_syncSlowDuration);
		foreach (var enemy in slowedUnits) {
			if (enemy != null)
				enemy.SetMovementSpeedBuff(_syncSlowAmount);
		}
		yield return null;
	}

    public override IEnumerator OnDeactivateSync(Hero sourceHero, Hero otherHero, bool onDestroy = false)
	{
		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
        if (onDestroy) {
            if (triggerP1 != null) {
                Destroy(triggerP1.gameObject);
            }
            if (triggerP2 != null) {
                Destroy(triggerP2.gameObject);
            }
        }

        yield return null;
	}

    void OnDestroy() {
        if (triggerP1 != null) {
            Destroy(triggerP1.gameObject);
        }
        if (triggerP2 != null) {
            Destroy(triggerP2.gameObject);
        }
    }
    /* END SYNC POWER */
	
	public override float GetCostActivate ()
	{
		return costActivate;
	}
	public override float GetCostThisUpdate ()
	{
		return costPerSecond * Time.deltaTime;
	}
	public override float GetCostActivateSync ()
	{
		return costActivateSync;
	}
	
}
