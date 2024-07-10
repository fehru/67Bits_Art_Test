using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace SSBQuests.Editor
{
    [CustomEditor(typeof(Quest))]
    [CanEditMultipleObjects]
    public class QuestEditor : UnityEditor.Editor
    {
        private Quest _quest;

        public override void OnInspectorGUI()
        {
            _quest = target as Quest;
            serializedObject.Update();

            #region Quest Name
            GUIStyle style = EditorStyles.boldLabel;
            style.normal.textColor = Color.yellow;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Quest Settings:", style);
            GUILayout.Space(5);

            style.fontSize = 25;
            EditorGUILayout.LabelField(_quest.Title.Length > 0 ? _quest.Title : "New Quest", style);

            style.fontSize = 12;
            style.alignment = TextAnchor.UpperRight;
            GUILayout.Space(15);

            style.normal.textColor = Color.white;

            if (_quest.Icon)
                EditorGUILayout.BeginHorizontal();
            var texture = AssetPreview.GetAssetPreview(_quest.Icon);
            GUILayout.Label(texture);
            EditorGUILayout.BeginVertical();
            EditorGUIUtility.labelWidth = 35;
            if (_quest.Title.Length == 0) EditorGUILayout.HelpBox("Please add a Title for the Quest", MessageType.Error);
            DrawProprety(nameof(_quest.Title), 2);
            if (!_quest.Icon) EditorGUILayout.HelpBox("Your Quest has no Icon", MessageType.Warning);
            DrawProprety(nameof(_quest.Icon), 5);
            EditorGUILayout.EndVertical();
            if (_quest.Icon)
                EditorGUILayout.EndHorizontal();
            DrawProprety(nameof(_quest.Description), 5);
            #endregion

            if (_quest.Objectives.Length > 0)
                DrawProprety(nameof(_quest.Objectives), 5);
            if (GUILayout.Button("Add New Objective"))
                Array.Resize(ref _quest.Objectives, _quest.Objectives.Length + 1);

            if (_quest.LinkedQuests.Length > 0)
                DrawProprety(nameof(_quest.LinkedQuests), 5);
            if (GUILayout.Button("Add New Linked Quest"))
                Array.Resize(ref _quest.LinkedQuests, _quest.Objectives.Length + 1);

            EditorGUIUtility.labelWidth = 150;
            DrawProprety(nameof(_quest.HasSteps));
            DrawProprety(nameof(_quest.CanSaveOnProgression), 15);

            DrawProprety(nameof(_quest.Id));
            if (GUILayout.Button("Generate Id"))
                _quest.GenerateId();

            serializedObject.ApplyModifiedProperties();
        }
        public void DrawProprety(string proprety, float space = 0, GUILayoutOption[] options = null)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(proprety), options);
            EditorGUILayout.Space(space);
        }
    }
}
