using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using static BulletBase;


// 아이템 획득 시 인벤토리에 넣어주는 클래스
public class Inventory
{ 
    const int Default_Inventory_Size = 36;

    ItemSlot[] slots;
    public ItemSlot this[uint index] => slots[index];

    int SlotCount => slots.Length;

    DragSlot dragSlot;

    uint dragSlotIndex = 999999999;
    public DragSlot DragSlot => dragSlot;

    ItemDataManager itemDataManager;

    Inventory_UI invenUI;

    Player owner;

    public Player Owner => owner;

    public Action<int> onReload;


    public Inventory(GameManager owner, uint size = Default_Inventory_Size)
    {
        slots = new ItemSlot[size];
        for(uint i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot(i);
        }
        dragSlot = new DragSlot(dragSlotIndex);
        itemDataManager = GameManager.Instance.ItemData;
        invenUI = GameManager.Instance.InventoryUI;
        this.owner = owner.Player;
    }

    /// <summary>
    /// 인벤토리에 특정아이템을 1개 추가하는 함수
    /// </summary>
    /// <param Name="code">추가할 아이템의 코드</param>
    /// <returns>true면 성공 false면 추가 실패</returns>
    public bool AddItem(ItemCode code)
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (AddItem(code, (uint)i))
            {
                if(owner != null)
                {
                    if (slots[i].ItemData != null)
                    {
                        Owner.Weight += slots[i].ItemData.weight;
                    }
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 인벤토리의 특정 슬롯에 특정 아이템을 1개 추가하는 함수
    /// </summary>
    /// <param Name="code">추가할 아이템의 코드</param>
    /// <param Name="slotIndex">아이템을 추가할 슬롯의 인덱스</param>
    /// <returns>true면 성공 false면 추가 실패</returns>
    public bool AddItem(ItemCode code, uint slotIndex)
    {
        bool result = false;

        if (IsValidIndex(slotIndex))    // 인덱스가 적절한지 확인
        {
            ItemData data = itemDataManager[code];
            ItemSlot slot = slots[slotIndex];      // 슬롯 가져오기
            if (slot.IsEmpty)
            {
                // 슬롯이 비었으면
                slot.AssignSlotItem(data);          // 그대로 아이템 설정
                result = true;
            }
            else
            {
                // 슬롯이 안비어있다.
                if (slot.ItemData == data)
                {
                    // 같은 종류의 아이템이면
                    result = slot.SetSlotCount(out _);   // 아이템 증가 시도
                }
                else
                {
                    // 다른 종류의 아이템
                }
            }
        }
        else
        {
            // 잘못된 슬롯
        }
        return result;
    }

    public void AddItem(ItemCode code, int count)
    {
        for (int i = 0; i <= count; i++)
        {
            AddItem(code);
        }
    }

    public void RemoveItem(uint slotIndex, uint count = 1)
    {
        {
            ItemSlot slot = slots[slotIndex];
            MinusValue(slot, (int)count);
            slot.DecreaseSlotItem(count);
        }
    }

    /// <summary>
    /// 특정 아이템을 count만큼 인벤토리에서 제거하는 함수
    /// </summary>
    /// <param name="code">제거할 아이템</param>
    /// <param name="count">제거할 개수</param>
    /// <returns>제거한 아이템의 수</returns>
    public int RemoveItem(ItemCode code, uint count = 1)
    {
        int result = 0;
        for (uint i = 0; i < SlotCount; i++)
        {
            // 슬롯이 안 비어있다.
            if (!slots[i].IsEmpty /*&& slots[i].ItemData.bulletType == code*/)
            {
                ItemSlot slot = slots[i];      // 슬롯 가져오기
                if(slot.ItemData.itemId  == code)
                {
                    if (slot.ItemCount < count)
                    {
                        // 필요한 총알 수 보다 슬롯에 있는 총알이 적다.
                        result += (int)slot.ItemCount;
                        MinusValue(slot, result);
                        count -= slot.ItemCount;
                        slot.DecreaseSlotItem(slot.ItemCount);
                    }
                    else
                    {
                        // 필요한 총알 수 보다 슬롯에 있는 총알이 많거나 같다.
                        result += (int)count;
                        MinusValue(slot, (int)count);
                        slot.DecreaseSlotItem(count);
                        break;
                    }
                }
                else
                {
                    result = 0;
                    Debug.Log("아이템 제거 실패");
                }
            }
        }
        Debug.Log(result);
        return result;
    }
    

    public void MoveItem(ItemSlot from, ItemSlot to)
    {
        // from 지점과 to지점은 서로 다른 위치이고 모두 valid한 슬롯이어야 한다.
        if ((from != to) && IsValidIndex(from) && IsValidIndex(to))
        {
            bool fromIsTemp = (from == dragSlot);
            ItemSlot fromSlot = fromIsTemp ? DragSlot : from;   // from슬롯 가져오기

            if (!fromSlot.IsEmpty)
            {
                // from에 아이템이 있다.
                ItemSlot toSlot = null;


                if (to == dragSlot)
                {
                    // to가 drag슬롯이면
                    toSlot = DragSlot;      // 슬롯 저장하고
                    DragSlot.SetFromIndex(fromSlot.Index);  // 드래고 시작 슬롯 저장(fromIndex)
                }
                else
                {
                    toSlot = to;        // drag슬롯이 아니면 슬롯만 저장
                }

                if (fromSlot.ItemData == toSlot.ItemData)
                {
                    // 같은 종류의 아이템 => to에 채울 수 있는데까지 채운다. to에 넘어간 만큼 from을 감소시킨다.
                    toSlot.SetSlotCount(out uint overCount, fromSlot.ItemCount);    // from이 가진 개수만큼 to 추가
                    fromSlot.DecreaseSlotItem(fromSlot.ItemCount - overCount);      // from에서 to로 넘어간 개수만큼만 감소
                }
                else
                {
                    if (fromIsTemp)
                    {
                        ItemSlot returnSlot = slots[DragSlot.FromIndex];

                        if (returnSlot.IsEmpty)
                        {
                            returnSlot.AssignSlotItem(toSlot.ItemData, toSlot.ItemCount, toSlot.IsEquiped);
                            toSlot.AssignSlotItem(DragSlot.ItemData, DragSlot.ItemCount, DragSlot.IsEquiped);
                            DragSlot.ClearSlot();
                        }
                        else
                        {
                            if (returnSlot.ItemData == toSlot.ItemData)
                            {
                                MoveItem(toSlot, returnSlot);
                                returnSlot.SetSlotCount(out uint overCount, toSlot.ItemCount);
                                toSlot.DecreaseSlotItem(toSlot.ItemCount - overCount);
                            }
                            SwapSlot(dragSlot, toSlot);
                        }
                    }
                    else
                    {
                        SwapSlot(fromSlot, toSlot);
                    }
                }
            }
        }
    }



    public void SwapSlot(ItemSlot slotA, ItemSlot slotB)
    {
        ItemData dragData = slotA.ItemData;
        uint dragCount = slotA.ItemCount;
        bool isEquipped = slotA.IsEquiped;

        slotA.AssignSlotItem(slotB.ItemData, slotB.ItemCount, slotB.IsEquiped);
        slotB.AssignSlotItem(dragData, dragCount, isEquipped);
    }

    public void SlotSorting(ItemType type, bool isAcending)
    {
        // 정렬을 위한 임시 리스트
        List<ItemSlot> temp = new List<ItemSlot>(slots);  // slots를 기반으로 리스트 생성

        // 정렬방법에 따라 임시 리스트 정렬
        switch (type)
        {
            case ItemType.Buff:
                temp.Sort((current, other) =>       // current, y는 temp리스트에 들어있는 요소 중 2개
                {
                    if (current.ItemData == null)   // 비어있는 슬롯을 뒤쪽으로 보내기
                        return 1;
                    if (other.ItemData == null)
                        return -1;
                    if (isAcending)                 // 오름차순/내림차순에 따라 처리  
                    {
                        return current.ItemData.itemType.CompareTo(other.ItemData.itemType);
                    }
                    else
                    {
                        return other.ItemData.itemType.CompareTo(current.ItemData.itemType);
                    }
                });
                break;
        }
        // 임시 리스트의 내용을 슬롯에 설정
        List<(ItemData, uint)> sortedData = new List<(ItemData, uint)>(SlotCount);  // 튜플 사용
        foreach (var slot in temp)
        {
            sortedData.Add((slot.ItemData, slot.ItemCount));    // 필요 데이터만 복사해서 가지기
        }

        int index = 0;
        foreach (var data in sortedData)
        {
            slots[index].AssignSlotItem(data.Item1, data.Item2);     // 복사한 내용을 슬롯에 설정
            index++;
        }
    }

    public void ClearInventory()
    {
        foreach(var slot in slots)
        {
            slot.ClearSlot();
            MinusValue(slot, (int)slot.ItemCount);
        }
        Owner.Weight = 0.0f;
        
    }

    /// <summary>
    /// 인벤토리에 있는 모든 아이템을 버리는 함수
    /// </summary>
    public void DropAllItems()
    {
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                RemoveItem(slot.ItemData.itemId, slot.ItemCount);
                // World.DropItem(slot.ItemData, slot.ItemCount, owner.transform.position);
            }
        }
    }
    /// <summary>
    /// 슬롯 인덱스가 적절한 인덱스인지 확인하는 함수
    /// </summary>
    /// <param Name="invenFromIndex">확인할 인덱스</param>
    /// <returns>true면 적절한 인덱스, false면 잘못된 인덱스</returns>
    bool IsValidIndex(uint index)
    {
        return (index < SlotCount) || (index == dragSlotIndex);
    }

