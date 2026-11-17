using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SplineFollower : MonoBehaviour
{
    private SplineContainer _container;
    private float _speed;
    private bool _reverse;
    private float _t;
    private float _splineLength;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    /// <summary>
    /// Configure and start following a spline.
    /// </summary>
    /// <param name="container">The SplineContainer to follow.</param>
    /// <param name="speed">World-units per second.</param>
    /// <param name="reverse">If true, travel from end to start.</param>
    public void Play(SplineContainer container, float speed, bool reverse)
    {
        _container = container;
        _speed = speed;
        _reverse = reverse;
        _splineLength = container.Spline.GetLength();

        _t = reverse ? 1f : 0f;
        _isPlaying = true;

        // Snap to start position immediately
        UpdatePosition();
    }

    public void Stop()
    {
        _isPlaying = false;
    }

    private void Update()
    {
        if (!_isPlaying || _container == null) return;

        float delta = (_speed * Time.deltaTime) / _splineLength;

        if (_reverse)
        {
            _t -= delta;
            if (_t <= 0f)
            {
                _t = 0f;
                _isPlaying = false;
            }
        }
        else
        {
            _t += delta;
            if (_t >= 1f)
            {
                _t = 1f;
                _isPlaying = false;
            }
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float3 position;
        float3 tangent;
        float3 up;
        SplineUtility.Evaluate(_container.Spline, _t, out position, out tangent, out up);

        // SplineContainer knots are in local space — convert to world
        transform.position = _container.transform.TransformPoint(position);
    }
}
