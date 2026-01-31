using UnityEngine;
using System.Collections.Generic;
public enum GraphType
{
    Square
}

[System.Serializable]
public class GraphSettings
{
    [Min(2)] public int numberOfVertices = 2;
    [Min(1)] public int minVertDegree = 1;
    [Min(1)] public int maxVertDegree = 4;

    public GraphType type;

}

public class GraphNode
{
    public List<GraphNode> connectedNodes = new List<GraphNode>();
    public Vector2 position = new Vector2();

    public void ConnectNode(GraphNode node)
    {
        if (!connectedNodes.Contains(node))
        {
            connectedNodes.Add(node);
            node.ConnectNode(this);
        }
    }

    public int CurrentDegree()
    {
        return connectedNodes.Count;
    }

}

public class Graph
{
    public List<GraphNode> allNodes = new List<GraphNode>();
    public void AddNode(GraphNode node)
    {
        allNodes.Add(node);
    }

}