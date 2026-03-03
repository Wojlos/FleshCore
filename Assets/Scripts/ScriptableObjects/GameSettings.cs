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
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("start conditions")]
    public int startingMaxBuildings =  1;
    public int startingMaxUnits =  1;

    public List<ResourceCost> startingResources = new List<ResourceCost>();

    [Header("building settings")]
    public int pumpMaxBuildingIncrease = 1;

    public List<BuildingSettings> playerBuildingSettings = new List<BuildingSettings>();
    public List<ImmunoBuildingSettings> immunoBuildingSettings = new List<ImmunoBuildingSettings>();

    [Header("Enemy settings")]

    public int maxImmunoBuildings = 1;
    public int maxImmunoUnits = 30;
    public int maxImmunoPatroleGroups = 1;
    public int minPatroleSize = 10;


}
