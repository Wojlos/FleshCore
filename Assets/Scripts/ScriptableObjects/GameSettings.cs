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
    [TextArea(2, 5)]
    public string description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
    public List<ResourceCost> buildCost = new List<ResourceCost>();
    public List<ResourceCost> upkeep = new List<ResourceCost>();
    public List<ResourceCost> production = new List<ResourceCost>();
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
    
    [Space(5)]
    [Tooltip("Color for player unit travel groups")]
    public Color playerUnitGroupColor = new Color(0f, 0f, 0.5f); // Dark blue
    
    [Tooltip("Color for immuno unit travel groups")]
    public Color immunoUnitGroupColor = new Color(0.5f, 0.5f, 0f); // Dark yellow

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
    // TURRET / TARGET GROUP SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("Turret Settings")]

    [Tooltip("World-space detection radius for defence outpost turrets")]
    [Min(0.5f)]
    public float turretRange = 5f;

    [Tooltip("Color of the turret range circle visualization")]
    public Color turretRangeVisualColor = new Color(1f, 0.3f, 0.3f, 0.6f);

    [Tooltip("Show turret range circle when hovering over DefenceOutpost button")]
    public bool showTurretRangeOnHover = true;

    [Tooltip("Probability per turret bullet hit to kill 1 enemy unit (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float turretDamageChance = 0.35f;

    [Header("Target Group Settings")]

    [Tooltip("Radius around the group center in which swarm targets wander")]
    [Min(0.1f)]
    public float targetGroupWanderRadius = 1.5f;

    [Tooltip("Minimum wander speed for individual swarm targets")]
    [Min(0.01f)]
    public float swarmTargetSpeedMin = 0.3f;

    [Tooltip("Maximum wander speed for individual swarm targets")]
    [Min(0.01f)]
    public float swarmTargetSpeedMax = 0.8f;

    [Tooltip("SmoothDamp follow time for swarm target visuals (higher = laggier/looser trail)")]
    [Min(0.01f)]
    public float swarmTargetFollowSmoothing = 0.2f;

    [Tooltip("Radius the swarm center orbits around a node while the group is idle on it")]
    [Min(0f)]
    public float targetGroupOrbitRadius = 1.2f;

    [Tooltip("Angular speed of the swarm center orbit in degrees per second")]
    public float targetGroupOrbitSpeed = 30f;

    // ═══════════════════════════════════════════════════════════════════════
    // SELECTION SETTINGS
    // ═══════════════════════════════════════════════════════════════════════
    [Header("UI Settings")]
    
    [Tooltip("Delay before showing building tooltip on hover (seconds)")]
    public float tooltipHoverDelay = 0.1f;

    [Header("Selection Settings")]
    
    [Tooltip("Minimum coverage percentage required to select units from a node (0.0 - 1.0, default 0 = any coverage)")]
    [Range(0f, 1f)]
    public float minSelectionCoverageThreshold = 0f;
    
    [Tooltip("Number of sample points per unit radius for coverage calculation (higher = more accurate but slower)")]
    [Range(3, 20)]
    public int selectionSamplePointsPerRadius = 8;

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
