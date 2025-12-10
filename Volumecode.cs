using UnityEngine;
using UnityEngine.UI;

public class Volumecode : MonoBehaviour
{
    [Header("Slider")]
    public Slider slider;

    [Header("Music Manager")]
    public MusicManager musicManager;

    void Awake()
    {
        // Load saved volume
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
        slider.value = savedVolume;

        // Apply volume immediately
        SetVolume(savedVolume);

        // Ensure slider listener
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(SetVolume);
    }

    /// <summary>
    /// Called whenever slider changes
    /// </summary>
    public void SetVolume(float value)
    {
        if (musicManager != null)
            musicManager.SetMusicVolume(value);

        // Optional: global AudioListener for SFX
        AudioListener.volume = value;

        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }
}
