using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Splines;
using System.Collections.Generic;
using System.Linq;
using float3 = Unity.Mathematics.float3;

public class TilemapsManager : MonoBehaviour
{
    public static TilemapsManager Instance;

    [SerializeField] private MapSettings _mapSettings;
    [SerializeField] private RuleTile _resourceRuleTile;
    [SerializeField] private GameObject _tilemapPrefab;
    [SerializeField] private Grid _grid;
    [SerializeField] private float _tileScale = 0.3f;
    [SerializeField] private int _shadowcasterBorder = 2;
    [SerializeField] private int _shadowcasterDownsample = 2;
    [SerializeField] private float _shadowcasterVertexDistance = 0.5f;

    private readonly Dictionary<ResourceType, Tilemap> _tilemaps = new Dictionary<ResourceType, Tilemap>();
    private Tilemap _shadowcasterTilemap;

    private struct ResourceCentre
    {
        public Vector2 position;
        public int priority;
        public ResourceSettings resource;
    }

    private struct NodeExclusionData
    {
        public Vector2 position;
        public float radius;
    }

    private List<NodeExclusionData> _nodeExclusions = new List<NodeExclusionData>();
    private List<Vector2> _splineSamplePoints = new List<Vector2>();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GenerateResources(List<BaseNode> mapNodes)
    {
        ClearTilemaps();

        if (_mapSettings == null || _mapSettings.resources == null || _mapSettings.resources.Count == 0)
            return;

        BuildExclusionData(mapNodes);

        Rect bounds = CalculateBounds(mapNodes);

        int seed = _mapSettings.seed != 0 ? _mapSettings.seed : Random.Range(1, int.MaxValue);
        System.Random rng = new System.Random(seed);

        int zOffset = -1;

        List<ResourceCentre> allCentres = new List<ResourceCentre>();

        foreach (var resource in _mapSettings.resources)
        {
            Tilemap tilemap = CreateTilemap(resource, zOffset);
            _tilemaps[resource.resourceType] = tilemap;
            zOffset--;

            List<Vector2> centres = GenerateCentres(bounds, resource.density, rng);

            if (resource.isBottom)
            {
                foreach (var c in centres)
                    allCentres.Add(new ResourceCentre { position = c, priority = -1, resource = resource });
            }
            else
            {
                foreach (var c in centres)
                    allCentres.Add(new ResourceCentre { position = c, priority = rng.Next(), resource = resource });
            }
        }

        var nonBottomCentres = allCentres
            .Where(c => !c.resource.isBottom)
            .OrderByDescending(c => c.priority)
            .ToList();

        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        foreach (var centre in nonBottomCentres)
        {
            Tilemap tilemap = _tilemaps[centre.resource.resourceType];
            DrawResource(tilemap, centre.position, centre.resource, occupied, rng);
        }

        var bottomCentres = allCentres
            .Where(c => c.resource.isBottom)
            .ToList();

        foreach (var centre in bottomCentres)
        {
            Tilemap tilemap = _tilemaps[centre.resource.resourceType];
            DrawResource(tilemap, centre.position, centre.resource, occupied, rng);
        }

        BuildShadowcaster();
    }

