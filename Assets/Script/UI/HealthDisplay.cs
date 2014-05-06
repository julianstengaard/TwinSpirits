using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public class HealthDisplay : MonoBehaviour
{
    public int PlayerNumber;
    public GameObject[] Hearts;

    public Material HeartFullMaterial;
    public Material HeartHalfMaterial;
    public Material HeartEmptyMaterial;

	private Vector3 _heartBaseScale;
	private float _healthPrevious;
	private float _tweenScale = 3f;

    private Hero _player;

	// Use this for initialization
	void Start () {
		_heartBaseScale = Hearts[0].transform.localScale;
        //Find player
        FindPlayer();
	}

	void FixedUpdate ()
	{
	    if (_player != null)
	        UpdateHearts();
	    else
	        FindPlayer();
	}

    private void FindPlayer() {
        _player = GameObject.Find("UI").GetComponent<SpiritMeterUI>().GetPlayer(PlayerNumber);
    }

    private void UpdateHearts() {
        float healthDisplayed = 0f;
        
        bool done = false;
        foreach (var heart in Hearts) {
            if (done) {
                heart.renderer.enabled = false;
                continue;
            } 
            heart.renderer.enabled = true;
            //Full heart
            if (_player.Health - healthDisplayed > 1f) {
                heart.renderer.material = HeartFullMaterial;
            }
            //Half heart
            else if (_player.Health - healthDisplayed > 0f) {
                heart.renderer.material = HeartHalfMaterial;
            }
            //Empty heart
            else {
                heart.renderer.material = HeartEmptyMaterial;
            }
            healthDisplayed += 2f;

            if (healthDisplayed >= _player.FullHealth)
                done = true;
        }

		//Tween the hearts, that changed
		if (_healthPrevious != _player.Health) {
			var low = (_healthPrevious < _player.Health) ? _healthPrevious : _player.Health;
			var high = (_healthPrevious < _player.Health) ? _player.Health : _healthPrevious;
			for(int i = 0; i < Hearts.Length; i++) {
				if (low/2f < i + 1 && high/2f > i) {
					TweenParms heartTweenStart = new TweenParms().Prop(
						"localScale", _heartBaseScale * _tweenScale).Ease(
						EaseType.EaseInBack).Delay(0f);
					HOTween.To(Hearts[i].transform, 0.2f, heartTweenStart);

					TweenParms heartTweenEnd = new TweenParms().Prop(
						"localScale", _heartBaseScale).Ease(
						EaseType.EaseInOutBack).Delay(0.2f);
					HOTween.To(Hearts[i].transform, 0.2f, heartTweenEnd);
				}
			}
		}
		_healthPrevious = _player.Health;
    }
}
