using UnityEngine;
using System.Collections.Generic;

public enum GraphType
{
    Square,
    PoissonDelaunay
}

[System.Serializable]
public class GraphSettings
{
    [Min(2)] public int size = 2;
    [Min(0.1f)] public float minDistance = 1f;
    [Min(0.1f)] public float maxDistance = 2f;
    [Min(0.1f)] public float maxEdgeLength = 3f;
    [Min(1)] public int scaleFactor = 4;

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