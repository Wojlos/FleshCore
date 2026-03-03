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

        if (!_active || _unitProductionPerSecond <= 0) return;

        float spawnInterval = 1f / _unitProductionPerSecond;
        _unitProductionTimer += Time.deltaTime;
        
        if (_unitProductionTimer >= spawnInterval)
        {
            _unitProductionTimer -= spawnInterval;
            _node.CreateUnits(1, true);
        }
        
    }
}
