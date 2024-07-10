using System;
using UnityEngine;
using System.Linq;

namespace SSBQuests
{
    [CreateAssetMenu(fileName = "Default Quest", menuName = "67Bits/Quest/New Quest", order = 0)]
    public class Quest : ScriptableObject
    {
        [ReadOnly] public string Id;
        public Sprite Icon;
        public string Title;
        [TextArea(2, 5)] public string Description;
        [Tooltip("Should the quest follow the objectives in order or shoult it show all objectives at once")]
        public bool HasSteps = true;
        [Tooltip("Should the quest progression be saved every time or just when the quest is competed")]
        public bool CanSaveOnProgression = true;
        public Objective[] Objectives;
        public QuestLink[] LinkedQuests;
        [Serializable]
        public class QuestLink
        {
            public Quest Quest;
            public Status RequiredStatus;
            public Status TargetStatus;
        }

        #region Quest Progression
        public void ChangeStatus(Status newStatus)
        {
            var quest = GetQuestData();
            if (!IsQuestActive())
            {
                if (newStatus == Status.Selected)
                    TutorialPath.Instance.QuestObjects.Clear();
                if ((newStatus == Status.Active || newStatus == Status.Selected) && !QuestManager.ActiveQuests.ContainsKey(this))
                    QuestManager.ActiveQuests.Add(this, quest);
            }

            quest.Status = newStatus;
            quest.OnStatusChanged?.Invoke(newStatus);
            OnQuestUpdate(newStatus);
        }
        public void UpdateProgression(ObjectiveType objectiveType, int newValue = 1)
        {
            var questData = GetQuestData();
            for (int i = 0; i < questData.Objectives.Length; i++)
            {
                var objective = questData.Objectives[i];
                var totalValue = GetObjectiveTotalValue(objectiveType);
                if (objective.ObjectiveType == objectiveType)
                {
                    objective.CurrentValue += newValue;
                    // Call the changes in UI
                    objective.OnProgressionChanged(objective.CurrentValue, totalValue);
                    break;
                }
            }
            if (CanSaveOnProgression) QuestManager.Instance.SaveQuest();
            if (!IsQuestCompleted(questData))
                return;
            ChangeStatus(Status.Complete);
            QuestManager.Instance.SaveQuest();
        }
        private void OnQuestUpdate(Status status)
        {
            for (int i = 0; i < LinkedQuests.Length; i++)
            {
                QuestLink quest = LinkedQuests[i];
                if (quest.RequiredStatus == status)
                {
                    quest.Quest.ChangeStatus(quest.TargetStatus);
                }
            }
        }
        private bool IsQuestCompleted(QuestData questData)
        {
            for (int i = 0; i < Objectives.Length; i++)
            {
                var objective = Objectives[i];
                for (int j = 0; j < questData.Objectives.Length; j++)
                {
                    QuestData.ObjectiveData quest = questData.Objectives[j];
                    if (GetObjectiveCurrentValue(quest.ObjectiveType) < objective.TotalValue)
                        return false;
                }
            }
            return true;
        }
        #endregion

        #region Listeners
        public void OnProgressionListener(ObjectiveType objectiveType, Action<int, int> callback)
        {
            var objectives = GetQuestData().Objectives;

            for (int i = 0; i < objectives.Length; i++)
            {
                var objective = objectives[i];
                if (objective.ObjectiveType == objectiveType)
                {
                    objective.OnProgressionChanged += callback;
                    return;
                }
            }
        }
        public void AddStatusListener(Action<Status> callback)
        {
            var questData = GetQuestData();
            questData.OnStatusChanged += callback;
        }
        public void RemoveStatusListener(Action<Status> callback)
        {
            var questData = GetQuestData();
            questData.OnStatusChanged -= callback;
        }
        #endregion

