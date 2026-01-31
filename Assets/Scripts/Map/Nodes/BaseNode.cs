using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

public abstract class BaseNode : MonoBehaviour
{
    
    private List<BaseNode> _connectedNodes = new List<BaseNode>();
    private List<Spline> _splines = new List<Spline>();


    public void ConnectNode(BaseNode node)
    {
        if (!_connectedNodes.Contains(node))
        {
            _connectedNodes.Add(node);
            node.ConnectNode(this);
        }

    }
    public List<BaseNode> GetConnectedNodes()
    {
        return _connectedNodes;
    }

    public void AddSpline(Spline s)
    {
        _splines.Add(s);
    }

}
