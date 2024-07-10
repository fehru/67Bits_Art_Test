using UnityEngine;

[CreateAssetMenu(menuName = "Save System/ Save Settings", fileName = "SaveSettings")]
public class SaveSettings : ScriptableObject
{
    [field: Header("On Application Start")]
    [field: Space(5)]
    [field: Tooltip("Creates a singleton instance on start, so it tracks when application pauses and quit, even without initialization scene")]
    [field: SerializeField] public bool AutoStart { get; private set; } = true;
    [field: SerializeField] public bool LoadAtApplicationStart { get; private set; }

    [field: Header("Save Data File")]
    [field: Space(5)]
    [field: Tooltip("Don't load data if PlayerPrefs doesn't contain a key for 'SaveData' and store this key when saving")]
    [field: SerializeField] public bool LinkToPlayerPrefs { get; private set; } = true;
    public string SaveDataPath
    {
        get
        {
            if (SaveFileName != null && SaveFileName.Length > 0) return $"{Application.persistentDataPath}/{SaveFileName}";
            else return $"{Application.persistentDataPath}/save.data";
        }
    }
    [field: Tooltip("The name and extension the file should have (EX: 'save.data')")]
    [field: SerializeField] public string SaveFileName { get; private set; } = "save.data";
    [field: Header("Auto Save")]
    [field: Space(5)]
    [field: SerializeField] public bool SaveWhenPause { get; private set; }
    [field: SerializeField] public bool SaveWhenQuit { get; private set; }
    [field: Header("Load")]
    [field: Space(5)]
    [field: SerializeField] public bool GoToLastSaveSceneWhenLoad { get; private set; }
#if UNITY_EDITOR
    [field: Header("Editor")]
    [field: Space(5)]
    [field: SerializeField] public bool EnableSavingInEditor { get; private set; }
    [field: SerializeField] public bool EnableLoadingInEditor { get; private set; }
#endif
}
