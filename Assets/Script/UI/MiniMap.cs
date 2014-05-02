using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holoville.HOTween;

public class MiniMap : MonoBehaviour {
    public GameObject PrefabIslandDiscovered;
    public GameObject PrefabIslandDanger;
    public GameObject PrefabIslandDone;
    public GameObject PrefabPlayers;
    public GameObject PrefabBridge;

    private GameObject[,] _miniMapIslands;
    private GameObject[,] _miniMapHorizontalBridges;
    private GameObject[,] _miniMapVerticalBridges;
    private GameObject _players;

    private bool _gameOver = false;
    private int _islandsDone = 0;
    private int _totalIslands = 1;

    private float _mapIslandSpacing = 1.5f;
    private float _bridgeDepthOffset = 0.05f;

    public float _miniMapLeftBorder;
    public float _miniMapTopBorder;
    public float _miniMapRightBorder;
    public float _miniMapBottomBorder;
    private Vector3 _miniMapAnchor;
    private Vector3 _miniMapBaseScale;

    private MazeCell _playerCell;

	// Use this for initialization
	void Start () {
        _miniMapAnchor = gameObject.transform.localPosition;
        _miniMapBaseScale = gameObject.transform.localScale;
	}

    public void CreateMiniMap(Maze maze) {
        _totalIslands = 0;
        _miniMapIslands = new GameObject[maze.width, maze.height];
        _miniMapHorizontalBridges = new GameObject[maze.width, maze.height];
        _miniMapVerticalBridges = new GameObject[maze.width, maze.height];

        //Count islands
        for (int y = 0; y < maze.height; y++) {
            for (int x = 0; x < maze.width; x++) {
                if (!maze.GetCell(x, y).HasNoDoors()) {
                    _totalIslands++;
                }
            }
        }
        print("MiniMap found: " + _totalIslands + " islands");
    }

    public void SetCellDone(GameObject cell) {
        MazeCell doneCell = cell.GetComponent<MazeInstance>().represents;
        if (_miniMapIslands[doneCell.column, doneCell.row] != null) {
            GameObject.Destroy(_miniMapIslands[doneCell.column, doneCell.row]);
        }
        InstantiateMapIsland(doneCell.column, doneCell.row, "DONE");
        _islandsDone++;
    }

    public void SetCellDangerous(GameObject cell) {
        MazeCell dangerCell = cell.GetComponent<MazeInstance>().represents;
        if (_miniMapIslands[dangerCell.column, dangerCell.row] != null) {
            GameObject.Destroy(_miniMapIslands[dangerCell.column, dangerCell.row]);
        }
        InstantiateMapIsland(dangerCell.column, dangerCell.row, "DANGER");
    }

    public void SetNeighborsDiscovered(GameObject cell) {
        MazeCell rootCell = cell.GetComponent<MazeInstance>().represents;
        InstantiateMapIsland(rootCell.column, rootCell.row, "DISCOVER");
        if (rootCell.doors[0] == true) {
            InstantiateMapIsland(rootCell.column - 1, rootCell.row, "DISCOVER");
            InstantiateMapBridge(rootCell.column - 1, rootCell.row, false);
        } if (rootCell.doors[1] == true) {
            InstantiateMapIsland(rootCell.column, rootCell.row - 1, "DISCOVER");
            InstantiateMapBridge(rootCell.column, rootCell.row - 1, true);
        } if (rootCell.doors[2] == true) {
            InstantiateMapIsland(rootCell.column + 1, rootCell.row, "DISCOVER");
            InstantiateMapBridge(rootCell.column, rootCell.row, false);
        } if (rootCell.doors[3] == true) {
            InstantiateMapIsland(rootCell.column, rootCell.row + 1, "DISCOVER");
            InstantiateMapBridge(rootCell.column, rootCell.row, true);
        }
        UpdateMiniMapPosition();
    }

    public void SetInitialBorders(GameObject origin) {
        MazeCell originCell = origin.GetComponent<MazeInstance>().represents;
        _miniMapLeftBorder = originCell.column * _mapIslandSpacing;
        _miniMapTopBorder = -originCell.row * _mapIslandSpacing;
        _miniMapRightBorder = originCell.column * _mapIslandSpacing;
        _miniMapBottomBorder = -originCell.row * _mapIslandSpacing;
        UpdateMiniMapPosition(true);
        InstantiatePlayers(originCell.column, originCell.row);
        SetPlayerPosition(origin);
    }

    private void InstantiatePlayers(int c, int r) {
        _players = (GameObject)Instantiate(PrefabPlayers, Vector3.zero, Quaternion.identity);

        _players.transform.parent = gameObject.transform;
        _players.transform.localScale = Vector3.one;
        _players.transform.localRotation = Quaternion.identity;

        Vector3 playersLocalPosition = new Vector3(c * _mapIslandSpacing, -r * _mapIslandSpacing, 0f);
        _players.transform.localPosition = playersLocalPosition;
    }

