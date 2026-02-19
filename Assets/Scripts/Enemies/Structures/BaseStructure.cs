using UnityEngine;
using System.Collections.Generic;

public abstract class BaseStructure : MonoBehaviour
{
    protected StructureType _type;
    protected BaseNode _node;
    private float _internalTimer = 0;
    protected int _unitProductionPerSecond = 1;

    protected bool _active = true;

    private void SetType(StructureType type)
    {
        _type = type;
    }

    public void AssignNode(BaseNode node)
    {
        _node = node;
        _node.AssignStructure(this);
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
        // if (!_active) return;

        _internalTimer += Time.deltaTime;
        if (_internalTimer >= 1f)
        {
            _internalTimer = 0f;
            if (EnemyManager.Instance.TryChangeUnits(_unitProductionPerSecond))
            {
            _node.ChangeUnitCount(_unitProductionPerSecond, false);
            }
        }
          
    }
    public StructureType GetType()=> _type;
    public void SetUnitProduction(int u)
    {
        _unitProductionPerSecond = u;
        
    }
    protected virtual void Disable()
    {
        _active = false;
    }
    protected virtual void Enable()
    {
        _active = true;
    }

}
