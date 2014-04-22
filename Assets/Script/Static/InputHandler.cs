using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using InControl;

public class InputHandler : MonoBehaviour {
    private List<InputDevice> activeDevices = new List<InputDevice>();
    private bool _playersControlled = false;
    private bool _isActive = false;



    private void Reset() {
        _isActive = false;
        _playersControlled = false;
        activeDevices = new List<InputDevice>();
    }

    void OnLevelWasLoaded(int level) {
        if (level == 0)
            Reset();
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

	    if (Application.loadedLevel == 1 && !_playersControlled) {
	        AssumeControl();
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
            _playersControlled = true;
        }
    }
}
