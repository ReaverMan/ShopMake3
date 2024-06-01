using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class EquipmentData
{
    public Equipment equipmentType;
    public Vector3 position;
    public Quaternion rotation;
}

public class SaveLoadManager : MonoBehaviour
{
    private string savePath;

    private void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "equipment.json");
    }

    public void SaveEquipments(List<EquipmentData> equipments)
    {
        string json = JsonUtility.ToJson(new Wrapper<EquipmentData> { Items = equipments });
        File.WriteAllText(savePath, json);
    }

    public List<EquipmentData> LoadEquipments()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            Wrapper<EquipmentData> wrapper = JsonUtility.FromJson<Wrapper<EquipmentData>>(json);
            return wrapper.Items;
        }

        return new List<EquipmentData>();
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}
