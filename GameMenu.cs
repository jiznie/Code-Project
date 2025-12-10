using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    public GameObject hamburgerMenu;
    public GameObject exitDialog;
    public Slider volumeSlider;
    public MusicManager musicManager;

    private bool menuOpen = false;

    void Start()
    {
        // Load saved volume
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = savedVolume;

        // Apply volume immediately
        ApplyVolume(savedVolume);

        if (exitDialog != null)
            exitDialog.SetActive(false);

        // Ensure slider listener
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(ApplyVolume);
    }

    /// <summary>
    /// Applies volume to both AudioListener and MusicManager
    /// </summary>
    private void ApplyVolume(float value)
    {
        AudioListener.volume = value;
        musicManager?.SetMusicVolume(value);

        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();
    }

    public void OnHamburgerClick()
    {
        menuOpen = !menuOpen;
        hamburgerMenu.SetActive(menuOpen);
    }

    public void OnResumeClick()
    {
        Time.timeScale = 1;
        hamburgerMenu.SetActive(false);
    }

    public void OnPauseClick()
    {
        Time.timeScale = 0;
    }

    public void OnRestartClick()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnExitClick()
    {
        exitDialog.SetActive(true);
    }

    public void OnExitDialogContinue()
    {
        exitDialog.SetActive(false);
    }

    public void OnExitDialogExit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(1);
    }
}
