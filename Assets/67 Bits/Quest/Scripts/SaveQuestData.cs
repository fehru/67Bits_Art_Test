using System.Collections.Generic;

namespace SSBQuests
{
    public class SaveQuestData
    {
        private static SaveQuestData instance;
        public static SaveQuestData Instance
        {
            get
            {
                if (instance == null) instance = new SaveQuestData();
                return instance;
            }
            set { instance = value; }
        }
        //Quest
        public string CurrentQuestId;
        public int CurrentQuestStatus;
        public List<(int Type, int CurrentValue)> CurrentQuestObjectives = new();
    }
}