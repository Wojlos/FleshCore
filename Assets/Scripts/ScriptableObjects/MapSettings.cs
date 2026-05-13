using UnityEngine;
using System.Collections.Generic;

public enum ResourceDrawingMode
{
    Spherical,
    DLANoise
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

    [Tooltip("Color gradient used for excavation particles when harvesting this resource")]
    public Gradient particleColorGradient;
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

    [Header("DLA Noise Textures")]
    [Tooltip("Black-and-white DLA noise images used by the DLANoise drawing mode. Center of image = center of resource node.")]
    public List<Texture2D> smalDLANoises = new List<Texture2D>();

    [Header("Spline Visuals")]
    [Tooltip("Material applied to spline extrude meshes")]
    public Material splineMaterial;

    [Tooltip("Radius of the extruded spline tube")]
    [Min(0.01f)]
    public float splineRadius = 0.2f;

    [Tooltip("Number of mesh segments per world unit along the spline")]
    [Min(1)]
    public int splineSegmentsPerUnit = 12;

    [Tooltip("Flip the normals of the extruded spline mesh")]
    public bool splineFlipNormals = true;

    [Tooltip("Sorting order for spline meshes (lower = drawn earlier / behind other sprites)")]
    public int splineSortingOrder = -10;

    [Header("Spline Bends")]
    [Tooltip("Average number of random bend points added per world unit of spline length")]
    [Min(0f)]
    public float bendPointsPerUnit = 0.4f;

    [Tooltip("Minimum perpendicular offset of a bend point from the straight line")]
    [Min(0f)]
    public float bendOffsetMin = 0.15f;

    [Tooltip("Maximum perpendicular offset of a bend point from the straight line")]
    [Min(0f)]
    public float bendOffsetMax = 0.5f;

    [Tooltip("Fraction of spline length at each end where no bend points are placed (0-0.5)")]
    [Range(0f, 0.5f)]
    public float bendMargin = 0.15f;
}
