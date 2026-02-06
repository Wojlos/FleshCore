using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

public abstract class BaseNode : MonoBehaviour
{
    
    private List<BaseNode> _connectedNodes = new List<BaseNode>();
    private List<Spline> _splines = new List<Spline>();
    private BaseBuilding _building;

    public void ConnectNode(BaseNode node)
    {
        if (!_connectedNodes.Contains(node))
        {
            _connectedNodes.Add(node);
            node.ConnectNode(this);
        }

    }
    public List<BaseNode> GetConnectedNodes() => _connectedNodes;

    public void AddSpline(Spline s)
    {
        _splines.Add(s);
    }

    public void AssignBuilding(BaseBuilding building)
    {
        _building = building;   
    }

    public BaseBuilding GetBuilding()=> _building;
    public void RemoveBuilding()
    {
        var b = _building;
        _building = null;
        b.Cleanup();
        Destroy(b.gameObject);
        
    }
}
