using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public struct NodeSelectionData
{
    public BaseNode node;
    public float coveragePercentage;

    public NodeSelectionData(BaseNode node, float coverage)
    {
        this.node = node;
        this.coveragePercentage = coverage;
    }
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private SplineContainer _splineContainerPrefab;

    [SerializeField] private Transform _splinesParent;

    private readonly List<BaseNode> _mapNodes = new List<BaseNode>();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void InitializeMap(List<BaseNode> nodes)
    {
        _mapNodes.Clear();
        if (nodes != null) _mapNodes.AddRange(nodes);

        // If you re-initialize, you might want to clear old spline objects
        // (Uncomment if you want that behavior)
        ClearExistingSplineObjects();

        var checkedNodes = new HashSet<BaseNode>();

        foreach (var n in _mapNodes)
        {
            if (n == null) continue;

            foreach (var conN in n.GetConnectedNodes())
            {
                if (conN == null) continue;

                if (!checkedNodes.Contains(conN))
                {
                    AddSpline(conN, n);
                }
            }

            checkedNodes.Add(n);
        }
    }

    private void AddSpline(BaseNode n1, BaseNode n2)
    {
        if (_splineContainerPrefab == null)
        {
            Debug.LogError("MapManager: _splineContainerPrefab is not assigned.");
            return;
        }

        SplineContainer container = Instantiate(_splineContainerPrefab, _splinesParent ? _splinesParent : transform);

        for (int i = container.Splines.Count - 1; i >= 0; i--)
        {
            container.RemoveSplineAt(i);
        }

        Spline newSpline = new Spline();
        container.AddSpline(newSpline); // now container has spline at index 0

        newSpline.Add(new BezierKnot(n1.transform.position), TangentMode.AutoSmooth);
        newSpline.Add(new BezierKnot(n2.transform.position), TangentMode.AutoSmooth);

        var extrude = container.GetComponent<SplineExtrude>();
        if (extrude != null)
            extrude.Rebuild();

        n1.AddSplineContainer(container);
        n2.AddSplineContainer(container);

        container.gameObject.name = $"Spline_{n1.name}_to_{n2.name}";
    }

    private void ClearExistingSplineObjects()
    {
        Transform parent = _splinesParent ? _splinesParent : transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child.GetComponent<SplineContainer>() != null)
                Destroy(child.gameObject);
        }
    }

    public List<BaseNode> GetAllNodes() => _mapNodes;

    public List<BaseNode> GetAllNodesInArea(Rect area)
    {
        var selectionData = GetAllNodesInAreaWithCoverage(area);
        var result = new List<BaseNode>();
        foreach (var data in selectionData)
        {
            result.Add(data.node);
        }
        return result;
    }

    public List<NodeSelectionData> GetAllNodesInAreaWithCoverage(Rect screenArea)
    {
        var result = new List<NodeSelectionData>();
        if (_mapNodes == null || _mapNodes.Count == 0) return result;

        Camera cam = Camera.main;
        if (cam == null) return result;

        var settings = GameManager.Instance.GetGameSettings();
        float minThreshold = settings.minSelectionCoverageThreshold;
        int samplesPerRadius = settings.selectionSamplePointsPerRadius;

        foreach (var node in _mapNodes)
        {
            if (node == null) continue;

            var collider = node.GetComponent<CircleCollider2D>();
            if (collider == null) continue;

            float coverage = CalculateCircleColliderCoverage(node, collider, screenArea, cam, samplesPerRadius);
            
            if (coverage >= minThreshold)
            {
                result.Add(new NodeSelectionData(node, coverage));
            }
        }

        return result;
    }

    private float CalculateCircleColliderCoverage(BaseNode node, CircleCollider2D collider, Rect screenArea, Camera cam, int samplesPerRadius)
    {
        Vector3 worldCenter = node.transform.position + (Vector3)collider.offset;
        float worldRadius = collider.radius * Mathf.Max(node.transform.lossyScale.x, node.transform.lossyScale.y);

        int gridSize = samplesPerRadius * 2;
        int totalPoints = 0;
        int pointsInSelection = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                float normalizedX = (x / (float)(gridSize - 1)) * 2f - 1f;
                float normalizedY = (y / (float)(gridSize - 1)) * 2f - 1f;

                if (normalizedX * normalizedX + normalizedY * normalizedY > 1f)
                    continue;

                Vector3 samplePoint = worldCenter + new Vector3(
                    normalizedX * worldRadius,
                    normalizedY * worldRadius,
                    0f
                );

                Vector3 screenPos = cam.WorldToScreenPoint(samplePoint);
                
                if (screenPos.z < 0f) continue;

                totalPoints++;

                if (screenArea.Contains(new Vector2(screenPos.x, screenPos.y)))
                {
                    pointsInSelection++;
                }
            }
        }

        if (totalPoints == 0) return 0f;
        return pointsInSelection / (float)totalPoints;
    }
}
