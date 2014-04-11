using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class TriggerForSpiritImmortal : MonoBehaviour
{
    private Vector3 _originPosition;
    private Vector3 _targetPosition;
    private float _explosionRadius;
    private float _damageOnExplosion;
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

    private Hero _triggerer;
    private SpiritImmortal _powerSource;

    // Use this for initialization
    void Start()
    {
        gameObject.collider.enabled = false;
    }

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
    }

    // Update is called once per frame
    void Update() {
        //Move towards target
        if (_isActivated) {
            if (!_doneFlying) {
                MoveTrigger();
            }
            else {
                gameObject.collider.enabled = true;
            }
        }
        UpdateColor();
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

    private void UpdateColor()
    {
        gameObject.renderer.material.SetColor("_Color", TriggerColorCurrent);
    }
}
