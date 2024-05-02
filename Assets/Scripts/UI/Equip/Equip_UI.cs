using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Equip_UI : MonoBehaviour
{
    Equip equip;

    PlayerInput inputActions;

    public Equip Equip => equip;

    [SerializeField] EquipSlot_UI[] equipSlot_UI;

    [SerializeField] DropSlotUI dropSlot;

    [SerializeField] InventoryManager invenManager;

    [SerializeField] RectTransform invenTransform;

    [SerializeField] CanvasGroup canvas;

    public ItemData data01;
    public ItemData data02;
    public ItemData data03;
    public ItemData data04;
    public ItemData data05;

    //Button sortButton;

    Player Owner => equip.Owner;


    ///// <summary>
    ///// 아이템을 장비했다고 알리는 델리게이트(ItemSlot : 장비한 아이템의 슬롯에 대한 정보)
    ///// </summary>
    //public Action<ItemSlot> onEquipped;


    private void Awake()
    {
        inputActions = new PlayerInput();

        Transform child = transform.GetChild(0);
        equipSlot_UI = child.GetComponentsInChildren<EquipSlot_UI>();

        child = transform.GetChild(1);
        dropSlot = child.GetComponent< DropSlotUI>();

        //child = transform.GetChild(2);
        // weightPanel = child.GetComponent<WeightPanel_UI>();

        //child = transform.GetChild(3);
        //sortButton = child.GetComponent<Button>();
        //sortButton.onClick.AddListener(() =>
        //{
        //    // OnItemSort(ItemType.Buff);
        //});

        invenManager = GetComponentInParent<InventoryManager>();

        invenTransform = GetComponent<RectTransform>();

        canvas = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        // 정비창 키
    }



    void OnDisable()
    {
        // 정비창 키
    }

    public void InitializeInventory(Equip playerEquip)
    {
        equip = playerEquip;

        for (uint i = 0; i < equipSlot_UI.Length; i++)
        {
            equipSlot_UI[i].InitializeSlot(equip[i]);
            equipSlot_UI[i].onDragBegin += OnItemMoveBegin;
            equipSlot_UI[i].onDragEnd += OnItemMoveEnd;
            // equipSlot_UI[i].onRightClick += OnRightClick;
            equipSlot_UI[i].OnClick += OnClick;
        }
        invenManager.DragSlot.InitializeSlot(equip.DragSlot);  // 임시 슬롯 초기화

        // dropSlot.onDropOk += OnDropOk;
        dropSlot.Close();

        // Close();
    }

    //private void Start()
    //{
    //    GameManager.Instance.WeaponBase.onReload += equip.Reload;
    //}

    ////public void PlusValue(ItemSlot slot)
    ////{
    ////    //Money += (int)slot.ItemData.Price;
    ////    //Owner.Weight += slot.ItemData.weight;
    ////}

    ///// <summary>
    ///// 게임이 끝난 이후 로컬인벤토리 정리하고 메인화면으로 나가는 함수
    ///// </summary>
    ////public void InventoryResult()
    ////{
    ////    //int tenThousand = Money / 10000;
    ////    //int Thousand = (Money % 10000) / 1000;
    ////    //int hundred = (Money % 1000) / 100;

    ////    //Debug.Log($"10000원 {tenThousand}장 1000원 {Thousand}장 100원 {hundred}개");

    ////    GameManager game = GameManager.Instance;

    ////    //game.WorldInventory_UI.Money += Money;
    ////    equip.ClearInventory();
    ////    //Money = 0;
    ////    //Owner.Weight = 0;

    ////    // 이후에 메인화면으로 나가기
    ////}



    /// <summary>
    /// 아이템 드래그 시작하면 실행되는 함수
    /// </summary>
    /// <param name="index">시작한 슬롯의 index</param>
    private void OnItemMoveBegin(ItemSlot slot)
    {
        invenManager.DragSlot.InitializeSlot(equip.DragSlot);  // 임시 슬롯 초기화
        equip.MoveItem(slot, invenManager.DragSlot.ItemSlot);
        invenManager.DragSlot.Open();
    }



    /// <summary>
    /// 아이템 드래그가 끝이나면 실행되는 함수
    /// </summary>
    /// <param name="index">끝난 슬롯의 index</param>
    private void OnItemMoveEnd(ItemSlot slot, RectTransform rect)
    {
        equip.MoveItem(invenManager.DragSlot.ItemSlot, slot);

        Inventory_UI inven;
        inven = FindObjectOfType<Inventory_UI>();

        if (inven != null)
        {
            //inven.MinusValue(slot, (int)slot.ItemCount);
            //inven.PlusValue(slot);
        }

        if (invenManager.DragSlot.ItemSlot.IsEmpty)
        {
            invenManager.DragSlot.Close();
        }

        // 마우스를 땟을 때 위치가 장비창이라면 
        // slot.EquipItem();                    장비하고
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

    ///// <summary>
    ///// 슬롯을 우클릭 시 실행되는 함수
    ///// </summary>
    ///// <param name="index">우클릭한 슬롯의 index</param>
    //private void OnRightClick(uint index)
    //{
    //    // 버리기, 상세보기 등 UI따로 띄우기
    //    Slot_UI target = slotsUI[index];
    //    dropSlot.Open(target.ItemSlot);
    //}

    ///// <summary>
    ///// 버리기 창에서 확인 버튼을 누르면 실행되는 함수
    ///// </summary>
    ///// <param name="index">아이템을 버릴 슬롯의 index</param>
    ///// <param name="count">아이템 버릴 개수</param>
    ////private void OnDropOk(uint index, uint count)
    ////{
    ////    inventory.RemoveItem(index, count);
    ////    dropSlot.Close();
    ////}

    public void open()
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

    private void InventoryOnOff(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (canvas.interactable)
        {
            Close();
        }
        else
        {
            open();
        }
    }
}
