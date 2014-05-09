using UnityEngine;

public class CollectableIceSword : Collectable {
	#region implemented abstract members of Collectable
	public override void Collected (Hero collector)	{
        collector.AddEffectToWeapons(new SlowdownMovementSpeed(1f, 3f, 0.25f));
        base.CreatePopUpText("Ice sword", collector);
		//GameObject.Destroy(gameObject);
	}
	#endregion
}
