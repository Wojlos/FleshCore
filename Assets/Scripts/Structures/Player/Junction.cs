using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Junction : MonoBehaviour
{
    private BezierKnot _knot;
    private List<SplineKnotIndex> _attachedKnots = new List<SplineKnotIndex>();


    public void AddKnot(SplineKnotIndex newKnot){
        _attachedKnots.Add(newKnot);
    }

    public BezierKnot GetKnot(){
        return new BezierKnot(new float3(transform.position.x, transform.position.y, 0f));
    }

    public List<SplineKnotIndex> GetKnotList(){
        return _attachedKnots;
    }
}
