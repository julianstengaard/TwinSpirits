using UnityEngine;
using System.Collections;

public class SpiritBungie : SpiritPower 
{
	private Color 		targetHeroColor;
	private BaseUnit[] 	enemiesToKill;
	private AudioClip 	_circleSound;

	private float 		currentBungieTime	= 0f;
	private float 		durationToBungie	= 0.5f;
	private float 		closestDistance		= 2f;
	private Vector3 	initialPosition;
	
	private float		syncTimer;
	private float		damageTimer;
	private bool		circleActive;
	private float		circleMinWidth		= 0.01f;
	private float		circleMaxWidth		= 0.4f;
	private float		syncDuration		= 5f;
	private float		intervalDuration	= 1f;
	private float 		damagePerInterval	= 50f;

	private CharacterController heroCC;

	private GameObject link;
	private GameObject[] circleLink;
	private GameObject _particleEffectPrefab;

	private Hero srcHero;
	private Hero othHero;

	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  0f;
		costActivateSync 	= 50f;
		_particleEffectPrefab = (GameObject) Resources.Load("SpiritCircleParticle", typeof(GameObject));
		_circleSound = (AudioClip) Resources.Load("Audio/Cirkel (ulti)", typeof(AudioClip));
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		CreateBungieLink(otherHero, sourceHero, "SpiritLinkChain");

		//Debug.Log("Activating" + this.GetType());

		heroCC = otherHero.GetComponent<CharacterController>();

		currentBungieTime = 0;
		initialPosition = otherHero.transform.position;
		Vector3 directionToBungie = GetBungieMovement(currentBungieTime, initialPosition, sourceHero.transform.position - initialPosition, durationToBungie);
		heroCC.Move(directionToBungie - otherHero.transform.position);

