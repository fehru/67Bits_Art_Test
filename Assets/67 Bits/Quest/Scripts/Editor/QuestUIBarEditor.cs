using UnityEditor;

namespace SSBQuests
{
    [CustomEditor(typeof(QuestUIBar))]
    public class QuestUIBarEditor : UnityEditor.Editor
    {
        SerializedProperty fillTypeProp;
        SerializedProperty imageProp;
        SerializedProperty sliderProp;
        SerializedProperty counterTextProp;
        SerializedProperty iconProp;

        private void OnEnable()
        {
            fillTypeProp = serializedObject.FindProperty("<FillType>k__BackingField");
            imageProp = serializedObject.FindProperty("<ImageFill>k__BackingField");
            sliderProp = serializedObject.FindProperty("<Slider>k__BackingField");
            iconProp = serializedObject.FindProperty("<IconImage>k__BackingField");
            counterTextProp = serializedObject.FindProperty("<CounterText>k__BackingField");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(fillTypeProp);

            FillType fillType = (FillType)fillTypeProp.enumValueIndex;

            if (fillType == FillType.Image)
            {
                EditorGUILayout.PropertyField(imageProp);
            }
            if (fillType == FillType.Slider)
            {
                EditorGUILayout.PropertyField(sliderProp);
            }

            EditorGUILayout.PropertyField(counterTextProp);
            EditorGUILayout.PropertyField(iconProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}