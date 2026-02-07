using UnityEngine;
using System.Collections.Generic;

public abstract class BaseBuilding : MonoBehaviour
{
    [SerializeField] private GameObject _disabledMarker;
    private BuildingType _type;
    private BaseNode _node;
    private List<ResourceCost> _upkeep = new List<ResourceCost>();
    private float _upkeepTimer = 0;
    protected bool _active = false;

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
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
        _upkeepTimer += Time.deltaTime;

        if (_upkeepTimer >= 1f)
        {
            _upkeepTimer = 0f;
            if(!ResourceManager.Instance.TryChangeResourcesCurrent(_upkeep, false))
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }      
    }
    public BuildingType GetType()=> _type;

    protected virtual void Disable()
    {
        _active = false;
        _disabledMarker.SetActive(true);
        
    }
    protected virtual void Enable()
    {
        _active = true;
        _disabledMarker.SetActive(false);
    }

}