    private void InstantiateMapIsland(int c, int r, string type) {
        if (type == "DONE") {
            _miniMapIslands[c, r] = (GameObject)Instantiate(PrefabIslandDone, Vector3.zero,
                Quaternion.identity);
        } else if (type == "DISCOVER") {
            if (_miniMapIslands[c, r] == null) {
                _miniMapIslands[c, r] = (GameObject)Instantiate(PrefabIslandDiscovered, Vector3.zero, Quaternion.identity);
            }
        } else if (type == "DANGER") {
            _miniMapIslands[c, r] = (GameObject) Instantiate(PrefabIslandDanger, Vector3.zero, Quaternion.identity);
        } else {
            Debug.LogError("Not a valid map island type");
        }
        _miniMapIslands[c, r].transform.parent = gameObject.transform;
        _miniMapIslands[c, r].transform.localScale = Vector3.one;
        _miniMapIslands[c, r].transform.localRotation = Quaternion.identity;

        Vector3 islandLocalPosition = new Vector3(c*_mapIslandSpacing, -r*_mapIslandSpacing, 0f);

        _miniMapLeftBorder = Mathf.Min(_miniMapLeftBorder, islandLocalPosition.x);
        _miniMapTopBorder = Mathf.Max(_miniMapTopBorder, islandLocalPosition.y);
        _miniMapRightBorder  = Mathf.Max(_miniMapRightBorder, islandLocalPosition.x);
        _miniMapBottomBorder = Mathf.Min(_miniMapBottomBorder, islandLocalPosition.y);

        _miniMapIslands[c, r].transform.localPosition = islandLocalPosition;
    }

    private void InstantiateMapBridge(int c, int r, bool rotated) {
        if (rotated) {
            if (_miniMapVerticalBridges[c, r] == null) {
                _miniMapVerticalBridges[c, r] = (GameObject) Instantiate(PrefabBridge, Vector3.zero, Quaternion.identity);
                _miniMapVerticalBridges[c, r].transform.parent = gameObject.transform;
                _miniMapVerticalBridges[c, r].transform.localScale = Vector3.one;

                _miniMapVerticalBridges[c, r].transform.localPosition = new Vector3(c*_mapIslandSpacing,
                    -(r*_mapIslandSpacing + (0.5f*_mapIslandSpacing)), _bridgeDepthOffset);
                _miniMapVerticalBridges[c, r].transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            }
        } else {
            if (_miniMapHorizontalBridges[c, r] == null) {
                _miniMapHorizontalBridges[c, r] = (GameObject)Instantiate(PrefabBridge, Vector3.zero, Quaternion.identity);
                _miniMapHorizontalBridges[c, r].transform.parent = gameObject.transform;
                _miniMapHorizontalBridges[c, r].transform.localScale = Vector3.one;

                _miniMapHorizontalBridges[c, r].transform.localPosition = new Vector3(c * _mapIslandSpacing + (0.5f * _mapIslandSpacing), -(r * _mapIslandSpacing),
                            _bridgeDepthOffset);
                _miniMapHorizontalBridges[c, r].transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            }
        }
    }

    public void SetPlayerPosition(GameObject cell) {
		if(cell == null) return;
        _playerCell = cell.GetComponent<MazeInstance>().represents;
        Vector3 targetPosition = new Vector3(_playerCell.column * _mapIslandSpacing, -_playerCell.row * _mapIslandSpacing, 0f);
        _players.transform.localPosition = targetPosition;

        TweenParms playersMoveTween = new TweenParms().Prop(
            "localPosition", targetPosition).Ease(
                EaseType.EaseInExpo).Delay(0f);
        HOTween.To(_players.transform, 0.3f, playersMoveTween);
    }

    public void UpdateMiniMapPosition(bool first = false) {
        float time = 0.3f;
        Vector3 targetScale = gameObject.transform.localScale;

        //Scale to fit corner
        float longestSide = Mathf.Max(Mathf.Abs(_miniMapRightBorder - _miniMapLeftBorder),
            Mathf.Abs(_miniMapBottomBorder - _miniMapTopBorder));

        if (longestSide > 2f) {
            targetScale = (2 / longestSide) * _miniMapBaseScale;
            TweenParms miniMapScaleTween = new TweenParms().Prop(
                "localScale", targetScale).Ease(
                    EaseType.EaseInExpo).Delay(0f);
            HOTween.To(gameObject.transform, time, miniMapScaleTween);
        }

        //Move top left island to anchor
        float deltaX = _miniMapLeftBorder * targetScale.x;
        float deltaY = _miniMapTopBorder * targetScale.y;
        Vector3 targetPosition = _miniMapAnchor - new Vector3(deltaX, deltaY, 0f);

        if (!first) {
            TweenParms miniMapMoveTween = new TweenParms().Prop(
                "localPosition", targetPosition).Ease(
                    EaseType.EaseInExpo).Delay(0f);
            HOTween.To(gameObject.transform, time, miniMapMoveTween);
        } else {
            gameObject.transform.localPosition = targetPosition;
        }
    }

    public int GetIslandsDone() {
        return _islandsDone;
    }
    public int GetIslandsTotal() {
        return _totalIslands;
    }
    public MazeCell GetPlayerCellPosition() {
        return _playerCell;
    }
}
