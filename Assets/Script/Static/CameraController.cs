using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public bool SinglePlayer;
	public bool SwitchPlayer;

	public GameObject Blacker;
    public Camera UICamera;

	public AudioSource MainAudio;
	public AudioSource LoopAudio;

    public AudioClip MainMusicInit;
	public AudioClip MainMusicLoop;
	public AudioClip BossMusic;
    public AudioClip GameOverMusic;
	private MusicState _currentMusicState;

    private MiniMap _miniMap;

	private bool fading = false;

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
    private bool _gameOver = false;
	private bool _restartReady = false;

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
		SetMusicState(MusicState.Init);
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
		
		if (_restartReady) {
			//Waiting for restart
			if (_player1.GetInputDevice().Action2.WasPressed || _player2.GetInputDevice().Action2.WasPressed) {
				//Restart
				print("to menu");
				_restartReady = false;
				StartCoroutine(GameToMenu(0f));
			} else if (_player1.GetInputDevice().Action1.WasPressed || _player2.GetInputDevice().Action1.WasPressed) {
				//Back to menu
				print("restarting");
				_restartReady = false;
				StartCoroutine(Restart(0f));
			}
		}
		else if (SinglePlayer && !SwitchPlayer) {
			_target = _player1.transform.position;
		    _cameraLookTarget = _target;
		} else if (SinglePlayer && SwitchPlayer) {
            _target = _player2.transform.position;
            _cameraLookTarget = _target;
		} else if (!_player1.dead || !_player2.dead) {
            //MAIN CAMERA MODE TAKES PLACE HERE! Get fancy camera values based on player distances
            UpdateSmartCameraValues();
            _target = (_player1.transform.position + _player2.transform.position) * 0.5f;
			_target.y = _player1.transform.position.y > _player2.transform.position.y  ? _player1.transform.position.y : _player2.transform.position.y;
            _cameraLookTarget = _target + new Vector3(0f, 0f, _cameraZLookOffset);
		} /*else if (!_player1.dead && _player2.dead) {
            _target = _player1.transform.position;
            _cameraLookTarget = _target;
		} else if (_player1.dead && !_player2.dead) {
            _target = _player2.transform.position;
            _cameraLookTarget = _target;
		} */ else {
            if (!_gameOver) {
                SetGameOver(false);
			}
		}

		//Vector3 wantedPosition = target + Vector3.back * distance + Vector3.up * height;
		Vector3 wantedPosition = _target + Vector3.back * _cameraZOffset + Vector3.up * _cameraHeight;

		if ((wantedPosition - transform.position).magnitude > 0.1f)	{
			transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * 2f);
		}

		//if ((_currentLook - _cameraLookTarget).magnitude > 0.01f) {
		_currentLook = Vector3.Lerp(_currentLook, _cameraLookTarget, Time.deltaTime * 3f);
		//}

		if (_intro) {
            transform.LookAt(_cameraLookTarget);
		} else {
			transform.LookAt (_currentLook);
		}

		if (_intro && (wantedPosition - transform.position).magnitude < 22220.2f) {
			print ("intro over");
			_intro = false;
		}
	}

	private IEnumerator FadeToBlack() {
		yield return new WaitForSeconds(3);
		for(var i = 0.0f; i <= 100; i++) {
			Blacker.renderer.material.SetColor("_Color", new Color(0,0,0, i / 100.0f));
			yield return new WaitForSeconds(0.01f);
		}
		//yield return new WaitForSeconds(10);
		//GameToMenu();
	}

	private IEnumerator GameToMenu(float delay) {
		FindObjectOfType<InputHandler>().RestartSameSettings = false;
		yield return new WaitForSeconds(delay);
		Application.LoadLevel(0);
	}

	private IEnumerator Restart(float delay) {
		var handler = FindObjectOfType<InputHandler>();
		handler.RestartSameSettings = true;
		yield return new WaitForSeconds(delay);
		Application.LoadLevel(1);
	}

    public void SetGameOver(bool victory) {
        _gameOver = true;
		_restartReady = true;

		SetMusicState(MusicState.GameOver);

        //GAME OVER
        _target = (_player1.transform.position + _player2.transform.position) * 0.5f;
        _cameraLookTarget = _target;
        _cameraZOffset = 3f;
        _cameraHeight = 50f;

		int done = _miniMap.GetIslandsDone() - 1;
        string s = done == 1 ? "" : "s";
        if (!victory) {
            SwitchUIToGameOver("You lost...", "But survived " + done + " island" + s + " of " + (_miniMap.GetIslandsTotal() - 1) + " total");
        } else {
            SwitchUIToGameOver("You won this time...", "You beat " + done + " island" + s + " of " + (_miniMap.GetIslandsTotal() - 1) + " total");
        }

        if (!fading) {
            StartCoroutine(FadeToBlack());
            fading = true;
        }
    }

    private void SwitchUIToGameOver(string head, string body) {
        var transforms = UICamera.GetComponentsInChildren<Transform>(true);
		foreach(Transform t in UICamera.transform) {
			if (t.gameObject == UICamera.gameObject) {} 
			else if (t.gameObject.name == "GameOverTextHead") {
				t.gameObject.SetActive(true);
				t.GetComponent<TextMesh>().text = head;
			} 
			else if (t.gameObject.name == "GameOverTextBody") {
				t.gameObject.SetActive(true);
				t.GetComponent<TextMesh>().text = body;
			}
			else {
				t.gameObject.SetActive(false);
			}
		}
			

		/*
        foreach (var t in transforms) {
            if (t.gameObject == UICamera.gameObject) {} 
            else if (t.gameObject.name == "GameOverTextHead") {
                t.gameObject.SetActive(true);
                t.GetComponent<TextMesh>().text = head;
            } 
            else if (t.gameObject.name == "GameOverTextBody") {
                t.gameObject.SetActive(true);
                t.GetComponent<TextMesh>().text = body;
            }
			else {
                t.gameObject.SetActive(false);
            }

        }
        */
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

    public void SetMiniMap(MiniMap map) {
        _miniMap = map;
    }

	public void SetMusicState(MusicState state) {
		if (state == MusicState.Init && _currentMusicState != state) {
			_currentMusicState = state;
			MainAudio.Stop();
			MainAudio.loop = false;
			MainAudio.clip = MainMusicInit;
			MainAudio.Play();
			StartCoroutine(DelayedSwitchToMainLoopMusic(MainMusicInit.length));
		} else if (state == MusicState.MainLoop && _currentMusicState != state) {
			_currentMusicState = state;
			LoopAudio.clip = MainMusicLoop;
			LoopAudio.Play();
			LoopAudio.loop = true;
		} else if (state == MusicState.Boss && _currentMusicState != state) {
			_currentMusicState = state;
			MainAudio.Stop();
			MainAudio.loop = true;
			MainAudio.clip = BossMusic;
			MainAudio.Play();
			LoopAudio.Stop();
		} else if (state == MusicState.GameOver && _currentMusicState != state) {
			_currentMusicState = state;
			MainAudio.Stop();
			MainAudio.loop = true;
			MainAudio.clip = GameOverMusic;
			MainAudio.Play();
			LoopAudio.Stop();
		}
	}

	private IEnumerator DelayedSwitchToMainLoopMusic(float time) {
		LoopAudio.clip = MainMusicLoop;
		yield return new WaitForSeconds(time - 1f);
		if (_currentMusicState == MusicState.Init) {
			SetMusicState(MusicState.MainLoop);
		}
	}

	public enum MusicState {
		Null,
		Init,
		MainLoop,
		Boss,
		GameOver
	};
}
