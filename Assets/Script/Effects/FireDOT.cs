using UnityEngine;
using System.Collections;

public class FireDOT : MonoBehaviour {

	private BaseEnemy _target;
	private GameObject _particleEffectPrefab;
	private GameObject _particleEffect;
	private float timer = 0f;

	public void InitDOT(float duration, float damageInterval, float damagePerInterval) {
		_target = gameObject.GetComponent<BaseEnemy>();
		_particleEffectPrefab = (GameObject) Resources.Load("FireDOTParticle", typeof(GameObject));

		StartCoroutine(DoDOT(duration, damageInterval, damagePerInterval));
		_particleEffect = (GameObject) Instantiate(_particleEffectPrefab, transform.position, Quaternion.identity);
		_particleEffect.transform.parent = gameObject.transform;
	} 

	private IEnumerator DoDOT(float duration, float damageInterval, float damagePerInterval) {
		while (timer <= duration) {
			//Check if target is dead
			if (_target.gameObject == null || _target.dead) {
				break;
			}
			//Else wait and do damage
			yield return new WaitForSeconds(damageInterval);
			timer += damageInterval;
			_target.TakeDamage(damagePerInterval);
		}
		Destroy(_particleEffect);
		Destroy(this, 0.5f);
		yield return null;
	}
}
