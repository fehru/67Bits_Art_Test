using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using System;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    #region Attributes

    [Tooltip("Key for saving. Change this to access different saves :)")]
    [SerializeField] private string PlayerPrefsKey = "Default";
    private string FullKey => this.GetType().Name + PlayerPrefsKey;

    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private List<GroupVolume> _volumes;

    #endregion

    // ________________

    #region Save / Load data

    [Serializable]
    public class GroupVolume
    {
        public string ParamName;
        [Range(0, 1)] public float PercentageVolume;
        public bool IsMuted = false;
    }

    [Serializable]
    public class GroupVolumeListWrapper
    {
        public List<GroupVolume> GroupVolumes;
    }

    private void SaveSettings()
    {
        GroupVolumeListWrapper wrapper = new GroupVolumeListWrapper();
        wrapper.GroupVolumes = _volumes;
        //PlayerSettingSaver.Save(wrapper, FullKey);
    }

    private void LoadSettings()
    {
        //GroupVolumeListWrapper wrapper = PlayerSettingSaver.Load<GroupVolumeListWrapper>(FullKey);

        //if (wrapper != null && 
        //    wrapper.GroupVolumes != null && 
        //    wrapper.GroupVolumes.Count != 0)
        //{
        //    _volumes = wrapper.GroupVolumes;
        //}
    }
    
    #endregion

    // ________________
    
    #region Volume

    private void SetVolumeInMixer(GroupVolume volume)
    {
        if (!_audioMixer.GetFloat(volume.ParamName, out float valueInMixer))
        {
            Debug.LogError($"Audio Mixer group exposed param {volume.ParamName} not found.", this.gameObject);
            return;
        }

        float dBValue;

        if (volume.IsMuted || volume.PercentageVolume < 0.0001 /* = -80 db*/ )
            dBValue = -80.0f;
        else
            dBValue = Mathf.Log10(volume.PercentageVolume) * 20;

        _audioMixer.SetFloat(volume.ParamName, dBValue);
    }

    private void SetAllVolumesInMixer()
    {
        foreach (var volume in _volumes)
        {
            SetVolumeInMixer(volume);
        }
    }

    public void SetVolume(string name, float newValue)
    {
        bool found = false;

        foreach (var volume in _volumes)
        {
            if (volume.ParamName == name)
            {
                volume.PercentageVolume = newValue;
                found = true;
                SetVolumeInMixer(volume);
                break;
            }
        }

        if (!found)
        {
            if (_audioMixer.GetFloat(name, out float valueInMixer))
            {
                GroupVolume newVolume = new GroupVolume() {
                    ParamName = name,
                    PercentageVolume = newValue
                };

                _volumes.Add(newVolume);

                SetVolumeInMixer(newVolume);
            }
            else
            {
                Debug.LogError($"Audio Mixer group exposed param {name} not found.", this.gameObject);
                return;
            }
        }
    }

    public bool TryGetVolume(string name, out float gotValue)
    {
        foreach (var volume in _volumes)
        {
            if (volume.ParamName == name)
            {
                gotValue = volume.PercentageVolume;
                return true;
            }
        }

        if (_audioMixer.GetFloat(name, out float valueInMixer))
        {
            gotValue = valueInMixer;
            return true;
        }

        // else for all cases
        Debug.LogError($"Audio Mixer group exposed param {name} not found.", this.gameObject);
        gotValue = -1;
        return false;
    }

    private void InitAudioValue()
    {
        LoadSettings();

        SetAllVolumesInMixer();
    }
    
    #endregion

    // ________________

    #region MonoBehavior

    private void Awake()
    {
        base.Awake();

        InitAudioValue();
    }
    
    private void Start()
    {
        InitAudioValue();
    }

    void OnDisable()
    {
        SaveSettings();
    }

    void OnApplicationPause()
    {
        SaveSettings();
    }

    void OnApplicationQuit()
    {
        SaveSettings();
    }

    #endregion

}