    bool IsValidIndex(ItemSlot slot)
    {
        return (slot != null) || (slot == dragSlot);
    }



    public void MergeItems()
    {
        uint count = (uint)slots.Length - 1;
        for (uint i = 0; i < count; i++)
        {
            ItemSlot target = slots[i];
            for (uint j = count - 1; j > i; j--)
            {
                // 같은 종류의 아이템이면
                if (target.ItemData == slots[j].ItemData)
                {
                    MoveItem(slots[j], target);     // j에 있는 것을 i에 넣기

                    if (!slots[j].IsEmpty)
                    {
                        // 남은 아이템이 있으면 i의 다음 슬롯과 교체하기
                        SwapSlot(slots[i + 1], slots[j]);
                        break;
                    }
                }
            }
        }
    }

    public void MinusValue(ItemSlot slot, int count)
    {
        if (slot.ItemData != null)
        {
            if(Owner != null)
            Owner.Weight -= (int)(slot.ItemData.weight * count);
        }
    }

    /// <summary>
    /// 인벤토리 내부에서 특정 한 아이템을 찾는 함수
    /// </summary>
    /// <param name="itemType">특정한 아이템의 타입</param>
    /// <returns>true면 아이템이 인벤토리 내부에 있다, false면 없다.</returns>
    public bool FindItem(ItemCode code)
    {
        bool result = false;
        for (int i = 0; i < SlotCount; i++)
        {
            if (!slots[i].IsEmpty && slots[i].ItemData.itemId == code)
            {
                Debug.Log("열쇠 확인");
                //slots[i].DecreaseSlotItem();
                result = true;
                break;
            }
            else
            {
                //Debug.Log("열쇠 찾지 못함");
            }
        }
        return result; // 루프를 끝까지 돌았는데도 아이템을 찾지 못했으면 false 반환
    }