    private Rect CalculateBounds(List<BaseNode> nodes)
    {
        if (nodes == null || nodes.Count == 0)
            return new Rect(0, 0, 0, 0);

        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (var node in nodes)
        {
            Vector3 pos = node.transform.position;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        float padding = _mapSettings.padding;
        return new Rect(
            minX - padding,
            minY - padding,
            (maxX - minX) + padding * 2f,
            (maxY - minY) + padding * 2f
        );
    }

    private Tilemap CreateTilemap(ResourceSettings resource, int zOffset)
    {
        GameObject go = new GameObject($"Tilemap_{resource.resourceType}");
        go.transform.SetParent(_grid.transform);
        go.transform.localPosition = new Vector3(0f, 0f, zOffset);
        go.transform.localScale = new Vector3(_tileScale, _tileScale, 1f);

        Tilemap tilemap = go.AddComponent<Tilemap>();
        TilemapRenderer renderer = go.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = zOffset;

        if (resource.material != null)
            renderer.material = resource.material;

        return tilemap;
    }

    private List<Vector2> GenerateCentres(Rect bounds, float minDistance, System.Random rng)
    {
        List<Vector2> points = new List<Vector2>();
        List<Vector2> activeList = new List<Vector2>();
        int maxAttempts = 30;

        // First point
        Vector2 first = new Vector2(
            (float)(bounds.x + rng.NextDouble() * bounds.width),
            (float)(bounds.y + rng.NextDouble() * bounds.height)
        );
        points.Add(first);
        activeList.Add(first);

        while (activeList.Count > 0)
        {
            int index = rng.Next(0, activeList.Count);
            Vector2 current = activeList[index];
            bool found = false;

            for (int i = 0; i < maxAttempts; i++)
            {
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float dist = minDistance + (float)(rng.NextDouble() * minDistance);
                Vector2 candidate = current + new Vector2(
                    Mathf.Cos(angle) * dist,
                    Mathf.Sin(angle) * dist
                );

                if (!bounds.Contains(candidate))
                    continue;

                if (IsFarEnough(candidate, points, minDistance))
                {
                    points.Add(candidate);
                    activeList.Add(candidate);
                    found = true;
                    break;
                }
            }

            if (!found)
                activeList.RemoveAt(index);
        }

        return points;
    }

    private bool IsFarEnough(Vector2 candidate, List<Vector2> existing, float minDistance)
    {
        float sqrMin = minDistance * minDistance;
        foreach (var p in existing)
        {
            if ((candidate - p).sqrMagnitude < sqrMin)
                return false;
        }
        return true;
    }

    private void DrawResource(Tilemap tilemap, Vector2 centre, ResourceSettings resource, HashSet<Vector2Int> occupied, System.Random rng)
    {
        switch (resource.drawingMode)
        {
            case ResourceDrawingMode.Spherical:
                DrawSpherical(tilemap, centre, resource.nodeSize, occupied);
                break;
            case ResourceDrawingMode.DLANoise:
                DrawDLANoise(tilemap, centre, resource.nodeSize, occupied, rng);
                break;
            default:
                Debug.LogWarning($"TilemapsManager: DrawingMode '{resource.drawingMode}' not implemented.");
                break;
        }
    }

    private static readonly int OccupancyPrecision = 1000;

    private Vector2Int WorldToOccupancyKey(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x * OccupancyPrecision),
            Mathf.RoundToInt(worldPos.y * OccupancyPrecision)
        );
    }

    private Vector3Int WorldToCell(Tilemap tilemap, Vector3 worldPos)
    {
        Vector3 local = tilemap.transform.InverseTransformPoint(worldPos);
        return new Vector3Int(
            Mathf.FloorToInt(local.x / _grid.cellSize.x),
            Mathf.FloorToInt(local.y / _grid.cellSize.y),
            0
        );
    }

    private Vector3 CellCenterToWorld(Tilemap tilemap, Vector3Int cell)
    {
        Vector3 local = new Vector3(
            (cell.x + 0.5f) * _grid.cellSize.x,
            (cell.y + 0.5f) * _grid.cellSize.y,
            0f
        );
        return tilemap.transform.TransformPoint(local);
    }

    private void DrawSpherical(Tilemap tilemap, Vector2 centre, float radius, HashSet<Vector2Int> occupied)
    {
        float scaledCellSize = _grid.cellSize.x * _tileScale;
        int cellRadius = Mathf.CeilToInt(radius / scaledCellSize);
        Vector3Int centreCell = WorldToCell(tilemap, new Vector3(centre.x, centre.y, 0f));

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector3Int cell = new Vector3Int(centreCell.x + x, centreCell.y + y, 0);
                Vector3 cellWorld = CellCenterToWorld(tilemap, cell);
                Vector2 cellWorld2D = new Vector2(cellWorld.x, cellWorld.y);

                float dist = Vector2.Distance(cellWorld2D, centre);

                if (dist <= radius)
                {
                    Vector2Int key = WorldToOccupancyKey(cellWorld);
                    if (occupied.Contains(key))
                        continue;

                    if (IsInExclusionZone(cellWorld2D))
                        continue;

                    tilemap.SetTile(cell, _resourceRuleTile);
                    occupied.Add(key);
                }
            }
        }
    }

    private void DrawDLANoise(Tilemap tilemap, Vector2 centre, float radius, HashSet<Vector2Int> occupied, System.Random rng)
    {
        var textures = _mapSettings.smalDLANoises;
        if (textures == null || textures.Count == 0)
        {
            Debug.LogWarning("TilemapsManager: No DLA noise textures assigned in MapSettings.smalDLANoises.");
            return;
        }

        Texture2D tex = textures[rng.Next(textures.Count)];
        if (tex == null)
            return;

        int texW = tex.width;
        int texH = tex.height;
        float worldSize = radius * 2f;

        float scaledCellSize = _grid.cellSize.x * _tileScale;
        float pixelWorldSize = worldSize / Mathf.Max(texW, texH);
        int pixelStep = Mathf.Max(1, Mathf.FloorToInt(scaledCellSize / pixelWorldSize));

        for (int px = 0; px < texW; px += pixelStep)
        {
            for (int py = 0; py < texH; py += pixelStep)
            {
                Color pixel = tex.GetPixel(px, py);
                if (pixel.a < 0.1f)
                    continue;

                float offsetX = ((float)px / texW - 0.5f) * worldSize;
                float offsetY = ((float)py / texH - 0.5f) * worldSize;
                Vector2 worldPos = centre + new Vector2(offsetX, offsetY);

                Vector3Int cell = WorldToCell(tilemap, new Vector3(worldPos.x, worldPos.y, 0f));
                Vector3 cellWorld = CellCenterToWorld(tilemap, cell);
                Vector2 cellWorld2D = new Vector2(cellWorld.x, cellWorld.y);

                Vector2Int key = WorldToOccupancyKey(cellWorld);
                if (occupied.Contains(key))
                    continue;

                if (IsInExclusionZone(cellWorld2D))
                    continue;

                tilemap.SetTile(cell, _resourceRuleTile);
                occupied.Add(key);
            }
        }
    }

    private void BuildExclusionData(List<BaseNode> mapNodes)
    {
        _nodeExclusions.Clear();
        _splineSamplePoints.Clear();

        if (mapNodes == null) return;

        float nodeBaseRadius = _mapSettings.nodeExclusionRadius;
        HashSet<SplineContainer> sampledContainers = new HashSet<SplineContainer>();

        foreach (var node in mapNodes)
        {
            float visualRadius = 0f;
            var collider = node.GetComponent<CircleCollider2D>();
            if (collider != null)
                visualRadius = collider.radius * Mathf.Max(node.transform.lossyScale.x, node.transform.lossyScale.y);

            _nodeExclusions.Add(new NodeExclusionData
            {
                position = new Vector2(node.transform.position.x, node.transform.position.y),
                radius = visualRadius + nodeBaseRadius
            });

            foreach (var container in node.GetSplineContainers())
            {
                if (container == null || sampledContainers.Contains(container))
                    continue;

                sampledContainers.Add(container);
                SampleSpline(container);
            }
        }
    }

    private void SampleSpline(SplineContainer container)
    {
        if (container.Splines.Count == 0) return;

        Spline spline = container.Splines[0];
        float splineLength = spline.GetLength();
        if (splineLength <= 0f) return;

        float step = _mapSettings.splineSampleResolution;
        int sampleCount = Mathf.CeilToInt(splineLength / step) + 1;

        for (int i = 0; i <= sampleCount; i++)
        {
            float t = Mathf.Clamp01((float)i / sampleCount);
            float3 pos = SplineUtility.EvaluatePosition(spline, t);
            Vector3 worldPos = container.transform.TransformPoint(pos);
            _splineSamplePoints.Add(new Vector2(worldPos.x, worldPos.y));
        }
    }

    private bool IsInExclusionZone(Vector2 worldPos)
    {
        float noiseScale = _mapSettings.exclusionNoiseScale;
        float noiseAmplitude = _mapSettings.exclusionNoiseAmplitude;
        float noise = (Mathf.PerlinNoise(worldPos.x * noiseScale, worldPos.y * noiseScale) - 0.5f) * 2f * noiseAmplitude;

        float nodeExclusion = _mapSettings.nodeExclusionRadius;
        foreach (var node in _nodeExclusions)
        {
            float effectiveRadius = node.radius + noise;
            float dist = Vector2.Distance(worldPos, node.position);
            if (dist <= effectiveRadius)
                return true;
        }

        float splineExclusion = _mapSettings.splineExclusionRadius;
        float effectiveSplineRadius = splineExclusion + noise;
        float sqrSplineRadius = effectiveSplineRadius * effectiveSplineRadius;
        foreach (var samplePoint in _splineSamplePoints)
        {
            if ((worldPos - samplePoint).sqrMagnitude <= sqrSplineRadius)
                return true;
        }

        return false;
    }

    public void CalculateResourceTileCounts(List<BaseNode> mapNodes)
    {
        if (mapNodes == null || _mapSettings == null) return;

        float gatherRange = _mapSettings.resourceGatheringRange;

        foreach (var node in mapNodes)
        {
            Dictionary<ResourceType, int> counts = new Dictionary<ResourceType, int>();
            Vector3 nodePos = node.transform.position;

            foreach (var resource in _mapSettings.resources)
            {
                if (!_tilemaps.TryGetValue(resource.resourceType, out Tilemap tilemap))
                    continue;

                int tileCount = CountTilesInRange(tilemap, nodePos, gatherRange);
                counts[resource.resourceType] = tileCount;
            }

            node.SetResourceTileCounts(counts);
        }
    }

    private int CountTilesInRange(Tilemap tilemap, Vector3 worldCenter, float range)
    {
        float tileScale = tilemap.transform.localScale.x;
        float scaledCellSize = _grid.cellSize.x * tileScale;
        int cellRadius = Mathf.CeilToInt(range / scaledCellSize);
        Vector3Int centreCell = WorldToCell(tilemap, worldCenter);

        int count = 0;

        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector3Int cell = new Vector3Int(centreCell.x + x, centreCell.y + y, 0);
                Vector3 cellWorld = CellCenterToWorld(tilemap, cell);

                float dist = Vector2.Distance(new Vector2(cellWorld.x, cellWorld.y), new Vector2(worldCenter.x, worldCenter.y));

                if (dist <= range && tilemap.HasTile(cell))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public MapSettings GetMapSettings() => _mapSettings;

    public void ClearTilemaps()
    {
        foreach (var kvp in _tilemaps)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        _tilemaps.Clear();

        if (_shadowcasterTilemap != null)
        {
            Destroy(_shadowcasterTilemap.gameObject);
            _shadowcasterTilemap = null;
        }
    }

    private void BuildShadowcaster()
    {
        float shadowScale = _tileScale * Mathf.Max(1, _shadowcasterDownsample);

        GameObject go = Instantiate(_tilemapPrefab, _grid.transform);
        go.name = "Tilemap_Shadowcaster";
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(shadowScale, shadowScale, 1f);

        _shadowcasterTilemap = go.GetComponent<Tilemap>();
        TilemapRenderer renderer = go.GetComponent<TilemapRenderer>();
        renderer.enabled = false;

        CompositeCollider2D composite = go.GetComponent<CompositeCollider2D>();
        if (composite != null)
            composite.vertexDistance = _shadowcasterVertexDistance;

        HashSet<Vector3Int> compositeCells = new HashSet<Vector3Int>();

        foreach (var kvp in _tilemaps)
        {
            Tilemap srcTilemap = kvp.Value;
            BoundsInt srcBounds = srcTilemap.cellBounds;

            for (int x = srcBounds.xMin; x < srcBounds.xMax; x++)
            {
                for (int y = srcBounds.yMin; y < srcBounds.yMax; y++)
                {
                    Vector3Int srcCell = new Vector3Int(x, y, 0);
                    if (!srcTilemap.HasTile(srcCell))
                        continue;

                    Vector3 worldPos = CellCenterToWorld(srcTilemap, srcCell);
                    Vector3Int dstCell = WorldToCell(_shadowcasterTilemap, worldPos);
                    compositeCells.Add(dstCell);
                }
            }
        }

        HashSet<Vector3Int> erodedCells = new HashSet<Vector3Int>(compositeCells);

        foreach (var cell in compositeCells)
        {
            bool onBorder = false;
            for (int dx = -_shadowcasterBorder; dx <= _shadowcasterBorder && !onBorder; dx++)
            {
                for (int dy = -_shadowcasterBorder; dy <= _shadowcasterBorder && !onBorder; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    Vector3Int neighbor = new Vector3Int(cell.x + dx, cell.y + dy, 0);
                    if (!compositeCells.Contains(neighbor))
                        onBorder = true;
                }
            }

            if (onBorder)
                erodedCells.Remove(cell);
        }

        foreach (var cell in erodedCells)
        {
            _shadowcasterTilemap.SetTile(cell, _resourceRuleTile);
        }
    }
}