        #region GetInfo
        public bool IsQuestActive(QuestData quest = null)
        {
            var questData = quest ?? GetQuestData();
            return (questData.Status == Status.Active ||
                    questData.Status == Status.Selected);
        }
        public bool IsObjectiveActive(Objective objective)
        {
            return GetObjectiveCurrentValue(objective.ObjectiveType) < objective.TotalValue;
        }
        public bool IsObjectiveActive(ObjectiveType objectiveType)
        {
            for (int i = 0; i < Objectives.Length; i++)
            {
                if (Objectives[i].ObjectiveType == objectiveType)
                {
                    var objective = Objectives[i];
                    return GetObjectiveCurrentValue(objective.ObjectiveType) < objective.TotalValue;
                }
            }
            return false;
        }
        public Objective GetCurrentObjective()
        {
            for (int i = 0; i < Objectives.Length; i++)
            {
                var objective = Objectives[i];
                if (IsObjectiveActive(objective))
                    return objective;
            }
            return Objectives.Last();
        }
        public (int current, int total) GetCurrentObjectiveInfo()
        {
            for (int i = 0; i < Objectives.Length; i++)
            {
                var objective = Objectives[i];
                if (GetObjectiveCurrentValue(objective.ObjectiveType) < objective.TotalValue)
                {
                    return (GetObjectiveCurrentValue(objective.ObjectiveType),
                        objective.TotalValue);
                }
            }
            return (0, 0);
        }
        public int GetObjectiveCurrentValue(ObjectiveType objectiveType)
        {
            var questData = GetQuestData();
            for (int i = 0; i < questData.Objectives.Length; i++)
            {
                var data = questData.Objectives[i];
                if (data.ObjectiveType.Equals(objectiveType))
                    return data.CurrentValue;
            }
            return 0;
        }
        private int GetObjectiveTotalValue(ObjectiveType objectiveType)
        {
            for (int i = 0; i < Objectives.Length; i++)
            {
                var data = Objectives[i];
                if (data.ObjectiveType.Equals(objectiveType))
                    return data.TotalValue;
            }
            return 0;
        }
        public bool TryGetObjectiveId(ObjectiveType objectiveType, out int objectiveIndex)
        {
            objectiveIndex = 0;
            for (int i = 0; i < Objectives.Length; i++)
            {
                if (Objectives[i].ObjectiveType == objectiveType)
                {
                    objectiveIndex = i;
                    return true;
                }
            }
            return false;
        }
        public QuestData GetQuestData()
        {
            if (!QuestManager.LoadedQuests.TryGetValue(this, out var questData))
            {
                QuestManager.LoadedQuests.Add(this, new QuestData(this.Id, Objectives, Status.Inative));
                questData = QuestManager.LoadedQuests[this];
            }
            return questData;
        }
        #endregion

        [ContextMenu("Generate Id")]
        public void GenerateId() => Id = Guid.NewGuid().ToString();
        private void OnValidate()
        {
            if (Objectives != null && Objectives.Length > 5)
            {
                Objectives = Objectives.Take(5).ToArray();
            }
            if (String.IsNullOrEmpty(Id))
                GenerateId();
            for (int i = 0; i < Objectives.Length; i++)
            {
                Objective objetive = Objectives[i];
                objetive.Name = objetive.ObjectiveType.ToString();
            }
            for (int i = 0; i < LinkedQuests.Length; i++)
            {
                var linkedQuest = LinkedQuests[i];
                if (linkedQuest.Quest && linkedQuest.Quest.LinkedQuests.Length != 0)
                {
                    foreach (var item in linkedQuest.Quest.LinkedQuests)
                    {
                        if (item.Quest && item.Quest.Equals(this))
                        {
                            LinkedQuests[i].Quest = null;
                            Debug.LogError("Quest already linked with this item.");
                            break;
                        }
                    }
                }

                if (LinkedQuests[i].Quest && linkedQuest.Quest.Equals(this))
                {
                    LinkedQuests[i].Quest = null;
                    Debug.LogError("Same quest cannot be linked.");
                }
            }
        }
    }
    /// <summary>
    /// Custom enum for the game objectives, every new type of objective must be created here
    /// </summary>
    public enum ObjectiveType
    {
        CollectCoin, KillEnemies, ProtectedBase
    }
    [Serializable]
    public class Objective
    {
        [HideInInspector] public string Name;
        public ObjectiveType ObjectiveType;
        public int TotalValue;
        [TextArea(2, 5)] public string Description;
    }
}