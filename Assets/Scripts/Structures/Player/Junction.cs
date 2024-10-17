using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Junction : MonoBehaviour
{
    private BezierKnot _knot;
    private List<SplineKnotIndex> _attachedKnots = new List<SplineKnotIndex>();
    private List<Junction> _neighbours = new List<Junction>();
    
    private List<Junction> _prev = new List<Junction>();
    private List<Junction> _next = new List<Junction>();


    private int _resource = 0;

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
}