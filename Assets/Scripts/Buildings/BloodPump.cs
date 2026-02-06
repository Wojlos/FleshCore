using UnityEngine;

public class BloodPump : BaseBuilding
{
    private int maxBuildingIncrease;
    public BloodPump() : base(BuildingType.BloodPump)
    {
        
    }
    private void Awake() {
        var settings = GameManager.Instance.GetGameSettings();
        maxBuildingIncrease = settings.pumpMaxBuildingIncrease;

        ResourceManager.Instance.ChangeMaxBuildings(maxBuildingIncrease);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        ResourceManager.Instance.ChangeMaxBuildings(-maxBuildingIncrease);

    }

}
