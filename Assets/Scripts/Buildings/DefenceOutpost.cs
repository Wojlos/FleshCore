using UnityEngine;

public class DefenceOutpost : PlayerBuilding
{
    [SerializeField] private TurretController _turret;

    public DefenceOutpost() : base(BuildingType.DefenceOutpost)
    {
        
    }
    private float _unitProductionTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        if (_turret != null && _active)
            _turret.Activate();
    }

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

    public override void Enable()
    {
        base.Enable();
        if (_turret != null)
            _turret.Activate();
    }

    public override void Disable()
    {
        base.Disable();
        if (_turret != null)
            _turret.Deactivate();
    }
}
