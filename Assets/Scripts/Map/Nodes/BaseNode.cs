using UnityEngine;
using System.Collections.Generic;

public abstract class BaseNode : MonoBehaviour
{
    
    private List<BaseNode> _connectedNodes = new List<BaseNode>();

    public void ConnectNode(BaseNode node)
    {
        if (!_connectedNodes.Contains(node))
        {
            _connectedNodes.Add(node);
            node.ConnectNode(this);
        }

    }

}
