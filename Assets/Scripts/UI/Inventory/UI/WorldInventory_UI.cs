using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WorldInventory_UI : MonoBehaviour
{

    WorldInventory worldInven;
    public WorldInventory WorldInven => worldInven;

    Slot_UI[] worldSlotUI;

    InventoryManager invenManager;

    WorldSelectMenuUI worldSelect;

    DropSlotUI worldDropSlot;

    SellItemUI worldSellItem;

    Button sortButton;

    Scrollbar scrollbar;

    MoneyPanel_UI moneyPanel;

    RectTransform worldTransform;

    CanvasGroup canvas;


    int money = 500;

    /// <summary>
    /// 월드 인벤토리 돈 관리하는 프로퍼티인데 나중에 아이템 개수로 바꿀 예정
    /// </summary>
    public int Money
    {
        get => money;
        set
        {
            if (money != value)
            {
                money = value;
                onMoneyChange?.Invoke(money);
            }
        }
    }
    // 데이터를 저장할 구조체 정의
    [System.Serializable]
    public struct WorldInventoryData
    {
        public int money;
    }

    public void SaveWorldInventoryData()
    {
        WorldInventoryData data = new WorldInventoryData
        {
            money = this.Money
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "worldInventoryData.json"), json);
        Debug.Log("월드 인벤토리 데이터를 저장했습니다.");
    }

    public void LoadWorldInventoryData()
    {
        string path = Path.Combine(Application.persistentDataPath, "worldInventoryData.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            WorldInventoryData data = JsonConvert.DeserializeObject<WorldInventoryData>(json);

            this.Money = data.money;

            Debug.Log("월드 인벤토리 데이터를 불러왔습니다.");
        }
        else
        {
            Debug.LogWarning("저장된 월드 인벤토리 데이터가 없습니다.");
        }
    }

    public Action<int> onMoneyChange;

    private void Awake()
    {
        Transform child = transform.GetChild(0);
        worldSlotUI = child.GetComponentsInChildren<Slot_UI>();

        child = transform.GetChild(1);
        worldSelect = child.GetComponent<WorldSelectMenuUI>();

        child = transform.GetChild(2);
        worldDropSlot = child.GetComponent<DropSlotUI>();

        child = transform.GetChild(3);
        worldSellItem = child.GetComponent<SellItemUI>();

        child = transform.GetChild(4);
        moneyPanel = child.GetComponent<MoneyPanel_UI>();

        child = transform.GetChild(5);
        sortButton = child.GetComponent<Button>();
        sortButton.onClick.AddListener(() =>
        {
            OnItemSort(ItemType.Buff);
        });

        scrollbar = GetComponentInChildren<Scrollbar>();

        invenManager = GetComponentInParent<InventoryManager>();

        worldTransform = GetComponent<RectTransform>();

        canvas = GetComponent<CanvasGroup>();
    }

    public void InitializeWorldInventory(WorldInventory playerInventory)
    {
        worldInven = playerInventory;

        for (uint i = 0; i < worldSlotUI.Length; i++)
        {
            worldSlotUI[i].InitializeSlot(worldInven[i]);
            worldSlotUI[i].onDragBegin += OnItemMoveBegin;
            worldSlotUI[i].onDragEnd += OnItemMoveEnd;
            worldSlotUI[i].onRightClick += OnRightClick;
            worldSlotUI[i].OnClick += OnClick;
        }
        invenManager.DragSlot.InitializeSlot(worldInven.DragSlot);  // 임시 슬롯 초기화

        worldSelect.onItemDrop += OnItemDrop;

        worldDropSlot.onDropOk += OnDropOk;
        worldDropSlot.Close();

        worldSelect.onItemSell += OnItemSell;
        worldSellItem.Close();

        worldSellItem.onSellOK += OnSellOk;
        worldSelect.Close();

        onMoneyChange += moneyPanel.Refresh;
        moneyPanel.Refresh(Money);

        scrollbar.value = 1;

        //Close();
    }



    public void PlusValue(ItemSlot slot)
    {
        worldInven.PlusMoney(slot, (int)slot.ItemCount);
    }

    /// <summary>
    /// 아이템 드래그 시작하면 실행되는 함수
    /// </summary>
    /// <param name="slot">시작한 슬롯의</param>
    private void OnItemMoveBegin(ItemSlot slot)
    {
        invenManager.DragSlot.InitializeSlot(worldInven.DragSlot);  // 임시 슬롯 초기화
        worldInven.MoveItem(slot, invenManager.DragSlot.ItemSlot);
        invenManager.DragSlot.Open();
    }

    /// <summary>
    /// 아이템 드래그가 끝이나면 실행되는 함수
    /// </summary>
    /// <param name="slot">끝난 슬롯의</param>
    /// <param name="rect">끝난 슬롯의 RectTransform</param>
    private void OnItemMoveEnd(ItemSlot slot, RectTransform rect)
    {
        worldInven.MoveItem(invenManager.DragSlot.ItemSlot, slot);

        Inventory_UI inven;
        inven = rect.GetComponentInParent<Inventory_UI>();

        if (inven != null && slot != null)
        {
            inven.PlusValue(slot);
        }

        if (invenManager.DragSlot.ItemSlot.IsEmpty)
        {
            invenManager.DragSlot.Close();
        }
    }

    /// <summary>
    /// 슬롯을 클릭하면 실행되는 함수
    /// </summary>
    /// <param name="slot">클릭한 슬롯</param>
    /// /// <param name="rect">끝난 슬롯의 RectTransform</param>
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
        Slot_UI target = worldSlotUI[index];
        worldSelect.Open(target.ItemSlot);
    }

    /// <summary>
    /// 인벤토리를 정렬하는 함수
    /// </summary>
    /// <param name="type">정렬되는 순서</param>
    private void OnItemSort(ItemType type)
    {
        worldInven.SlotSorting(type, true);
    }

    /// <summary>
    /// 아이템 버리기 창을 여는 함수
    /// </summary>
    /// <param name="index">아이템을 버릴 슬롯의 인덱스</param>
    private void OnItemDrop(uint index)
    {
        Slot_UI target = worldSlotUI[index];
        worldSelect.Close();
        worldDropSlot.Open(target.ItemSlot);
    }

    /// <summary>
    /// 아이템 팔기 창을 여는 함수
    /// </summary>
    /// <param name="index">아이템을 판매할 슬롯의 인덱스</param>
    private void OnItemSell(ItemSlot slot)
    {
        // if문을 이용해서 상점 창이 열렸을때만 열리도록 수정하기
        worldSelect.Close();
        if (!(slot.ItemData.itemType == ItemType.Price))    // 판매하려는 아이템이 돈이 아니라면
        {
            worldSellItem.Open(slot);   // 판매 창 열기
        }
    }

    /// <summary>
    /// 버리기 창에서 확인 버튼을 누르면 실행되는 함수
    /// </summary>
    /// <param name="index">아이템을 버릴 슬롯의 index</param>
    /// <param name="count">아이템 버릴 개수</param>
    private void OnDropOk(uint index, uint count)
    {
        worldInven.RemoveItem(index, count);
        worldDropSlot.Close();
    }

    /// <summary>
    /// 판매 창에서 확인 버튼을 누르면 실행되는 함수
    /// </summary>
    /// <param name="slot">아이템을 판매할 슬롯</param>
    /// <param name="count">판매할 아이템의 개수</param>
    private void OnSellOk(ItemSlot slot, uint count)
    {
        worldInven.PlusMoney(slot, (int)count);
        worldInven.RemoveItem(slot.Index, count);
        worldDropSlot.Close();
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