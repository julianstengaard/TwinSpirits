using UnityEngine;
using System.Collections;
using System.Threading;
using RAIN.Core;
using RAIN.Navigation.NavMesh;

public class CalcNavigation : MonoBehaviour {
	private bool hasGenerated = false;

	// Use this for initialization
	public void GenerateMesh () {
		if(hasGenerated) return;
		hasGenerated = true;
		StartCoroutine(generate());
	}

	private IEnumerator generate() {
		var rig = GetComponent<NavMeshRig>();
		
		RAIN.Navigation.NavMesh.NavMesh mesh = rig.NavMesh;

		mesh.UnregisterNavigationGraph();

		mesh.StartCreatingContours(rig, 2);
		while (mesh.Creating) {
			mesh.CreateContours();
			yield return new WaitForFixedUpdate();
			Debug.Log("Loading : " + mesh.CreatingProgress + " at " + Time.time);
		}
		mesh.RegisterNavigationGraph();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
