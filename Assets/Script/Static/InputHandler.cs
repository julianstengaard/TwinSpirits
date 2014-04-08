using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

public class InputHandler : MonoBehaviour {
	private List<InputDevice> activeDevices = new List<InputDevice>();

	// Use this for initialization
	void Start () {
		InputManager.Setup();
		InputManager.AttachDevice(new UnityInputDevice(new FPSProfile()));
	}
	
	// Update is called once per frame
	void Update () {
		InputManager.Update();

		if(activeDevices.Contains(InputManager.ActiveDevice))
			return;

		if(InputManager.ActiveDevice.LeftBumper) {
			var heroes = GameObject.FindObjectsOfType<Hero>();

			foreach(var hero in heroes) {
				if(!hero.IsControlled) {
					hero.AttachInputDevice(InputManager.ActiveDevice);
					activeDevices.Add(InputManager.ActiveDevice);
					break;
				}
			}

		}
	}
}
