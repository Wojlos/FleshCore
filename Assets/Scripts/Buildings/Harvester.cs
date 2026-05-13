using UnityEngine;
using System.Collections.Generic;

public class Harvester : PlayerBuilding
{
    public Harvester() : base(BuildingType.Harvester)
    {
        
    }
    private Dictionary<ResourceType, int> _resourceProduction =  new Dictionary<ResourceType, int>();
    private float _productionTimer;
    private SawBlade _sawBlade;

    protected override void Awake()
    {
        base.Awake();
        _sawBlade = GetComponentInChildren<SawBlade>();
    }

    public override void ApplySettings(BuildingSettings settings)
    {
        base.ApplySettings(settings);
        CalculateProduction();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, true);
        InitializeSawBlade();
    }

    private void InitializeSawBlade()
    {
        if (_sawBlade == null || _node == null) return;

        var mapSettings = TilemapsManager.Instance.GetMapSettings();
        if (mapSettings == null) return;

        float range = mapSettings.resourceGatheringRange;
        var edgePoints = TilemapsManager.Instance.GetEdgeTilePositionsInRange(_node.transform.position, range);

        var gradients = new Dictionary<ResourceType, Gradient>();
        foreach (var resource in mapSettings.resources)
        {
            if (resource.particleColorGradient != null)
                gradients[resource.resourceType] = resource.particleColorGradient;
        }

        _sawBlade.Initialize(edgePoints, gradients, _node.transform.position);
        _sawBlade.SetActive(true);
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
        if (_active)
        {
            ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, false);
        }
        base.Cleanup();
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
        if (_sawBlade != null) _sawBlade.SetActive(false);
    }

    public override void Enable()
    {
        if(_active) return;

        base.Enable();
        ResourceManager.Instance.ChangeResourcesPerSecond(_resourceProduction, true);
        if (_sawBlade != null) _sawBlade.SetActive(true);
    }
    

}
