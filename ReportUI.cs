using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class ReportUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject toggleGroupPanel;       // Parent of all anomaly toggles
    public Button reportRoomButton;           // Main button to toggle/show report
    public Button finalSubmitButton;          // Button to submit the final report
    public TMP_Text reportRoomButtonText;     // Text on report button

    [Header("Toggles")]
    public List<Toggle> anomalyToggles;      // List of anomaly toggles

    [Header("Dependencies")]
    public AnomalyManager anomalyManager; // Must be assigned in Inspector

    private bool togglesVisible = false;
    private int currentRoomIndex = -1;

    void Start()
    {
        HideAllUI();

        reportRoomButton.onClick.RemoveAllListeners();
        reportRoomButton.onClick.AddListener(OnReportRoomClicked);

        finalSubmitButton.onClick.RemoveAllListeners();
        finalSubmitButton.onClick.AddListener(OnFinalSubmitClicked);

        if (anomalyManager != null)
            OnRoomChanged(anomalyManager.CurrentRoomIndex);
        else
            OnRoomChanged(0);
    }

    public void HideAllUI()
    {
        gameObject.SetActive(false);
        HideToggles();
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
        HideToggles();
        UpdateUIState(currentRoomIndex);
    }

    public void ResetUIForRetry()
    {
        togglesVisible = false;
        HideAllUI();
        UpdateUIState(currentRoomIndex);
    }

    public void OnRoomChanged(int newRoomIndex)
    {
        currentRoomIndex = newRoomIndex;
        HideToggles();
        UpdateUIState(newRoomIndex);
    }

    private void UpdateUIState(int roomIndex)
    {
        if (anomalyManager == null) return;

        bool isConfirmed = anomalyManager.HasReportConfirmedForRoom(roomIndex);

        if (reportRoomButtonText != null)
        {
            if (isConfirmed)
                reportRoomButtonText.text = $"Room {roomIndex + 1}: Report Confirmed (Click to Edit)";
            else if (togglesVisible)
                reportRoomButtonText.text = $"Confirm Report for Room {roomIndex + 1}";
            else
                reportRoomButtonText.text = $"Room {roomIndex + 1}: Check Anomalies";
        }

        bool allRequiredRoomsConfirmed = Enumerable.Range(0, anomalyManager.TotalRooms)
            .All(i => anomalyManager.IsRoomIgnored(i) || anomalyManager.HasReportConfirmedForRoom(i));

        bool isLastRoom = roomIndex == anomalyManager.TotalRooms - 1;

        if (finalSubmitButton != null)
        {
            finalSubmitButton.gameObject.SetActive(isLastRoom);
            finalSubmitButton.interactable = allRequiredRoomsConfirmed && !togglesVisible;
        }
    }

    private void OnReportRoomClicked()
    {
        if (!anomalyManager.IsDetectivePhase)
        {
            // Prevent showing toggles in Exploration Phase
            return;
        }

        if (togglesVisible)
            ConfirmReport();
        else
            ShowToggles();
    }

    private void ShowToggles()
    {
        if (toggleGroupPanel != null)
            toggleGroupPanel.SetActive(true);

        togglesVisible = true;
        ClearToggles();
        UpdateUIState(currentRoomIndex);
    }

    private void HideToggles()
    {
        if (toggleGroupPanel != null)
            toggleGroupPanel.SetActive(false);

        togglesVisible = false;
        UpdateUIState(currentRoomIndex);
    }

    private void ConfirmReport()
    {
        if (anomalyManager == null) return;

        List<AnomalyManager.AnomalyType> selections = new List<AnomalyManager.AnomalyType>();
        for (int i = 0; i < anomalyToggles.Count; i++)
            if (anomalyToggles[i].isOn)
                selections.Add((AnomalyManager.AnomalyType)(i + 1));

        anomalyManager.StoreReportForRoom(selections);
        HideToggles();
    }

    private void ClearToggles()
    {
        foreach (var toggle in anomalyToggles)
            toggle.isOn = false;
    }

    private void OnFinalSubmitClicked()
    {
        if (anomalyManager == null)
        {
            Debug.LogError("ReportUI: AnomalyManager reference is missing! Cannot evaluate report.");
            return;
        }

        finalSubmitButton.interactable = false;
        anomalyManager.EvaluateFinalReport();
    }
}