    /// <summary>
    /// 아이템을 구매할 수 있는지 확인하는 함수
    /// </summary>
    /// <param name="code">구매할 아이템</param>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool BuyPosible(ItemCode code, int count) 
    {
        bool result = false;
        int itemCount = 0;
        foreach(ItemSlot slot in slots)
        {
            if(slot.ItemData.itemId == code)
            {
                itemCount += (int)slot.ItemCount;
            }
        }
        if ( count > itemCount)
        {
            return true;
        }

        return result;
    }

    /// <summary>
    /// 총알을 장전하는 함수
    /// </summary>
    /// <param name="type">장전할 총알의 타입</param>
    /// <param name="count">장전할 총알의 개수</param>
    public void Reload(ItemCode type, int count)
    {
        onReload?.Invoke(RemoveItem(type, (uint)count));
    }

    /// <summary>
    /// 플레이어가 사망하거나 중간에 나갔을 때 인벤토리를 비우는 함수
    /// </summary>
    public void GameOver()
    {
        ClearInventory();
    }

    public void SaveInventoryToJson()
    {
        List<ItemSlotData> slotDataList = new List<ItemSlotData>();
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty)
            {
                slotDataList.Add(new ItemSlotData()
                {
                    ItemCode = slot.ItemData.itemId,
                    ItemCount = slot.ItemCount,
                    IsEquipped = slot.IsEquiped  // IsEquipped 프로퍼티가 ItemSlot에 존재해야 합니다
                });
            }
        }

        string json = JsonConvert.SerializeObject(slotDataList, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "inventory.json"), json);
        Debug.Log("유저 인벤토리를 저장했습니다.");
    }

    public void LoadInventoryFromJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "inventory.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            List<ItemSlotData> slotDataList = JsonConvert.DeserializeObject<List<ItemSlotData>>(json);
            ClearInventory();  // 기존 인벤토리 클리어

            foreach (var slotData in slotDataList)
            {
                ItemData itemData = itemDataManager.GetItemDataByCode(slotData.ItemCode);
                ItemSlot slot = GetEmptySlot();  // 빈 슬롯 찾기 메서드 구현 필요
                if (slot != null)
                {
                    slot.AssignSlotItem(itemData, slotData.ItemCount, slotData.IsEquipped);
                }
            }
            Debug.Log("유저 인벤토리의 저장 데이터를 불러옵니다.");
        }
        else
        {
            Debug.LogWarning("저장된 유저 인벤토리 데이터가 없습니다.");
        }
    }
    public ItemSlot GetEmptySlot()
    {
        // 모든 슬롯을 순회하며 비어 있는 슬롯을 찾음
        foreach (var slot in slots)
        {
            if (slot.IsEmpty)
            {
                return slot;
            }
        }
        return null; // 비어 있는 슬롯이 없으면 null 반환
    }


#if UNITY_EDITOR
    public void Test_InventoryPrint()
    {
        string invenInfo = "";
        string dataName = "";
        foreach (var slot in slots)
        {

            if (slot.ItemData != null)
            {
                if (slot.ItemData.itemType == ItemType.Buff) dataName = "버프아이템";

                else if (slot.ItemData.itemType == ItemType.Grenade) dataName = "투척무기";

                else if (slot.ItemData.itemType == ItemType.Bullet) dataName = "총기";

                else if (slot.ItemData.itemType == ItemType.Trap) dataName = "함정";

                else if (slot.ItemData.itemType == ItemType.Key) dataName = "열쇠";

                else if (slot.ItemData.itemType == ItemType.Price) dataName = "화폐";

                invenInfo += $"{dataName}({slot.ItemCount}/{slot.ItemData.maxItemCount}) ";
            }
            else
            {
                invenInfo += "(빈칸) ";
            }
        }
        Debug.Log($"[{invenInfo}]");
    }



#endif

}
