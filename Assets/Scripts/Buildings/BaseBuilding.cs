using UnityEngine;
using System.Collections.Generic;

public abstract class BaseBuilding : MonoBehaviour
{
    [SerializeField] private GameObject _disabledMarker;
    private BuildingType _type;
    protected BaseNode _node;
    protected int _unitProductionPerSecond = 0;

    protected bool _active = true;

    protected BaseBuilding(BuildingType type)
    {
        _type = type;
    }
    public void AssignNode(BaseNode node)
    {
        _node = node;
        _node.AssignBuilding(this);
    }

    public virtual void Cleanup()
    {
        
    }

    void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
        
    }

    public virtual void ApplySettings(BuildingSettings settings)
    {
        if (settings == null) return;

        SetUnitProduction(settings.unitProductionPerSecond);
    }
    public BuildingType GetType()=> _type;
    public void SetUnitProduction(int u)
    {
        _unitProductionPerSecond = u;
        
    }
    public virtual void Disable()
    {
        _active = false;
        if (_disabledMarker != null)
            _disabledMarker.SetActive(true);
        
    }
    public virtual void Enable()
    {
        _active = true;
        if (_disabledMarker != null)
            _disabledMarker.SetActive(false);
    }

}
