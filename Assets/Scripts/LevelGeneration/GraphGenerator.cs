using System;
using UnityEngine;
using System.Collections.Generic;


public static class GraphGenerator
{
    public static Graph GenerateGraph(GraphSettings settings)
    {
        switch (settings.type)
        {
            case GraphType.Square: return SquareGraphBuilder.BuildGraph(settings);
            case GraphType.PoissonDelaunay: return PoissonDelaunayGraphBuilder.BuildGraph(settings);

            default:
                throw new NotImplementedException($"No implementation for creating graph of type '{settings.type}'.");
        }

    }


}