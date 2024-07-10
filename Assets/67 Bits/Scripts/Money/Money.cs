using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Money : Singleton<Money>
{
    public static Action OnMoneyChange;
    public static PlayerMoneyUI moneyUI;
    PlayerMoneyUI _moneyUI 
    { 
        get 
        {
            if(moneyUI == null) moneyUI = FindObjectOfType<PlayerMoneyUI>();
            return moneyUI;
        } 
    }
    public void AddMoney(int money)
    {
        if (_moneyUI == null) return;
        SaveData.Instance.playerMoney += money;
        SaveData.Instance.playerMoney = UnityEngine.Mathf.Clamp(SaveData.Instance.playerMoney, 0, 999999999);
        _moneyUI.UpdateMoneyValue();
        OnMoneyChange?.Invoke();
    }
    public void SetMoney(int money)
    {
        if (_moneyUI == null) return;
        SaveData.Instance.playerMoney = money;
        SaveData.Instance.playerMoney = UnityEngine.Mathf.Clamp(SaveData.Instance.playerMoney, 0, 999999999);
        _moneyUI.UpdateMoneyValue();
        OnMoneyChange?.Invoke();
    }
}
