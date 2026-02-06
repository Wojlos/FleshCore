using UnityEngine;
using System.Collections.Generic;

public class Harvester : BaseBuilding
{
    public Harvester() : base(BuildingType.Harvester)
    {
        
    }
    private Dictionary<ResourceType, int> _resourceProduction =  new Dictionary<ResourceType, int>();
    private float _productionTimer;
    private void Awake() {
        _resourceProduction[ResourceType.Blood] = 1;
        _resourceProduction[ResourceType.Lipids] = 1;
        _resourceProduction[ResourceType.Bone] = 1;
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, true);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, false);

    }

    void Update()
    {

        _productionTimer += Time.deltaTime;

        if (_productionTimer >= 1f)
        {
            _productionTimer = 0f;
            ResourceManager.Instance.ChangeResourcesCurrent(_resourceProduction, true);
        }
    }

}
