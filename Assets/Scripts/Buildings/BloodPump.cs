using UnityEngine;

public class BloodPump : PlayerBuilding
{
    private int maxBuildingIncrease;
    public BloodPump() : base(BuildingType.BloodPump)
    {
        
    }
    protected override void Awake()
    {
        base.Awake();
        var settings = GameManager.Instance.GetGameSettings();
        maxBuildingIncrease = settings.pumpMaxBuildingIncrease;
        ResourceManager.Instance.ChangeMaxBuildings(maxBuildingIncrease);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        ResourceManager.Instance.ChangeMaxBuildings(-maxBuildingIncrease);

    }

    public override void Disable()
    {
        if(!_active) return;
        base.Disable();
        ResourceManager.Instance.ChangeMaxBuildings(-maxBuildingIncrease);
    }

    public override void Enable()
    {
        if(_active) return;
        base.Enable();
        ResourceManager.Instance.ChangeMaxBuildings(maxBuildingIncrease);
    }
}
