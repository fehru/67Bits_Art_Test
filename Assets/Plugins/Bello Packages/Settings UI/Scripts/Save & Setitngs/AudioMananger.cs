using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public static class AudioSettings
{
    public static bool musicOn;
    public static bool sfxOn;

    public static void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("Music", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("SFX", sfxOn ? 1 : 0);
    }

    public static void LoadAudioSettings()
    {
        musicOn = PlayerPrefs.GetInt("Music", 1) == 1 ? true : false;
        sfxOn = PlayerPrefs.GetInt("SFX", 1) == 1 ? true : false;
    }
}
public class AudioMananger : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mainAudioMixer;

    [SerializeField]
    private UnityEvent showSettingsFeel;
    [SerializeField]
    private UnityEvent hideSettingsFeel;
    [SerializeField]
    private UnityEvent onEnableMusic;
    [SerializeField]
    private UnityEvent onDisableMusic;
    [SerializeField]
    private UnityEvent onEnableSfx;
    [SerializeField]
    private UnityEvent onDisableSfx;

    bool showSettings = false;

    //[SerializeField] private AudioMixerGroup sfxAudioMixer;
    //private void Awake()
    //{
    //    foreach (AudioSource s in Resources.FindObjectsOfTypeAll<AudioSource>())
    //    {
    //        if (s.outputAudioMixerGroup == null) s.outputAudioMixerGroup = sfxAudioMixer;
    //    }
    //}

    private void Start()
    {
        AudioSettings.LoadAudioSettings();
        SetAudioButtons();
        StartCoroutine(StartSfxDelay());
    }
    /// <summary>
    /// Desliga o som no ínicio para evitar bugs sonoros dos sfx
    /// </summary>
    /// <returns></returns>
    IEnumerator StartSfxDelay()
    {
        bool sfXBackup = AudioSettings.sfxOn;
        AudioSettings.sfxOn = false;
        SetAudioSettings();
        yield return new WaitForSeconds(0);
        AudioSettings.sfxOn = sfXBackup;
        SetAudioSettings();
    }
    public void ToogleMusic()
    {
        AudioSettings.musicOn = !AudioSettings.musicOn;
        SetAudioSettings();
        SetAudioButtons();
        AudioSettings.SaveAudioSettings();
    }
    public void ToogleSFX()
    {
        AudioSettings.sfxOn = !AudioSettings.sfxOn;
        SetAudioSettings();
        SetAudioButtons();
        AudioSettings.SaveAudioSettings();
    }
    public void ToogleShowSettings()
    {
        showSettings = !showSettings;
        if (showSettings) showSettingsFeel.Invoke();
        else hideSettingsFeel.Invoke();
    }
    public void SetAudioSettings()
    {
        if (AudioSettings.musicOn)
        {
            mainAudioMixer.SetFloat("MusicVolume", 0);
        }
        else
        {
            mainAudioMixer.SetFloat("MusicVolume", -99);
        }
        if (AudioSettings.sfxOn)
        {
            mainAudioMixer.SetFloat("SfxVolume", 0);
        }
        else
        {
            mainAudioMixer.SetFloat("SfxVolume", -99);
        }
    }
    public void SetAudioButtons()
    {
        if (AudioSettings.musicOn)
        {
            onEnableMusic.Invoke();
        }
        else
        {
            onDisableMusic.Invoke();
        }
        if (AudioSettings.sfxOn)
        {
            onEnableSfx.Invoke();
        }
        else
        {
            onDisableSfx.Invoke();
        }
    }
}
