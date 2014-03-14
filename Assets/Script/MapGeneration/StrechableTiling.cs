using UnityEngine;
using System.Collections;

public class StrechableTiling : MonoBehaviour {

	public GameObject stretchSource;
	public float baseTiling = 1;
	public Vector3 dimensionToTile = Vector3.up;
	public bool keepTilingUpdated = false;

	void Start () 
	{
		if 		(dimensionToTile.x != 0)
			renderer.material.mainTextureScale = new Vector2 (baseTiling * stretchSource.transform.localScale.x, 1);
		else if (dimensionToTile.y != 0)
			renderer.material.mainTextureScale = new Vector2 (baseTiling * stretchSource.transform.localScale.y, 1);
		else if (dimensionToTile.z != 0)
			renderer.material.mainTextureScale = new Vector2 (baseTiling * stretchSource.transform.localScale.z, 1);
	}

	void Update ()
	{
		if (keepTilingUpdated)
			Start();
	}

}
