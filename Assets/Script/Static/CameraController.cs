using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public bool SinglePlayer;
	public bool SwitchPlayer;

    private Hero _player1;
    private Hero _player2;

    //The new playerCameraZoomEffect
    private float _cameraHeight = 8f;
    private float _cameraZOffset = 8f;
    private float _cameraZLookOffset = 0f;
    private Vector3 _cameraLookTarget;


    private Vector3 _target;
    private Vector3 _currentLook;
    private bool _intro = true;

	// Use this for initialization
	void Start () {
		var ps = GameObject.FindObjectsOfType<Hero>();
		if(ps.Length > 0) {
			foreach(var player in ps) {
				if (player.PlayerSlot == Hero.Player.One)
					_player1 = player;
				else if (player.PlayerSlot == Hero.Player.Two)
					_player2 = player;
			}
		}
	}

	void LateUpdate () {
		// Early out if we don't have a target
		if(_player2 == null) {
			Start ();
			return;
		}

        //Reset camera to default
	    _cameraZLookOffset = 0f;
        _cameraHeight = 8f;
        _cameraZOffset = 8f;

		if (SinglePlayer && !SwitchPlayer) {
			_target = _player1.transform.position;
		    _cameraLookTarget = _target;
		} else if (SinglePlayer && SwitchPlayer) {
            _target = _player2.transform.position;
            _cameraLookTarget = _target;
		} else if (!_player1.dead && !_player2.dead) {
            //MAIN CAMERA MODE TAKES PLACE HERE! Get fancy camera values based on player distances
            UpdateSmartCameraValues();
            _target = (_player1.transform.position + _player2.transform.position) * 0.5f;
            _cameraLookTarget = _target + new Vector3(0f, 0f, _cameraZLookOffset);
		} else if (!_player1.dead && _player2.dead) {
            _target = _player1.transform.position;
            _cameraLookTarget = _target;
		} else if (_player1.dead && !_player2.dead) {
            _target = _player2.transform.position;
            _cameraLookTarget = _target;
		} else {
            //GAME OVER
            _target = (_player1.transform.position + _player2.transform.position) * 0.5f;
            _cameraLookTarget = _target;
            _cameraZOffset = 3f;
            _cameraHeight = 50f;
		}

		//Vector3 wantedPosition = target + Vector3.back * distance + Vector3.up * height;
		Vector3 wantedPosition = _target + Vector3.back * _cameraZOffset + Vector3.up * _cameraHeight;
		if ((wantedPosition - transform.position).magnitude > 0.1f)	{
			transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * 2f);
		}
		else if (_intro) {
			_intro = false;
		}
        if ((_currentLook - _cameraLookTarget).magnitude > 0.01f) {
            _currentLook = Vector3.Lerp(_currentLook, _cameraLookTarget, Time.deltaTime * 4f);
		}

		if (_intro) {
            transform.LookAt(_cameraLookTarget);
		} else {
			transform.LookAt (_currentLook);
		}
	}

    //Calculate new magic values
    private void UpdateSmartCameraValues()
    {
        //Get player distances (in 2 relevant axes)
        float playerDistanceWidth           = Mathf.Abs(_player1.transform.position.x - _player2.transform.position.x);
		float playerDistanceDepth           = Mathf.Abs(_player1.transform.position.z - _player2.transform.position.z);
		
        //Get numbers between 0-1 based on distances
        float cameraLerpValueWidth          = Mathf.InverseLerp(2f, 35f, playerDistanceWidth);
        float cameraLerpValueDepth          = Mathf.InverseLerp(2f, 35f, playerDistanceDepth);
        float cameraLerpValueDiagonal       = Mathf.InverseLerp(2f, 48f, playerDistanceWidth + playerDistanceDepth);

        //Use numbers to get eachs preferred height, z-offset & z-look-offset
        float cameraHeightFromWidth         = Mathf.Lerp(5f, 14f, cameraLerpValueWidth);
        float cameraHeightFromDepth         = Mathf.Lerp(5f, 18f, cameraLerpValueDepth);
        float cameraHeightFromDiagonal      = Mathf.Lerp(5f, 18f, cameraLerpValueDiagonal);
        float cameraZOffsetFromWidth        = Mathf.Lerp(6f, 14f, cameraLerpValueWidth);
        float cameraZOffsetFromDepth        = Mathf.Lerp(6f, 23f, cameraLerpValueDepth);
        float cameraZLookOffsetFromWidth    = Mathf.Lerp(0f, 3f, cameraLerpValueWidth);
        float cameraZLookOffsetFromDepth    = Mathf.Lerp(0f, -9f, cameraLerpValueDepth);

        //Use the component that requires the higest value and update the camera transform
        _cameraZLookOffset  = Mathf.Min(0f, cameraZLookOffsetFromWidth + cameraZLookOffsetFromDepth);
        _cameraHeight       = Mathf.Max(Mathf.Max(cameraHeightFromWidth, cameraHeightFromDepth), cameraHeightFromDiagonal);
        _cameraZOffset      = Mathf.Max(Mathf.Max(cameraZOffsetFromWidth, cameraZOffsetFromDepth), cameraHeightFromDiagonal);
    }
}
