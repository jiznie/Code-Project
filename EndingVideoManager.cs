using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndingVideoManager: MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;   // Assign your VideoPlayer here
    public RawImage videoDisplay;     // Assign your RawImage here

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu"; // Scene to load after video
    public float fadeDuration = 1f;               // Fade out duration in seconds
    public float delayAfterVideo = 1f;            // Seconds to wait after video finishes

    public CanvasGroup fadeOverlay; // Optional: full-screen black overlay

    void Start()
    {
        if (videoPlayer == null || videoDisplay == null)
        {
            Debug.LogError("Assign VideoPlayer and RawImage in inspector!");
            return;
        }

        // Create a RenderTexture for the video
        RenderTexture rt = new RenderTexture(1920, 1080, 0);
        videoPlayer.targetTexture = rt;
        videoDisplay.texture = rt;

        // Start playing
        videoPlayer.Play();
        StartCoroutine(WaitForVideoEnd());
    }

    IEnumerator WaitForVideoEnd()
    {
        while (videoPlayer.isPlaying)
        {
            yield return null;
        }

        // Optional delay
        yield return new WaitForSeconds(delayAfterVideo);

        // Fade out if overlay assigned
        if (fadeOverlay != null)
        {
            float time = 0f;
            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                fadeOverlay.alpha = Mathf.Clamp01(time / fadeDuration);
                yield return null;
            }
        }

        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
