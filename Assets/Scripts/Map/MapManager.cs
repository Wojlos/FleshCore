using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [SerializeField] private SplineContainer _splineContainer;

    private SplineExtrude _splineExtrude;
    private List<BaseNode> _mapNodes = new List<BaseNode>();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _splineExtrude = _splineContainer.GetComponent<SplineExtrude>();
    }

    public void InitializeMap(List<BaseNode> nodes)
    {
        _mapNodes = nodes;
        List<BaseNode> checkedNodes = new List<BaseNode>();
        foreach(var n in nodes)
        {
            foreach(var conN in n.GetConnectedNodes())
            {
                if (!checkedNodes.Contains(conN))
                {
                    AddSpline(conN, n);
                }
            }
            checkedNodes.Add(n);    
        }        
        _splineExtrude.Rebuild();
        
    }

    private void AddSpline(BaseNode n1, BaseNode n2)
    {
        Spline newSpline = new Spline();
        _splineContainer.AddSpline(newSpline);
        newSpline.Add(new BezierKnot(n1.transform.position), TangentMode.AutoSmooth);
        newSpline.Add(new BezierKnot(n2.transform.position), TangentMode.AutoSmooth);
        n1.AddSpline(newSpline);
        n2.AddSpline(newSpline);
    }

}
