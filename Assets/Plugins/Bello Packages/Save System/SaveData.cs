using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    private static SaveData instance;
    public static SaveData Instance
    {
        get
        {
            if (instance == null) instance = new SaveData();
            return instance;
        }
        set { instance = value; }
    }
    // Store the last loaded scene
    public string lastScene;
    public string saveScene;
    public int playerMoney;
    public int playerScore;
    public int playerUnlockProgress;
    // Button
    public int currentIncomeLevel;

    public List<CustomDataType> objects;
}
[System.Serializable]
public class CustomData
{
    public int id;
    public CustomDataType type;
    public Vector3 position;
    public Vector4 rotation;
    public Vector4 color;
    public CustomData(int id, CustomDataType type, Vector3 position, Vector4 rotation, Vector4 color)
    {
        this.id = id;
        this.type = type;
        this.position = position;
        this.rotation = rotation;
        this.color = color;
    }
}
[System.Serializable]
public enum CustomDataType
{
    player = 0,
    enemy = 1,
    itemA = 2,
    itemB = 3,
}
