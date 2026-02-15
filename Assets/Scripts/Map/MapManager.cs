using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

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
        var result = new List<BaseNode>();
        if (_mapNodes == null || _mapNodes.Count == 0) return result;

        Camera cam = Camera.main;
        if (cam == null) return result;

        foreach (var node in _mapNodes)
        {
            if (node == null) continue;

            Vector3 screenPos = cam.WorldToScreenPoint(node.transform.position);
            if (screenPos.z < 0f) continue;

            if (area.Contains(new Vector2(screenPos.x, screenPos.y)))
                result.Add(node);
        }

        return result;
    }
}
