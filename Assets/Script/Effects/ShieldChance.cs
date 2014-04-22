using UnityEngine;
using System.Collections;

public class ShieldChance : Effect 
{
	private float _chance;
	private Hero hero;
	private bool isShieldUp = false;
	private GameObject shield;
	public GameObject shieldPrefab;

	public ShieldChance(float chance) {
		_chance = chance;
	}

	public override void DoEffect (BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage) {
		if(isShieldUp || Random.Range (0f, 1f) >= _chance)
			return;

		hero = source.GetComponent<Hero>();

		if(hero == null) {
			Debug.Log ("Unable to find hero");
			return;
		}
		
		isShieldUp = true;
		hero.immortal = true;
		shield = shieldPrefab != null ? (GameObject)GameObject.Instantiate(shieldPrefab, hero.transform.position, Quaternion.identity) : SpiritImmortal.CreateShieldMesh(new Color(0f, 0.6f, 0f, 0.5f), hero.transform);
		shield.transform.parent = hero.transform;
		var timer = shield.AddComponent<SimpleSelfTimer>();
		timer.trigger += RemoveEffect;
		timer.timeout = 5f;
	}	
	
	public override IEnumerator DoEffectCoroutine (BaseUnit target, GameObject source, Vector3 attackPosition) {
//		yield return new WaitForSeconds(5);
//		RemoveEffect();
		yield break;
	}

	private void RemoveEffect() {
		GameObject.Destroy(shield);
		hero.immortal = false;
		isShieldUp = false;
	}

	public override int GetPriority() {
		return 1;
	}

	public override bool IsSplashable () {
		return false;
	}
}
