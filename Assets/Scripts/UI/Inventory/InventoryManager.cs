using UnityEngine;


// 인벤토리 및 장비창 열고 닫기용 클래스
public class InventoryManager : MonoBehaviour
{
    DragSlotUI dragSlot;
    Inventory_UI inven;
    Equip_UI equip;

    public DragSlotUI DragSlot => dragSlot;

    private void Awake()
    {
        dragSlot = GetComponentInChildren<DragSlotUI>();

        inven = GetComponentInChildren<Inventory_UI>();
        equip = GetComponentInChildren<Equip_UI>();

    }
    public void Open()
    {
        inven.Open();
        equip.Open();

        Debug.Log("인벤토리 켜짐");
    }

    public void Close()
    {
        inven.Close();
        equip.Close();

        Debug.Log("인벤토리 꺼짐");
    }
}
