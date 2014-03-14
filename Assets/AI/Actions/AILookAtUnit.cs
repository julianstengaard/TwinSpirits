using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Action;
using RAIN.Representation;

[RAINAction("Look At Unit")]
public class AILookAtUnit : RAINAction
{
	public Expression target;

    public AILookAtUnit()
    {
        actionName = "AILookAtUnit";
    }

    public override void Start(AI ai)
    {
        base.Start(ai);
    }

    public override ActionResult Execute(AI ai)
    {
		Debug.Log("stuff");
		var unit = target.Evaluate(ai.DeltaTime, ai.WorkingMemory).GetValue<GameObject>();
		if(unit == null)
			return ActionResult.FAILURE;
		Debug.Log("happened");
		Debug.DrawLine(ai.Body.transform.position, unit.transform.position);

		ai.Body.transform.Rotate(Vector3.up, 90);
        return ActionResult.SUCCESS;
    }

    public override void Stop(AI ai)
    {
        base.Stop(ai);
    }
}