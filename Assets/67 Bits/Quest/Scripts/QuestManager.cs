using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SSBQuests
{
    public class QuestManager : Singleton<QuestManager>
    {
        /// <summary>
        /// Store all loaded quests data
        /// </summary>
        public static Dictionary<Quest, QuestData> LoadedQuests { get; set; } = new Dictionary<Quest, QuestData>();
        /// <summary>
        /// Store all active quests data
        /// </summary>
        public static Dictionary<Quest, QuestData> ActiveQuests { get; set; } = new Dictionary<Quest, QuestData>();
        [Tooltip("Store all quest's scriptables")]
        [SerializeField] private Quest[] _quests;
        [Tooltip("Reference for quest's UI object")]
        [SerializeField] private QuestUI _questUI;
        public Quest _CurrentQuest { get; private set; }
        [SerializeField] private bool loadQuestsAtStart = true;

#if UNITY_EDITOR
        [Space(25), ButtonInvoke(nameof(LoadAllQuests), null, ButtonInvoke.DisplayIn.EditMode, "Load All Quests")] public bool LoadQuests;
#endif
        protected override void Awake()
        {
            base.Awake();
            if (loadQuestsAtStart) LoadAllQuests();
            LoadQuest();
        }
        [ContextMenu("Load All Quests")]
        private void LoadAllQuests()
        {
            _quests = Resources.LoadAll<Quest>("").ToArray();
        }
        public void LoadQuest()
        {
            if (PlayerPrefs.HasKey(nameof(SaveQuestData)))
            {
                var loadData = SSBQuests.SaveQuest.LoadCustomJson(SaveQuestData.Instance);
                for (int i = 0; i < _quests.Length; i++)
                {
                    if (_quests[i].Id == loadData.CurrentQuestId)
                    {
                        _CurrentQuest = _quests[i];
                        break;
                    }
                }

                var questData = _CurrentQuest.GetQuestData();
                for (int i = 0; i < questData.Objectives.Length; i++)
                {
                    var currentData = questData.Objectives[i];
                    questData.Status = (Status)loadData.CurrentQuestStatus;
                    currentData.ObjectiveType = (ObjectiveType)loadData.CurrentQuestObjectives[i].Type;
                    currentData.CurrentValue = loadData.CurrentQuestObjectives[i].CurrentValue;
                }
                _questUI.SetQuestContent(_CurrentQuest);
            }
            else StartCoroutine(SetNewQuest());
        }
        public void SaveQuest()
        {
            var saveData = SaveQuestData.Instance;
            var listObjectives = new List<(int Type, int CurrentValue)>();
            saveData.CurrentQuestId = _CurrentQuest.Id;
            var questData = _CurrentQuest.GetQuestData();
            saveData.CurrentQuestStatus = (int)questData.Status;
            for (int i = 0; i < questData.Objectives.Length; i++)
            {
                var data = questData.Objectives[i];
                listObjectives.Add(((int)data.ObjectiveType, data.CurrentValue));
            }
            saveData.CurrentQuestObjectives = new(listObjectives);
            SSBQuests.SaveQuest.SaveGameCustomJson(SaveQuestData.Instance);
        }
        public IEnumerator SetNewQuest(Quest newQuest = null)
        {
            _CurrentQuest = newQuest?? _quests[0];
            yield return new WaitForFixedUpdate();
            _CurrentQuest.ChangeStatus(Status.Selected);
            _questUI.SetQuestContent(_CurrentQuest);
        }
        public IEnumerator ChangeCameraObjective(GameObject target)
        {
            yield return new WaitForSeconds(2f);
            ChangeCameraFocus(target.transform);
            yield return new WaitForSeconds(2f);
            ChangeCameraFocus();
        }
        /// <summary>
        /// Change the current cinemachine follow target and look target, as default is reset the target to gamemanager's player transform
        /// </summary>
        /// <param name="focus"></param>
        private void ChangeCameraFocus(Transform focus = null)
        {
            GameReferences.CinemachineVirtualCamera.Follow = focus?? GameReferences.PlayerTransform;
            GameReferences.CinemachineVirtualCamera.LookAt = focus?? GameReferences.PlayerTransform;
        }
    }
}