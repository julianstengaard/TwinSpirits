using Holoville.HOTween;
using UnityEngine;
using System.Collections;

public class SpiritImmortal : SpiritPower 
{
    private GameObject _shield;
	private GameObject _shieldPrefab;
    private bool _shieldActive;

	private GameObject pullSphere;

	private CharacterController[] enemies;
	private Vector3 center;

    private float _throwRange = 5f;

    private GameObject _triggerPrefab;
    public bool Player1Triggered = false;
    public bool Player2Triggered = false;
    private bool readyForGravity = false;

    private TriggerForSpiritImmortal triggerP1;
    private TriggerForSpiritImmortal triggerP2;
    private bool readyToPull = false;
	
	void Start() {
		costActivate 		=  10f;
		costPerSecond 		=  10f;
		costActivateSync 	= 100f;
        _shieldPrefab = (GameObject) Resources.Load("SpiritShield");
        _triggerPrefab = (GameObject) Resources.Load("SpiritImmortalTrigger");
	}
	
	/* BEGIN REGULAR POWER */
	public override IEnumerator OnActivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Activating" + this.GetType());
	    _shield = (GameObject) Instantiate(_shieldPrefab, otherHero.transform.position, otherHero.transform.rotation);
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
        _shield.transform.position = otherHero.transform.position + otherHero.transform.TransformDirection(Vector3.forward);
        _shield.transform.rotation = Quaternion.LookRotation(otherHero.transform.TransformDirection(Vector3.right), Vector3.up);

		return null;
	}
	public override IEnumerator OnDeactivate (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType());
		GameObject.Destroy(_shield);
		otherHero.immortal = false;
        otherHero.SpiritShieldActive = false;

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
        //This cant be used twice
        if (secondSync) 
            return null;

	    if (!secondSync)
	    {
	        //Pay for activation
	        sourceHero.ChangeSpiritAmount(-costActivateSync);
	        otherHero.ChangeSpiritAmount(-costActivateSync);

	        //Stop other Heros effect
	        otherHero.SwitchToSyncPower();
	    }
	    readyToPull = true;
	    ThrowGravityTriggers(sourceHero, otherHero);
		return null;
	}

    private void Update()
    {
        if (readyToPull && Player1Triggered && Player2Triggered)
        {
            readyToPull = false;
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
        GameObject triggerOtherGO = (GameObject) GameObject.Instantiate(_triggerPrefab, otherHero.transform.position, Quaternion.identity);
        GameObject triggerSourceGO = (GameObject)GameObject.Instantiate(_triggerPrefab, sourceHero.transform.position, Quaternion.identity);
        triggerP1 = triggerOtherGO.GetComponent<TriggerForSpiritImmortal>();
        triggerP2 = triggerSourceGO.GetComponent<TriggerForSpiritImmortal>();
        Vector3 throwDirectionOther = otherHero.transform.TransformDirection(Vector3.forward) * _throwRange;
        Vector3 throwDirectionSource = sourceHero.transform.TransformDirection(Vector3.forward) * _throwRange;
        triggerP1.ActivateTrigger(otherHero.transform.position + throwDirectionOther, otherHero, this);
        triggerP2.ActivateTrigger(sourceHero.transform.position + throwDirectionSource, sourceHero, this);
    }

    public override IEnumerator OnUpdateSync (Hero sourceHero, Hero otherHero)
	{
		return null;
	}

	IEnumerator PullEnemies() {
        //Destroy triggers
        if (triggerP1 != null)
            GameObject.Destroy(triggerP1.gameObject);
        if (triggerP2 != null)
            GameObject.Destroy(triggerP2.gameObject);

		//Effect
		pullSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		pullSphere.transform.position = center;
	    float radius = (center - triggerP1.transform.position).magnitude;
        pullSphere.transform.localScale = Vector3.one * (radius * 2f);
		pullSphere.collider.enabled = false;
		pullSphere.renderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		pullSphere.renderer.material.SetColor("_Color", new Color(0f, 1f, 1f, 0.5f));

		yield return new WaitForFixedUpdate();

		//Try to pull enemies over n ticks
		int ticks = 10;
		for (int i = 0; i < ticks; i++) {
            pullSphere.transform.localScale = Vector3.Lerp(Vector3.one * (radius * 2f), Vector3.one, i / (float)ticks);
			foreach (CharacterController enemy in enemies) {
				var offset = center - enemy.transform.position;
				var sqrMagnitude = offset.sqrMagnitude;
                if (sqrMagnitude > radius)
                {
					continue;
				}
				else if(sqrMagnitude > 1f) {
					offset = offset.normalized;
					enemy.Move(offset);
				}
			}
			yield return new WaitForFixedUpdate();
		}
		GameObject.Destroy(pullSphere);
		yield return null;
	}
	
	public override IEnumerator OnDeactivateSync (Hero sourceHero, Hero otherHero)
	{
		//Debug.Log("Deactivating" + this.GetType() + " SYNC POWER!");
		yield return null;
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
