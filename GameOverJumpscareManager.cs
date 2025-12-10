using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverJumpscareManager : MonoBehaviour
{
    [Header("Video Player Settings")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Jumpscare Video & Sound")]
    public VideoClip jumpscareVideo;
    public AudioClip jumpscareSound; // Sound to play with the jumpscare

    [Header("Audio Settings")]
    public AudioSource audioSource; // Assign an AudioSource for the sound

    [Header("Next Scene")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Settings")]
    public float postJumpscareDelay = 2f; // Delay before loading main menu

    private void Start()
    {
        SetupVideoPlayer();
        StartCoroutine(PlayJumpscareWithSound());
    }

    private void SetupVideoPlayer()
    {
        if (videoPlayer == null || videoDisplay == null)
        {
            Debug.LogError("Assign VideoPlayer and RawImage!");
            return;
        }

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        int width = jumpscareVideo != null ? (int)jumpscareVideo.width : 1920;
        int height = jumpscareVideo != null ? (int)jumpscareVideo.height : 1080;

        RenderTexture rt = new RenderTexture(width, height, 0);
        videoPlayer.targetTexture = rt;
        videoDisplay.texture = rt;

        videoDisplay.raycastTarget = false;

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;

        // Setup AudioSource if missing
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private IEnumerator PlayJumpscareWithSound()
    {
        if (videoPlayer == null || jumpscareVideo == null)
            yield break;

        videoPlayer.clip = jumpscareVideo;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        if (jumpscareSound != null && audioSource != null)
        {
            audioSource.clip = jumpscareSound;
            audioSource.Play();
        }

        // Wait until video finishes
        while (videoPlayer.isPlaying)
            yield return null;

        videoPlayer.Stop();
        if (audioSource != null) audioSource.Stop();

        // Keep jumpscare visible for extra delay before returning to main menu
        yield return new WaitForSeconds(postJumpscareDelay);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
