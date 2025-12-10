using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayTwoIntroVideos : MonoBehaviour
{
    [Header("Video Player Settings")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Videos")]
    public VideoClip video1;
    public VideoClip video2;

    [Header("UI Buttons")]
    public Button fastForwardButton;
    public Button skipButton;

    [Header("Next Scene")]
    public string nextSceneName = "NarrativeScene";

    private float normalSpeed = 1f;
    private float fastForwardSpeed = 3f;

    private int currentVideoIndex = 0;
    private bool isSkipping = false;

    private Canvas fadeCanvas;
    private Image fadeImage;
    private float fadeDuration = 1f;

    private void Start()
    {
        SetupFadeOverlay();
        SetupVideoPlayer();
        SetupButtons();
        StartCoroutine(PlayVideosSequentially());
    }

    private void SetupFadeOverlay()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("FadeCanvas");
        fadeCanvas = canvasGO.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;

        // Add CanvasGroup to control alpha
        CanvasGroup canvasGroup = canvasGO.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Create full-screen black Image
        GameObject imageGO = new GameObject("FadeImage");
        imageGO.transform.SetParent(canvasGO.transform, false);
        fadeImage = imageGO.AddComponent<Image>();
        fadeImage.color = Color.black;
        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void SetupVideoPlayer()
    {
        if (videoPlayer == null || videoDisplay == null)
        {
            Debug.LogError("Assign VideoPlayer and RawImage!");
            return;
        }

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;

        int width = video1 != null ? (int)video1.width : 1920;
        int height = video1 != null ? (int)video1.height : 1080;

        RenderTexture rt = new RenderTexture(width, height, 0);
        videoPlayer.targetTexture = rt;
        videoDisplay.texture = rt;

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
    }

    private void SetupButtons()
    {
        if (fastForwardButton != null)
            fastForwardButton.onClick.AddListener(ToggleFastForward);

        if (skipButton != null)
            skipButton.onClick.AddListener(SkipVideo);
    }

    private IEnumerator PlayVideosSequentially()
    {
        VideoClip[] clips = new VideoClip[] { video1, video2 };

        while (currentVideoIndex < clips.Length)
        {
            isSkipping = false;
            yield return StartCoroutine(PlayVideo(clips[currentVideoIndex]));
            currentVideoIndex++;
        }

        // After last video, fade and load scene
        yield return StartCoroutine(FadeAndLoadScene(nextSceneName));
    }

    private IEnumerator PlayVideo(VideoClip clip)
    {
        if (clip == null || videoPlayer == null)
            yield break;

        videoPlayer.clip = clip;
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.playbackSpeed = normalSpeed;
        videoPlayer.Play();

        while (videoPlayer.isPlaying && !isSkipping)
            yield return null;

        videoPlayer.Stop();
    }

    private void ToggleFastForward()
    {
        if (videoPlayer == null) return;

        videoPlayer.playbackSpeed = (videoPlayer.playbackSpeed == normalSpeed) ? fastForwardSpeed : normalSpeed;
    }

    private void SkipVideo()
    {
        if (videoPlayer == null) return;

        isSkipping = true;

        // If it’s the last video, fade and load scene
        if (currentVideoIndex == 1) // second video
        {
            StopAllCoroutines();
            StartCoroutine(FadeAndLoadScene(nextSceneName));
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            if (fadeImage != null) fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }
}
