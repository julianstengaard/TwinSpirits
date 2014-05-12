using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using InControl;

public class InputHandler : MonoBehaviour {
    private List<InputDevice> activeDevices = new List<InputDevice>();
    private bool _playersControlled = false;
    private bool _isActive = false;

    public bool SkipMenuDebug = false;
	public bool RestartSameSettings = false;

    void Start() {
        if (SkipMenuDebug) {
            Reset();
            Activate();
        }
    }

    private void Reset() {
        _isActive = false;
        _playersControlled = false;
        activeDevices = new List<InputDevice>();
    }

    void OnLevelWasLoaded(int level) {
        if (level == 0) {
            Reset();
		}
		else if (level == 1 && RestartSameSettings) {
			print ("from restart");
			_playersControlled = false;
			_isActive = true;
			DetroyOtherInputHandler();
		}
        else if (level == 1 && !SkipMenuDebug) {
            DetroyOtherInputHandler();
        }
    }

    public void Activate() {
        _isActive = true;
        InputManager.Setup();
        InputManager.AttachDevice(new UnityInputDevice(new FPSProfile()));
    }

    // Update is called once per frame
    void Update() {
        if (!_isActive) return;

        InputManager.Update();
        if (!_playersControlled) {
            if (SkipMenuDebug) {
                if (InputManager.ActiveDevice.Action1.WasPressed) {
                    if (PlayerJoin(InputManager.ActiveDevice))
                        AssumeControlDebug(InputManager.ActiveDevice);
                }
            }
            else if (Application.loadedLevel == 1) {
                AssumeControl();
            }
        }
    }

    public bool PlayerJoin (InputDevice inputDevice) {
        if (!activeDevices.Contains(inputDevice)) {
            activeDevices.Add(inputDevice);
            return true;
        }
        return false;
    }

    private void AssumeControl() {
        var heroes = GameObject.FindObjectsOfType<Hero>();

        if (heroes.Length != 2 && activeDevices.Count() != 2) {
            Debug.LogError("Input needs 2 players and 2 inputs");
        } else {
            if (heroes[0].PlayerSlot == Hero.Player.One) {
                heroes[0].AttachInputDevice(activeDevices[0]);
                heroes[1].AttachInputDevice(activeDevices[1]);
            } else if (heroes[0].PlayerSlot == Hero.Player.Two) {
                heroes[1].AttachInputDevice(activeDevices[0]);
                heroes[0].AttachInputDevice(activeDevices[1]);
            }

			if (heroes[0].IsControlled && heroes[1].IsControlled) {
            	_playersControlled = true;
				RestartSameSettings = false;
				print ("controlling");
			}
        }
    }

    private void AssumeControlDebug(InputDevice inputDevice) {
        var heroes = GameObject.FindObjectsOfType<Hero>();

        int playersConnected = 0;
        foreach (var hero in heroes) {
            if (!hero.IsControlled) {
                hero.AttachInputDevice(inputDevice);
                playersConnected++;
                break;
            }
        }
        if (playersConnected >= 2) {
            _playersControlled = true;
        }
    }

    private void DetroyOtherInputHandler() {
        InputHandler[] inputHandlers = FindObjectsOfType<InputHandler>();
        for (int i = 0; i < inputHandlers.Length; i++) {
            if (inputHandlers[i].gameObject != gameObject) {
				if (inputHandlers[i].RestartSameSettings == true) {
					return;
				}
                Destroy(inputHandlers[i].gameObject);
			}
        }
    }
}
