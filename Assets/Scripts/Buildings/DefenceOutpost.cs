using UnityEngine;

public class DefenceOutpost : PlayerBuilding
{
    public DefenceOutpost() : base(BuildingType.DefenceOutpost)
    {
        
    }
    private float _unitProductionTimer = 0f;
    protected override void OnUpdate()
    {

        base.OnUpdate();

        if (!_active) return;
        _unitProductionTimer += Time.deltaTime;
        if (_unitProductionTimer >= 1f)
        {
            _unitProductionTimer = 0f;
            _node.CreateUnits(_unitProductionPerSecond, true);
        }
        
    }
}
