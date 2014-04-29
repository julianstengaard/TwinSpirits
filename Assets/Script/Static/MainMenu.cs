using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;

public class MainMenu : MonoBehaviour {
    public GameObject NewGameMenu;
    public GameObject PlayerJoinMenu;

    public AudioClip SelectSound;
    public AudioClip AcceptSound;

    private InputHandler _inputHandler;

	public TextMesh RegenMesh;
	public TextMesh LevelLengthMesh;
	public TextMesh DifficultyMesh;
	public TextMesh PlayButtonMesh;

    public TextMesh GameStartingMesh;
    public TextMesh Player1JoinMesh;
    public GameObject Player1Joined;
    public TextMesh Player2JoinMesh;
    public GameObject Player2Joined;

    private LevelCreationInfo _levelCreationInfoGO;
	private List<TextMesh> _selectables = new List<TextMesh>();

	private Color _textColor = new Color(0.55f, 0.35f, 0.16f);
	private Color _textColorHighlight = new Color(1f, 0f, 0f);

	private int _difficulty = 1;
	private int _levelLength = 10;
	private int _regen = 0;

	private bool _inputReady = true;
    private bool _gameStarting = false;

    private int _selectedField;
    private int _currentMenu = 0;
    private int _playersJoined = 0;

	// Use this for initialization
	void Start () {
	    FindLevelInputHandler();
	    FindLevelCreationInfo();

		//Add selectables
		_selectables.Add(DifficultyMesh);
		_selectables.Add(RegenMesh);
		_selectables.Add(LevelLengthMesh);
		_selectables.Add(PlayButtonMesh);

		//Set selection at Play
		_selectedField = 3;

		UpdateHighlight();
		ChangeLevelLength();
		ChangeRegen();
	}

    // Update is called once per frame
    private void Update() {
        if (_currentMenu == 0) {
            //Start game button (switch to next menu)
            if (_selectedField == 3 && InputManager.ActiveDevice.Action1) {
                NewGameMenu.SetActive(false);
                PlayerJoinMenu.SetActive(true);
                gameObject.audio.PlayOneShot(AcceptSound);
                _currentMenu = 1;
            }

            //Check if stick has been reset
            if (InputManager.ActiveDevice.LeftStickX == 0 && InputManager.ActiveDevice.LeftStickY == 0) {
                _inputReady = true;
            }

            //Move up/down
            if (_inputReady) {
                if (InputManager.ActiveDevice.LeftStickY < -0.3f) {
                    _inputReady = false;
                    _selectedField = (_selectedField + 1)%_selectables.Count();
                    UpdateHighlight();
                    gameObject.audio.PlayOneShot(SelectSound);
                } else if (InputManager.ActiveDevice.LeftStickY > 0.3f) {
                    _inputReady = false;
                    _selectedField = (_selectables.Count() + (_selectedField - 1))%(_selectables.Count());
                    UpdateHighlight();
                    gameObject.audio.PlayOneShot(SelectSound);
                }

				//If over Difficulty
				if (_selectedField == 0) {
					//Move left/right
					if (InputManager.ActiveDevice.LeftStickX < -0.3f) {
						_inputReady = false;
						ChangeDifficulty(-1);
					} else if (InputManager.ActiveDevice.LeftStickX > 0.3f) {
						_inputReady = false;
						ChangeDifficulty(1);
					}
				}

                //If over Regen
                if (_selectedField == 1) {
                    //Move left/right
                    if (InputManager.ActiveDevice.LeftStickX < -0.3f) {
                        _inputReady = false;
                        ChangeRegen(-1);
                    } else if (InputManager.ActiveDevice.LeftStickX > 0.3f) {
                        _inputReady = false;
                        ChangeRegen(1);
                    }
                }

                //If over Level Length
                if (_selectedField == 2) {
                    //Move left/right
                    if (InputManager.ActiveDevice.LeftStickX < -0.3f) {
                        _inputReady = false;
                        ChangeLevelLength(-1);
                    } else if (InputManager.ActiveDevice.LeftStickX > 0.3f) {
                        _inputReady = false;
                        ChangeLevelLength(1);
                    }
                }
            }
        }

        else if (_currentMenu == 1) {
            if (!_gameStarting && _playersJoined == 2) {
                _gameStarting = true;
                gameObject.audio.PlayOneShot(AcceptSound);
                StartCoroutine(StartGameInSeconds(1f));
            } else {
                if (InputManager.ActiveDevice.Action1.WasPressed) {
                    if (_inputHandler.PlayerJoin(InputManager.ActiveDevice)) {
                        _playersJoined++;
                        UpdateJoinedStatus();
                    }
                }
            }
        }
    }

    private IEnumerator StartGameInSeconds(float time) {
        GameStartingMesh.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        GameObject.DontDestroyOnLoad(_levelCreationInfoGO.gameObject);
        GameObject.DontDestroyOnLoad(_inputHandler.gameObject);
        Application.LoadLevel(1);
    }

    private void UpdateJoinedStatus() {
        if (_playersJoined > 0) {
            Player1JoinMesh.text = "PLAYER 1";
            Player1Joined.SetActive(true);
        } 
        if (_playersJoined == 2) {
            Player2JoinMesh.text = "PLAYER 2";
            Player2Joined.SetActive(true);
        }
    }

    private void FindLevelCreationInfo() {
        LevelCreationInfo[] levelCreationInfos = FindObjectsOfType<LevelCreationInfo>();
        for (int i = 0; i < levelCreationInfos.Length; i++) {
            if (i > 0)
                Destroy(levelCreationInfos[i].gameObject);
            else
                _levelCreationInfoGO = levelCreationInfos[i];
        }
    }

    private void FindLevelInputHandler() {
        InputHandler[] inputHandlers = FindObjectsOfType<InputHandler>();
        for (int i = 0; i < inputHandlers.Length; i++) {
            if (i > 0)
                Destroy(inputHandlers[i].gameObject);
            else {
                _inputHandler = inputHandlers[i];
                _inputHandler.Activate();
            }
        }
    }

	void UpdateHighlight()
	{
		for (int i = 0; i < _selectables.Count(); i++) {
			if ((int) _selectedField == i) {
				_selectables.ElementAt(i).color = _textColorHighlight;
			} else {
				_selectables.ElementAt(i).color = _textColor;
			}
		}
	}

	void ChangeLevelLength(int i = 0) {
		_levelLength = Mathf.Max(2, _levelLength + i);
		LevelLengthMesh.text = _levelLength.ToString();
		_levelCreationInfoGO.levelLength = _levelLength;
	}
	void ChangeRegen(int i = 0) {
		_regen = Mathf.Max(0, _regen + i);
		RegenMesh.text = _regen.ToString();
		_levelCreationInfoGO.spiritRegen = _regen;
	}
	void ChangeDifficulty(int i = 0) {
		//_selectedField = (_selectables.Count() + (_selectedField - 1))%(_selectables.Count());
		_difficulty = (3 + (_difficulty + i)) % 3;
		if (_difficulty == 0)
			DifficultyMesh.text = "EASY";
		if (_difficulty == 1)
			DifficultyMesh.text = "NORMAL";
		if (_difficulty == 2)
			DifficultyMesh.text = "HARD";
		_levelCreationInfoGO.DamageRecievedModifier = Mathf.Max((float) _difficulty, 0.5f);
	}
}
