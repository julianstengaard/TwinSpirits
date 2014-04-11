using System.Configuration;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlowForSpiritImmortal : MonoBehaviour
{
    private float slowAmount = 2f;
    private List<BaseUnit> slowedUnits = new List<BaseUnit>();

    void OnTriggerEnter(Collider other)
    {
        var enemy = other.gameObject.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            if (!slowedUnits.Contains(enemy))
            {
                slowedUnits.Add(enemy);
            }
            enemy.SetMovementSpeedBuff(-slowAmount);
        }
    } 
    void OnTriggerExit(Collider other)
    {
        var enemy = other.gameObject.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            if (slowedUnits.Contains(enemy))
            {
                slowedUnits.Remove(enemy);
            }
            enemy.SetMovementSpeedBuff(slowAmount);
        }
    }

    private void OnDestroy()
    {
        foreach (var slowedUnit in slowedUnits)
        {
            slowedUnit.SetMovementSpeedBuff(slowAmount);
        }
    }
}
