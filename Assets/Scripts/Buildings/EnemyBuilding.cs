using UnityEngine;

public abstract class EnemyBuilding : BaseBuilding
{
    private float _internalTimer = 0;

    protected EnemyBuilding(BuildingType type) : base(type, ConflictSide.Immuno)
    {
    }

    public void ApplySettings(ImmunoBuildingSettings settings)
    {
        if (settings == null) return;
        SetUnitProduction(settings.unitProductionPerSecond);
        var hs = GetComponent<HealthSystem>();
        if (hs != null)
        {
            hs.MaxHealth = settings.startHp;
            hs.HealToFull();
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (!_active) return;

        _internalTimer += Time.deltaTime;
        if (_internalTimer >= 1f)
        {
            _internalTimer = 0f;
            _node.CreateUnits(_unitProductionPerSecond, false);
        }
    }
}
