using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class LightningForSpiritLightning : MonoBehaviour {
	private bool _isActivated;
    private Hero _sourceHero;
    private Hero _otherHero;

	private float _timeToDestination;
	private float _startTime;
    private float _lightningTimer;
    private float _lightningDamageInterval;
    private float _lightningDamageIntervalTimer;
    private Vector3 _center;
    private Camera _mainCamera;
    private float _lightningSphereColliderRadius;
    private float _damagePerLightningInterval;
    private GameObject _particleEffectPrefab;

    // Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
    private void Update() {
        //Move towards target
        if (_isActivated) {
            if (Time.time < _lightningTimer) {
                Run();
            } else {
                Destroy(gameObject);
            }
        }
    }

    private void Run() {
        _center = (_sourceHero.transform.position + _otherHero.transform.position) * 0.5f;
        _lightningDamageIntervalTimer += Time.deltaTime;
	    transform.position = _center + Vector3.up * transform.localScale.y * 0.45f;
		Vector3 rotationTarget = _mainCamera.transform.position - new Vector3(1f, _mainCamera.transform.position.y, 1f) + Vector3.up * transform.position.y;
		transform.rotation = Quaternion.LookRotation(transform.position - rotationTarget);
		if (_lightningDamageIntervalTimer >= _lightningDamageInterval) {
			_lightningDamageIntervalTimer = 0f;
			DoLightningDamage(_center);
		}
    }

    private void DoLightningDamage(Vector3 position) {
        Collider[] hits = Physics.OverlapSphere(position, _lightningSphereColliderRadius, 1 << 8);
        foreach (var other in hits) {
            if (other.tag == "Enemy") {
                other.gameObject.GetComponent<BaseUnit>().TakeDamage(_damagePerLightningInterval, gameObject);
                CreateLightningDamageParticle(other.gameObject);
            }
        }
    }

    private void CreateLightningDamageParticle(GameObject target) {
        GameObject _particleEffect = (GameObject)Instantiate(_particleEffectPrefab, target.transform.position, Quaternion.identity);
        GameObject.Destroy(_particleEffect, 1f);
    }

    public void Activate(Hero source, Hero other, float lightningDuration, float lightningDamageInterval, Camera mainCamera, float colliderRadius, float damagePerInterval, GameObject particlePrefab) {
		_isActivated = true;
	    _otherHero = other;
	    _sourceHero = source;

        _lightningTimer = lightningDuration + Time.time;
        _lightningDamageInterval = lightningDamageInterval;
        _lightningDamageIntervalTimer = lightningDamageInterval;

        _mainCamera = mainCamera;
        _lightningSphereColliderRadius = colliderRadius;
        _damagePerLightningInterval = damagePerInterval;
        _particleEffectPrefab = particlePrefab;
    }

}
