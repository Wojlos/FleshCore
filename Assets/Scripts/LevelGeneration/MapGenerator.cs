using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private GraphSettings _graphSettings;

    [SerializeField]
    private GameObject _nodePrefab;

    private Graph _graph;

    private readonly List<GameObject> _spawnedNodes = new List<GameObject>();

    public void GenerateMap()
    {
        CleanMap();

        _graph = GraphGenerator.GenerateGraph(_graphSettings);
        Debug.Log($"Generated nodes: {_graph.allNodes.Count}");

        foreach (var node in _graph.allNodes)
        {
            GameObject go = Instantiate(
                _nodePrefab,
                new Vector3(node.position.x, node.position.y, 0f),
                Quaternion.identity,
                transform
            );

            _spawnedNodes.Add(go);
        }
    }

    public void CleanMap()
    {
        for (int i = 0; i < _spawnedNodes.Count; i++)
        {
            if (_spawnedNodes[i] != null)
                DestroyImmediate(_spawnedNodes[i]);
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
