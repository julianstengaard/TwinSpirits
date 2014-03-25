using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RAIN.Core;
using RAIN.Action;

[RAINAction("Is unit alive")]
public class AIIsAlive : RAINAction
{
    public AIIsAlive()
    {
        actionName = "AIIsAlive";
    }

    public override void Start(AI ai)
    {
        base.Start(ai);
    }

    public override ActionResult Execute(AI ai)
    {
		var unit = ai.Body.GetComponent<BaseUnit>();

		Debug.Log("Is Dead? : " + unit.dead);
		
		if(unit != null) {
			if(unit.dead) {
				return ActionResult.FAILURE;
			} else return ActionResult.SUCCESS;
		}
		return ActionResult.FAILURE;
    }

    public override void Stop(AI ai)
    {
        base.Stop(ai);
    }
}