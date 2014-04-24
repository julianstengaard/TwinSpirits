using UnityEngine;
using System.Collections;

//[RequireComponent(typeof(Rigidbody))]
public abstract class Collectable : MonoBehaviour {
    protected GameObject _popUpText;

    protected void Start() {
        _popUpText = (GameObject) Resources.Load("PopUpText");
    }

	protected IEnumerator delayedLayerChange() {
		var curLayer = gameObject.layer;
		gameObject.layer = LayerMask.NameToLayer("Default");
		yield return new WaitForSeconds(1);
		gameObject.layer = curLayer;
	}

	public abstract void Collected(Hero collector);
	public virtual bool IsCollectable(Hero collector) {
		return true;
	}

    protected void CreatePopUpText(string name, Hero collector) {
        var popUp = (GameObject)GameObject.Instantiate(_popUpText, transform.position, Quaternion.identity);
        popUp.GetComponent<PopUpText>().Activate(name, transform.position, Camera.main, collector.PlayerSlot);
    }
}
