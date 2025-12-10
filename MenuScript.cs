using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuUI;        // Main menu canvas
    public GameObject settingsPanel; // Settings panel

    // Called when the "Play" button is clicked
    public void StartGame()
    {
        // Hide the menu so it isn't visible in the next scene
        if (menuUI != null)
            menuUI.SetActive(false);

        // Load next scene in Build Settings. Assuming the game starts on the scene after the menu (index 0).
        // If the main menu is Scene 0, this loads Scene 1 (Night 1).
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1, LoadSceneMode.Single);
    }

    // Called when the "Settings" button is clicked
    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    // Called when the "Close Settings" button is clicked
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // Called when the "Exit" button is clicked
    public void ExitGame()
    {
        Debug.Log("Game exited!");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}