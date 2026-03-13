using UnityEngine;
using System.Collections.Generic;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance;

    private List<BaseNode> _mapNodes = new List<BaseNode>();

    // nextStep[from][to] = the next node you should go to when traveling from "from" toward "to"
    private Dictionary<BaseNode, Dictionary<BaseNode, BaseNode>> _nextStep =
        new Dictionary<BaseNode, Dictionary<BaseNode, BaseNode>>();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _mapNodes = MapManager.Instance.GetAllNodes();
        BuildNextStepTable();
    }

    /// <summary>
    /// Rebuild the cached next-step routing table (call this if your graph changes at runtime).
    /// </summary>
    public void Rebuild()
    {
        _mapNodes = MapManager.Instance.GetAllNodes();
        BuildNextStepTable();
    }

    private void BuildNextStepTable()
    {
        _nextStep.Clear();

        // Ensure dictionary entries exist for all nodes
        for (int i = 0; i < _mapNodes.Count; i++)
        {
            var node = _mapNodes[i];
            if (node != null && !_nextStep.ContainsKey(node))
                _nextStep[node] = new Dictionary<BaseNode, BaseNode>();
        }

        // For each start node, run BFS and compute the first hop toward every reachable node.
        foreach (var start in _mapNodes)
        {
            if (start == null) continue;

            // firstHop[x] = first step from "start" toward x along a shortest path
            var firstHop = new Dictionary<BaseNode, BaseNode>();
            var visited = new HashSet<BaseNode>();
            var queue = new Queue<BaseNode>();

            visited.Add(start);
            queue.Enqueue(start);

            // (Optional) define start->start as start, so lookups are always defined
            _nextStep[start][start] = start;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var neighbors = current.GetConnectedNodes();

                for (int i = 0; i < neighbors.Count; i++)
                {
                    var neighbor = neighbors[i];
                    if (neighbor == null) continue;
                    if (visited.Contains(neighbor)) continue;

                    visited.Add(neighbor);

                    // If neighbor is directly adjacent to start, first hop is neighbor itself.
                    // Otherwise, inherit the first hop of "current".
                    BaseNode hop = (current == start) ? neighbor : firstHop[current];
                    firstHop[neighbor] = hop;

                    // Cache: from start to neighbor, go to hop first
                    _nextStep[start][neighbor] = hop;

                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    public List<BaseNode> GetPath(BaseNode start, BaseNode end)
    {
        var path = new List<BaseNode>();

        if (start == null || end == null)
            return path;

        if (start == end)
        {
            path.Add(start);
            return path;
        }

        // If something changed and table isn't built/doesn't contain nodes, you can rebuild or fail gracefully.
        if (!_nextStep.TryGetValue(start, out var startTable))
            return path;

        if (!startTable.ContainsKey(end))
            return path; // in a connected graph this shouldn't happen, but safe guard

        // Reconstruct by repeatedly asking: "from current, what's the next step toward end?"
        var current = start;
        path.Add(current);

        // Safety guard to prevent infinite loops if the table is corrupted
        int safety = _mapNodes.Count + 5;

        while (current != end && safety-- > 0)
        {
            if (!_nextStep.TryGetValue(current, out var tableForCurrent))
                return new List<BaseNode>(); // fail safe

            if (!tableForCurrent.TryGetValue(end, out var next))
                return new List<BaseNode>(); // fail safe

            current = next;
            path.Add(current);
        }

        // If safety ran out, something is wrong; return empty (or return the partial path if you prefer)
        if (current != end)
            return new List<BaseNode>();

        return path;
    }

    public int GetGraphDistance(BaseNode from, BaseNode to)
    {
        if (from == null || to == null) return int.MaxValue;
        if (from == to) return 0;

        var path = GetPath(from, to);
        return path.Count > 0 ? path.Count - 1 : int.MaxValue;
    }
}
