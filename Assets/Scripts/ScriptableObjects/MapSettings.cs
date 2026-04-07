using UnityEngine;
using System.Collections.Generic;

public enum ResourceDrawingMode
{
    Spherical
}

[System.Serializable]
public class ResourceSettings
{
    public ResourceType resourceType;

    [Tooltip("Minimum distance between resource node centres")]
    [Min(0.5f)]
    public float density = 3f;

    [Tooltip("Radius of each resource node in tile units")]
    [Min(1f)]
    public float nodeSize = 2f;

    public ResourceDrawingMode drawingMode = ResourceDrawingMode.Spherical;

    [Tooltip("Material applied to the tilemap created for this resource")]
    public Material material;

    [Tooltip("Scale of tiles on the tilemap (smaller = smaller tiles)")]
    [Min(0.01f)]
    public float tileScale = 0.2f;

    [Tooltip("How many tiles of this resource in range are needed to produce 1 resource per second")]
    [Min(1)]
    public int tilesPerOneResource = 10;

    [Tooltip("If true, this resource is the bottom fill layer (drawn last, skips tiles occupied by other resources)")]
    public bool isBottom = false;
}

[CreateAssetMenu(fileName = "MapSettings", menuName = "Scriptable Objects/MapSettings")]
public class MapSettings : ScriptableObject
{
    [Tooltip("Random seed for resource generation (0 = random each time)")]
    public int seed = 0;

    [Tooltip("Padding beyond the node bounding box for resource spawning area")]
    [Min(0f)]
    public float padding = 10f;

    [Tooltip("Resource configurations to populate on the map")]
    public List<ResourceSettings> resources = new List<ResourceSettings>();


    [Header("Exclusion Zones")]
    [Tooltip("Base exclusion radius around map nodes (measured from node visual edge)")]
    [Min(0f)]
    public float nodeExclusionRadius = 1f;

    [Tooltip("Base exclusion radius around spline paths")]
    [Min(0f)]
    public float splineExclusionRadius = 0.5f;

    [Tooltip("Perlin noise frequency for exclusion zone edges (higher = more jagged)")]
    [Min(0.01f)]
    public float exclusionNoiseScale = 2f;

    [Tooltip("How much the exclusion boundary varies due to noise (in world units)")]
    [Min(0f)]
    public float exclusionNoiseAmplitude = 0.5f;

    [Tooltip("Distance between spline sample points (smaller = more accurate, slower)")]
    [Min(0.01f)]
    public float splineSampleResolution = 0.1f;

    [Header("Resource Gathering")]
    [Tooltip("World-space radius around each node for counting resource tiles")]
    [Min(0.5f)]
    public float resourceGatheringRange = 3f;

    [Tooltip("Color of the gathering range circle preview")]
    public Color gatheringRangeVisualColor = Color.yellow;

    [Tooltip("Show the gathering range circle when hovering over Harvester button")]
    public bool showGatheringRangeOnHover = true;
}
