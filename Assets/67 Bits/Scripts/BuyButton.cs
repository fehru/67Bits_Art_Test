using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class BuyButton : MonoBehaviour
{
    [SerializeField] private BuyButtonType type;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private int price;
    [ReadOnly][SerializeField] private bool canBuy;
    [ReadOnly][SerializeField] private bool max;

    [Header("Events")]
    [SerializeField] private UnityEvent onUnlock;
    [SerializeField] private UnityEvent onLock;
    [SerializeField] private UnityEvent onMax;
    private void Start()
    {
        canBuy = true;
        UpdatePrice();
        UpdateText();
        UpdateStatus();
        Money.OnMoneyChange += UpdateStatus;
    }
    private void OnDisable()
    {
        Money.OnMoneyChange -= UpdateStatus;
    }
    public void Buy()
    {
        if (SaveData.Instance.playerMoney < price || !canBuy) { return; }

        switch (type)
        {
            case BuyButtonType.Income: SaveData.Instance.currentIncomeLevel++; break;
        }
        int oldPrice = price;
        UpdatePrice();
        Money.Instance.AddMoney(-oldPrice);
        UpdateText();
    }
    public void UpdatePrice()
    {
        switch (type)
        {
            case BuyButtonType.Income: 
                price =
                    GameManager.Instance.GameSettings.StartIncomePrice +
                    GameManager.Instance.GameSettings.IncomePricePerLevel * SaveData.Instance.currentIncomeLevel;
                break;
        }
    }
    public void UpdateStatus()
    {
        if (UpdateMaxStatus()) return;
        if (SaveData.Instance.playerMoney < price && canBuy)
        {
            canBuy = false;
            onLock.Invoke();
            return;
        }
        if(SaveData.Instance.playerMoney >= price && !canBuy)
        {
            canBuy = true;
            onUnlock.Invoke();
            return;
        }
    }
    private bool UpdateMaxStatus()
    {
        if (!max)
        {
            switch (type)
            {
                case BuyButtonType.Income:
                    if (SaveData.Instance.currentIncomeLevel <= GameManager.Instance.GameSettings.MaxIncomeLevel)
                        return false;
                    break;
            }
            max = true;
            onMax.Invoke();
        }
        return max;
    }
    public void UpdateText()
    {

        switch (type)
        {
            case BuyButtonType.Income:      levelText.text = "Level " + SaveData.Instance.currentIncomeLevel.ToString(); break;
        }
        priceText.text = price.ToString();
    }
}
public enum BuyButtonType
{
    Income,
    FireRate,
    FireRange,
    Year,
    Custom
}
