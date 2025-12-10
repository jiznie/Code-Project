using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class NarrativeOnlyManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text narrativeText;

    [TextArea]
    public string narrative =
        "You've been hired as the night guard of the museum...\nBe vigilant tonight.";

    [TextArea]
    public string nextScenePrompt =
        "Day 1: The Silent Gallery\n\nPress any key to continue.";

    public float typingSpeed = 0.03f;
    public string nextSceneName = "Night1"; // Scene to load after narrative

    private void Start()
    {
        if (narrativeText != null) narrativeText.text = "";
        StartCoroutine(RunNarrativeSequence());
    }

    private IEnumerator RunNarrativeSequence()
    {
        // Type the main narrative
        yield return StartCoroutine(TypeText(narrative));

        // Type the scene prompt
        yield return StartCoroutine(TypeText(nextScenePrompt));

        // Wait for any key press
        while (!Input.anyKeyDown) yield return null;

        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator TypeText(string textToType)
    {
        narrativeText.text = "";
        foreach (char c in textToType)
        {
            narrativeText.text += c;
            if (Input.anyKeyDown)
            {
                narrativeText.text = textToType; // Skip typing
                break;
            }
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
