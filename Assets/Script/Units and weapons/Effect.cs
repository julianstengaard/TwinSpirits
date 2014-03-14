using UnityEngine;
using System.Collections;

public abstract class Effect
{
	public abstract void DoEffect(BaseUnit target, GameObject source, Vector3 attackPosition, ref float damage); 
	public abstract IEnumerator DoEffectCoroutine(BaseUnit target, GameObject source, Vector3 attackPosition);

	public abstract int GetPriority();
	public abstract bool IsSplashable();
}
