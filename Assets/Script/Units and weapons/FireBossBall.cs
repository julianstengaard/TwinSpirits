using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class FireBossBall : MonoBehaviour {
    public Color BallColor;
    public AudioClip ExplodeSound;

    private bool _arches;
    private float _height;
    private Vector3 _originPosition;
    private Vector3 _targetPosition;
    private float _hitRadius;
    private float _damageOnHit;
    private bool _isActivated;
    private bool _doneFlying;
    private bool _exploded;
    private float _timeToDestination;
    private float _startTime;

    private List<BaseUnit> _playersHit = new List<BaseUnit>();

    // Use this for initialization
    void Start() {
        BallColor = new Color(1f, 1f, 0f, 0.7f);
    }

    public void ActivateBall(Vector3 target, float height, float hitRadius, float damageOnHit, bool arches, float time) {
        _isActivated = true;
        _originPosition = transform.position;
        _targetPosition = target;
        _height = height;
        _hitRadius = hitRadius;
        _damageOnHit = damageOnHit;
        _arches = arches;
        _timeToDestination = time;
        _startTime = Time.time;
    }

    // Update is called once per frame
    void Update() {
        //Move towards target
        if (_isActivated) {
            if (!_doneFlying) {
                if (_arches) MoveBall();
                else if (!_arches) MoveBall(true);
            } else if (_arches && !_exploded) {
                _exploded = true;
                StartCoroutine(Explode());
            } else if (!_arches) {
                GameObject.Destroy(gameObject);
            }
            UpdateColor();
        }
    }

    private IEnumerator Explode() {
        gameObject.audio.clip = ExplodeSound;
        gameObject.audio.Play();

        //Tween Color
        BallColor = new Color(1f, 1f, 0f, 1f);
        Color finalColor = new Color(1f, 1f, 0f, 0f);
        TweenParms tweenColorParms = new TweenParms().Prop(
            "BallColor", finalColor).Ease(
            EaseType.EaseInCirc).Delay(0f);
        HOTween.To(this, 0.3f, tweenColorParms);

        //Tween in explosion
        gameObject.transform.localScale *= 0f;
        Vector3 finalSize = Vector3.one * _hitRadius * 2f;
        TweenParms tweenParms = new TweenParms().Prop(
            "localScale", finalSize).Ease(
            EaseType.EaseInCirc).Delay(0f);
        HOTween.To(gameObject.transform, 0.3f, tweenParms);

        yield return new WaitForSeconds(0.2f);
        Collider[] hits = Physics.OverlapSphere(gameObject.transform.position, _hitRadius, 1 << 10);
        foreach (var other in hits) {
            if (other.tag == "Player") {
                other.gameObject.GetComponent<BaseUnit>().TakeDamage(_damageOnHit, gameObject);
            }
        }
        yield return new WaitForSeconds(1f);
        GameObject.Destroy(gameObject);
    }

    private void MoveBall(bool dealDamageOnCollision = false) {
        float pct = (Time.time - _startTime) / _timeToDestination;
        float heightLerp = _arches ? Mathf.Lerp(0f, _height, Mathf.Sin((Mathf.PI) * pct)) : _height;
        Vector3 from = gameObject.transform.position;
        Vector3 to = Vector3.Lerp(_originPosition, _targetPosition, pct) + new Vector3(0f, heightLerp, 0f);
       
        if (dealDamageOnCollision) {
            CheckForBallCollision(from, to);
        }
        gameObject.transform.position = to;

        if (pct >= 0.95f)
            _doneFlying = true;
    }
    
    private void CheckForBallCollision(Vector3 from, Vector3 to) {
        RaycastHit[] hits = Physics.SphereCastAll(from, _hitRadius, to - from, (to - from).magnitude, 1 << 10);
        foreach (RaycastHit hit in hits) {
            var player = hit.collider.gameObject.GetComponent<BaseUnit>();
            if (!_playersHit.Contains(player)) {
                gameObject.audio.clip = ExplodeSound;
                gameObject.audio.Play();
                player.TakeDamage(_damageOnHit, gameObject);
                _playersHit.Add(player);
            }
        }
    }

    private void UpdateColor() {
        gameObject.renderer.material.SetColor("_Color", BallColor);
    }
}
