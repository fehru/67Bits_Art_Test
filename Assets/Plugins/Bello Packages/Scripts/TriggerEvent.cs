using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Bello.Unity;

public class TriggerEvent : MonoBehaviour
{
    public enum TriggerCompareType
    {
        CollisionLayer,
        CollisionTag,
    }
    public enum TriggerType
    {
        OnTriggerEnter,
        OnTriggerExit,
    }
    [System.Serializable]
    public class ObjectEvent : UnityEvent<GameObject> { }
    [System.Serializable]
    public class TriggerEventAction
    {
        public string name;
        [Header("Trigger Objects Settings")]
        [Space(5)]
        public TriggerType triggerType;
        [Space(15)]
        public TriggerCompareType triggerCompareType;
        public LayerMask collisionLayers;
        [TagSelector] public string collisionTag;

        [Header("Trigger Activation Settings")]
        [Space(5)]
        public float StartDelay;
        public float LoopDelay;
        public int LoopCount;

        [Header("Trigger Events")]
        [Space(5)]
        public ObjectEvent triggerEvents;
    }
    [field: SerializeField] public TriggerEventAction[] Events { get; private set; }

    CancellationTokenSource cancelTrigger = new CancellationTokenSource();

    private void OnTriggerEnter(Collider col)
    {
        foreach (TriggerEventAction Event in Events)
        {
            if (Event.triggerType != TriggerType.OnTriggerEnter) continue;
            switch (Event.triggerCompareType)
            {
                case TriggerCompareType.CollisionLayer:
                    if (Event.collisionLayers.Include(col.gameObject.layer)) TriggerEnterEvents(col.gameObject, Event);
                    break;
                case TriggerCompareType.CollisionTag:
                    if (col.CompareTag(Event.collisionTag)) TriggerEnterEvents(col.gameObject, Event);
                    break;
            }
        }
    }
    private void OnTriggerExit(Collider col)
    {
        foreach (TriggerEventAction Event in Events)
        {
            if (Event.triggerType != TriggerType.OnTriggerExit) continue;
            switch (Event.triggerCompareType)
            {
                case TriggerCompareType.CollisionLayer:
                    if (Event.collisionLayers.Include(col.gameObject.layer)) TriggerExitEvents(col.gameObject, Event);
                    break;
                case TriggerCompareType.CollisionTag:
                    if (col.CompareTag(Event.collisionTag)) TriggerExitEvents(col.gameObject, Event);
                    break;
            }
        }
    }
    public async void TriggerEnterEvents(GameObject contextObject, TriggerEventAction Event)
    {
        int loopCount = 0;
        await Task.Delay((int)(Event.StartDelay * 1000));
        do
        {
            if (cancelTrigger.IsCancellationRequested) return;
            Event.triggerEvents.Invoke(contextObject);
            loopCount++;
            await Task.Delay((int)(Event.LoopDelay * 1000));
        } while (loopCount <= Event.LoopCount && !cancelTrigger.IsCancellationRequested);
    }
    public async void TriggerExitEvents(GameObject contextObject, TriggerEventAction Event)
    {
        int loopCount = 0;
        await Task.Delay((int)(Event.StartDelay * 1000));
        do
        {
            if (cancelTrigger.IsCancellationRequested) return;
            Event.triggerEvents.Invoke(contextObject);
            loopCount++;
            await Task.Delay((int)(Event.LoopDelay * 1000));
        } while (loopCount <= Event.LoopCount && !cancelTrigger.IsCancellationRequested);
    }

    public void DestroyOnTrigger(GameObject context)
    {
        Destroy(context);
    }
    public void DesactiveOnTrigger(GameObject context)
    {
        context.SetActive(false);
    }

    private void OnDisable()
    {
        cancelTrigger.Cancel();
    }
}

