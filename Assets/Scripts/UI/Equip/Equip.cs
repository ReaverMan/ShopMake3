using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Equip
{
    private const int Default_Inventory_Size = 6;
    public EquipSlot[] slots;
    public EquipSlot this[uint index] => slots[index];
    private int SlotCount => slots.Length;
    private Player owner;
    private DragSlot dragSlot;
    private uint dragSlotIndex = 999999999;
    public DragSlot DragSlot => dragSlot;
    private ItemDataManager itemDataManager;
    public Player Owner => owner;
    private Equip_UI equipUI;
    public QuickSlot quickSlot;

    public Equip(GameManager owner, uint size = Default_Inventory_Size)
    {
        slots = new EquipSlot[size];

        for (uint i = 0; i < slots.Length; i++)
        {
            slots[i] = new EquipSlot(i);
        }

        dragSlot = new DragSlot(dragSlotIndex);
        itemDataManager = GameManager.Instance.ItemData;
        equipUI = GameManager.Instance.EquipUI;
        this.owner = owner.Player;

        Transform equipUIObject = equipUI.gameObject.transform.GetChild(0);

        for (uint i = 0; i < slots.Length; i++)
        {
            slots[i].slotType = equipUIObject.GetChild((int)i).GetComponent<EquipSlot_UI>().slotType;
        }
    }

    public bool AddItem(ItemCode code)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (AddItem(code, (uint)i))
            {
                return true;
            }
        }

        return false;
    }

    public bool AddItem(ItemCode code, uint slotIndex)
    {
        if (IsValidIndex(slotIndex))
        {
            ItemData data = itemDataManager[code];
            if (data == null)
            {
                Debug.LogError($"itemDataManager에서 ItemCode {code}에 대한 데이터를 찾을 수 없습니다.");
                return false;
            }

            EquipSlot slot = slots[slotIndex];
            if (slot.slotType.Contains(data.itemType))
            {
                if (slot.IsEmpty)
                {
                    slot.AssignSlotItem(data);
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsValidIndex(uint index)
    {
        return index < SlotCount;
    }

    private bool IsValidIndex(ItemSlot slot)
    {
        return (slot != null) || (slot == dragSlot);
    }

    private bool RemoveItem(uint slotIndex, uint count = 1)
    {
        if (IsValidIndex(slotIndex))
        {
            EquipSlot slot = slots[slotIndex];
            slot.DecreaseSlotItem(count);
            return true; // 아이템 제거 성공
        }
        return false; // 유효하지 않은 인덱스
    }

    public bool RemoveItem(ItemCode code, uint count = 1)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            ItemData data = itemDataManager[code];
            EquipSlot slot = slots[i];

            if (slot.slotType.Contains(data.itemType))
            {
                if (!slot.IsEmpty)
                {
                    return RemoveItem((uint)i, count); // 내부 메서드 호출 결과 반환
                }
            }
        }
        return false; // 해당 코드에 해당하는 아이템을 찾지 못함
    }

    public void UnEquipAllItems()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                RemoveItem(slot.ItemData.itemId);
            }
        }
    }

    private EquipSlot GetEmptySlot(ItemCode itemCode)
    {
        // 모든 슬롯을 순회하며 비어 있는 슬롯을 찾음
        foreach (var slot in slots)
        {
            if (slot.IsEmpty && slot.slotType.Contains(itemDataManager[itemCode].itemType))
            {
                return slot;
            }
        }
        return null; // 비어 있는 슬롯이 없으면 null 반환
    }
    public void SaveEquipmentData(string path)
    {
        List<EquipmentData> equipmentDataList = new List<EquipmentData>();
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                uint itemId = (uint)slot.ItemData.itemId; // ItemCode를 uint로 변환
                ItemType itemType = slot.ItemData.itemType;
                equipmentDataList.Add(new EquipmentData(itemId, itemType));
            }
        }
        string json = JsonUtility.ToJson(new EquipmentDataWrapper(equipmentDataList));
        File.WriteAllText(path, json);
        Debug.Log($"장비 데이터 저장됨: {json}");
    }

    public void LoadEquipmentData(string path)
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            EquipmentDataWrapper wrapper = JsonUtility.FromJson<EquipmentDataWrapper>(json);
            foreach (var data in wrapper.equipments)
            {
                ItemCode itemCode = (ItemCode)data.itemId; // uint를 ItemCode로 변환
                AddItem(itemCode);
            }
            Debug.Log($"장비 데이터 로드됨: {json}");
        }
        else
        {
            Debug.LogWarning("장비 데이터 파일을 찾을 수 없습니다.");
        }
    }

    public void ReEquipLoadedItems()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                Equipment equipment = ConvertItemTypeToEquipment(slot.ItemData.itemType);
                Owner.Equipped(equipment);
            }
        }
    }

    private Equipment ConvertItemTypeToEquipment(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Gun:
                return Equipment.Gun;
            case ItemType.Grenade:
                return Equipment.Throw;
            case ItemType.Helmet:
                return Equipment.Helmet;
            case ItemType.Armor:
                return Equipment.Vest;
            case ItemType.BackPack:
                return Equipment.BackPack;
            // 필요한 경우 다른 아이템 타입도 추가합니다.
            default:
                return Equipment.None; // 기본값을 반환합니다.
        }
    }

}

    [System.Serializable]
public class EquipmentData
{
    public uint itemId;
    public ItemType itemType;

    public EquipmentData(uint itemId, ItemType itemType)
    {
        this.itemId = itemId;
        this.itemType = itemType;
    }
}

[System.Serializable]
public class EquipmentDataWrapper
{
    public List<EquipmentData> equipments;

    public EquipmentDataWrapper(List<EquipmentData> equipments)
    {
        this.equipments = equipments;
    }
}