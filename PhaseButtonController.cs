using UnityEngine;

public class PhaseButtonController : MonoBehaviour
{
    public AnomalyManager anomalyManager;
    public GameObject buttonObject; // The button (or panel containing the button) to be controlled

    void Start()
    {
        if (anomalyManager == null)
        {
            Debug.LogError("PhaseButtonController: AnomalyManager not assigned!");
            // Disable the script if the manager isn't assigned to prevent errors
            enabled = false; 
            return;
        }

        UpdateButtonVisibility();
    }

    void Update()
    {
        // 🔥 FIX: Added null check for safety
        if (anomalyManager == null)
        {
            enabled = false;
            return;
        }
        
        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        // Ensure both the button object and the manager are available before proceeding
        if (buttonObject == null || anomalyManager == null) return;
        
        // Button should be visible if we are NOT in the Detective Phase (i.e., we are in the Exploration Phase)
        bool shouldBeVisible = !anomalyManager.IsDetectivePhase;
        
        // Only call SetActive if the state needs to change, optimizing performance
        if (buttonObject.activeSelf != shouldBeVisible)
            buttonObject.SetActive(shouldBeVisible);
    }
}