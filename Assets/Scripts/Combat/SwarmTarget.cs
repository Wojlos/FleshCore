using UnityEngine;

public class SwarmTarget : MonoBehaviour
{
    private TargetGroup _group;
    private float _wanderRadius;
    private float _wanderSpeed;
    private float _followSmoothing;

    private Vector2 _currentWaypoint;
    private Vector2 _wanderPos;
    private Vector2 _followVelocity;
    private bool _initialized;

    public void Initialize(TargetGroup group, float wanderRadius, float wanderSpeed, float followSmoothing)
    {
        _group = group;
        _wanderRadius = wanderRadius;
        _wanderSpeed = wanderSpeed;
        _followSmoothing = Mathf.Max(0.01f, followSmoothing);
        _initialized = true;

        _wanderPos = (Vector2)transform.position - _group.GetSwarmCenter();
        PickNewWaypoint();
    }

    public TargetGroup GetGroup() => _group;

    private void Update()
    {
        if (!_initialized || _group == null) return;

        // Local wander offset drifts toward the current waypoint at wander speed.
        Vector2 wanderDir = _currentWaypoint - _wanderPos;
        float wanderStep = _wanderSpeed * Time.deltaTime;
        if (wanderDir.magnitude <= wanderStep)
        {
            _wanderPos = _currentWaypoint;
            PickNewWaypoint();
        }
        else
        {
            _wanderPos += wanderDir.normalized * wanderStep;
        }

        // Desired world position = swarm center (incl. orbit) + local wander offset.
        Vector2 desired = _group.GetSwarmCenter() + _wanderPos;

        // Smoothly follow the desired position so group movement looks organic, not rigid.
        Vector2 current = transform.position;
        Vector2 smoothed = Vector2.SmoothDamp(current, desired, ref _followVelocity, _followSmoothing);
        transform.position = new Vector3(smoothed.x, smoothed.y, transform.position.z);
    }

    private void PickNewWaypoint()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(0f, _wanderRadius);
        _currentWaypoint = new Vector2(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist);
    }
}
