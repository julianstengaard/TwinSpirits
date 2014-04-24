using System.Collections;
using UnityEngine;
using Holoville.HOTween;

public class PopUpText : MonoBehaviour {
    private bool _activated;
    private Hero.Player _pickUpPlayer;

    private Vector3 _baseScale;
    private TextMesh _textMesh;
    private Vector3 _initPosition;
    private Vector3 _targetPosition;
    private Vector3 _currentRotationUp;
    private Vector3 _wantedRotationUp;
    private Camera _mainCamera;

    public float InitTime;

    void Start() {
        if (!_activated)
            gameObject.renderer.enabled = false;
    }

    void LateUpdate() {
        if (_activated) {
            _currentRotationUp = Vector3.Lerp(_currentRotationUp, _wantedRotationUp, Time.deltaTime * 2f);
            transform.rotation = Quaternion.LookRotation(transform.position - _mainCamera.transform.position, _currentRotationUp);
        }
    }

    private IEnumerator TweenText() {
        _wantedRotationUp = Vector3.up;

        transform.localScale *= 0f;
        gameObject.renderer.enabled = true;

        TweenParms popOutTween = new TweenParms().Prop(
            "localScale", _baseScale).Prop(
            "position", _initPosition + Vector3.up).Ease(
            EaseType.EaseOutBounce).Delay(0f);
        HOTween.To(gameObject.transform, 0.3f, popOutTween);

        yield return new WaitForSeconds(0.3f);
        transform.parent = _mainCamera.transform;

        //Is there already another?
        var popups = FindObjectsOfType<PopUpText>();
        _targetPosition = new Vector3(0f, 0.2f, 4f);
        foreach (var popup in popups) {
            if (!popup.gameObject.Equals(this.gameObject) && popup.InitTime < InitTime) {
                print("wut");
                _targetPosition = new Vector3(0f, 1f, 4f);
            }
        }
        _wantedRotationUp = _mainCamera.transform.up;
        TweenParms moveToCameraTween = new TweenParms().Prop(
            "localPosition", _targetPosition).Ease(
            EaseType.EaseInOutQuint).Delay(0f);
        HOTween.To(gameObject.transform, 0.6f, moveToCameraTween);

        yield return new WaitForSeconds(1.5f);

        _wantedRotationUp = (_pickUpPlayer == Hero.Player.One ? Vector3.left : Vector3.right);
        _targetPosition += Vector3.down * 10f + (_pickUpPlayer == Hero.Player.One ? Vector3.left * 1.4f : Vector3.right * 1.4f) * 10f;

        TweenParms moveAwayTween = new TweenParms().Prop(
            "localPosition", _targetPosition).Ease(
            EaseType.EaseInCirc).Delay(0f);
        HOTween.To(gameObject.transform, 0.4f, moveAwayTween);

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);

        yield return null;
    }



    public void Activate(string text, Vector3 initPosition, Camera mainCamera, Hero.Player pickUpPlayer) {
        _activated = true;
        _pickUpPlayer = pickUpPlayer;
        _baseScale = transform.localScale;
        _textMesh = gameObject.GetComponent<TextMesh>();
        _textMesh.text = text;
        _initPosition = initPosition;
        _mainCamera = mainCamera;
        InitTime = Time.time;
        StartCoroutine(TweenText());
    }

}
