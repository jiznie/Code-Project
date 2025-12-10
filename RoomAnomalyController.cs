using UnityEngine;
using System.Collections.Generic;

public class RoomAnomalyController : MonoBehaviour
{
    [System.Serializable]
    public class AnomalyObject
    {
        public AnomalyManager.AnomalyType type;
        public GameObject anomalyVisual;
    }

    [Header("Anomaly Configuration")]
    public List<AnomalyObject> allPossibleAnomalies;

    private List<AnomalyManager.AnomalyType> _activeAnomalies = new List<AnomalyManager.AnomalyType>();

    void Awake()
    {
        // Room must stay active forever — ONLY hide content
        HideRoom();
        HideAllAnomalies();
    }

    // Called by AnomalyManager at Start()
    public void SetupAnomalies(List<AnomalyManager.AnomalyType> expectedAnomaliesForThisNight)
    {
        _activeAnomalies = expectedAnomaliesForThisNight;
        HideAllAnomalies();
    }

    // ❗ FIXED VERSION: do NOT deactivate the room GameObject anymore
    public void HideRoom()
    {
        // Hide all children (visuals, props) but keep root active
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // Always hide anomaly visuals
        HideAllAnomalies();
    }

    // Shows room contents (visuals only)
    public void ShowRoom()
    {
        // Ensure room root is active
        gameObject.SetActive(true);

        // Enable children (visible content)
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        // During exploration, anomalies remain hidden
    }

    // Shows only anomalies that are active for the night
    public void ShowAllAnomalies()
    {
        foreach (var anomaly in allPossibleAnomalies)
        {
            if (anomaly.anomalyVisual != null)
            {
                bool active = _activeAnomalies.Contains(anomaly.type);
                anomaly.anomalyVisual.SetActive(active);
            }
        }
    }

    // Hides all anomaly visuals
    public void HideAllAnomalies()
    {
        foreach (var anomaly in allPossibleAnomalies)
        {
            if (anomaly.anomalyVisual != null)
            {
                anomaly.anomalyVisual.SetActive(false);
            }
        }
    }

    // Returns anomalies present in this room
    public List<AnomalyManager.AnomalyType> GetAllAnomalies()
    {
        if (_activeAnomalies == null || _activeAnomalies.Count == 0)
            return new List<AnomalyManager.AnomalyType> { AnomalyManager.AnomalyType.None };

        return _activeAnomalies;
    }
}
