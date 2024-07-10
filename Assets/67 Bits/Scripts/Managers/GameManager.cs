// Game Manager will allways run before all Default time scripts.
// You can check it in execution order settings!

using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public static Action[] GameEvents;
    public static Camera MainCamera
    {
        get
        {
            _mainCamera ??= Camera.main;
            return _mainCamera;
        }
    }
    private static Camera _mainCamera;
    public static CinemachineVirtualCamera CinemachineVirtualCamera
    {
        get
        {
            if (!_cinemachineCamera) _cinemachineCamera = FindObjectOfType<CinemachineVirtualCamera>();
            return _cinemachineCamera;
        }
        set { _cinemachineCamera = value; }
    }
    private static CinemachineVirtualCamera _cinemachineCamera;
    public enum GameEvent
    {
        GameStart,
        GameFinish,
        GameFail,
        GameWin,
        GamePaused
    }
    private static GameSettings _gameSettings;
    public GameSettings GameSettings
    {
        get
        {
            if (_gameSettings == null)
            {
                GameSettings[] gameSettings = Resources.LoadAll<GameSettings>("Data");
                if (gameSettings == null || gameSettings.Length < 1) return null;
                _gameSettings = gameSettings[0];
            }
            return _gameSettings;
        }
    }
    public static int SceneId => SceneManager.GetActiveScene().buildIndex;

    public static Transform PlayerTransform { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeGame()
    {
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;
    }
    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        SetEventsList();
    }
    private void OnDisable()
    {
        if (Instance == this)
            SetEventsList();
    }
    /// <summary>
    /// Generate a string id for a point in the world in the current scene
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static int GetFullId(Vector3 position)
    {
        string id = $"{SceneManager.GetActiveScene().buildIndex}{Mathf.Abs(position.x).ToString("00")}{Mathf.Abs(position.z).ToString("00")}";
        return int.Parse(id);
    }
    #region Events
    public static void SetEventsList()
    {
        GameEvents = new Action[Enum.GetValues(typeof(GameEvent)).Length];
    }
    /// <summary>
    /// Call only on start, events are itinialized on Awake
    /// </summary>
    /// <param name="gameEvent"></param>
    public static void PlayEvent(GameEvent gameEvent)
    {
        GameEvents[gameEvent.GetHashCode()]?.Invoke();
    }
    public static Action GetEvent(GameEvent gameEvent) => GameEvents[gameEvent.GetHashCode()];
    #endregion
}