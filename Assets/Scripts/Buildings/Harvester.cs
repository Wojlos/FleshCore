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

    protected override void OnUpdate()
    {

        base.OnUpdate();
        if (!_active) return;
        _productionTimer += Time.deltaTime;

        if (_productionTimer >= 1f)
        {
            _productionTimer = 0f;
            ResourceManager.Instance.TryChangeResourcesCurrent(_resourceProduction, true);
        }
    }

    protected override void Disable()
    {
        if(!_active) return;

        base.Disable();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, false);
    }

    protected override void Enable()
    {
        if(_active) return;

        base.Enable();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, true);
    }
    

}
