using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_UI : MonoBehaviour
{
    Inventory inventory;

    public Inventory Inventory => inventory;

    Slot_UI[] slotsUI;

    DropSlotUI dropSlot;

    WeightPanel_UI weightPanel;

    InventoryManager invenManager;

    RectTransform invenTransform;

    Button sortButton;

    LocalSelectMeunUI selectMenu;

    Player Owner => inventory.Owner;

    CanvasGroup canvas;

    Equip_UI equip;


    /// <summary>
    /// 인벤토리 돈 관리하는 프로퍼티인데 나중에 아이템 개수로 바꿀 예정
    /// </summary>


    /// <summary>
    /// 인벤토리의 무게에 변화가 있으면 실행되는 델리게이트
    /// </summary>
    public Action<int> onWeightChange;

    /// <summary>
    /// 인벤토리의 돈에 변화가 있으면 실행되는 델리게이트
    /// </summary>
    public Action<int> onMoneyChange;

    private void Awake()
    {

        Transform child = transform.GetChild(0);
        slotsUI = child.GetComponentsInChildren<Slot_UI>();

        child = transform.GetChild(1);
        dropSlot = child.GetComponent<DropSlotUI>();

        child = transform.GetChild(2);
        weightPanel = child.GetComponent<WeightPanel_UI>();

        child = transform.GetChild(3);
        sortButton = child.GetComponent<Button>();
        sortButton.onClick.AddListener(() =>
        {
            OnItemSort(ItemType.Buff);
        });

        child = transform.GetChild(4);
        selectMenu = child.GetComponent<LocalSelectMeunUI>();

        invenManager = GetComponentInParent<InventoryManager>();

        invenTransform = GetComponent<RectTransform>();

        canvas = GetComponent<CanvasGroup>();
    }

    public void InitializeInventory(Inventory playerInventory)
    {
        inventory = playerInventory;

        for (uint i = 0; i < slotsUI.Length; i++)
        {
            slotsUI[i].InitializeSlot(inventory[i]);
            slotsUI[i].onDragBegin += OnItemMoveBegin;
            slotsUI[i].onDragEnd += OnItemMoveEnd;
            slotsUI[i].onRightClick += OnRightClick;
            slotsUI[i].OnClick += OnClick;
        }
        invenManager.DragSlot.InitializeSlot(inventory.DragSlot);  // 임시 슬롯 초기화

        selectMenu.onItemDrop += OnItemDrop;

        selectMenu.onItemEquip += OnItemEquip;
        selectMenu.onItemUnEquip += OnItemUnEquip;
        selectMenu.Close();

        dropSlot.onDropOk += OnDropOk;
        dropSlot.Close();

        if (Owner != null)
        {
            Owner.onWeightChange += weightPanel.Refresh;
            weightPanel.Refresh(Owner.Weight);
        }
        Close();

        //inventory.onReload += GameManager.Instance.WeaponBase.ReLoad;
    }

    private void Start()
    {
        equip = GameManager.Instance.EquipUI;
    }

    public void PlusValue(ItemSlot slot)
    {
        if (Owner != null)
            Owner.Weight += slot.ItemData.weight;
    }

    /// <summary>
    /// 게임이 끝난 이후 로컬인벤토리 정리하고 메인화면으로 나가는 함수
    /// </summary>
    public void InventoryResult()
    {
        inventory.ClearInventory();
        Owner.Weight = 0;

        // 이후에 메인화면으로 나가기
    }



    /// <summary>
    /// 아이템 드래그 시작하면 실행되는 함수
    /// </summary>
    /// <param name="index">시작한 슬롯의 index</param>
    private void OnItemMoveBegin(ItemSlot slot)
    {
        invenManager.DragSlot.InitializeSlot(inventory.DragSlot);  // 임시 슬롯 초기화
        inventory.MoveItem(slot, invenManager.DragSlot.ItemSlot);
        invenManager.DragSlot.Open();
    }



    /// <summary>
    /// 아이템 드래그가 끝이나면 실행되는 함수
    /// </summary>
    /// <param name="index">끝난 슬롯의 index</param>
    private void OnItemMoveEnd(ItemSlot slot, RectTransform rect)
    {
        // if(rect != 장비창의 RectTransform)

        Equip_UI equip_UI;
        equip_UI = rect.GetComponentInParent<Equip_UI>();

        //if (equip_UI != null)
        //{
        inventory.MoveItem(invenManager.DragSlot.ItemSlot, slot);
        //}
        //else
        //{
        //    slot.AssignSlotItem(invenManager.DragSlot.ItemSlot.ItemData, invenManager.DragSlot.ItemSlot.ItemCount, true);
        //    inventory.DragSlot.ClearSlot();
        //}


        // else
        // 장비창이 가지고 있는 함수 실행;
        // 인벤토리에 다시 아이템 가져오고 isEqupped = true 해주기

        WorldInventory_UI worldInven;
        worldInven = rect.GetComponentInParent<WorldInventory_UI>();

        if (worldInven != null && slot != null)
        {
            inventory.MinusValue(slot, (int)slot.ItemCount);
        }

        if (invenManager.DragSlot.ItemSlot.IsEmpty)
        {
            invenManager.DragSlot.Close();
        }

        // 마우스를 땟을 때 위치가 장비창이라면 
        // slot.EquipWeapon();                    장비하고
        // 장비를 장비창에 복사하고(인벤토리에 있는 장비는 그대로 두고)
        // onEquipped?.Invoke(slot);            아이템 슬롯 정보 알려주기
    }

    /// <summary>
    /// 슬롯을 클릭하면 실행되는 함수
    /// </summary>
    /// <param name="index"></param>
    private void OnClick(ItemSlot slot, RectTransform rect)
    {
        if (!invenManager.DragSlot.ItemSlot.IsEmpty)
        {
            OnItemMoveEnd(slot, rect);
        }
    }

    /// <summary>
    /// 슬롯을 우클릭 시 실행되는 함수
    /// </summary>
    /// <param name="index">우클릭한 슬롯의 index</param>
    private void OnRightClick(uint index)
    {
        // 버리기, 상세보기 등 UI따로 띄우기
        Slot_UI target = slotsUI[index];
        selectMenu.Open(target.ItemSlot);
    }

    /// <summary>
    /// 인벤토리를 정렬하는 함수
    /// </summary>
    /// <param name="type">정렬되는 순서</param>
    private void OnItemSort(ItemType type)
    {
        inventory.SlotSorting(type, true);
    }

    /// <summary>
    /// 아이템 버리기 창을 여는 함수
    /// </summary>
    /// <param name="index">아이템을 버릴 슬롯의 인덱스</param>
    private void OnItemDrop(uint index)
    {
        Slot_UI target = slotsUI[index];
        selectMenu.Close();
        dropSlot.Open(target.ItemSlot);
    }

    private void OnItemEquip(ItemSlot slot)
    {
        if (!slot.IsEquiped)
        {

            if (!(slot.ItemData.itemType == ItemType.Buff || slot.ItemData.itemType == ItemType.Grenade))
            {
                if (equip.EquipItem(slot))
                {
                    Owner.SlotNumber.AddItem(slot.ItemData.itemPrefab, slotsUI[slot.Index].Equipment);
                    slot.IsEquiped = true;
                }
                else
                {
                    Debug.Log("장비 아이템 장착 실패");
                }
            }
            else
            {
                if (equip.EquipItem(slot))
                {
                    Owner.SlotNumber.AddItem(slot.ItemData.itemPrefab, slotsUI[slot.Index].Equipment);
                }
                else if (equip.UnEquipItem(slot))
                {
                    Owner.UnEquipped(slot.ItemData.itemType);
                    equip.EquipItem(slot);
                    Owner.SlotNumber.AddItem(slot.ItemData.itemPrefab, slotsUI[slot.Index].Equipment);
                }
                else
                {
                    Debug.Log("사용 아이템 장착 실패");
                }
            }
        }
        selectMenu.Close();
    }
    public void ClearEquip()
    {
        Debug.Log("인벤장비해제");
        if (equip != null)
        {
            equip.AllUnEquip();
            for (int i = 0; i < slotsUI.Length; i++)
            {
                if (slotsUI[i].ItemSlot.IsEquiped)
                {
                    slotsUI[i].ItemSlot.IsEquiped = false;
                }
            }
        }
    }
    public void OnItemUnEquip(ItemSlot slot)
    {
        if (slot.IsEquiped)
        {
            if (Owner.UnEquipped(slot.ItemData.itemType))
            {
                equip.UnEquipItem(slot);
                slot.IsEquiped = false;
            }
            else
            {
                Debug.Log("아이템 해제 실패");
            }
        }
        selectMenu.Close();
    }

    /// <summary>
    /// 버리기 창에서 확인 버튼을 누르면 실행되는 함수
    /// </summary>
    /// <param name="index">아이템을 버릴 슬롯의 index</param>
    /// <param name="count">아이템 버릴 개수</param>
    private void OnDropOk(uint index, uint count)
    {
        ItemSlot slot = slotsUI[index].ItemSlot;

        if (slot.IsEquiped)
        {
            if (inventory.RemoveItem(slot.ItemData.itemId, count) < 1)
            {
                equip.UnEquipItem(slotsUI[index].ItemSlot);
            }
        }
        else
        {
            inventory.RemoveItem(index, count);

        }
        dropSlot.Close();
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

    //public void InventoryOnOff()
    //{
    //    if (canvas.interactable)
    //    {
    //        Close();
    //    }
    //    else
    //    {
    //        Open();
    //    }
    //}
}
