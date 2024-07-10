using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Button Settings")][Space(5)]
    public int StartIncomePrice;
    public int IncomePricePerLevel;
    public int MaxIncomeLevel;
}
