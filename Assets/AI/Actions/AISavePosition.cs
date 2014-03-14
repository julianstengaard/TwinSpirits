using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Action;
using RAIN.Representation;

[RAINAction("Save Position")]
public class AISavePosition : RAINAction
{
	public Expression target;

	private GameObject unit;

    public AISavePosition()
    {
        actionName = "AISavePosition";
    }

    public override void Start(AI ai)
    {
        base.Start(ai);
		unit = target.Evaluate(ai.DeltaTime, ai.WorkingMemory).GetValue<GameObject>();
    }

    public override ActionResult Execute(AI ai)
    {
		var targetPosition = unit.transform.position;
		ai.WorkingMemory.SetItem("targetPosition", unit.transform.position);
		Debug.DrawLine(ai.Body.transform.position, unit.transform.position, Color.red, 3f);
		return ActionResult.SUCCESS;
    }

    public override void Stop(AI ai)
    {
        base.Stop(ai);
    }
}