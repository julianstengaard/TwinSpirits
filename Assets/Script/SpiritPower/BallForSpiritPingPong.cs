using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BallForSpiritPingPong : MonoBehaviour {
    public GameObject ParticleEffectPrefab;
    private Hero _receiver;
    private Hero _origin;
    private Vector3 _currentOriginPosition;
    private Vector3 _currentTargetPosition;
    
    private float _ballRadius = 0.2f;
    private float _syncBallRadius = 0.3f;
    private List<BaseUnit> _enemiesHit = new List<BaseUnit>();
    private List<BaseUnit> _enemiesHitPrevious = new List<BaseUnit>();

    private float _speedCurrent;
    private float _speedInit = 5f;
    private float _accelerationPerBounce = 1.1f; //pct
    private float _catchRadiusSqr = 0.6f;
    private float _syncSuctionThresholdSqr = 9f;
    private float _syncSuctionAmount = 2f;
    private float _ballLifeTime = 10f;
    private float _ballLifeTimeCounter = 0f;
    private float _syncBallLifeTime = 10f;
    private float _syncBallLifeTimeCounter = 0f;

    private int _currentBounces;
    private int _maxBounces = 10;
    private int _maxBouncesSync = 50;

    private float _damagePerHitBase = 15f;
    private float _damagePerHitCurrent;
    private float _damageIncreasePerBounce = 1.1f; //pct
    private float _damagePerHitBaseSync = 30f;
    private float _damagePerHitCurrentSync;
    private float _damageIncreasePerBounceSync = 1.1f; //pct

    private bool _sync;
    private bool _ballActive = false;

    public void Activate(Hero source, Hero target, bool sync) {
        _origin = source;
        _receiver = target;
        _sync = sync;
        _ballActive = true;

        _ballLifeTimeCounter = 0f;
        _syncBallLifeTimeCounter = 0f;

        _currentBounces = 0;
        _damagePerHitCurrent = _damagePerHitBase;
        _damagePerHitCurrentSync = _damagePerHitBaseSync;
        _currentOriginPosition = _origin.transform.position + Vector3.up;
        _currentTargetPosition = _receiver.transform.position + Vector3.up;
        _speedCurrent = _speedInit;
    }

    private void Update() {
        if (_sync && _ballActive) {
			if (_syncBallLifeTime < _syncBallLifeTimeCounter) {
				_ballActive = false;
                Destroy(gameObject);
			} else {
				UpdateBall(_maxBouncesSync, true);
				_syncBallLifeTimeCounter += Time.deltaTime;
			}
		} else if (_ballActive) {
            if (_ballLifeTime < _ballLifeTimeCounter) {
                _ballActive = false;
                Destroy(gameObject);
            } else {
                UpdateBall(_maxBounces, false);
                _ballLifeTimeCounter += Time.deltaTime;
            }
        }
    }

    private void BounceBall() {
        //Switch origin and reciever
        Hero tempHero = _receiver;
        _receiver = _origin;
        _origin = tempHero;

        //Find the new positions
        _currentOriginPosition = gameObject.transform.position;
        _currentTargetPosition = _receiver.transform.position + Vector3.up;
    }

    private void UpdateBall(int maxBounces, bool sync) {
        bool bounced = false;
        Vector3 previousPosition = gameObject.transform.position;

        if (sync && !(_currentBounces >= _maxBouncesSync)) {
            //Is the ball close enough to the target for turning the ball?
            if (_syncSuctionThresholdSqr > (previousPosition - (_receiver.transform.position + Vector3.up)).sqrMagnitude) {
                //Turn the target from the the origin to make its path overlap the player
                Vector3 targetDirection = _currentTargetPosition - _currentOriginPosition;
                Vector3 wantedTargetDirection = ((_receiver.transform.position + Vector3.up) - _currentOriginPosition).normalized * targetDirection.magnitude;
                _currentTargetPosition = _currentOriginPosition + Vector3.RotateTowards(targetDirection, wantedTargetDirection, _syncSuctionAmount * Time.deltaTime, 0.0F);

                Vector3 wantedOriginDirection = (previousPosition - _currentTargetPosition).normalized * targetDirection.magnitude;
                _currentOriginPosition = _currentTargetPosition + Vector3.RotateTowards(-targetDirection, wantedOriginDirection, _syncSuctionAmount * Time.deltaTime, 0.0F);
            }
        }
        gameObject.transform.position = GetBallMovement(_currentOriginPosition, gameObject.transform.position, _currentTargetPosition, sync);
        CheckForBallCollision(previousPosition, gameObject.transform.position, sync);

        //No more bounce for you
        if ((sync && _currentBounces >= _maxBouncesSync) || (!sync && _currentBounces >= _maxBounces)) {
            //gameObject.renderer.material.SetColor("_Color", _ballColorNoBounce);
            return;
        }

        if ((GetVectorWithYSet(gameObject.transform.position, 0f) - (GetVectorWithYSet(_receiver.transform.position, 0f))).sqrMagnitude < _catchRadiusSqr) {
            BounceBall();
            bounced = true;
        }

        if (bounced) {
            _ballLifeTimeCounter = 0f;
            _syncBallLifeTimeCounter = 0f;
            _currentBounces++;
            _speedCurrent *= _accelerationPerBounce;
            _damagePerHitCurrent *= _damageIncreasePerBounce;
            _damagePerHitCurrentSync *= _damageIncreasePerBounceSync;
        }
    }

    public Vector3 GetBallMovement(Vector3 origin, Vector3 current, Vector3 target, bool sync) {
        float dPath = Vector3.Distance(origin, target);
        float dCurrent = Vector3.Distance(origin, current);
        float pct = dCurrent / dPath;
        Vector3 direction = (target - origin).normalized;
        Vector3 baseMoveSpeed = direction * _speedCurrent * Time.deltaTime;
        Vector3 move = Vector3.Lerp(baseMoveSpeed * 2f, baseMoveSpeed, pct);

        //Rotate to look towards direction
        gameObject.transform.rotation = Quaternion.LookRotation(direction);
        return current + move;
    }

    private void CheckForBallCollision(Vector3 from, Vector3 to, bool sync) {
        //Copy the previous list
        _enemiesHitPrevious = new List<BaseUnit>(_enemiesHit);
        //then clear it
        _enemiesHit.Clear();
        RaycastHit[] hits = Physics.CapsuleCastAll(from + Vector3.down, from + Vector3.up, _ballRadius, to - from, (to - from).magnitude, 1 << 8);
        foreach (RaycastHit hit in hits) {
            var enemy = hit.collider.gameObject.GetComponent<BaseUnit>();
            if (!_enemiesHitPrevious.Contains(enemy)) {
                if (sync) {
                    enemy.TakeDamage(_damagePerHitCurrentSync, gameObject);
                } else {
                    enemy.TakeDamage(_damagePerHitCurrent, gameObject);
                }
                StartCoroutine(CreateBallDamageParticle(enemy.gameObject));
                _enemiesHit.Add(enemy);
            }
        }
    }

    private IEnumerator CreateBallDamageParticle(GameObject target) {
        GameObject _particleEffect = (GameObject)Instantiate(ParticleEffectPrefab, target.transform.position, Quaternion.identity);
        GameObject.Destroy(_particleEffect, 1f);
        yield return null;
    }

    private Vector3 GetVectorWithYSet(Vector3 vector, float y) {
        return new Vector3(vector.x, y, vector.z);
    }
}
