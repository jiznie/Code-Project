using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NarrativeIntroManager : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text narrativeText;
    public Image portraitImage;
    public Button skipButton;
    public CanvasGroup canvasGroup; // For fade in/out

    [Header("Narrative Pages")]
    [Tooltip("Add your text pages here in Inspector.")]
    [TextArea]
    public string[] pages;

    [Header("Audio")]
    public AudioSource typingSound;
    public AudioSource ambienceSound;

    [Header("Scene Navigation")]
    public int nextSceneIndex = 0;

    private int currentPage = 0;
    private bool isTyping = false;

    void Start()
    {
        // Safety checks to prevent NULL or Out-of-Range errors
        if (pages == null || pages.Length == 0)
        {
            Debug.LogError("No narrative pages assigned!");
            return;
        }

        skipButton.onClick.AddListener(SkipOrNextPage);

        // Start ambience
        if (ambienceSound != null) ambienceSound.Play();

        // Fade In then show first page
        StartCoroutine(FadeCanvas(true, () =>
        {
            ShowPage(currentPage);
        }));
    }

    void ShowPage(int pageIndex)
    {
        if (pageIndex >= pages.Length)
        {
            StartCoroutine(FadeOutAndEnd());
            return;
        }

        StopAllCoroutines();
        StartCoroutine(TypePageText(pages[pageIndex]));
    }

    IEnumerator TypePageText(string text)
    {
        isTyping = true;
        narrativeText.text = "";

        foreach (char c in text)
        {
            narrativeText.text += c;
            if (typingSound != null)
                typingSound.Play();

            yield return new WaitForSeconds(0.1f);
        }

        isTyping = false;
    }

    void SkipOrNextPage()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            narrativeText.text = pages[currentPage];
            isTyping = false;
            return;
        }

        currentPage++;
        ShowPage(currentPage);
    }

    IEnumerator FadeOutAndEnd()
    {
        yield return FadeCanvas(false, () =>
        {
            SceneManager.LoadScene(nextSceneIndex);
        });
    }

    IEnumerator FadeCanvas(bool fadeIn, System.Action callback = null)
    {
        float target = fadeIn ? 1 : 0;
        float start = canvasGroup.alpha;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * 1.25f;
            canvasGroup.alpha = Mathf.Lerp(start, target, time);
            yield return null;
        }

        callback?.Invoke();
    }
}
