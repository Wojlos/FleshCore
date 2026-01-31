using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private GraphSettings _graphSettings;

    [SerializeField]
    private GameObject _nodePrefab;

    private Graph _graph;

    private readonly List<BaseNode> _spawnedNodes = new List<BaseNode>();

    public void GenerateMap()
    {
        CleanMap();

        _graph = GraphGenerator.GenerateGraph(_graphSettings);

        foreach (var node in _graph.allNodes)
        {
            GameObject go = Instantiate(
                _nodePrefab,
                new Vector3(node.position.x, node.position.y, 0f),
                Quaternion.identity,
                transform
            );
            BaseNode newNode = go.GetComponent<BaseNode>();
            _spawnedNodes.Add(newNode);
        }
        ConnectNodes();
    }

    private void ConnectNodes()
    {
        foreach(var n in _graph.allNodes)
        {
            var mapNode = GetMapNodeOnPosition(n.position);
            foreach(var connectedNode in n.connectedNodes)
            {
                mapNode.ConnectNode(GetMapNodeOnPosition(connectedNode.position));
            }
        }
    }

    private BaseNode GetMapNodeOnPosition(Vector2 pos)
    {
        foreach(var n in _spawnedNodes)
        {
            if(n.transform.position == new Vector3(pos.x, pos.y, 0f))
            {
                return n;
            }
        }
        return null;
    }
    public void CleanMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }

        _spawnedNodes.Clear();
        _graph = null;
    }

    private void OnDrawGizmos()
    {
        if (_graph == null || _graph.allNodes == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (var node in _graph.allNodes)
        {
            Gizmos.DrawSphere(
                new Vector3(node.position.x, node.position.y, 0f),
                0.15f
            );
        }

        Gizmos.color = Color.white;

        HashSet<(GraphNode, GraphNode)> drawnEdges = new HashSet<(GraphNode, GraphNode)>();

        foreach (var node in _graph.allNodes)
        {
            foreach (var other in node.connectedNodes)
            {
                var edge = node.GetHashCode() < other.GetHashCode()
                    ? (node, other)
                    : (other, node);

                if (drawnEdges.Contains(edge))
                    continue;

                drawnEdges.Add(edge);

                Gizmos.DrawLine(
                    new Vector3(node.position.x, node.position.y, 0f),
                    new Vector3(other.position.x, other.position.y, 0f)
                );
            }
        }
    }
}
