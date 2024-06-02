using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : BuffBase
{
    public override void Use()
    {
        if (player != null)
        {
            player.Heal(amountBuff);
            Debug.Log("HP가 회복되었습니다.");
            Destroy(gameObject);
            base.Use();
        }
        else
        {
            Debug.LogError("Player is not assigned when trying to use Heal.");
        }
    }

#if UNITY_EDITOR
    public void Test_Use()
    {
        Use();
    }
#endif
}
