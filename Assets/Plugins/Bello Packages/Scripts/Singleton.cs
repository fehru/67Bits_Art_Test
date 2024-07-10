using UnityEngine;
/// <summary>
/// This class uses a public static "Main Instance" to perform code, similar to default "Singletons", 
/// but does not require the object to be the only instance in the scene.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour where T : Component
{
    [Header("Singleton Settings")]
    [Space(5)]
    [Tooltip("Should this object be destroyed if there is another in the main instance?")]
    [SerializeField] private bool _updateName = false;
    [SerializeField] private bool _destroyIfNotMain;
    [SerializeField] private bool dontDestroyOnLoad;

    private static T instance;
    public static T Instance
    {
        get
        {
            if (!instance && Application.isPlaying)
            {
                instance = FindObjectOfType<T>();
                if (!instance)
                {
                    GameObject gameObject = new GameObject($"{typeof(T).Name} (Singleton)");
                    instance = gameObject.AddComponent<T>();
                    return instance;
                }
            }
            return instance;
        }
        set { }
    }
    protected virtual void Awake()
    {
        if (!instance) instance = this as T;
        else if (instance.gameObject != gameObject)
        {
            if (_destroyIfNotMain)
                Destroy(gameObject);
            else enabled = false;
            return;
        }
        if (_updateName) gameObject.name = $"{Instance.GetType().Name} (Main Singleton)";
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }
    private static T CreateNewInstance()
    {
        return null;
    }
}
