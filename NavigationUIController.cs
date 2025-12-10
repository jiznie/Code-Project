using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Needed for Coroutines

public class NavigationUIController : MonoBehaviour
{
    public Button leftButton;
    public Button rightButton;
    public AnomalyManager anomalyManager;
    public TMP_Text roomText; // Optional: For displaying "Room X/Y"

    // Flag to prevent double-navigation from fast clicks or multiple listeners
    private bool _canNavigate = true;
    private const float NAVIGATION_COOLDOWN = 0.2f;

    void Start()
    {
        if (leftButton != null) leftButton.onClick.AddListener(OnLeft);
        if (rightButton != null) rightButton.onClick.AddListener(OnRight);
        UpdateButtons();
    }

    // FIX: Added Update to continuously refresh button interactivity state and text
    void Update()
    {
        if (anomalyManager == null) return;

        UpdateButtons();

        // --- Keyboard Navigation Controls ---
        if (_canNavigate)
        {
            int currentRoom = anomalyManager.CurrentRoomIndex;
            int totalRooms = anomalyManager.TotalRooms;

            if (Input.GetKeyDown(KeyCode.LeftArrow) && currentRoom > 0)
            {
                // Navigate left and start cooldown
                OnLeft();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) && currentRoom < totalRooms - 1)
            {
                // Navigate right and start cooldown
                OnRight();
            }
        }
        // ------------------------------------
    }

    public void OnLeft()
    {
        if (anomalyManager == null || !_canNavigate) return;

        int idx = anomalyManager.CurrentRoomIndex - 1;
        anomalyManager.GoToRoom(idx);

        // Start cooldown to prevent double-skipping
        StartCoroutine(NavigationCooldown());
    }

    public void OnRight()
    {
        if (anomalyManager == null || !_canNavigate) return;

        int idx = anomalyManager.CurrentRoomIndex + 1;
        anomalyManager.GoToRoom(idx);

        // Start cooldown to prevent double-skipping
        StartCoroutine(NavigationCooldown());
    }

    // Coroutine to reset the navigation flag after a short delay
    IEnumerator NavigationCooldown()
    {
        _canNavigate = false;
        yield return new WaitForSeconds(NAVIGATION_COOLDOWN);
        _canNavigate = true;
    }

    void UpdateButtons()
    {
        if (anomalyManager == null) return;
        int currentRoom = anomalyManager.CurrentRoomIndex;
        int totalRooms = anomalyManager.TotalRooms;

        if (leftButton != null)
        {
            // Disable left button only if in the first room (index 0)
            leftButton.interactable = currentRoom > 0 && _canNavigate;
        }

        if (rightButton != null)
        {
            // Disable right button only if in the last room
            rightButton.interactable = currentRoom < totalRooms - 1 && _canNavigate;
        }

        // Update the Room text display
        if (roomText != null)
        {
            roomText.text = $"Room {currentRoom + 1}/{totalRooms}";
        }

        // Update button colors based on interactivity
        if (leftButton != null)
        {
            ColorBlock cb = leftButton.colors;
            cb.normalColor = leftButton.interactable ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.5f);
            leftButton.colors = cb;
        }
        if (rightButton != null)
        {
            ColorBlock cb = rightButton.colors;
            cb.normalColor = rightButton.interactable ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.5f);
            rightButton.colors = cb;
        }
    }
}