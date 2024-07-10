using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bello.Unity;

public class PhysicsForce2D : MonoBehaviour
{
    [SerializeField] private bool onTriggerEnter;
    [SerializeField] private bool onTriggerExit;
    [SerializeField] private bool onTriggerStay;

    [SerializeField] private float force;
    [SerializeField] private ForceMode2D forceMode;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private Transform targetPosition;

    [Header("Extra Settings")][Space(5)]
    [SerializeField] private bool onlyAffectLightObjects;
    [field: SerializeField] public float MaxLightObjectMass { get; set; }

    public void ApplyForce(Rigidbody2D rig, Vector3 targetPosition, float force, ForceMode2D forceMode)
    {
        if (onlyAffectLightObjects && rig.mass > MaxLightObjectMass) return;
        Vector2 newDirection = ((Vector2)targetPosition - rig.position) * force;
        rig.AddForce(newDirection, forceMode);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!onTriggerEnter) return;
        Rigidbody2D tempRigidbody;
        if (targetLayers.Include(col.gameObject.layer) && col.TryGetComponent<Rigidbody2D>(out tempRigidbody))
        {
            ApplyForce(tempRigidbody, targetPosition.position, force * tempRigidbody.mass, forceMode);
        }
    }
    private void OnTriggerExit2D(Collider2D col)
    {
        if (!onTriggerExit) return;
        Rigidbody2D tempRigidbody;
        if (col.TryGetComponent<Rigidbody2D>(out tempRigidbody))
        {
            ApplyForce(tempRigidbody, targetPosition.position, force * tempRigidbody.mass, forceMode);
        }
    }
    private void OnTriggerStay2D(Collider2D col)
    {
        if (!onTriggerStay) return;
        Rigidbody2D tempRigidbody;
        if (col.TryGetComponent<Rigidbody2D>(out tempRigidbody))
        {
            ApplyForce(tempRigidbody, targetPosition.position, force, forceMode);
        }
    }
}
