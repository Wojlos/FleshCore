using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ResourceCost
{
    public ResourceType type;
    public int cost;
}

[System.Serializable]
public class BuildingSettings
{
    public BuildingType type;
    public Sprite sprite;
    public List<ResourceCost> buildCost = new List<ResourceCost>();
    public List<ResourceCost> upkeep = new List<ResourceCost>();
    public int maxUnitsIncrease = 1;
    public int unitProductionPerSecond = 0;
    public int startHp = 30;
}

[System.Serializable]
public class ImmunoBuildingSettings
{
    public BuildingType type;
    public int unitProductionPerSecond = 0;
    public int startHp = 30;
    public int maxUnitsIncrease = 30;

}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    // ═══════════════════════════════════════════════════════════════════════
    // START CONDITIONS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Start Conditions")]
    
    [Tooltip("Maximum number of buildings the player can have at game start")]
    public int startingMaxBuildings = 1;
    
    [Tooltip("Maximum number of units the player can have at game start")]
    public int startingMaxUnits = 1;
    
    [Tooltip("Resources the player starts with")]
    public List<ResourceCost> startingResources = new List<ResourceCost>();
    
    [Space(5)]
    [Tooltip("Number of units to spawn at game start on the player's starting node")]
    public int startingPlayerUnits = 20;
    
    [Tooltip("Minimum graph distance (node hops) lymph nodes must spawn from any player building")]
    public int minLymphNodeDistanceFromPlayer = 2;

    // ═══════════════════════════════════════════════════════════════════════
    // BUILDING SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Building Settings")]
    
    [Tooltip("How much max building capacity increases per pump built")]
    public int pumpMaxBuildingIncrease = 1;
    
    [Tooltip("Configuration for all player buildings")]
    public List<BuildingSettings> playerBuildingSettings = new List<BuildingSettings>();
    
    [Tooltip("Configuration for all enemy buildings")]
    public List<ImmunoBuildingSettings> immunoBuildingSettings = new List<ImmunoBuildingSettings>();
    
    [Space(5)]
    [Tooltip("How many HP per second player buildings regenerate when not sieged")]
    public float playerBuildingHealingSpeed = 1f;
    
    [Tooltip("How many HP per second enemy buildings regenerate when not sieged")]
    public float immunoBuildingHealingSpeed = 0.5f;

    // ═══════════════════════════════════════════════════════════════════════
    // COMBAT SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Combat Settings")]
    
    [Tooltip("Time interval between battle damage calculations (seconds)")]
    public float battleTickInterval = 0.2f;
    
    [Space(5)]
    [Tooltip("Probability per player unit to deal 1 damage per battle tick (0.0 - 1.0)")]
    public float playerBattleDamageChance = 0.05f;
    
    [Tooltip("Probability per enemy unit to deal 1 damage per battle tick (0.0 - 1.0)")]
    public float enemyBattleDamageChance = 0.05f;
    
    [Space(5)]
    [Tooltip("Probability per player unit to deal 1 damage to buildings during siege (0.0 - 1.0)")]
    public float playerSiegeDamageChance = 0.05f;
    
    [Tooltip("Probability per enemy unit to deal 1 damage to buildings during siege (0.0 - 1.0)")]
    public float enemySiegeDamageChance = 0.05f;

    // ═══════════════════════════════════════════════════════════════════════
    // UNIT MOVEMENT SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Unit Movement Settings")]
    
    [Tooltip("Minimum wait time at each node before player units continue (seconds)")]
    public float playerStepDelayMin = 0.8f;
    
    [Tooltip("Maximum wait time at each node before player units continue (seconds)")]
    public float playerStepDelayMax = 1.2f;
    
    [Space(5)]
    [Tooltip("Minimum wait time at each node before enemy units continue (seconds)")]
    public float enemyStepDelayMin = 0.8f;
    
    [Tooltip("Maximum wait time at each node before enemy units continue (seconds)")]
    public float enemyStepDelayMax = 1.2f;
    
    [Space(5)]
    [Tooltip("Minimum movement speed for player units (animation speed multiplier)")]
    public float playerSpeedMin = 0.8f;
    
    [Tooltip("Maximum movement speed for player units (animation speed multiplier)")]
    public float playerSpeedMax = 1.2f;
    
    [Space(5)]
    [Tooltip("Minimum movement speed for enemy units (animation speed multiplier)")]
    public float enemySpeedMin = 0.8f;
    
    [Tooltip("Maximum movement speed for enemy units (animation speed multiplier)")]
    public float enemySpeedMax = 1.2f;

    // ═══════════════════════════════════════════════════════════════════════
    // ENEMY AI SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Enemy AI Settings")]
    
    [Tooltip("Maximum number of enemy buildings allowed on the map")]
    public int maxImmunoBuildings = 1;
    
    [Tooltip("Maximum number of enemy units at game start")]
    public int maxStartImmunoUnits = 30;
    
    [Tooltip("Maximum number of patrol groups per lymph node")]
    public int maxImmunoPatroleGroups = 1;
    
    [Tooltip("Number of enemy units required to spawn a new lymph node")]
    public int immunoUnitsToSpawnLymphNode = 15;
    
    [Tooltip("Minimum number of units in a patrol group")]
    public int minPatroleSize = 10;
    
    [Tooltip("Duration of lymph node alert phase before returning to normal (seconds)")]
    public float lymphNodeAlertDuration = 30f;

    // ═══════════════════════════════════════════════════════════════════════
    // NODE OWNERSHIP SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Node Ownership Settings")]
    
    [Tooltip("Maximum health points for node ownership system")]
    public int maxNodeHP = 30;
    
    [Space(5)]
    [Tooltip("Color for Immuno-owned nodes")]
    public Color immunoNodeColor = Color.white;
    
    [Tooltip("Color for Player-owned nodes")]
    public Color playerNodeColor = new Color(0.68f, 0.85f, 0.90f); // Light blue
    
    [Space(5)]
    [Tooltip("Probability per player unit to deal 1 damage to node ownership per tick (0.0 - 1.0)")]
    public float playerConquestDamageChance = 0.1f;
    
    [Tooltip("Probability per immuno unit to deal 1 damage to node ownership per tick (0.0 - 1.0)")]
    public float immunoConquestDamageChance = 0.1f;
    
    [Tooltip("Time interval between conquest damage calculations (seconds)")]
    public float nodeConquestTickInterval = 0.5f;
}
