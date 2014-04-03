using UnityEngine;
using System.Collections;

public enum Opening {
	LEFT,
	TOP,
	RIGHT, 
	BOT
}

public class BridgeOpening : MonoBehaviour {
	public Opening opening;
	public bool forced = false;
}
