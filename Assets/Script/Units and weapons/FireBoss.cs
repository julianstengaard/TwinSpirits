using UnityEngine;
using System.Collections;

public class FireBoss : MonoBehaviour {
    public AudioClip InitiateSound;

    public GameObject FireballStraightPrefab;
    public GameObject FireballArchPrefab;

    public float TimeBetweenVolleys = 3f;
    public int ArchBallsInVolley = 10;
    public int StraightBallLinesInVolley = 10;
    public int StraightBallsPerLine = 3;

    public float TimeBetweenArchBalls = 0.1f;
    public float TimeBetweenStraightBallLines = 0.1f;
    public float TimeBetweenStraightBalls = 0.1f;

    public float StraightBallDamage = 1f;
    public float StraightBallTravelTime = 2f;
    public float StraightBallDistance = 20f;
    private float _straightHitRadius;

    public float ArchBallDamage = 1f;
    public float ArchBallTravelTime = 2f;
    public float ArchBallHeight = 6f;
    public float ArchBallExplosionRadius = 2f;
    public float ArchBallDistanceMin = 2f;
    public float ArchBallDistanceMax = 20f;
    public bool  ArchBallFixedRadiusPerVolley = true;

    private bool _isActive;

    private float _volleyTimer;
    private bool _isAttacking;

    private int _lastAttack;

	// Use this for initialization
	void Start () {
        Activate();
	}

    // Update is called once per frame
	void Update () {
	    if (_isActive) {
	        if (!_isAttacking && _volleyTimer <= 0f) {
	            Attack();
            } else if (!_isAttacking && _volleyTimer > 0f) {
                _volleyTimer -= Time.deltaTime;
            } else {
                
            }
	    }
	}

    public void Activate() {
        gameObject.audio.clip = InitiateSound;
        gameObject.audio.Play();

        _isActive = true;
        _volleyTimer = TimeBetweenVolleys / 2f;
        _isAttacking = false;
        _lastAttack = 0;
        _straightHitRadius = (FireballStraightPrefab.collider as SphereCollider).radius * FireballStraightPrefab.transform.localScale.x;
    }

    private void Attack() {
        _volleyTimer = TimeBetweenVolleys;
        if (_lastAttack == 1) { 
            StartCoroutine(CreateArchVolley());
            _lastAttack = 0;
        } else if (_lastAttack == 0) {
            StartCoroutine(CreateStraightVolley());
            _lastAttack = 1;
        }
    }

    private IEnumerator CreateArchVolley() {
        _isAttacking = true;
        float initAngle = Random.Range(0f, 360f);
        float randomDistance = Random.Range(ArchBallDistanceMin, ArchBallDistanceMax);

        for (int i = 0; i < ArchBallsInVolley; i++) {
            if (!ArchBallFixedRadiusPerVolley) {
                randomDistance = Random.Range(ArchBallDistanceMin, ArchBallDistanceMax);
            }
            CreateArchBall(initAngle + ((i / (float)ArchBallsInVolley) * 360f), randomDistance, ArchBallHeight);

            if (TimeBetweenArchBalls > 0f) {
                yield return new WaitForSeconds(TimeBetweenArchBalls);
            }
        }
        _isAttacking = false;
    }

    private IEnumerator CreateStraightVolley() {
        _isAttacking = true;
        float initAngle = Random.Range(0f, 360f);

        for (int i = 0; i < StraightBallLinesInVolley; i++) {
            StartCoroutine(CreateStraightVolleyLine(initAngle + ((i / (float)ArchBallsInVolley) * 360f)));
            if (TimeBetweenStraightBallLines > 0f) {
                yield return new WaitForSeconds(TimeBetweenStraightBallLines);
            }
        }
        //Make sure last ball is fired
        yield return new WaitForSeconds(TimeBetweenStraightBalls * (StraightBallsPerLine - 1) - TimeBetweenStraightBallLines);
        //print("stopped");
        _isAttacking = false;
    }

    private IEnumerator CreateStraightVolleyLine(float angle) {
        for (int i = 0; i < StraightBallsPerLine; i++) {
            CreateStraightBall(angle, StraightBallDistance);
            yield return new WaitForSeconds(TimeBetweenStraightBalls);
        }
    }

    private void CreateArchBall(float angle, float distance, float height) {
        GameObject ballGO = (GameObject) GameObject.Instantiate(FireballArchPrefab, transform.position, Quaternion.identity);
        FireBossBall ball = ballGO.GetComponent<FireBossBall>();
        Vector3 throwDirection = Quaternion.AngleAxis(angle, Vector3.up) * gameObject.transform.forward * distance;
        ball.ActivateBall(gameObject.transform.position + throwDirection, height, ArchBallExplosionRadius,
            ArchBallDamage, true, ArchBallTravelTime);
    }

    private void CreateStraightBall(float angle, float distance) {
        GameObject ballGO = (GameObject)GameObject.Instantiate(FireballStraightPrefab, transform.position + Vector3.up * 0f, Quaternion.identity);
        FireBossBall ball = ballGO.GetComponent<FireBossBall>();
        Vector3 throwDirection = Quaternion.AngleAxis(angle, Vector3.up) * gameObject.transform.forward * distance;
        ball.ActivateBall(gameObject.transform.position + throwDirection + Vector3.up * 0f, _straightHitRadius, _straightHitRadius,
            StraightBallDamage, false, StraightBallTravelTime);
    }
}
