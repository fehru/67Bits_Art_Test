using UnityEngine;
using Bello.Unity;
[RequireComponent(typeof(InputSystemJoystickSetter))]
public class MobileJoystickInputSystem : MonoBehaviour
{
    private Vector2 startPoint;
    private Vector2 currentPoint;

    [SerializeField] private float maxHandleDistance;
    [SerializeField] private RectTransform joystickBKG;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private RectTransform joystickArea;

    InputSystemJoystickSetter inputSystemJoystickPath;

    private static float currentDistance;

    private void Awake()
    {
#if !UNITY_EDITOR
gameObject.SetActive(Application.isMobilePlatform);
#endif
        inputSystemJoystickPath = GetComponent<InputSystemJoystickSetter>();
    }
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began) startPoint = Input.GetTouch(0).position;
            if (RectTransformUtility.RectangleContainsScreenPoint(joystickArea, startPoint) && !startPoint.IsPointOverUIObject())
            {

                currentPoint = Input.GetTouch(0).position;

                currentDistance = Mathf.Min(Vector2.Distance(startPoint, currentPoint), maxHandleDistance);

                joystickBKG.position = startPoint;
                joystickHandle.position = startPoint + (GetJoystickAxis() * currentDistance);
            }
            else currentDistance = 0;

            if (UnityEngine.Input.GetTouch(0).phase == UnityEngine.TouchPhase.Ended)
            {
                currentPoint = Vector2.zero;
                currentDistance = 0;
                joystickBKG.localPosition = Vector2.zero;
                joystickHandle.localPosition = Vector2.zero;
            }
        }
        inputSystemJoystickPath.SendJoystickValue(GetJoystickAxis());
    }
    public Vector2 GetJoystickAxis()
    {
        if (Input.touchCount > 0) return (currentPoint - startPoint).normalized * currentDistance / maxHandleDistance;
        else return Vector2.zero;
    }
}
