using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Junction : MonoBehaviour
{
    [SerializeField] GameObject _tooltipPrefab;
    private Transform _uiCanvas;

    private BezierKnot _knot;
    private List<SplineKnotIndex> _attachedKnots = new List<SplineKnotIndex>();
    private List<Junction> _neighbours = new List<Junction>();
    private GameObject _toolTip;
    
    private List<Junction> _prev = new List<Junction>();
    private List<Junction> _next = new List<Junction>();
    private VeinEntrance _veinEntrance;

    private float _resource = 0f;

    private Dictionary<ResourceType, float> _netResources = new Dictionary<ResourceType, float>();
    // private bool _preview = false;

    void Awake(){
        _uiCanvas = GameObject.FindGameObjectsWithTag("UICanvas")[0].transform;
        _toolTip = Instantiate(_tooltipPrefab, transform.position, Quaternion.identity, _uiCanvas);
        _toolTip.SetActive(false);
    }

    void Update(){
        _toolTip.transform.position = transform.position;
    }

    public void SetToolTip(bool b){
        _toolTip.SetActive(b);
    }

    public void AddKnot(SplineKnotIndex newKnot){
        _attachedKnots.Add(newKnot);
    }

    public BezierKnot GetKnot(){
        return new BezierKnot(new float3(transform.position.x, transform.position.y, 0f));
    }

    public void RemoveKnotsBySpline(int spline){
        for(int k = 0; k < _attachedKnots.Count; k++){
            if(_attachedKnots[k].Spline == spline){
                _attachedKnots.RemoveAt(k);
            }
        }
    }

    public Junction GetNextOnSpline(int spline){
        Junction next = null;
        List<Junction> nextJunctions = GetNext();
        for(int n = 0; n < nextJunctions.Count; n++){
            List<SplineKnotIndex> nKnots = nextJunctions[n].GetKnotList();
            for(int k = 0; k < nKnots.Count; k++){
                if(nKnots[k].Spline == spline){
                    next = nextJunctions[n];
                }
            }
        }
        return next;
    }

    public List<SplineKnotIndex> GetKnotList(){
        return _attachedKnots;
    }

    [ContextMenu("Check resources")]
    public void CheckResources(){
        foreach (KeyValuePair<ResourceType, float> entry in _netResources){
            Debug.Log(entry.Key +" "+ entry.Value);
        }
    }

    public void LinkJunctions(Junction n){
        if(n == null) return;
        AddNeighbour(n);
        n.AddNeighbour(this);
    }

    public void AddNeighbour( Junction n){
        if(!_neighbours.Contains(n)){
            _neighbours.Add(n);
        }
    }

    public List<Junction> GetNeighbours(){
        return _neighbours;
    }

    public void AddPrev( Junction n){
        if(!_prev.Contains(n)){
            _prev.Add(n);
        }
    }

    public void RemovePrev( Junction n){
        if(_prev.Contains(n)){
            _prev.Remove(n);
        }
    }

    public List<Junction> GetPrev(){
        return _prev;
    }
    
    public void AddNext( Junction n){
        if(!_next.Contains(n)){
            _next.Add(n);
        }
    }

    public void RemoveNext( Junction n){
        if(_next.Contains(n)){
            _next.Remove(n);
        }
    }

    public List<Junction> GetNext(){
        return _next;
    }

    public float GetResource(ResourceType r){
        return _netResources[r];
    }

    public void ClearResources(){
        _netResources.Clear();
    }

    public void AddResource(ResourceType r, float v){
        if(!_netResources.ContainsKey(r)){
            _netResources[r] = 0f;
        }
        _netResources[r] += v;
        _toolTip.GetComponent<ResourceToolTip>().SetResources(_netResources);
    }

    public void SetEntrance(VeinEntrance v){
        _veinEntrance = v;
    }

    public VeinEntrance GetEntrance(){
        return _veinEntrance;
    }

    public Dictionary<ResourceType, float> GetNetResources(){
        return _netResources;
    }

}