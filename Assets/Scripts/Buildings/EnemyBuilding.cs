using UnityEngine;

public abstract class EnemyBuilding : BaseBuilding
{
    protected EnemyBuilding(BuildingType type) : base(type)
    {
    }

    public virtual void ApplySettings(ImmunoBuildingSettings settings)
    {
        if (settings == null) return;

        SetUnitProduction(settings.unitProductionPerSecond);
    }
}
