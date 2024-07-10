using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Lofelt.NiceVibrations;


public class HapticsSettingsMenu : MonoBehaviour
{
    [SerializeField] UnityEvent onEnable;
    [SerializeField] UnityEvent onDisable;


    public void ToogleHaptics()
    {
        HapticController.hapticsEnabled = !HapticController.hapticsEnabled;
        InvokeFeel();
        SaveHapticsSettings();
    }

    public void SetHapticsOn()
    {
        HapticController.hapticsEnabled = true;
        InvokeFeel();
        SaveHapticsSettings();
    }

    public void SetHapticsOff()
    {
        HapticController.hapticsEnabled = false;
        InvokeFeel();
        SaveHapticsSettings();
    }

    public void InvokeFeel()
    {
        if (HapticController.hapticsEnabled) onEnable.Invoke();
        else onDisable.Invoke();
    }

    public static void SaveHapticsSettings()
    {
        PlayerPrefs.SetInt("Haptics_Enable", HapticController.hapticsEnabled ? 1 : 0);
    }

    public static void LoadHapticsSettings()
    {
        int hapticsEnable = PlayerPrefs.GetInt("Haptics_Enable", 1);
        HapticController.hapticsEnabled = hapticsEnable == 1 ? true : false;
    }
    private void Awake()
    {
        LoadHapticsSettings();
    }

    private void Start()
    {
        InvokeFeel();
    }
}
