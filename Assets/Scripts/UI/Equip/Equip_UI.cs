using UnityEngine;

public class Equip_UI : MonoBehaviour
{
    Equip equip;

    public Equip Equip => equip;

    EquipSlot_UI[] equipSlot_UI;

    InventoryManager invenManager;

    RectTransform invenTransform;

    CanvasGroup canvas;

    Inventory_UI inven;

    Player Owner => equip.Owner;

    Transform weaponTransform;

    QuickSlot QuickSlot;

    private void Awake()
    {
        Transform child = transform.GetChild(0);

        equipSlot_UI = child.GetComponentsInChildren<EquipSlot_UI>();

        invenManager = GetComponentInParent<InventoryManager>();

        invenTransform = GetComponent<RectTransform>();

        canvas = GetComponent<CanvasGroup>();

        QuickSlot = GetComponent<QuickSlot>();

        if (equipSlot_UI == null || equipSlot_UI.Length == 0)
        {
            Debug.LogError("equipSlot_UI가 초기화되지 않았습니다.");
        }
    }

    public void InitializeInventory(Equip playerEquip)
    {
        equip = playerEquip;

        if (equipSlot_UI == null || equipSlot_UI.Length == 0)
        {
            Debug.LogError("equipSlot_UI가 초기화되지 않았습니다.");
            return;
        }

        for (uint i = 0; i < equipSlot_UI.Length; i++)
        {
            equipSlot_UI[i].InitializeSlot(equip[i]);

            QuickSlot.onWeaponChange = Equipment;
            QuickSlot.onGranadeChange = Equipment;
            QuickSlot.onETCChange = Equipment;
        }
        inven = GameManager.Instance.InventoryUI;

        Close();
    }

    void Equipment(Equipment type)
    {
        Owner.Equipped(type);
    }

    public bool EquipItem(ItemSlot slot)
    {
        return equip.AddItem(slot.ItemData.itemId);
    }

    public bool UnEquipItem(ItemSlot slot)
    {
        return equip.RemoveItem(slot.ItemData.itemId);
    }

    public void UseItem(uint index)
    {
        inven.Inventory.RemoveItem(equipSlot_UI[index].EquipSlot.ItemData.itemId);

        if (!inven.Inventory.FindItem(equipSlot_UI[index].EquipSlot.ItemData.itemId))
        {
            Owner.UnEquipped(equipSlot_UI[index].EquipSlot.ItemData.itemType);
            equip.RemoveItem(equipSlot_UI[index].EquipSlot.ItemData.itemId);
        }
    }

    public void AllUnEquip()
    {
        Debug.Log("Equip_UI AllUnEquip 호출");
        if (equipSlot_UI == null)
        {
            Debug.LogError("equipSlot_UI 배열이 초기화되지 않았습니다.");
            return;
        }

        for (int i = 0; i < equipSlot_UI.Length; i++)
        {
            if (equipSlot_UI[i] != null && equipSlot_UI[i].ItemSlot != null && equipSlot_UI[i].ItemSlot.IsEquiped)
            {
                UnEquipItem(equipSlot_UI[i].EquipSlot);
                if (Owner != null)
                {
                    Owner.UnEquipped(equipSlot_UI[i].EquipSlot.ItemData.itemType);
                }
                else
                {
                    Debug.LogError("Owner가 null입니다.");
                }
            }
        }
    }

    public void Open()
    {
        canvas.alpha = 1;
        canvas.interactable = true;
        canvas.blocksRaycasts = true;
    }

    public void Close()
    {
        canvas.alpha = 0;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
    }
}
