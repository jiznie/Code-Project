using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public event Action OnTimerLow;
    public event Action OnTimerEnd;

    [Header("UI")]
    public Text timerText;

    private float currentTime;
    private bool timerRunning = false;
    private bool isDetectivePhase = false;

    [Range(0f, 1f)] public float warningThreshold = 0.25f;

    // Exploration times per night (seconds)
    private readonly float[] explorationTimes = { 120f, 60f, 30f }; // Night 1,2,3
    // Detective phase times per night (seconds)
    private readonly float[] detectiveTimes = { 90f, 60f, 30f }; // Night 1,2,3

    private Coroutine timerCoroutine;

    // Public read-only access to current timer
    public float CurrentTime => currentTime;

    // Starts the timer for a specific night and phase
    public void StartTimerForNight(int nightIndex, bool detectivePhase = false)
    {
        isDetectivePhase = detectivePhase;
        currentTime = detectivePhase ? GetDetectiveTime(nightIndex) : GetExplorationTime(nightIndex);
        StartTimer();
    }

    private float GetExplorationTime(int nightIndex)
    {
        switch (nightIndex)
        {
            case 4: return explorationTimes[0]; // Night 1
            case 6: return explorationTimes[1]; // Night 2
            case 8: return explorationTimes[2]; // Night 3
            default: return 60f;
        }
    }

    private float GetDetectiveTime(int nightIndex)
    {
        switch (nightIndex)
        {
            case 4: return detectiveTimes[0]; // Night 1
            case 6: return detectiveTimes[1]; // Night 2
            case 8: return detectiveTimes[2]; // Night 3
            default: return 60f;
        }
    }

    private void StartTimer()
    {
        StopTimer();
        timerRunning = true;
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    public void StopTimer()
    {
        timerRunning = false;
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
    }

    public void ResetTimer()
    {
        StopTimer();
        currentTime = 0f;
        UpdateTimerUI();
    }

    private IEnumerator TimerRoutine()
    {
        while (timerRunning && currentTime > 0f)
        {
            UpdateTimerUI();

            if (currentTime <= GetWarningTime() && OnTimerLow != null)
                OnTimerLow.Invoke();

            yield return new WaitForSeconds(1f);
            currentTime -= 1f;
        }

        currentTime = 0f;
        UpdateTimerUI();

        if (OnTimerEnd != null)
            OnTimerEnd.Invoke();
    }

    private float GetWarningTime()
    {
        return currentTime * warningThreshold;
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
