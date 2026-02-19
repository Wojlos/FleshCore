using UnityEngine;
using System.Collections.Generic;

public abstract class BaseStructure : EnemyBuilding
{
    protected StructureType _structureType;
    private float _internalTimer = 0;

    protected BaseStructure(BuildingType type) : base(type)
    {
    }

    public void SetStructureType(StructureType type)
    {
        _structureType = type;
    }

    public void AssignNode(BaseNode node)
    {
        _node = node;
        _node.AssignStructure(this);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        
        if (!_active) return;

        _internalTimer += Time.deltaTime;
        if (_internalTimer >= 1f)
        {
            _internalTimer = 0f;
            _node.CreateUnits(_unitProductionPerSecond, false);
        }
          
    }
    
    public StructureType GetStructureType()=> _structureType;

}
