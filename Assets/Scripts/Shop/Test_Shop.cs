using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_Shop : MonoBehaviour
{
    public ItemCode code = ItemCode.SmallHeal;
    ShopInventory shopInventory;    // 상점 인벤토리
    public ShopInventoryUI shopInventoryUI;
    WorldInventory worldInventory;  // 월드 인벤토리
    public WorldInventory_UI worldInventoryUI;
    public Player player;

    void Start()
    {
        InitializeShopInventory();
        worldInventory = new WorldInventory(GameManager.Instance);
    }

    void InitializeShopInventory()
    {
        shopInventory = null;

        if (shopInventoryUI != null)
        {
            //shopInventoryUI.InitializeShop();
        }
        else
        {
            Debug.LogError("ShopInventoryUI is not initialized!");
            // 적절한 에러 핸들링 또는 UI 메시지 출력
        }
    }

    void BuyItem(ItemCode itemCode)
    {
        //ItemData itemData = GameManager.Instance.InventoryUI.Inventory.GetItem(itemCode);
        if (/*itemData != null && */worldInventory.AddItem(itemCode))
        {
            GameManager.Instance.InventoryUI.Inventory.RemoveItem(itemCode);
            Debug.Log($"Purchased {itemCode} and added to the world inventory.");
        }
        else
        {
            Debug.Log("Item not available or insufficient funds");
            // 사용자에게 실패 사유를 보다 명확히 알려주는 처리
        }
    }

}
