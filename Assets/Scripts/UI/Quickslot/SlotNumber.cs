using System;
using UnityEngine;

public class SlotNumber : MonoBehaviour
{
    QuickType[] quickTypes;
    Player player;

    // 무기 변경을 알리는 이벤트
    public event Action<WeaponBase> OnWeaponChanged;

    private void Awake()
    {
        quickTypes = GetComponentsInChildren<QuickType>();
        player = GetComponentInParent<Player>();
    }

    public bool SwapItem(Equipment equip, Transform obj, bool isEquip)
    {
        bool result = true;
        WeaponBase newWeapon = null;

        if (isEquip)
        {
            if (obj.GetChild(0) != null)
            {
                switch (player.Equipment)
                {
                    case Equipment.Gun:
                        obj.GetChild(0).parent = quickTypes[0].transform;
                        quickTypes[0].transform.GetChild(0).position = transform.position;
                        break;
                    case Equipment.Throw:
                        obj.GetChild(0).parent = quickTypes[3].transform;
                        quickTypes[3].transform.GetChild(0).position = transform.position;
                        break;
                    case Equipment.ETC:
                        obj.GetChild(0).parent = quickTypes[5].transform;
                        quickTypes[5].transform.GetChild(0).position = transform.position;
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            else
            {
                Debug.Log("장비 해제 실패");
            }

            switch (equip)
            {
                case Equipment.Gun:
                    if (quickTypes[0].transform.childCount > 0)
                    {
                        newWeapon = quickTypes[0].transform.GetChild(0).GetComponent<WeaponBase>();
                        quickTypes[0].transform.GetChild(0).parent = obj;
                        player.Equipment = quickTypes[0].Equipment;
                        obj.GetChild(0).position = obj.position;
                    }
                    break;
                case Equipment.Throw:
                    if (quickTypes[3].transform.childCount > 0)
                    {
                        quickTypes[3].transform.GetChild(0).parent = obj;
                        player.Equipment = quickTypes[3].Equipment;
                        obj.GetChild(0).position = obj.position;
                    }
                    break;
                case Equipment.ETC:
                    if (quickTypes[5].transform.childCount > 0)
                    {
                        quickTypes[5].transform.GetChild(0).parent = obj;
                        player.Equipment = quickTypes[5].Equipment;
                        obj.GetChild(0).position = obj.position;
                    }
                    break;
                default:
                    result = false;
                    break;
            }

            result = true;
        }
        else
        {
            switch (equip)
            {
                case Equipment.Gun:
                    if (quickTypes[0].transform.childCount > 0)
                    {
                        newWeapon = quickTypes[0].transform.GetChild(0).GetComponent<WeaponBase>();
                        quickTypes[0].transform.GetChild(0).parent = obj;
                        player.Equipment = quickTypes[0].Equipment;
                        obj.GetChild(0).position = obj.position;
                    }
                    break;
                case Equipment.Throw:
                    if (quickTypes[3].transform.childCount > 0)
                    {
                        quickTypes[3].transform.GetChild(0).parent = obj;
                        player.Equipment = quickTypes[3].Equipment;
                        obj.GetChild(0).position = obj.position;
                    }
                    break;
                case Equipment.ETC:
                    if (quickTypes[5].transform.childCount > 0)
                    {
                        quickTypes[5].transform.GetChild(0).parent = obj;
                        player.Equipment = quickTypes[5].Equipment;
                        obj.GetChild(0).position = obj.position;
                    }
                    break;
                default:
                    result = false;
                    break;
            }
        }

        if (newWeapon != null)
        {
            OnWeaponChanged?.Invoke(newWeapon);
        }

        return result;
    }

    public bool AddItem(GameObject obj, Equipment type)
    {
        bool result = false;

        for (int i = 0; i < quickTypes.Length; i++)
        {
            if (type == quickTypes[i].Equipment && (quickTypes[i].IsEmpty || quickTypes[i].equipment == Equipment.ETC))
            {
                Instantiate(obj, quickTypes[i].transform);
                result = true;
                break;
            }
        }

        return result;
    }
}
