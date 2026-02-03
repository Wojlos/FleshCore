using UnityEngine;

public abstract class BaseBuilding : MonoBehaviour
{
    private BuildingType _type;
    private BaseNode _node;

    protected BaseBuilding(BuildingType type)
    {
        _type = type;
    }

    public void AssignNode(BaseNode node)
    {
        _node = node;
    }

}
