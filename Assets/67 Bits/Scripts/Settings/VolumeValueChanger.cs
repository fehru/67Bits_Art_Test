using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class VolumeValueChanger : MonoBehaviour
{
    [SerializeField] private string ParamName;
    [SerializeField] private UnityEvent<float> _OnVolumeUpdate;

    [SerializeField] private TextMeshProUGUI _ValueText;

    public void SetVolume(float value)
    {
        AudioManager.Instance?.SetVolume(ParamName, (value / 10));
        //_ValueText.text = value.NumberToText(0);
    }

    public bool TryGetVolume(out float value)
    {
        if (AudioManager.Instance && AudioManager.Instance.TryGetVolume(ParamName, out value))
        {
            value *= 10;
            return true;
        }

        // else
        value = -1f;
        return false;
    }

    private void OnEnable()
    {
        if (TryGetVolume(out float value))
            _OnVolumeUpdate?.Invoke(value);
    }
}
