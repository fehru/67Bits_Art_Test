using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Money))]
public class MoneyEditorButtons : Editor
{
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            Money targetMoney = (Money)target;
            if (GUILayout.Button("Reset Money")) targetMoney.SetMoney(0);
            if (GUILayout.Button("Add Money")) targetMoney.AddMoney(1000);
            if (GUILayout.Button("Remove Money")) targetMoney.AddMoney(-1000);
            if (GUILayout.Button("Infinite Money")) targetMoney.AddMoney(999999999);
        }
        else
        {
            GUI.enabled = false;
            GUILayout.TextField("Debuggable Buttons Only In Play Mode!");
            GUI.enabled = true;
        }
    }
}
