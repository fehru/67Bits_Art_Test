using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PlayerMoneyUI : MonoBehaviour
{
    [SerializeField] private float speed = 1;
    [SerializeField] private TextMeshProUGUI moneyText;
    [ReadOnly] [SerializeField] private float current;
    [SerializeField] private UnityEvent onGain;
    [SerializeField] private UnityEvent onLoss;

    CancellationTokenSource cancell = new CancellationTokenSource();
    private void Start()
    {
        current = SaveData.Instance.playerMoney;
        UpdateMoneyValue();
    }
    public async void UpdateMoneyValue()
    {
        moneyText.text = this.current.ToString();
        float current = 0;
        if (SaveData.Instance.playerMoney > this.current) onGain.Invoke();
        else if (SaveData.Instance.playerMoney < this.current) onLoss.Invoke();
        while (current != 1 && this.current != SaveData.Instance.playerMoney)
        {
            current = Mathf.MoveTowards(current, 1, speed * Time.deltaTime);
            this.current = Mathf.Lerp(this.current, SaveData.Instance.playerMoney, current);
            moneyText.text = ((int)this.current).ToString();
            await Task.Yield();
            if (cancell.IsCancellationRequested) return;
        }
    }
    private void OnDisable()
    {
        cancell.Cancel();
    }
}
