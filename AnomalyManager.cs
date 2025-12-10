using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnomalyManager : MonoBehaviour
{
    public enum AnomalyType { None, Imagery, Displacement, Intruder, Unknown }

    [Header("Rooms")]
    public RoomAnomalyController[] rooms; // Must be size 3

    [Header("UI Panels")]
    public GameObject explorationCompletePanel;
    public GameObject reportTogglePanel;
    public GameObject timeUpPanel;
    public TMP_Text timeUpText;
    public GameObject warningPanel;
    public TMP_Text warningText;
    public GameObject nightCompletePanel;
    public CanvasGroup nightCompleteCanvasGroup;
    public GameObject gameOverPanel;

    [Header("Text / Overlay")]
    public Image distortionOverlay;

    [Header("Report UI")]
    public ReportUI reportUI;

    [Header("Night Config")]
    public NightConfig nightConfig;

    [Header("Timer & Music")]
    public TimerManager timerManager;
    public MusicManager musicManager;

    private const int NIGHT_1_INDEX = 4;
    private const int NIGHT_3_INDEX = 8;
    private const int GAME_OVER_INDEX = 9;
    private const int ENDING_SUCCESS_INDEX = 10;

    private int currentRoom = 0;
    private bool detectivePhase = false;
    private bool nightCompleted = false;
    private bool[] visitedRooms;
    private int remainingLives = 3;
    private List<List<AnomalyType>> finalReportSelections;
    private bool[] roomReportConfirmed;

    public int TotalRooms => rooms.Length;
    public int CurrentRoomIndex => currentRoom;
    public bool IsDetectivePhase => detectivePhase;
    [Range(0f, 1f)] public float intenseThreshold = 0.25f;

    void Awake()
    {
        if (rooms == null || rooms.Length != 3)
            Debug.LogError("AnomalyManager: The 'rooms' array must be size 3 and fully assigned in the Inspector.");

        finalReportSelections = new List<List<AnomalyType>>(rooms.Length);
        roomReportConfirmed = new bool[rooms.Length];
        for (int i = 0; i < rooms.Length; i++)
        {
            finalReportSelections.Add(new List<AnomalyType>());
            roomReportConfirmed[i] = false;
        }
        visitedRooms = new bool[rooms.Length];
    }

    void Start()
    {
        foreach (var r in rooms) if (r != null) r.HideRoom();

        int nightIndex = SceneManager.GetActiveScene().buildIndex;
        remainingLives = GetLivesForNight(nightIndex);
        SetupNightAnomalies(nightIndex);

        explorationCompletePanel?.SetActive(false);
        timeUpPanel?.SetActive(false);
        warningPanel?.SetActive(false);
        nightCompletePanel?.SetActive(false);

        if (nightCompleteCanvasGroup != null)
        {
            nightCompleteCanvasGroup.alpha = 0f;
            nightCompleteCanvasGroup.interactable = false;
            nightCompleteCanvasGroup.blocksRaycasts = false;
        }

        gameOverPanel?.SetActive(false);

        if (timerManager != null)
        {
            timerManager.OnTimerLow += HandleTimerLow;
            timerManager.OnTimerEnd += HandleTimerEnd;
        }

        currentRoom = 0;
        if (rooms.Length > 0 && rooms[currentRoom] != null)
        {
            rooms[currentRoom].ShowRoom();
            visitedRooms[currentRoom] = true;
        }

        detectivePhase = false;
        nightCompleted = false;

        EnterExplorationPhase();
    }

    public bool IsRoomIgnored(int roomIndex)
    {
        return SceneManager.GetActiveScene().buildIndex == NIGHT_3_INDEX && roomIndex == 1;
    }

    public void EnterExplorationPhase()
    {
        detectivePhase = false;
        foreach (var r in rooms) if (r != null) r.HideAllAnomalies();
        reportUI?.HideAllUI();
        int nightIndex = SceneManager.GetActiveScene().buildIndex;
        timerManager?.StartTimerForNight(nightIndex, false); // Exploration
        musicManager?.PlayNormal();
        Debug.Log("[AnomalyManager] Entered Exploration Phase.");
    }

    public void EndExplorationPhase()
    {
        if (detectivePhase || nightCompleted) return;

        timerManager?.StopTimer();
        reportUI?.HideAllUI();

        StartCoroutine(StartDetectivePhaseSetup(0f));
    }

    IEnumerator StartDetectivePhaseSetup(float delay)
    {
        yield return new WaitForSeconds(delay);

        detectivePhase = true;

        foreach (var r in rooms)
            if (r != null) r.HideRoom();

        currentRoom = 0;
        if (rooms.Length > 0 && rooms[currentRoom] != null)
            rooms[currentRoom].ShowRoom();

        foreach (var r in rooms)
            r?.ShowAllAnomalies();

        reportUI?.ShowUI();
        reportUI?.OnRoomChanged(currentRoom);

        if (nightCompleteCanvasGroup != null)
            nightCompleteCanvasGroup.interactable = true;

        timerManager?.StartTimerForNight(SceneManager.GetActiveScene().buildIndex, true); // Detective
    }

    void ResetStateForNextAttempt()
    {
        for (int i = 0; i < finalReportSelections.Count; i++) finalReportSelections[i].Clear();
        for (int i = 0; i < roomReportConfirmed.Length; i++) roomReportConfirmed[i] = false;

        timeUpPanel?.SetActive(false);
        warningPanel?.SetActive(false);

        StartCoroutine(StartDetectivePhaseSetup(0f));
        reportUI?.ResetUIForRetry();
        timerManager?.ResetTimer();
        musicManager?.PlayNormal();
    }

    int GetLivesForNight(int nightIndex)
    {
        if (nightIndex == NIGHT_1_INDEX) return 3;
        if (nightIndex == NIGHT_1_INDEX + 2) return 3;
        if (nightIndex == NIGHT_3_INDEX) return 2;
        return 1;
    }

    void HandleTimerLow() => musicManager?.PlayIntense();

    void HandleTimerEnd()
    {
        timerManager?.StopTimer();
        musicManager?.PlayNormal();

        if (detectivePhase)
        {
            // Detective phase timer ended → game over
            timeUpText.text = "Time's up!\nReport Failed!";
            ShowGameOver();
        }
        else
        {
            // Exploration phase ended → move to detective automatically
            Debug.Log("Exploration timer ended. Moving to Detective Phase automatically.");
            EndExplorationPhase();
        }
    }

    public void OnPlayerChooseTimeUpRetry()
    {
        remainingLives--;
        if (remainingLives <= 0) { ShowGameOver(); return; }
        ResetStateForNextAttempt();
    }

    public void OnPlayerChooseTimeUpExit() => SceneManager.LoadScene(1);

    public void GoToRoom(int index)
    {
        if (index < 0 || index >= rooms.Length) return;
        rooms[currentRoom]?.HideRoom();
        currentRoom = index;
        rooms[currentRoom]?.ShowRoom();

        if (detectivePhase)
        {
            rooms[currentRoom].ShowAllAnomalies();
            reportUI?.OnRoomChanged(currentRoom);
        }
        else
        {
            visitedRooms[currentRoom] = true;
        }
    }

    public void StoreReportForRoom(List<AnomalyType> selections)
    {
        if (currentRoom >= 0 && currentRoom < finalReportSelections.Count)
        {
            finalReportSelections[currentRoom].Clear();
            if (selections != null)
                finalReportSelections[currentRoom].AddRange(selections.Where(s => s != AnomalyType.None));

            roomReportConfirmed[currentRoom] = true;
        }
    }

    public bool HasReportConfirmedForRoom(int roomIndex)
    {
        if (roomIndex >= 0 && roomIndex < roomReportConfirmed.Length) return roomReportConfirmed[roomIndex];
        return false;
    }

    public void EvaluateFinalReport()
    {
        if (!detectivePhase || nightCompleted) return;

        bool allCorrect = true;
        int currentNightIndex = SceneManager.GetActiveScene().buildIndex;

        for (int i = 0; i < rooms.Length; i++)
        {
            if (IsRoomIgnored(i)) continue;
            if (!roomReportConfirmed[i]) { allCorrect = false; break; }
        }

        if (allCorrect)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                if (IsRoomIgnored(i)) continue;
                if (rooms[i] == null) continue;

                var correctSet = new HashSet<AnomalyType>(rooms[i].GetAllAnomalies().Where(a => a != AnomalyType.None));
                var playerSet = new HashSet<AnomalyType>(finalReportSelections[i]);

                if (!playerSet.SetEquals(correctSet))
                {
                    allCorrect = false;
                    break;
                }
            }
        }

        if (allCorrect)
        {
            nightCompleted = true;
            timerManager?.StopTimer();
            musicManager?.PlayNormal();

            if (currentNightIndex == NIGHT_3_INDEX)
                SceneManager.LoadScene(ENDING_SUCCESS_INDEX);
            else
                SceneManager.LoadScene(currentNightIndex + 1);
        }
        else
        {
            remainingLives--;
            if (remainingLives <= 0 || currentNightIndex == NIGHT_3_INDEX) ShowGameOver();
            else StartCoroutine(ShowWarningAndReset());
        }
    }

    IEnumerator ShowWarningAndReset()
    {
        warningPanel?.SetActive(true);
        if (warningText != null) warningText.text = $"Incorrect report. {remainingLives} lives remaining.";
        if (distortionOverlay != null)
        {
            Color c = distortionOverlay.color;
            c.a = Mathf.Min(distortionOverlay.color.a + 0.15f, 1f);
            distortionOverlay.color = c;
        }

        yield return new WaitForSeconds(2f);
        warningPanel?.SetActive(false);

        ResetStateForNextAttempt();
    }

    void ShowGameOver()
    {
        reportUI?.HideAllUI();
        timerManager?.StopTimer();
        musicManager?.PlayNormal();
        Debug.LogWarning("GAME OVER: Loading scene index 9.");
        SceneManager.LoadScene(GAME_OVER_INDEX);
    }

    public void RetryNight() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void ExitToMainMenu() => SceneManager.LoadScene(1);

    void SetupNightAnomalies(int nightIndex)
    {
        int configIndex = -1;
        if (nightIndex == NIGHT_1_INDEX || nightIndex == NIGHT_1_INDEX + 2 || nightIndex == NIGHT_3_INDEX)
            configIndex = (nightIndex - NIGHT_1_INDEX) / 2;

        if (nightConfig == null)
        {
            Debug.LogError("NightConfig asset is not assigned.");
            return;
        }

        if (configIndex >= 0 && configIndex < nightConfig.correctPerNight.Count)
        {
            var nightConfigData = nightConfig.correctPerNight[configIndex];

            if (nightConfigData.roomConfigurations.Count != rooms.Length)
                Debug.LogError("NightConfig room count does not match scene rooms.");
            else
            {
                for (int i = 0; i < rooms.Length; i++)
                    rooms[i]?.SetupAnomalies(nightConfigData.roomConfigurations[i].expectedAnomalies);
            }
        }
        else
        {
            for (int i = 0; i < rooms.Length; i++)
                rooms[i]?.SetupAnomalies(new List<AnomalyType>());
        }
    }

    void OnDestroy()
    {
        if (timerManager != null)
        {
            timerManager.OnTimerLow -= HandleTimerLow;
            timerManager.OnTimerEnd -= HandleTimerEnd;
        }
    }
}
