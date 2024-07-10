using System;
using System.Linq;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace SSBQuests
{
    /// <summary>
    /// Para quests com TrackQuestPoint ativo, será acionado o path, indicando o percurso do player até o target;
    /// Se houver vários objetivos na quest, será verificado o mais próximo, dando prioridade à ordem dos objetivos;
    /// </summary>
    public class QuestObject : MonoBehaviour
    {
        [Tooltip("Should this objective be targeted by the camera"), SerializeField]
        private bool _cameraFocusAtStart = false;
        [Tooltip("Should this objective be tracked by the quest path"), SerializeField]
        private bool _trackQuestPoint = false;
        [Tooltip("Global: Updates all active quests with same type\nQuestId: Updates all quests with same id\nBoth: Updates with both cases")]
        [SerializeField] private QuestObjectType _questObjectType;
        [field: Tooltip("List of target objectives to update progression and the progression value")]
        [field: SerializeField] public QuestObjective[] _QuestObjectives { get; set; }
        [field: Tooltip("Quests to be linked to this object")]
        [field: SerializeField] public QuestObjectEvent[] _Quests { get; set; }
        private void Start()
        {
            QuestManager.Instance._CurrentQuest.AddStatusListener(CheckTrackableQuest);
            QuestManager.Instance._CurrentQuest.AddStatusListener(FocusCamera);
        }
        private void OnDisable()
        {
            if (gameObject.scene.isLoaded)
            {
                QuestManager.Instance._CurrentQuest.RemoveStatusListener(CheckTrackableQuest);
                QuestManager.Instance._CurrentQuest.RemoveStatusListener(FocusCamera);
            }
        }
        [ContextMenu("Update Progression")]
        public void UpdateProgression()
        {
            switch (_questObjectType)
            {
                case QuestObjectType.Global:
                    SetGlobalProgression();
                    break;
                case QuestObjectType.QuestId:
                    SetQuestProgression();
                    break;
                case QuestObjectType.Both:
                    SetGlobalProgression();
                    SetQuestProgression();
                    break;
            }
        }
        public void FocusCamera(Status status)
        {
            if (_cameraFocusAtStart && status == Status.Selected)
                StartCoroutine(QuestManager.Instance.ChangeCameraObjective(this.gameObject));
        }
        public void EndTrackableQuest(Status status)
        {
            if (_trackQuestPoint && status == Status.Selected)
                TutorialPath.Instance.EndPath(this);
        }
        public void CheckTrackableQuest(Status status)
        {
            if (_trackQuestPoint && status == Status.Selected)
                TutorialPath.Instance.QuestObjects.Add(this);
        }
        private void SetGlobalProgression()
        {
            foreach (var questDictionary in QuestManager.ActiveQuests)
            {
                bool contains = false;
                for (int i = 0; i < _Quests.Length; i++)
                {
                    if (_Quests[i].Quest == questDictionary.Key)
                    {
                        contains = true;
                        break;
                    }
                }
                if (_questObjectType == QuestObjectType.Both && contains)
                    continue;
                UpdateQuestProgression(questDictionary.Key);
            }
        }
        private void SetQuestProgression()
        {
            for (int i = 0; i < _Quests.Length; i++)
                UpdateQuestProgression(_Quests[i].Quest);
        }
        private void UpdateQuestProgression(Quest quest)
        {
            for (int i = 0; i < _QuestObjectives.Length; i++)
                quest.UpdateProgression(_QuestObjectives[i].Objective, _QuestObjectives[i].Value);
            for (int i = 0; i < _Quests.Length; i++)
            {
                for (int j = 0; j < _Quests[i].QuestEvent.Length; j++)
                {
                    var questEvent = _Quests[i].QuestEvent[j];
                    questEvent.CheckTargetStatus(quest.GetQuestData().Status);
                }
            }
        }
        private void OnValidate()
        {
            for (int i = 0; i < _QuestObjectives.Length; i++)
            {
                QuestObjective item = _QuestObjectives[i];
                item.Name = item.Objective.ToString();
            }
            for (int i = 0; i < _Quests.Length; i++)
            {
                var quest = _Quests[i];
                quest.Name = quest.Quest.name;
                for (int j = 0; j < quest.QuestEvent.Length; j++)
                {
                    var questEvent = quest.QuestEvent[j];
                    questEvent.Name = questEvent.TargetStatus.ToString();
                }
            }
        }
    }
    /// <summary>
    /// <br />Global: Updates all active quests with same type <br />
    /// <br />QuestId: Updates all quests with same id <br />
    /// <br />Both: Updates with both cases <br />
    /// </summary>
    public enum QuestObjectType
    {
        Global, QuestId, Both
    }
    [Serializable]
    public class QuestObjective
    {
        [HideInInspector] public string Name;
        public ObjectiveType Objective;
        [Tooltip("Progression value to be added")]
        public int Value;
    }
    [Serializable]
    public class QuestObjectEvent
    {
        [HideInInspector] public string Name;
        public Quest Quest;
        public QuestEvent[] QuestEvent;
    }
    [Serializable]
    public class QuestEvent
    {
        [HideInInspector] public string Name;
        public Status TargetStatus;
        public UnityEvent OnTargetSelected;

        public bool CheckTargetStatus(Status statusToCheck)
        {
            var checkStatus = statusToCheck == TargetStatus;
            if (checkStatus)
                OnTargetSelected?.Invoke();
            return checkStatus;
        }
    }
}