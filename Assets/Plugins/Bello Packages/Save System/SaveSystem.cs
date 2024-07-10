using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using System;

public class SaveSystem : Singleton<SaveSystem>
{
    private static SaveSettings _saveSettings;
    public static SaveSettings SaveSettings
    {
        get
        {
            if (_saveSettings == null)
            {
                SaveSettings[] savesSettings = Resources.LoadAll<SaveSettings>("");
                if (savesSettings == null || savesSettings.Length < 1) return null;
                _saveSettings = savesSettings[0];
            }
            return _saveSettings;
        }
    }
    #region Save and Load Game
    public static void SaveGame()
    {
#if UNITY_EDITOR
        if (!SaveSettings.EnableSavingInEditor) return;
#endif
        if (SaveSettings.GoToLastSaveSceneWhenLoad) SaveData.Instance.lastScene = SceneManager.GetActiveScene().name;

        SaveDataFile(SaveData.Instance);
        Debug.Log("Game Saved");
    }
    public static void LoadGame()
    {
#if UNITY_EDITOR
        if (!SaveSettings.EnableLoadingInEditor) return;
#endif
        if (SaveSettings.LinkToPlayerPrefs && !PlayerPrefs.HasKey("SaveData")) return;

        var save = (SaveData)LoadDataFile();
        if (save == null) return;

        SaveData.Instance = save;

        if (SaveSettings.GoToLastSaveSceneWhenLoad &&
            SaveData.Instance.lastScene != null && SceneManager.GetSceneByName(SaveData.Instance.lastScene) != null)
            SceneManager.LoadScene(SaveData.Instance.lastScene);
    }
    public static void SaveGameJson()
    {
#if UNITY_EDITOR
        if (!SaveSettings.EnableSavingInEditor) return;
#endif
        if (SaveSettings.GoToLastSaveSceneWhenLoad) SaveData.Instance.lastScene = SceneManager.GetActiveScene().name;

        JsonSaveDataFile(SaveData.Instance);
        Debug.Log("Game Saved");
    }
    public static void LoadGameJson()
    {
#if UNITY_EDITOR
        if (!SaveSettings.EnableLoadingInEditor) return;
#endif
        if (SaveSettings.LinkToPlayerPrefs && !PlayerPrefs.HasKey("SaveDataJson")) return;

        var save = (SaveData)JsonLoadDataFile();
        if (save == null) return;

        SaveData.Instance = save;

        if (SaveSettings.GoToLastSaveSceneWhenLoad &&
            SaveData.Instance.lastScene != null && SceneManager.GetSceneByName(SaveData.Instance.lastScene) != null)
            SceneManager.LoadScene(SaveData.Instance.lastScene);
    }
    #endregion
    #region Save and Load Custom JSON
    public static void SaveGameCustomJson(object saveData)
    {
#if UNITY_EDITOR
        if (!SaveSettings.EnableSavingInEditor) return;
#endif
        var className = saveData.GetType().Name;
        CustomSaveDataFile(saveData);
        Debug.Log($"{className} Data Saved");
    }
    public static T LoadCustomJson<T>(T loadData)
    {
        var className = loadData.GetType().Name;
#if UNITY_EDITOR
        if (!SaveSettings.EnableLoadingInEditor) return default;
#endif
        if (SaveSettings.LinkToPlayerPrefs && !PlayerPrefs.HasKey(className)) return default;

        var save = CustomLoadDataFile(loadData);
        if (save == null) return default;

        loadData = (T)save;
        return loadData;
    }
    #endregion
    public static bool SaveDataFile(object saveData)
    {
        string data = JsonUtility.ToJson((SaveData)saveData);
        PlayerPrefs.SetString("SaveData", data);
        return true;
    }
    public static object LoadDataFile()
    {
        string dataJson = PlayerPrefs.GetString("SaveData", "");
        object data = JsonUtility.FromJson<SaveData>(dataJson);
        Debug.Log("Game Loaded");
        return data;
    }
    public static bool JsonSaveDataFile(object saveData)
    {
        try
        {
            string data = JsonConvert.SerializeObject((SaveData)saveData);
            PlayerPrefs.SetString("SaveDataJson", data);
            return true;
        }
        catch (Exception ex) { Debug.LogException(ex); }
        return false;
    }
    public static object JsonLoadDataFile()
    {
        try
        {
            string dataJson = PlayerPrefs.GetString("SaveDataJson", "");
            object data = JsonConvert.DeserializeObject<SaveData>(dataJson);
            Debug.Log("Game Loaded");
            return data;
        }
        catch (Exception ex) { Debug.LogException(ex); return false; }
    }
    public static bool CustomSaveDataFile<T>(T saveData)
    {
        try
        {
            var className = saveData.GetType().Name;
            string data = JsonConvert.SerializeObject(saveData);
            PlayerPrefs.SetString(className, data);
            return true;
        }
        catch (Exception ex) { Debug.LogException(ex); }
        return false;
    }
    public static object CustomLoadDataFile<T>(T loadData)
    {
        try
        {
            var className = loadData.GetType().Name;
            string dataJson = PlayerPrefs.GetString(className, "");
            object data = JsonConvert.DeserializeObject<T>(dataJson);
            Debug.Log($"{className} Data Loaded");
            return data;
        }
        catch (Exception ex) { Debug.LogException(ex); return false; }
    }

    #region OnApplication
    /// <summary>
    /// Runs on start up, before the first scene, creates the firt instance of SaveSystem and/or Load the gamene
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnApplicationInicialization()
    {
        if (SaveSettings.AutoStart) DontDestroyOnLoad(SaveSystem.Instance);
        if (SaveSettings.LoadAtApplicationStart) SaveSystem.LoadGameJson();
    }
    private static void OnApplicationPause(bool pause)
    {
        if (SaveSettings.SaveWhenPause) SaveGame();
    }
    private static void OnApplicationQuit()
    {
        if (SaveSettings.SaveWhenQuit) SaveGame();
    }
    #endregion
}
