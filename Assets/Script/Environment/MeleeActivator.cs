using UnityEngine;
using System;
using System.Collections.Generic;

public class MeleeActivator : BaseUnit {
	[SerializeField]
	protected SpawnActivator activator;
	public AudioSource ActivationSound;

	private bool HasActivated;

	protected override void Died () {
		if(HasActivated)
			return;
		HasActivated = true;
		activator.GenerateMesh();
		activator.StartEverything();
		ActivationSound.Play();
	}

	public override void SetMovementSpeedBuff (float movementSpeedBuff) { }
}
