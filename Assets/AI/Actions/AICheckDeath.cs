using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Action;
using RAIN.Representation;

[RAINAction("Check for alive target")]
public class AICheckDeath : RAINAction
{
	public Expression target;

    public AICheckDeath()
    {
        actionName = "AICheckDeath";
    }

    public override void Start(AI ai)
    {
        base.Start(ai);
    }

    public override ActionResult Execute(AI ai)
    {
		var unit = target.Evaluate(ai.DeltaTime, ai.WorkingMemory).GetValue<GameObject>().GetComponent<BaseUnit>();

		if(unit != null) {
			if(unit.dead)
				return ActionResult.FAILURE;
			else return ActionResult.SUCCESS;
		}
		return ActionResult.FAILURE;
    }

    public override void Stop(AI ai)
    {
        base.Stop(ai);
    }
}