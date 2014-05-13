using UnityEngine;
using System.Collections;

public class SyncCostLineScaler : MonoBehaviour {
    public GameObject Left;
    public GameObject Middle;
    public GameObject Right;
	public GameObject Badge;

    public Vector3 LeftEnd;
    public Vector3 RightEnd;

	private float _badgeYPos;
    private bool isVisible = true;

	void Start() {
		_badgeYPos = Badge.transform.localPosition.y;
	}

     void Update() {
        Scale();
     }

    void Scale() {
        Left.transform.localPosition = LeftEnd + Vector3.right * 0f;
        Right.transform.localPosition = RightEnd + Vector3.left * 0f;
		var center = (LeftEnd + RightEnd) * 0.5f;
        Middle.transform.localPosition = center;
		center.y = _badgeYPos;
		Badge.transform.localPosition = center;

        //Calculate scale/rotation
        float spanningScale = Vector3.Distance(Left.transform.localPosition, Right.transform.localPosition) - 1f;
        Quaternion rotation = Quaternion.identity;

        //Scale and rotate it
        var s = Middle.transform.localScale;
        s.x = spanningScale;
        Middle.transform.localScale = s;
        Middle.transform.localRotation = rotation;
    }

    public void SetVisible(bool b) {
        if (b && !isVisible) {
            isVisible = true;
            Left.renderer.enabled = true;
            Middle.renderer.enabled = true;
            Right.renderer.enabled = true;
			Badge.renderer.enabled = true;
        } else if (!b && isVisible) {
            isVisible = false;
            Left.renderer.enabled = false;
            Middle.renderer.enabled = false;
            Right.renderer.enabled = false;
			Badge.renderer.enabled = false;
        }
    }
}
