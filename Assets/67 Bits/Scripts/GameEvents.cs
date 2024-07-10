using System;
using UnityEngine;
using UnityEngine.Events;

public class GameEvents : MonoBehaviour {


    [SerializeField] private GameEvent[] gameEvents;


    [System.Serializable]
    public class GameEvent
    {
        [HideInInspector] public string Name; 
        public GameManager.GameEvent eventType;
        public UnityEvent events;
    }


    private void Start()
    {
        if(GameManager.Instance)
            foreach (GameEvent gameEvent in gameEvents)
                GameManager.GameEvents[gameEvent.eventType.GetHashCode()] += gameEvent.events.Invoke;
    }

    private void OnDestroy()
    {
        foreach (GameEvent gameEvent in gameEvents)
        {
            if(GameManager.GameEvents[gameEvent.eventType.GetHashCode()] is var currentEvent)
                currentEvent -= gameEvent.events.Invoke;
        }
    }

    public void StartAllEvents()
    {
        try
        {
            foreach (GameEvent gameEvent in gameEvents) gameEvent.events.Invoke();
            Debug.Log(gameObject.name + " started All it's events");
        }
        catch (Exception e)
        {
            Debug.LogError($"'{gameObject.name}' couldn't start events because: " + e);
        }
    }
    public void StartAllEventsTypes()
    {
        try
        {
            foreach (GameEvent gameEvent in gameEvents)
                GameManager.PlayEvent(gameEvent.eventType);
        }
        catch (Exception e)
        {
            Debug.LogError($"'{gameObject.name}' couldn't start event types because: " + e);
        }
    }
    private void OnValidate()
    {
        if (gameEvents != null)
            foreach (GameEvent gameEventType in gameEvents)
                gameEventType.Name = gameEventType.eventType.ToString();
    }
}
