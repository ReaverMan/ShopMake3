using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorManualKeyUse : DoorManual
{
    public bool isExitDoor = false;
    bool isOpend = false;

    public override void Interect()
    {
        Open();
    }

    protected override bool preOpen()
    {
        bool rst = false;
        if (GameManager.Instance.InventoryUI.Inventory.FindItem(ItemCode.Key))
        {
            rst = true;
            GameManager.Instance.InventoryUI.Inventory.RemoveItem(ItemCode.Key);
        }
        return rst;
            
    }

    protected override void Open()
    {
        if (isExitDoor)
        {
            if (preOpen())
            {
                Debug.Log("탈출");

                GameManager.Instance.EndGame("MainMenuScene");
            }
        }
        else 
        {

            if (!isOpend)
            {
                if (preOpen())
                {
                    animator.SetBool(IsOpenHash, true);
                    isOpend = true;
                }
            }
        }
    }
}
