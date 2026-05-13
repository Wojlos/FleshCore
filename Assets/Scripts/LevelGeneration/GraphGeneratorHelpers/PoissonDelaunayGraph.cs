using UnityEngine;
using System.Collections.Generic;

public static class PoissonDelaunayGraphBuilder
{
    private const int RejectionSamples = 30;

    public static Graph BuildGraph(GraphSettings settings)
    {
        float regionSize = settings.scaleFactor;
        float minDist = settings.minDistance;
        float maxDist = settings.maxDistance;
        float maxEdgeLen = settings.maxEdgeLength;

        var points = PoissonDiskSample(regionSize, minDist, maxDist);
        var edges = DelaunayTriangulation.Triangulate(points);

        return BuildGraphFromEdges(points, edges, maxEdgeLen);
    }

    private static List<Vector2> PoissonDiskSample(float regionSize, float minDist, float maxDist)
    {
        float cellSize = minDist / Mathf.Sqrt(2f);
        int gridSize = Mathf.CeilToInt(regionSize / cellSize);

        int[,] grid = new int[gridSize, gridSize];
        for (int x = 0; x < gridSize; x++)
            for (int y = 0; y < gridSize; y++)
                grid[x, y] = -1;

        List<Vector2> points = new List<Vector2>();
        List<int> activeList = new List<int>();

        Vector2 initialPoint = new Vector2(regionSize * 0.5f, regionSize * 0.5f);
        points.Add(initialPoint);
        activeList.Add(0);

        int gx = Mathf.FloorToInt(initialPoint.x / cellSize);
        int gy = Mathf.FloorToInt(initialPoint.y / cellSize);
        grid[gx, gy] = 0;

        while (activeList.Count > 0)
        {
            int activeIndex = Random.Range(0, activeList.Count);
            int pointIndex = activeList[activeIndex];
            Vector2 center = points[pointIndex];
            bool accepted = false;

            for (int k = 0; k < RejectionSamples; k++)
            {
                float angle = Random.value * Mathf.PI * 2f;
                float radius = minDist + Random.value * (maxDist - minDist);
                Vector2 candidate = center + new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );

                if (candidate.x < 0 || candidate.x >= regionSize ||
                    candidate.y < 0 || candidate.y >= regionSize)
                    continue;

                int cx = Mathf.FloorToInt(candidate.x / cellSize);
                int cy = Mathf.FloorToInt(candidate.y / cellSize);

                if (IsValid(candidate, cx, cy, grid, points, gridSize, minDist, cellSize))
                {
                    points.Add(candidate);
                    activeList.Add(points.Count - 1);
                    grid[cx, cy] = points.Count - 1;
                    accepted = true;
                    break;
                }
            }

            if (!accepted)
            {
                activeList.RemoveAt(activeIndex);
            }
        }

        return points;
    }

    private static bool IsValid(Vector2 candidate, int cx, int cy, int[,] grid,
        List<Vector2> points, int gridSize, float minDist, float cellSize)
    {
        int searchRadius = 2;
        for (int x = Mathf.Max(0, cx - searchRadius); x <= Mathf.Min(gridSize - 1, cx + searchRadius); x++)
        {
            for (int y = Mathf.Max(0, cy - searchRadius); y <= Mathf.Min(gridSize - 1, cy + searchRadius); y++)
            {
                int idx = grid[x, y];
                if (idx != -1)
                {
                    float dist = Vector2.Distance(candidate, points[idx]);
                    if (dist < minDist)
                        return false;
                }
            }
        }
        return true;
    }

    private static Graph BuildGraphFromEdges(List<Vector2> points, HashSet<(int, int)> edges, float maxEdgeLength)
    {
        var graph = new Graph();
        var nodes = new GraphNode[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            nodes[i] = new GraphNode();
            nodes[i].position = points[i];
            graph.AddNode(nodes[i]);
        }

        float maxLenSqr = maxEdgeLength * maxEdgeLength;
        float nodeRadius = 0.3f;

        foreach (var (a, b) in edges)
        {
            Vector2 pa = points[a];
            Vector2 pb = points[b];
            Vector2 diff = pb - pa;
            float edgeLenSqr = diff.sqrMagnitude;

            if (edgeLenSqr > maxLenSqr)
                continue;

            Vector2 mid = (pa + pb) * 0.5f;
            float radiusSqr = edgeLenSqr * 0.25f;
            bool passesGabriel = true;

            for (int i = 0; i < points.Count; i++)
            {
                if (i == a || i == b) continue;

                if ((points[i] - mid).sqrMagnitude < radiusSqr)
                {
                    passesGabriel = false;
                    break;
                }
            }

            if (!passesGabriel)
                continue;

            bool tooCloseToNode = false;

            for (int i = 0; i < points.Count; i++)
            {
                if (i == a || i == b) continue;

                float dist = PointToSegmentDistance(points[i], pa, pb);
                if (dist < nodeRadius)
                {
                    tooCloseToNode = true;
                    break;
                }
            }

            if (!tooCloseToNode)
            {
                nodes[a].ConnectNode(nodes[b]);
            }
        }

        return graph;
    }

    private static float PointToSegmentDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float t = Mathf.Clamp01(Vector2.Dot(ap, ab) / ab.sqrMagnitude);
        Vector2 closest = a + ab * t;
        return Vector2.Distance(p, closest);
    }
}
