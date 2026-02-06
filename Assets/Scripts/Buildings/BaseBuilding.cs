using UnityEngine;
using System.Collections.Generic;

public abstract class BaseBuilding : MonoBehaviour
{
    private BuildingType _type;
    private BaseNode _node;
    private List<ResourceCost> _upkeep = new List<ResourceCost>();
    private float _upkeepTimer = 0;


    protected BaseBuilding(BuildingType type)
    {
        _type = type;
    }

    public void SetUpkeep(List<ResourceCost> upkeep)
    {
        _upkeep = upkeep;
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
        _upkeepTimer += Time.deltaTime;

        if (_upkeepTimer >= 1f)
        {
            _upkeepTimer = 0f;
            ResourceManager.Instance.ChangeResourcesCurrent(_upkeep, false);
        }

        
    }

    public BuildingType GetType()=> _type;

}
