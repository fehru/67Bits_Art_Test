using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [Header("Camera Follow Settings")] [Space(5)]
    [SerializeField] private bool followTarget;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Vector3 offSet;
    [SerializeField] private float followSpeed;
    [Tooltip("This number multiplies the current offset")]
    [SerializeField] public float cameraDistance = 1;

    private float currentCameraDistance;

    private Camera thisCamera;
    public Camera _thisCamera { get { if (thisCamera == null) thisCamera = GetComponent<Camera>();  return thisCamera; } }

    private void Start()
    {
        transform.position = cameraTarget.position + offSet * cameraDistance;
    }
    private void FixedUpdate()
    {
        if (cameraTarget != null)
        {
            currentCameraDistance = Mathf.Lerp(currentCameraDistance, cameraDistance, followSpeed * Time.fixedDeltaTime);
            transform.position = Vector3.Lerp(transform.position, cameraTarget.position + offSet * currentCameraDistance, followSpeed * Time.fixedDeltaTime);
        }
    }

    public void ChangeCameraTarget(Transform newTarget)
    {
        cameraTarget = newTarget;
    }
}
