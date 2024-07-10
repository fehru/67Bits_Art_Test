using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SSBQuests
{
    public enum FillType
    {
        None,
        Image,
        Slider
    }
    public class QuestUIBar : MonoBehaviour
    {
        [field: SerializeField] public FillType FillType { get; private set; } = FillType.None;
        [field: SerializeField] public Image ImageFill { get; private set; }
        [field: SerializeField] public Slider Slider { get; private set; }
        [field: SerializeField] public Image IconImage { get; private set; }
        [field: SerializeField] public TextMeshProUGUI CounterText { get; private set; }
        public void SetObjective(Sprite icon, int currentValue, int totalValue)
        {
            if (FillType == FillType.Slider && Slider != null)
            {
                Slider.maxValue = totalValue;
                Slider.value = currentValue;
            }
            if (FillType == FillType.Image && ImageFill != null)
                ImageFill.fillAmount = currentValue;
            if (IconImage != null)
                IconImage.sprite = icon;
            if (CounterText != null)
                CounterText.text = $"{currentValue}/{totalValue}";
        }
        public void UpdateProgression(int currentValue, int totalValue)
        {
            if (FillType == FillType.Slider && Slider != null)
                Slider.value = currentValue;
            if (FillType == FillType.Image && ImageFill != null)
                ImageFill.fillAmount = currentValue;
            if (CounterText != null)
                CounterText.text = $"{currentValue}/{totalValue}";
        }
    }
}
