using UnityEngine;

public class TimedShieldBlink : MonoBehaviour {
    public Renderer MaterialToBlink;
	public string ColorToChange = "_TintColor";

    private Color _baseColor;
    private float _baseOpacity;
    private float _blinkOpacity = 0f;

    private float _timer;
    private float _startTime;

    private bool _isActive;
    private bool _blinking;
	
	// Update is called once per frame
	void Update () {
        if (_isActive) {
	        _timer -= Time.deltaTime;
            if (!_blinking && _timer < 3f) {
                _startTime = Time.time;
                _blinking = true;
            }
            if (_blinking && _timer < 3f)
				MaterialToBlink.material.SetColor(ColorToChange, GetColor(Mathf.Lerp(8f, 3f, _timer/3f)));
	    }
	}

    public void Activate(float duration) {
        _timer = duration;
        _isActive = true;
		_baseColor = MaterialToBlink.material.GetColor(ColorToChange);
        _baseOpacity = _baseColor.a;
    }

    private Color GetColor(float speed) {
        float time = Mathf.PingPong((Time.time - _startTime) * speed, 1f);
        float opacity = Mathf.Lerp(_baseOpacity, _blinkOpacity, time);
        return new Color(_baseColor.r, _baseColor.g, _baseColor.b, opacity);
    }
}
