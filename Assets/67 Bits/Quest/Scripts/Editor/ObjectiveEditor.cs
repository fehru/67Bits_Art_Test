using SSBQuests;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SSBQuests.Editor
{
    [CustomEditor(typeof(Objective))]
    [CanEditMultipleObjects]
    public class ObjectiveEditor : UnityEditor.Editor
    {
        private Objective _objective;

        public override void OnInspectorGUI()
        {
            //_objective = target as Objective;
            serializedObject.Update();

            serializedObject.ApplyModifiedProperties();
        }
        public void DrawProprety(string proprety, float space = 0, GUILayoutOption[] options = null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(proprety), options);
            EditorGUILayout.Space(space);
        }
    }
}
