using UnityEngine;
using System.Collections.Generic;
using static AnomalyManager; // Use AnomalyManager's enum

// Defines a list of anomalies expected for a single room.
[System.Serializable]
public class RoomAnomalyList
{
    public List<AnomalyManager.AnomalyType> expectedAnomalies = new List<AnomalyManager.AnomalyType>();
}

// Defines the configuration for all rooms within a single night.
[System.Serializable]
public class NightConfiguration
{
    // FIX: Added missing property to resolve CS1061 error in AnomalyManager.StartExplorationPhase()
    [Tooltip("The total time in seconds allowed for the exploration phase for this night.")]
    public float explorationTime = 60f;

    [Tooltip("The ordered list of configurations for each room (Room 1, Room 2, ...).")]
    public List<RoomAnomalyList> roomConfigurations = new List<RoomAnomalyList>();
}

// Defines a Scriptable Object asset to configure anomalies for each night.
[CreateAssetMenu(fileName = "NightConfig", menuName = "Config/Night Config")]
public class NightConfig : ScriptableObject
{
    public List<NightConfiguration> correctPerNight = new List<NightConfiguration>();
}