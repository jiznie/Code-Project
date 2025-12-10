using UnityEngine;
using TMPro;

public class ClockManager : MonoBehaviour
{
    public TimerManager timerManager;
    public AnomalyManager anomalyManager;
    public TMP_Text timeText;

    void Update()
    {
        if (timerManager == null || anomalyManager == null || timeText == null) return;

        float currentTime = timerManager.CurrentTime;

        // Format time as M:SS (e.g., 01:30)
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (anomalyManager.IsDetectivePhase)
        {
            // Now correctly shows time during Detective Phase
            timeText.text = $"DETECTIVE PHASE\n{timeString}";
        }
        else
        {
            // Shows time during Exploration Phase
            timeText.text = $"EXPLORATION\n{timeString}";
        }
    }
}