using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Equipment : byte
{
    Gun,
    Throw,
    ETC,
    Vest,
    Helmet,
    BackPack,
    None
}

public class Slot_UI_Base : MonoBehaviour
{
    ItemSlot itemSlot;
    public ItemSlot ItemSlot => itemSlot;

    public Image itemImage; // 아이템의 이미지

    TextMeshProUGUI itemText;

    bool IsEquipment => ItemSlot.ItemData.itemType == ItemType.Gun || ItemSlot.ItemData.itemType == ItemType.Armor
        || ItemSlot.ItemData.itemType == ItemType.Helmet || ItemSlot.ItemData.itemType == ItemType.BackPack;

    public uint Index => itemSlot.Index;

    Equipment equipment = Equipment.None;

    public Equipment Equipment
    {
        get => equipment;
        set
        {
            if (equipment != value)
            {
                equipment = value;
            }
        }
    }

    protected virtual void Awake()
    {
        Transform child = transform.GetChild(0);
        itemImage = child.GetComponent<Image>();
        itemText = GetComponentInChildren<TextMeshProUGUI>();
    }


    /// <summary>
    /// 슬롯을 초기화 하는 함수(=ItemSlot과 SlotUI를 연결)
    /// </summary>
    /// <param name="slot"></param>
    public virtual void InitializeSlot(ItemSlot slot)
    {
        itemSlot = slot;
        itemSlot.onSlotItemChange = Refresh;   // 슬롯의 아이템에 변경이 있을 때 Refresh함수 실행(이전에 연결된 함수는 모두 무시)
        Refresh();      // 첫 화면 갱신
    }

    /// <summary>
    /// 슬롯UI의 화면 갱신
    /// </summary>
    private void Refresh()
    {
        if (ItemSlot.IsEmpty)
        {
            // 비어 있을 때
            itemImage.color = Color.clear;   // 아이콘 투명하게
            itemImage.sprite = null;         // 스프라이트 비우기
            itemText.text = string.Empty;  // 글자도 제거
        }
        else
        {
            // 아이템이 들어있으면
            if (!IsEquipment)
            {
                // 아이템이 장비아이템이 아니면
                itemImage.sprite = ItemSlot.ItemData.itemImage;      // 스프라이트 이미지 설정
                itemImage.color = Color.white;                       // 이미지 보이게 만들기
                itemText.text = ItemSlot.ItemCount.ToString();    // 아이템 개수 쓰기
                itemText.alpha = 1;

            }
            else
            {
                itemImage.sprite = ItemSlot.ItemData.itemImage;      // 스프라이트 이미지 설정
                itemImage.color = Color.white;                       // 이미지 보이게 만들기
                itemText.text = ItemSlot.ItemCount.ToString();    // 아이템 개수 쓰기
                itemText.alpha = 0;


            }

            switch (ItemSlot.ItemData.itemType)
            {
                case ItemType.Gun:
                    Equipment = Equipment.Gun;
                    break;
                case ItemType.Grenade:
                    Equipment = Equipment.Throw;
                    break;
                case ItemType.Armor:
                    Equipment = Equipment.Vest;
                    break;
                case ItemType.Helmet:
                    Equipment = Equipment.Helmet;
                    break;
                case ItemType.BackPack:
                    Equipment = Equipment.BackPack;
                    break;
                case ItemType.Buff:
                    Equipment = Equipment.ETC;
                    break;
            }
        }
        OnRefresh();
    }

    protected virtual void OnRefresh()
    {


    }


}
