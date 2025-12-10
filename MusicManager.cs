using System.Collections;
using UnityEngine;

/// <summary>
/// MusicManager: controls two AudioSources (normal and intense) and crossfades between them.
/// Slider-compatible: moving volume to 0 stops audio; moving back >0 resumes it.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource normalMusic;
    public AudioSource intenseMusic;

    [Header("Crossfade")]
    public float crossfadeDuration = 1.0f;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    private Coroutine crossfadeCoroutine;

    void Awake()
    {
        // Defensive checks
        if (normalMusic == null) Debug.LogWarning("MusicManager: normalMusic not assigned.");
        if (intenseMusic == null) Debug.LogWarning("MusicManager: intenseMusic not assigned.");

        // Initialize volumes
        if (normalMusic != null) normalMusic.volume = masterVolume;
        if (intenseMusic != null) intenseMusic.volume = 0f;

        // Start music if available
        PlayMusicIfStopped(normalMusic);
        PlayMusicIfStopped(intenseMusic);
    }

    public void PlayNormal()
    {
        PlayMusicIfStopped(normalMusic);
        PlayMusicIfStopped(intenseMusic);
        StartCrossfade(normalMusic, intenseMusic);
    }

    public void PlayIntense()
    {
        PlayMusicIfStopped(normalMusic);
        PlayMusicIfStopped(intenseMusic);
        StartCrossfade(intenseMusic, normalMusic);
    }

    public void StopAllMusic()
    {
        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = null;
        }

        if (normalMusic != null) normalMusic.Stop();
        if (intenseMusic != null) intenseMusic.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);

        // Normal track
        if (normalMusic != null)
        {
            if (normalMusic.isPlaying)
                normalMusic.volume = masterVolume;
            else if (masterVolume > 0f)
            {
                normalMusic.volume = masterVolume;
                normalMusic.Play();
            }
        }

        // Intense track
        if (intenseMusic != null)
        {
            if (intenseMusic.isPlaying)
                intenseMusic.volume = masterVolume;
            else if (masterVolume > 0f)
            {
                intenseMusic.volume = masterVolume;
                intenseMusic.Play();
            }
        }
    }

    public bool IsPlaying()
    {
        return (normalMusic != null && normalMusic.isPlaying) ||
               (intenseMusic != null && intenseMusic.isPlaying);
    }

    public void PlayMusic()
    {
        PlayMusicIfStopped(normalMusic);
        PlayMusicIfStopped(intenseMusic);
    }

    private void PlayMusicIfStopped(AudioSource source)
    {
        if (source != null && !source.isPlaying && source.clip != null)
            source.Play();
    }

    private void StartCrossfade(AudioSource targetOn, AudioSource targetOff)
    {
        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(targetOn, targetOff));
    }

    private IEnumerator CrossfadeRoutine(AudioSource fadeIn, AudioSource fadeOut)
    {
        if (fadeIn == null && fadeOut == null) yield break;

        PlayMusicIfStopped(fadeIn);
        PlayMusicIfStopped(fadeOut);

        float elapsed = 0f;
        float startIn = (fadeIn != null) ? fadeIn.volume : 0f;
        float startOut = (fadeOut != null) ? fadeOut.volume : 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);

            if (fadeIn != null) fadeIn.volume = Mathf.Lerp(startIn, masterVolume, t);
            if (fadeOut != null) fadeOut.volume = Mathf.Lerp(startOut, 0f, t);

            yield return null;
        }

        if (fadeIn != null) fadeIn.volume = masterVolume;
        if (fadeOut != null) fadeOut.volume = 0f;
        if (fadeOut != null) fadeOut.Stop();

        crossfadeCoroutine = null;
    }
}
