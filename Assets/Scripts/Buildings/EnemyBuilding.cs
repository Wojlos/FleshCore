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

        if (!_active || _unitProductionPerSecond <= 0) return;

        float spawnInterval = 1f / _unitProductionPerSecond;
        _internalTimer += Time.deltaTime;
        
        if (_internalTimer >= spawnInterval)
        {
            _internalTimer -= spawnInterval;
            _node.CreateUnits(1, false);
        }
    }
}
