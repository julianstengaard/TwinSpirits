using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Splash : Effect 
{
	private float 		splashRadius = 10f;
	private string[] 	immuneTags;

	public Splash(float splashRadius, string[] immuneTags) 
	{
		this.splashRadius 	= splashRadius;
		this.immuneTags		= immuneTags;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage)
	{
		//Find splash targets
		Collider[] hitColliders = Physics.OverlapSphere(target.transform.position, splashRadius, 1 << 8 );
		
		foreach (Collider splashTarget in hitColliders) 
		{
			if (CollisionTargetIsValid(splashTarget.gameObject, target, source, immuneTags))
			{
				// TODO: change to apporpiate weapon
				var weapon = source.GetComponentInChildren<Weapon>();
				Debug.Log (weapon.AttackEffects.Count());
				var splashableEffects = weapon.AttackEffects.Where(item => item.IsSplashable()).ToList();
				Debug.Log (splashableEffects.Count());
				splashTarget.gameObject.GetComponent<BaseUnit>().EvaluateAttacks(source, target.transform.position, splashableEffects, immuneTags);
			}
		}
	}
	
	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition)
	{
		// THIS IS VISUAL ONLY! :)
		GameObject splash = (GameObject) GameObject.Instantiate(Resources.Load("Splash"), target.transform.position, Quaternion.Euler(Vector3.up));
		splash.transform.localScale = new Vector3(splashRadius, splash.transform.localScale.y, splashRadius);
		
		yield return new WaitForSeconds(0.2f);
		UnityEngine.Object.Destroy(splash);

		yield return null;
	}

	private bool CollisionTargetIsValid(GameObject other, BaseUnit target, GameObject source, string[] immuneTags)
	{
		if (other.GetComponent<BaseUnit>() == null || other == source || other == target.gameObject)
		{
			return false;
		}

		foreach (string tag in immuneTags)
		{
			if (tag == other.tag)
			{
				return false;
			}
		}
		return true;
	}

//	private List<Effect> GetSplashableEffects(GameObject source)
//	{
//		List<Effect> splashableEffects = new List<Effect>();
//		foreach(Effect effect in source.GetComponent<BaseUnit>().attackEffects)
//		{
//			if (effect.IsSplashable())
//			{
//				splashableEffects.Add(effect);
//			}
//		}
//		return splashableEffects;
//	}


	public override int GetPriority() {
		return 10;
	}

	public override bool IsSplashable ()
	{
		return false;
	}
}
