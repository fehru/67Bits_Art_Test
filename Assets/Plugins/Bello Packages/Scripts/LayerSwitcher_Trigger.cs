using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bello.Unity;

[RequireComponent(typeof(Collider))]
public class LayerSwitcher_Trigger : MonoBehaviour
{
    [SerializeField] private LayerMask obstaclesLayer;
    [SerializeField] private bool switchOnEnterLayer = true;
    [SerializeField] private int onTriggerEnterLayer;
    [SerializeField] private bool switchOnExitLayer = true;
    [SerializeField] private int onTriggerExitLayer;
    private void OnTriggerEnter(Collider col)
    {
        if(switchOnEnterLayer && obstaclesLayer.Include(col.gameObject.layer))
            col.gameObject.layer = onTriggerEnterLayer;
    }
    private void OnTriggerExit(Collider col)
    {
        if (switchOnExitLayer && obstaclesLayer.Include(col.gameObject.layer))
            col.gameObject.layer = onTriggerExitLayer;
    }
}
