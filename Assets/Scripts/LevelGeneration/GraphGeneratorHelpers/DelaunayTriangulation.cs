using UnityEngine;
using System.Collections.Generic;

public static class DelaunayTriangulation
{
    private struct Triangle
    {
        public int a, b, c;

        public Triangle(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    private struct Edge
    {
        public int a, b;

        public Edge(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public bool IsSame(Edge other)
        {
            return (a == other.a && b == other.b) || (a == other.b && b == other.a);
        }
    }

    private static bool CircumcircleContains(List<Vector2> pts, Triangle tri, Vector2 point)
    {
        Vector2 pa = pts[tri.a];
        Vector2 pb = pts[tri.b];
        Vector2 pc = pts[tri.c];

        float cross = (pb.x - pa.x) * (pc.y - pa.y) - (pb.y - pa.y) * (pc.x - pa.x);
        if (cross < 0)
        {
            Vector2 tmp = pb;
            pb = pc;
            pc = tmp;
        }

        float ax = pa.x - point.x;
        float ay = pa.y - point.y;
        float bx = pb.x - point.x;
        float by = pb.y - point.y;
        float cx = pc.x - point.x;
        float cy = pc.y - point.y;

        float det = (ax * ax + ay * ay) * (bx * cy - cx * by)
                  - (bx * bx + by * by) * (ax * cy - cx * ay)
                  + (cx * cx + cy * cy) * (ax * by - bx * ay);

        return det > 0;
    }

    public static HashSet<(int, int)> Triangulate(List<Vector2> points)
    {
        var result = new HashSet<(int, int)>();

        if (points.Count < 3)
            return result;

        List<Vector2> pts = new List<Vector2>(points);

        float minX = pts[0].x, minY = pts[0].y;
        float maxX = minX, maxY = minY;

        for (int i = 1; i < points.Count; i++)
        {
            if (pts[i].x < minX) minX = pts[i].x;
            if (pts[i].x > maxX) maxX = pts[i].x;
            if (pts[i].y < minY) minY = pts[i].y;
            if (pts[i].y > maxY) maxY = pts[i].y;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float deltaMax = Mathf.Max(dx, dy) + 0.01f;
        float midX = (minX + maxX) * 0.5f;
        float midY = (minY + maxY) * 0.5f;

        int st0 = pts.Count;
        int st1 = pts.Count + 1;
        int st2 = pts.Count + 2;

        pts.Add(new Vector2(midX - 20f * deltaMax, midY - deltaMax));
        pts.Add(new Vector2(midX, midY + 20f * deltaMax));
        pts.Add(new Vector2(midX + 20f * deltaMax, midY - deltaMax));

        List<Triangle> triangles = new List<Triangle>();
        triangles.Add(new Triangle(st0, st1, st2));

        for (int i = 0; i < points.Count; i++)
        {
            List<Edge> polygon = new List<Edge>();

            for (int j = triangles.Count - 1; j >= 0; j--)
            {
                if (CircumcircleContains(pts, triangles[j], pts[i]))
                {
                    polygon.Add(new Edge(triangles[j].a, triangles[j].b));
                    polygon.Add(new Edge(triangles[j].b, triangles[j].c));
                    polygon.Add(new Edge(triangles[j].c, triangles[j].a));
                    triangles.RemoveAt(j);
                }
            }

            List<Edge> uniqueEdges = new List<Edge>();
            for (int j = 0; j < polygon.Count; j++)
            {
                bool shared = false;
                for (int k = 0; k < polygon.Count; k++)
                {
                    if (j != k && polygon[j].IsSame(polygon[k]))
                    {
                        shared = true;
                        break;
                    }
                }
                if (!shared)
                    uniqueEdges.Add(polygon[j]);
            }

            for (int j = 0; j < uniqueEdges.Count; j++)
            {
                triangles.Add(new Triangle(uniqueEdges[j].a, uniqueEdges[j].b, i));
            }
        }

        foreach (var tri in triangles)
        {
            if (tri.a >= points.Count || tri.b >= points.Count || tri.c >= points.Count)
                continue;

            AddEdge(result, tri.a, tri.b);
            AddEdge(result, tri.b, tri.c);
            AddEdge(result, tri.c, tri.a);
        }

        return result;
    }

    private static void AddEdge(HashSet<(int, int)> edges, int a, int b)
    {
        var edge = a < b ? (a, b) : (b, a);
        edges.Add(edge);
    }
}
