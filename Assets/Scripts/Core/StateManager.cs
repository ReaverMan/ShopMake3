using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class EquipmentState
{
    public string equipmentName;
    public Vector3 localPosition;
    public Quaternion localRotation;
}

[Serializable]
public class PlayerState
{
    public List<EquipmentState> equippedItems = new List<EquipmentState>();
}

public class StateManager : MonoBehaviour
{
    private string saveFilePath;

    void Awake()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
    }

    public void SaveState(GameObject player)
    {
        PlayerState playerState = new PlayerState
        {
            equippedItems = GetEquippedItems(player)
        };

        string json = JsonUtility.ToJson(playerState, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Player state saved.");
    }

    private List<EquipmentState> GetEquippedItems(GameObject player)
    {
        List<EquipmentState> equippedItems = new List<EquipmentState>();
        foreach (Transform child in player.transform)
        {
            if (child.CompareTag("Equipment"))
            {
                EquipmentState equipmentState = new EquipmentState
                {
                    equipmentName = child.name,
                    localPosition = child.localPosition,
                    localRotation = child.localRotation
                };
                equippedItems.Add(equipmentState);
            }
        }
        return equippedItems;
    }

    public void LoadState(GameObject player)
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerState playerState = JsonUtility.FromJson<PlayerState>(json);

            ClearCurrentEquipment(player);
            LoadEquippedItems(player, playerState.equippedItems);
            Debug.Log("Player state loaded.");
        }
        else
        {
            Debug.LogWarning("Save file not found.");
        }
    }

    private void ClearCurrentEquipment(GameObject player)
    {
        foreach (Transform child in player.transform)
        {
            if (child.CompareTag("Equipment"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void LoadEquippedItems(GameObject player, List<EquipmentState> equippedItems)
    {
        foreach (EquipmentState equipmentState in equippedItems)
        {
            GameObject equipmentPrefab = Resources.Load<GameObject>(equipmentState.equipmentName);
            if (equipmentPrefab != null)
            {
                GameObject equipment = Instantiate(equipmentPrefab, player.transform);
                equipment.transform.localPosition = equipmentState.localPosition;
                equipment.transform.localRotation = equipmentState.localRotation;
                equipment.tag = "Equipment";
            }
            else
            {
                Debug.LogWarning($"Equipment prefab {equipmentState.equipmentName} not found.");
            }
        }
    }
}
