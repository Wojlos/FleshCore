using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RoadManager : MonoBehaviour
{
    private static RoadManager _instance;

    public static RoadManager Instance;

    [SerializeField] private SplineContainer _splineContainer;
    [SerializeField] private GameObject _preview;
    [SerializeField] private GameObject _junctionPrefab;
    [SerializeField] [Range(0.001f,1f)] private float _proximityFactor = 0.01f;
    [SerializeField] [Range(0.001f,1f)] private float _clampDistance = 0.5f;

    private List<GameObject> _roads = new List<GameObject>();
    private List<Junction> _allJunctions = new List<Junction>();
    private List<BezierKnot> _currentSplineJunctions = new List<BezierKnot>();
    private List<VeinEntrance> _allEntrances = new List<VeinEntrance>();

    private SplineContainer _previewContainer;
    private SplineInstantiate _splineInstantiate;

    private Junction _currentJunction;
    private Junction _baseJunction;
    private Spline _currentSpline;

    private bool _isBuilding = false;

    void Awake(){
        if (Instance != null)
        {
            Debug.LogError("There is more then one RoadManager on the Scene");
            return;
        }
        Instance = this;
        _previewContainer = _preview.GetComponent<SplineContainer>();
        _splineInstantiate = _preview.GetComponent<SplineInstantiate>();
        _currentSpline = _splineContainer.Spline;
    }

    void Update(){
        if(_baseJunction != null && _previewContainer.Splines[0].Count >= 1){
            // FindClosestSplinePosition(_baseJunction.transform.position);
            // var baseKnot = _baseJunction.GetKnot();
            // currentKnot.Position = _previewContainer.transform.InverseTransformPoint(_baseJunction.transform.position);
            var currentKnot = _baseJunction.GetKnot();
            currentKnot.Position = _previewContainer.transform.InverseTransformPoint(_baseJunction.transform.position);

            // _previewContainer.Splines[0].SetKnot(0, baseKnot);
            _previewContainer.Splines[0].SetKnot(_previewContainer.Splines[0].Count-1, currentKnot);
            _preview.SetActive(true);   
        }

    }
    
    public void AddJunction(Junction j, bool isBase){
    
        if(isBase){
            _baseJunction = j;
        }else{
            VeinEntrance entrance = GetVeinEntraceInRange(j.transform.position, _clampDistance);
            if(entrance != null){
                entrance.SetJunction(j);
            }            
            _allJunctions.Add(j);
            var knot = j.GetKnot();
            knot.Position = _splineContainer.transform.InverseTransformPoint(knot.Position);
            knot.Rotation =  Quaternion.identity;

            _currentSpline.Add(knot, TangentMode.AutoSmooth);

            SplineKnotIndex mainKnot = new SplineKnotIndex(_splineContainer.Splines.Count-1, _currentSpline.Count-1); 
            j.AddKnot(mainKnot);

            foreach(SplineKnotIndex k in GetNeighbouringKnots(mainKnot)){
                j.LinkJunctions(GetJunctionByKnot(k));
            }
            
            if(j.GetKnotList().Count > 1){
                foreach(SplineKnotIndex k in j.GetKnotList()){
                    _splineContainer.LinkKnots(k, mainKnot);
                }
                return;
            }


            GetSplinePoints(j.transform.position, out int closestSpline, out Vector3 closestPos, out float knotIdx);
            
            if(Vector3.Distance(closestPos, j.transform.position) <= _clampDistance){
                // bool newKnot = (j.GetKnotList().Count != 0); 
                SplineKnotIndex knotPrev = new SplineKnotIndex(closestSpline, (int)Mathf.Floor(knotIdx)); 
                SplineKnotIndex knotNext = new SplineKnotIndex(closestSpline, (int)Mathf.Ceil(knotIdx));
                j.AddKnot(knotNext);
                
                var prevJ = GetJunctionByKnot(knotPrev);
                var nextJ = GetJunctionByKnot(knotNext);

                j.AddPrev(prevJ);
                prevJ.RemoveNext(nextJ);
                prevJ.AddNext(j);

                j.AddNext(nextJ);
                nextJ.RemovePrev(prevJ);
                nextJ.AddPrev(j);

                _splineContainer.Splines[closestSpline].Insert((int)Mathf.Ceil(knotIdx), knot);
                UpdateSpline(closestSpline);
                _splineContainer.LinkKnots(knotNext, mainKnot);

            }
        }
    }

    public void AddEntrance(VeinEntrance ve){
        _allEntrances.Add(ve);
    }

    private void UpdateSpline(int spline){
        Debug.Log(spline);
        SplineKnotIndex firstKnot = new SplineKnotIndex(spline, 0);
        Debug.Log(firstKnot);
        Junction firstJ = GetJunctionByKnot(firstKnot);
        Junction j = firstJ.GetNextOnSpline(spline); 

        for(int i = 1; i < _splineContainer.Splines[spline].Count; i++){
            j.RemoveKnotsBySpline(spline);
            SplineKnotIndex knot = new SplineKnotIndex(spline, i);
            j.AddKnot(knot);
            j = j.GetNextOnSpline(spline);
        }
    }

    public void RemoveEntrance(VeinEntrance ve){
        _allEntrances.Remove(ve);
    }

    public Junction GetClosestJunction(Vector3 position){
        float maxDistance = 10000f;
        Junction closestJunction = null;

        foreach(Junction j in _allJunctions){
            float distance = Vector3.Distance(j.transform.position, position);
            if(distance < maxDistance ){
                maxDistance = distance;
                closestJunction = j;
            }
        }
        return closestJunction;
    }

    public VeinEntrance GetVeinEntraceInRange(Vector3 position, float maxDistance){
        // float maxDistance = 10000f;
        VeinEntrance closestEntrance = null;

        foreach(VeinEntrance ve in _allEntrances){
            float distance = Vector3.Distance(ve.transform.position, position);
            if(distance < maxDistance ){
                maxDistance = distance;
                closestEntrance = ve;
            }
        }
        return closestEntrance;
    }


    public void AddPreviewKnot(){
        var knot = new BezierKnot(new float3(_baseJunction.transform.position.x, _baseJunction.transform.position.y, 0f));
        knot.Position = _previewContainer.transform.InverseTransformPoint(_baseJunction.transform.position);
        _previewContainer.Splines[0].Add(knot, TangentMode.AutoSmooth);
        if(_previewContainer.Splines[0].Count == 1){
            _previewContainer.Splines[0].Add(knot, TangentMode.AutoSmooth);
        }

    }
    
    public void AddCurrentJunction(JunctionPreview j){
        // AddJunction(Instantiate(_junctionPrefab, j.transform.position, Quaternion.identity).GetComponent<Junction>(), false);
    }

    public Junction GetJunctionByKnot(SplineKnotIndex knot){
        foreach(var j in _allJunctions){
            if(j.GetKnotList().Contains(knot)){
                return j;
            }
        }
        return null;
    }

    public List<SplineKnotIndex> GetNeighbouringKnots(SplineKnotIndex knot){
        List<SplineKnotIndex> neighbours = new List<SplineKnotIndex>(); 
        int splineIdx = knot.Spline;
        Spline spline = _splineContainer.Splines[splineIdx];

        int knotIdx = knot.Knot;
        if(knotIdx != 0){
            neighbours.Add(new SplineKnotIndex(splineIdx, knotIdx-1));
        }
        if(knotIdx < spline.Count){
            neighbours.Add(new SplineKnotIndex(splineIdx, knotIdx+1));
        }
        return neighbours;
    }

    public void StopBuilding(){
        _currentJunction = null;
        _baseJunction = null;
        // _preview.SetActive(false);
    }

    public void AcceptRoad(){
        _splineContainer.AddSpline();
        _currentSpline = _splineContainer.Splines[(_splineContainer.Splines.Count-1)];

        _previewContainer.Spline.RemoveAt(_previewContainer.Spline.Count-1);

        InputManager.Instance.ClearStructure();
        // _splineInstantiate.UpdateInstances();
        // Invoke("AddK", 1f);
        // _splineInstantiate.Clear();
        _isBuilding = true;
        BuildSpline();
        _isBuilding = false;
        _previewContainer.Splines[0].Clear();
        
        StopBuilding();
    }

    private void BuildSpline(){
        // czy żyły mogą przez siebie przenikać?
        // jeśli tak, to widoczność może być trudna, trzeba dodać mosty/skrzyżowania
        // jeśli nie to za duży hałas ? 

        Junction prev = null;
        foreach(var k in _previewContainer.Splines[0]){
            Vector3 pos = _previewContainer.transform.TransformPoint(k.Position);
            Junction junction = GetClosestJunction(pos);
            if(junction == null || Vector3.Distance(junction.transform.position, pos) >= 1f){
                junction = Instantiate(_junctionPrefab, pos, Quaternion.identity).GetComponent<Junction>();
            }
            if(prev != null){
                junction.AddPrev(prev);
                prev.AddNext(junction);
            }
            prev = junction;
            AddJunction(junction, false);
        }
    }

    public void GetSplinePoints(Vector3 position, out int closestSpline, out Vector3 closestPos, out float knotIdx){
        float maxDistance = 100000f;
        closestPos = Vector3.zero;
        closestSpline = 0;

        float closestT = 0;

        int factor = 0;
        if(_isBuilding){
            factor = 1;
        }
        for(int s = 0; s < _splineContainer.Splines.Count - factor; s++){
            var spline = _splineContainer.Splines[s];
            if(spline.Count == 0){
                continue;
            }
            for(float i = 0f; i <= 1; i += _proximityFactor){
                spline.Evaluate(i, out float3 pPos, out float3 tangent, out float3 upVector);
                Vector3 pointPosition = _splineContainer.transform.TransformPoint(pPos);
                float distance = Vector3.Distance(pointPosition, position); 
                if(distance < maxDistance ){
                    closestSpline = s;
                    maxDistance = distance;
                    closestPos = pointPosition;
                    closestT = i;
                }
            }
        }
        
        knotIdx = _splineContainer.Splines[closestSpline].ConvertIndexUnit(closestT, PathIndexUnit.Knot);
    }

    public Vector3 FindClosestSplinePosition(Vector3 position){
        // Debug.Log(GetClosestJunction(position));

        GetSplinePoints(position, out int closestSpline, out Vector3 closestPos, out float knotIdx);
        return closestPos;
    }

    public List<Junction> GetAllJunctions(){
        return _allJunctions;
    }
    public Vector3 FindClosestSplineKnots(Vector3 position){
        GetSplinePoints(position, out int closestSpline, out Vector3 closestPos, out float knotIdx);
        return closestPos;
    }
}
