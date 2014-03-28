using UnityEngine;
using System;
using System.Collections;

public class SimpleSelfTimer : MonoBehaviour {
	public event Action trigger;
	public float timeout;

	void Start () {
		StartCoroutine(SetTimeout());
	}

	private IEnumerator SetTimeout() {
		yield return new WaitForSeconds(timeout);
		if(trigger != null)
			trigger.Invoke();
	}
}
