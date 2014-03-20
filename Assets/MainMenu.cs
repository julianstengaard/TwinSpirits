using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;

public class MainMenu : MonoBehaviour {

	public LevelCreationInfo LevelCreationInfoGO;

	public TextMesh RegenMesh;
	public TextMesh LevelLengthMesh;
	public TextMesh PlayButtonMesh;

	private List<TextMesh> selectables = new List<TextMesh>();
	private int selectedField;

	private Color textColor			= new Color(1f, 1f, 1f);
	private Color textColorHighlight = new Color(1f, 0f, 0f);

	private int levelLength = 10;
	private int regen = 0;

	private bool inputReady = true;


	// Use this for initialization
	void Start () {
		//Add selectables
		selectables.Add(RegenMesh);
		selectables.Add(LevelLengthMesh);
		selectables.Add(PlayButtonMesh);

		//Set up input
		InputManager.Setup();
		InputManager.AttachDevice(new UnityInputDevice(new FPSProfile()));

		//Set selection at Play
		selectedField = 2;

		UpdateHighlight();
		ChangeLevelLength();
		ChangeRegen();
	}
	
	// Update is called once per frame
	void Update () {
		InputManager.Update();
		
		//Start game
		if (selectedField == 2 && InputManager.ActiveDevice.Action1) {
			GameObject.DontDestroyOnLoad(LevelCreationInfoGO.gameObject);
			Application.LoadLevel(1);
		}

		//Check if stick has been reset
		if (InputManager.ActiveDevice.LeftStickX == 0 && InputManager.ActiveDevice.LeftStickY == 0) {
			inputReady = true;
		}

		//Move up/down
		if (inputReady) {
			if (InputManager.ActiveDevice.LeftStickY < -0.3f) {
				inputReady = false;
				selectedField = (selectedField + 1) % selectables.Count();
				UpdateHighlight();
			} else if (InputManager.ActiveDevice.LeftStickY > 0.3f) {
				inputReady = false;
				selectedField = (selectables.Count() + (selectedField - 1))  % (selectables.Count());
				UpdateHighlight();
			}

			//If over Regen
			if (selectedField == 0) {
				//Move left/right
				if (InputManager.ActiveDevice.LeftStickX < -0.3f) {
					inputReady = false;
					ChangeRegen(-1);
				} else if (InputManager.ActiveDevice.LeftStickX > 0.3f) {
					inputReady = false;
					ChangeRegen(1);
				}
			}

			//If over Level Length
			if (selectedField == 1) {
				//Move left/right
				if (InputManager.ActiveDevice.LeftStickX < -0.3f) {
					inputReady = false;
					ChangeLevelLength(-1);
				} else if (InputManager.ActiveDevice.LeftStickX > 0.3f) {
					inputReady = false;
					ChangeLevelLength(1);
				}
			}
		}
	}

	void UpdateHighlight()
	{
		for (int i = 0; i < selectables.Count(); i++) {
			if ((int) selectedField == i) {
				selectables.ElementAt(i).color = textColorHighlight;
			} else {
				selectables.ElementAt(i).color = textColor;
			}
		}
	}

	void ChangeLevelLength(int i = 0) {
		levelLength = Mathf.Max(2, levelLength + i);
		LevelLengthMesh.text = levelLength.ToString();
		LevelCreationInfoGO.levelLength = levelLength;
	}
	void ChangeRegen(int i = 0) {
		regen = Mathf.Max(0, regen + i);
		RegenMesh.text = regen.ToString();
		LevelCreationInfoGO.spiritRegen = regen;
	}
}