		return null;
	}
	public override IEnumerator OnUpdate (Hero sourceHero, Hero otherHero)
	{
		if (link == null) {
			return null;
		}

		if ((otherHero.transform.position - sourceHero.transform.position).magnitude > closestDistance)
		{
			UpdateBungieLink(otherHero, sourceHero);
			currentBungieTime = Mathf.Clamp(currentBungieTime + Time.deltaTime, 0, durationToBungie);
			Vector3 directionToBungie = GetBungieMovement(currentBungieTime, initialPosition, sourceHero.transform.position - initialPosition, durationToBungie);
			heroCC.Move(directionToBungie - otherHero.transform.position);
		}
		else
		{
			link.renderer.enabled = false;
			currentBungieTime = 0;
			initialPosition = otherHero.transform.position;
			OnDeactivate(sourceHero, otherHero);
		}

		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero, bool onDestroy)
	{
		//Debug.Log("Deactivating" + this.GetType());
		DestroyBungieLink (0f);
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
		//Only allow one of these per person
		if (circleActive)
			return null;

		if (!secondSync) {
			//Stop other Heros effect
			otherHero.SwitchToSyncPower();

            //Pay for activation
			sourceHero.ChangeSpiritAmount(-costActivateSync);
			otherHero.ChangeSpiritAmount(-costActivateSync);
        }
		StartCoroutine(DeathRay(sourceHero, otherHero));

		return null;
	}

	void Update () {
		if (circleActive) {
			syncTimer += Time.deltaTime;
			damageTimer += Time.deltaTime;
			//Is the circle time over?
			if (syncTimer > syncDuration)
				circleActive = false;

			//Should this tick deal damage?
			if (damageTimer > intervalDuration) {
				UpdateCircleLink(srcHero, othHero, damageTimer/intervalDuration, true);
				damageTimer -= intervalDuration;
			} else {
				UpdateCircleLink(srcHero, othHero, damageTimer/intervalDuration, false);
			}
		}
	}
	

	IEnumerator DeathRay(Hero sourceHero, Hero otherHero)
	{
		//Find enemies
		var enemies = GameObject.FindGameObjectsWithTag("Enemy");

		enemiesToKill = new BaseUnit[enemies.Length];
		for (int i = 0; i < enemies.Length; i++) {
			enemiesToKill[i] = enemies[i].GetComponent<BaseUnit>();
		}

		syncTimer = 0f;
		circleActive = true;
		CreateCircleLink(sourceHero, otherHero, "SpiritLinkRay", 16);
		srcHero = sourceHero;
		othHero = otherHero;
		yield return new WaitForSeconds(syncDuration);
		DestroyCircleLink (0f);
	}

	public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

    public override IEnumerator OnDeactivateSync(Hero sourceHero, Hero otherHero, bool onDestroy = false)
	{
        if (circleActive) {
            DestroyCircleLink(0f);
            circleActive = false;
        } else {
            DestroyBungieLink(0f);
        }
		return null;
	}
	/* END SYNC POWER */
	

	private Vector3 GetBungieMovement (float time, Vector3 startValue, Vector3 changeValue, float duration)
	{
		//float t = time / (duration/2f);
		return -changeValue/2 * (Mathf.Cos(Mathf.PI*time/duration) - 1) + startValue;
	}

	private void CreateBungieLink (Hero sourceHero, Hero otherHero, string prefab)
	{
		link = (GameObject) Instantiate(Resources.Load(prefab), Vector3.zero, Quaternion.identity); 
		UpdateBungieLink (sourceHero, otherHero);
	}

	private void CreateCircleLink (Hero sourceHero, Hero otherHero, string prefab, int divisions)
	{
		circleLink = new GameObject[divisions];
		for (int i = 0; i < divisions; i++) {
			circleLink[i] = (GameObject) Instantiate(Resources.Load(prefab), Vector3.up, Quaternion.identity); 
		}
		var audio = circleLink[0].AddComponent<AudioSource>();
		audio.dopplerLevel = 0f;
		audio.rolloffMode = AudioRolloffMode.Linear;
		audio.clip = _circleSound;
		audio.loop = true;
		audio.Play();
		UpdateCircleLink (sourceHero, otherHero, 0f, true);
	}

	private void UpdateCircleLink (Hero sourceHero, Hero otherHero, float pctToNextDamage, bool damageTick) {
	    Color rayColor = GetColor(pctToNextDamage);
		Vector3 center = (sourceHero.transform.position + otherHero.transform.position) * 0.5f + Vector3.up;
		float radius = (sourceHero.transform.position - otherHero.transform.position).magnitude * 0.5f;
		float width = Mathf.Clamp(1f/radius, circleMinWidth, circleMaxWidth);
		
		for (int i = 0; i < circleLink.Length; i++) {
			float rotationY = (360f/circleLink.Length) * i;
			Quaternion rotation = Quaternion.Euler(0f, rotationY, 0f);
			Vector3 linkRelativePosition = rotation * new Vector3(1f,0f,0f);
			circleLink[i].transform.position = center + (linkRelativePosition * radius);

			//Calculate scale/rotation
			float spanningScale = (radius * 0.2f * Mathf.PI)/circleLink.Length;
			rotation *= Quaternion.Euler(0, 90, 0);

			//Scale and rotate it
			var s = circleLink[i].transform.localScale;
			s.x = spanningScale;
			s.z = width;
			circleLink[i].transform.localScale = s;
			circleLink[i].transform.localRotation = rotation;
            circleLink[i].renderer.material.SetColor("_Color", rayColor);
		}

		if (damageTick) {
			DealDamageWithCircleLink(center, radius, width);
		}
	}

	private void DealDamageWithCircleLink(Vector3 center, float radius, float width) {
        //Find enemies
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemiesToKill = new BaseUnit[enemies.Length];
        for (int i = 0; i < enemies.Length; i++) {
            enemiesToKill[i] = enemies[i].GetComponent<BaseUnit>();
        }

	    float hitRangeSqr = (width*2f)*(width*2f);
		foreach (var enemy in enemiesToKill) {
			if (enemy != null && !enemy.dead) {
				//Is enemy in the DANGER ZONE?!
				Vector3 circleOrigin = GetVectorWithYSet(center, 0f);
                Vector3 hitDirection = GetVectorWithYSet(enemy.transform.position, 0f) - circleOrigin;
                Vector3 hitPointOnCircle = GetVectorWithYSet(circleOrigin + hitDirection.normalized * radius, enemy.transform.position.y);

                float enemyDistanceSqr = (hitPointOnCircle - enemy.collider.ClosestPointOnBounds(hitPointOnCircle)).sqrMagnitude;
                if (enemyDistanceSqr < hitRangeSqr) {
					enemy.TakeDamage(damagePerInterval, gameObject);
					StartCoroutine(CreateCircleDamageParticle(enemy.gameObject));
				}
			}
		}
	}

	private IEnumerator CreateCircleDamageParticle(GameObject target) {
		GameObject _particleEffect = (GameObject) Instantiate(_particleEffectPrefab, target.transform.position, Quaternion.identity);
		GameObject.Destroy(_particleEffect, 1f);
		yield return null;
	}

	private void DestroyCircleLink (float time)
	{
		for (int i = 0; i < circleLink.Length; i++) {
			if (circleLink[i] != null)
				GameObject.Destroy(circleLink[i], time);
		}
		circleActive = false;
	}

	private void UpdateBungieLink (Hero sourceHero, Hero otherHero)
	{
		link.renderer.enabled = true;

		Vector3 midPoint = (sourceHero.transform.position + otherHero.transform.position) * 0.5f + Vector3.up;
		link.transform.position = midPoint;

		//Calculate scale/rotation
		float spanningScale = Vector3.Distance(sourceHero.transform.position, otherHero.transform.position)/10f;
		Quaternion rotation = Quaternion.LookRotation(sourceHero.transform.position - otherHero.transform.position, Vector3.up);
		rotation *= Quaternion.Euler(0, 90, 0);

		//Scale and rotate it
		var s = link.transform.localScale;
		s.x = spanningScale;
		link.transform.localScale = s;
		link.transform.localRotation = rotation;
	}

	private void DestroyBungieLink (float time)
	{
	    if (link != null) {
	        GameObject.Destroy(link, time);
	    }
	}

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

    private Color GetColor(float pct) {
        if (pct < 0.5f)
            return new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.01f, pct * 2));
        else
            return new Color(1f, 1f, 1f, Mathf.Lerp(0.01f, 1f, (pct - 0.8f) * 5f));
    }

    private Vector3 GetVectorWithYSet(Vector3 vector, float y) {
        return new Vector3(vector.x, y, vector.z);
    }
	
}
