using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class Veins : MonoBehaviour
{
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _pulseSpeedFactor = 0.5f;

    private SplineExtrude _sExtrude;

    private float _timer = 0f;
    private float _maxTime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        _sExtrude = GetComponent<SplineExtrude>();
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime * _pulseSpeedFactor;
        if(_timer >= _maxTime){
            _timer = 0f;
        }
        _sExtrude.Radius = _curve.Evaluate(_timer);
        _sExtrude.Rebuild();
    }
}
