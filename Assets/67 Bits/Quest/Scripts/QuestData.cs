using System;
using System.Collections.Generic;

namespace SSBQuests
{
    public enum Status
    {
        Inative, Active, Selected, Fail, Complete
    }
    [Serializable]
    public class QuestData
    {
        public string Id;
        public ObjectiveData[] Objectives;
        [Serializable]
        public class ObjectiveData
        {
            public Action<int, int> OnProgressionChanged;
            public ObjectiveType ObjectiveType;
            public int CurrentValue;
        }
        public Status Status;
        public Action<Status> OnStatusChanged;
        public QuestData(string Id, Objective[] Objectives, Status Status)
        {
            this.Id = Id;
            List<ObjectiveData> newObjectives = new();
            for (int i = 0; i < Objectives.Length; i++)
            {
                ObjectiveData objective = new ObjectiveData();

                objective.ObjectiveType = Objectives[i].ObjectiveType;
                newObjectives.Add(objective);
            }
            this.Objectives = newObjectives.ToArray();
            this.Status = Status;
        }
    }
}