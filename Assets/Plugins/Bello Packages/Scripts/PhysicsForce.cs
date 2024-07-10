using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bello.Unity;

public class PhysicsForce : MonoBehaviour
{
    [SerializeField] private bool onTriggerEnter;
    [SerializeField] private bool onTriggerExit;
    [SerializeField] private bool onTriggerStay;

    [SerializeField] private float force;
    [SerializeField] private ForceMode forceMode;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private Transform targetPosition;

    [Header("Extra Settings")][Space(5)]
    [SerializeField] private bool onlyAffectLightObjects;
    [field: SerializeField] public float MaxLightObjectMass { get; set; }

    private void OnTriggerEnter(Collider col)
    {
        if (!onTriggerEnter) return;
        Rigidbody tempRigidbody;
        if (targetLayers.Include(col.gameObject.layer) && col.TryGetComponent<Rigidbody>(out tempRigidbody))
        {
            ApplyForce(tempRigidbody, targetPosition.position, force * tempRigidbody.mass, forceMode);
        }
    }
    private void OnTriggerExit(Collider col)
    {
        if (!onTriggerExit) return;
        Rigidbody tempRigidbody;
        if (col.TryGetComponent<Rigidbody>(out tempRigidbody))
        {
            ApplyForce(tempRigidbody, targetPosition.position, force * tempRigidbody.mass, forceMode);
        }
    }
    private void OnTriggerStay(Collider col)
    {
        if (!onTriggerStay) return;
        Rigidbody tempRigidbody;
        if (col.TryGetComponent<Rigidbody>(out tempRigidbody))
        {
            ApplyForce(tempRigidbody, targetPosition.position, force, forceMode);
        }
    }
    private void ApplyForce(Rigidbody rig, Vector3 targetPosition, float force, ForceMode forceMode)
    {
        if (onlyAffectLightObjects && rig.mass > MaxLightObjectMass) return;
        Vector3 newDirection = (targetPosition - rig.position) * force;
        rig.AddForce(newDirection, forceMode);
    }
}
