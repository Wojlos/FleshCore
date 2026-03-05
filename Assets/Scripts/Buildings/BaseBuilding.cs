using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HealthSystem))]
public abstract class BaseBuilding : MonoBehaviour
{
    [SerializeField] private GameObject _disabledMarker;
    private BuildingType _type;
    public ConflictSide Side { get; private set; }
    protected BaseNode _node;
    protected int _unitProductionPerSecond = 0;

    protected bool _active = true;
    protected bool _isSieged = false;
    public bool IsSieged => _isSieged;

    protected BaseBuilding(BuildingType type, ConflictSide side)
    {
        _type = type;
        Side = side;
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
        var hs = GetComponent<HealthSystem>();
        if (hs != null)
        {
            hs.MaxHealth = settings.startHp;
            hs.HealToFull();
        }
    }
    public BuildingType GetBuildingType()=> _type;
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

    public virtual void OnEncounterReported(BaseNode encounterNode, UnitTravelGroup reporter) { }

    public void SetSieged(bool sieged)
    {
        _isSieged = sieged;
        if (sieged)
            Disable();
        else
            Enable();
    }

}
