using UnityEngine;
using System.Collections.Generic;

public abstract class PlayerBuilding : BaseBuilding
{
    private List<ResourceCost> _upkeep = new List<ResourceCost>();
    private int _maxUnitsIncrease = 0;
    private float _upkeepTimer = 0;

    protected PlayerBuilding(BuildingType type) : base(type, ConflictSide.Player)
    {
    }

    public void SetUpkeep(List<ResourceCost> upkeep)
    {
        _upkeep = upkeep;
    }

    public override void ApplySettings(BuildingSettings settings)
    {
        base.ApplySettings(settings);
        if (settings == null) return;

        SetUpkeep(settings.upkeep);
        _maxUnitsIncrease = settings.maxUnitsIncrease;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        _upkeepTimer += Time.deltaTime;

        if (_upkeepTimer >= 1f)
        {
            _upkeepTimer = 0f;
            if(!ResourceManager.Instance.TryChangeResourcesCurrent(_upkeep, false))
            {
                Disable();
            }
            else if (!_isSieged)
            {
                Enable();
            }
        }      
    }

    public override void Cleanup()
    {
        base.Cleanup();
        ResourceManager.Instance.ChangeResourcesPerSecond(_upkeep, true);
        ResourceManager.Instance.ChangeMaxUnits(-_maxUnitsIncrease);
    }
}
