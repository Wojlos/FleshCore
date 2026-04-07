using UnityEngine;
using System.Collections.Generic;

public class Harvester : PlayerBuilding
{
    public Harvester() : base(BuildingType.Harvester)
    {
        
    }
    private Dictionary<ResourceType, int> _resourceProduction =  new Dictionary<ResourceType, int>();
    private float _productionTimer;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void ApplySettings(BuildingSettings settings)
    {
        base.ApplySettings(settings);
        CalculateProduction();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, true);
    }

    private void CalculateProduction()
    {
        _resourceProduction.Clear();

        if (_node == null) return;

        var mapSettings = TilemapsManager.Instance.GetMapSettings();
        if (mapSettings == null) return;

        foreach (var resource in mapSettings.resources)
        {
            int tileCount = _node.GetResourceTileCount(resource.resourceType);
            int production = CalculateProductionForResource(tileCount, resource.tilesPerOneResource);
            _resourceProduction[resource.resourceType] = production;
        }
    }

    private int CalculateProductionForResource(int tileCount, int tilesPerOneResource)
    {
        if (tileCount <= 0) return 0;
        return Mathf.CeilToInt((float)tileCount / tilesPerOneResource);
    }

    public static Dictionary<ResourceType, int> CalculateProductionForNode(BaseNode node)
    {
        var production = new Dictionary<ResourceType, int>();

        var mapSettings = TilemapsManager.Instance.GetMapSettings();
        if (mapSettings == null || node == null) return production;

        foreach (var resource in mapSettings.resources)
        {
            int tileCount = node.GetResourceTileCount(resource.resourceType);
            if (tileCount <= 0)
            {
                production[resource.resourceType] = 0;
                continue;
            }
            production[resource.resourceType] = Mathf.CeilToInt((float)tileCount / resource.tilesPerOneResource);
        }

        return production;
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

    public override void Disable()
    {
        if(!_active) return;

        base.Disable();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, false);
    }

    public override void Enable()
    {
        if(_active) return;

        base.Enable();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, true);
    }
    

}
