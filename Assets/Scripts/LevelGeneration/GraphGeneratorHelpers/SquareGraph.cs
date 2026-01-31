using UnityEngine;
using System.Collections.Generic;

public static class SquareGraphBuilder
{
    public static Graph BuildGraph(GraphSettings settings)
    {
        var grid = CreateGrid(settings.numberOfVertices);
        var graph = ConnectNodes(grid);
        
        return graph;
    }
    private static Graph ConnectNodes(GraphNode[,] grid)
    {
        var graph = new Graph();
        var verts = grid.GetLength(0);
        for (int i = 0; i < verts-1; i++)
        {
            for (int j = 0; j < verts-1; j++)
            {
                var node = grid[i, j];
                node.position = new Vector2(i,j);
                graph.AddNode(node);
                if(i != 0)
                {
                    node.ConnectNode(grid[i-1,j]);
                }
                if(j != 0)
                {
                    node.ConnectNode(grid[i,j-1]);
                }
                if(i != 0 && j != 0)
                {
                    node.ConnectNode(grid[i-1,j-1]);
                }
            }
        }

        return graph;
    }
    private static GraphNode[,] CreateGrid(int verts)
    {
        GraphNode[,] grid = new GraphNode[verts, verts];

        for (int i = 0; i < verts; i++)
        {
            for (int j = 0; j < verts; j++)
            {
                grid[i, j] = new GraphNode();
            }
        }

        return grid;
    }
}