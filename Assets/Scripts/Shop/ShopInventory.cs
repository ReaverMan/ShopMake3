using UnityEngine;

public class ShopInventory
{
    private static readonly int Default_Inventory_Size = 144;
    public ItemSlot[] items; // 인벤토리 아이템 슬롯 배열
    private ShopInventoryUI shopInvenUI;

    public ShopInventory()
    {
        InitializeInventory(Default_Inventory_Size);
    }

    private void InitializeInventory(int size)
    {
        items = new ItemSlot[size];
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = new ItemSlot((uint)i);
        }

        shopInvenUI = GameManager.Instance.ShopInventoryUI; // ShopInventoryUI 참조 설정
    }

    public void AddItem(ItemCode code)
    {
        // GameManager.Instance.ItemData를 통해 ItemData를 가져옴
        ItemDataManager itemDataManager = GameManager.Instance.ItemData;
        if (itemDataManager == null)
        {
            Debug.Log("itemDataManager가 null입니다.");
            return;
        }

        ItemData data = itemDataManager[code];
        if (data != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].IsEmpty)
                {
                    items[i].AssignSlotItem(data);
                    break;
                }
                else
                {
                    continue;
                }
            }
        }
        else
        {
            Debug.LogError($"ItemDataManager에서 {code}에 대한 데이터를 찾을 수 없습니다.");
        }
    }

    // 인덱서를 추가하여 슬롯에 접근할 수 있게 합니다.
    public ItemSlot this[uint index]
    {
        get
        {
            if (index < items.Length)
            {
                return items[index];
            }
            return null;
        }
    }
}
