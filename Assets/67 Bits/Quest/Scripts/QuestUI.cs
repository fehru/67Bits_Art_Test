using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SSBQuests
{
    public class QuestUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _panel;
        [SerializeField] private Transform _content;
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _descriptionText;

        [SerializeField] private GameObject _prefabQuestUIBar;
        public ObjectiveIcon ObjectiveIcon;
        private List<QuestUIBar> _questUIBars = new List<QuestUIBar>();

        public void SetQuestContent(Quest quest)
        {
            if (_image != null)
                _image.sprite = quest.Icon;
            if (_titleText != null)
                _titleText.text = quest.Title;
            if (_descriptionText != null && !quest.HasSteps)
                _descriptionText.text = quest.Description;
            else if (_descriptionText != null)
            {
                _descriptionText.text = quest.GetCurrentObjective().Description;
            }
            for (int i = 0; i < quest.Objectives.Length; i++)
            {
                if (_questUIBars.Count <= quest.Objectives.Length)
                {
                    var newQuestUI = Instantiate(_prefabQuestUIBar, _content).GetComponent<QuestUIBar>();
                    newQuestUI.SetObjective(
                        ObjectiveIcon.Icons[i].Icon,
                        quest.GetObjectiveCurrentValue(quest.Objectives[i].ObjectiveType),
                        quest.Objectives[i].TotalValue
                    );
                    QuestManager.Instance._CurrentQuest.OnProgressionListener(quest.Objectives[i].ObjectiveType, newQuestUI.UpdateProgression);
                    _questUIBars.Add(newQuestUI);
                }
            }
        }
    }
}