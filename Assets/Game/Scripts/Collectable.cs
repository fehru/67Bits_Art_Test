using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
    [SerializeField] private AnimationCurve movementCurve;
    [SerializeField] private float movementSpeed = 2.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onSpawn;
    [SerializeField] private UnityEvent _onStartMove;
    [SerializeField] private UnityEvent _onCollect;

    Vector3 startPoint;
    private void Awake()
    {
        startPoint = transform.position;
        _onSpawn.Invoke();
    }
    public void Collect()
    {
        StopAllCoroutines();
        StartCoroutine(MoveToCollect());
    }
    private IEnumerator MoveToCollect()
    {
        var targetTransform = FindObjectOfType<Player>().transform;
        float lerpValue = 0;

        _onStartMove.Invoke();

        while (lerpValue < 1)
        {
            lerpValue += Time.deltaTime * movementSpeed;
            transform.position = Vector3.Lerp(startPoint, targetTransform.position, movementCurve.Evaluate(lerpValue));
            yield return new WaitForEndOfFrame();
        }
        _onCollect.Invoke();

        yield return new WaitForSeconds(1);
        _onSpawn.Invoke();
        transform.position = startPoint;
    }
}
