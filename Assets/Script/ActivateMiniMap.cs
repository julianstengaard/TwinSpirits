using UnityEngine;
using System.Collections;

public class ActivateMiniMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var _miniMap = GameObject.FindGameObjectWithTag("MiniMap").GetComponent<MiniMap>();
	    _miniMap.SetInitialBorders(gameObject.transform.root.gameObject);
        _miniMap.SetCellDone(gameObject.transform.root.gameObject);
        _miniMap.SetNeighborsDiscovered(gameObject.transform.root.gameObject);
	}
}
