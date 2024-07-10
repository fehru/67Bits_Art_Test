using System;
using System.Collections.Generic;
using UnityEngine;

namespace SSBQuests
{
    [CreateAssetMenu(fileName = "Objective Icon", menuName = "67Bits/Quest/Objective Icons", order = 1)]
    public class ObjectiveIcon : ScriptableObject
    {
        public List<ObjectiveSettings> Icons;
        [Serializable]
        public class ObjectiveSettings
        {
            [HideInInspector] public string Name;
            public Sprite Icon;

        }
        private void OnValidate()
        {
            var enumLength = Enum.GetValues(typeof(ObjectiveType)).Length;

            if (Icons.Count < enumLength)
            {
                enumLength -= Icons.Count;
                for (int i = 0; i < enumLength; i++)
                {
                    Icons.Add(new ObjectiveSettings());
                }
            }
            for (int i = 0; i < Icons.Count; i++)
            {
                Icons[i].Name = ((ObjectiveType)i).ToString();
            }
        }
    }
}