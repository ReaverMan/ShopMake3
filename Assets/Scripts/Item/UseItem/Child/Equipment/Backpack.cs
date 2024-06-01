using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using static UnityEditor.Progress;

public class Backpack : ItemBase
{
    public float amountIncrese = 30.0f;
    public override void Use()
    {
        if (player != null)
        {
            player.limitWeight += amountIncrese;
            player.MaxWeight += amountIncrese;
            Debug.Log("방어력이 증가하였습니다.");
        }
        else
        {
            Debug.LogError("Player is not assigned when trying to use Heal.");
        }
    }

    public override void UnUse()
    {
        player.limitWeight -= amountIncrese;
        player.MaxWeight -= amountIncrese; 
    }
}
