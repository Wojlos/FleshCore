using UnityEngine;
using System.Collections.Generic;

public class TargetGroup : MonoBehaviour
{
    [Header("Target Scaling (Capped Linear)")]
    [SerializeField] private int _unitCount = 10;
    [SerializeField] private int _linearCap = 8;
    [SerializeField] private int _unitsPerExtraTarget = 20;
    [SerializeField] private int _maxTargets = 15;

    [Header("Target Visuals")]
    [SerializeField] private GameObject _targetPrefab;

    private List<SwarmTarget> _targets = new List<SwarmTarget>();
    private bool _registered;
    private int _spawnIndex;

    private UnitTravelGroup _owner;

    // Orbit state
    private float _orbitAngle;
    private float _orbitStrength;          // 0 = no orbit (moving), 1 = full orbit (idle on node)
    private Vector2 _orbitOffset;          // current smoothed orbit offset added to the center
    private Vector2 _orbitVelocity;

    private void Awake()
    {
        SetupCollider();
        RegisterWithEnemyManager();

        // Random initial phase so multiple groups on the same node spread out
        // and adding/removing a group never reshuffles the others.
        _orbitAngle = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        UpdateOrbit();
    }

    private void SetupCollider()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        var col = GetComponent<CircleCollider2D>();
        if (col == null)
            col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.1f;
    }

    private void OnDestroy()
    {
        UnregisterFromEnemyManager();
        // Targets are un-parented (world space), so destroy them explicitly.
        ClearTargets();
    }

    public void SetUnitCount(int count)
    {
        _unitCount = count;
    }

    public void SetOwner(UnitTravelGroup owner)
    {
        _owner = owner;
    }

    public UnitTravelGroup GetOwner() => _owner;

    // World-space center the swarm follows, including the idle orbit offset.
    public Vector2 GetSwarmCenter() => (Vector2)transform.position + _orbitOffset;

    public List<SwarmTarget> GetTargets() => _targets;

    public List<Transform> GetTargetTransforms()
    {
        _targets.RemoveAll(t => t == null);
        var transforms = new List<Transform>();
        foreach (var t in _targets)
            transforms.Add(t.transform);
        return transforms;
    }

    // Incrementally reconciles the visual count: survivors keep their exact
    // position/movement, only the excess is removed or new ones are added.
    public void SpawnTargets()
    {
        int visualCount = CalculateVisualCount(_unitCount);

        _targets.RemoveAll(t => t == null);
        if (_targets.Count == visualCount) return;

        if (_targets.Count > visualCount)
        {
            RemoveExcessTargets(visualCount);
            return;
        }

        AddTargets(visualCount - _targets.Count);
    }

    private void RemoveExcessTargets(int desiredCount)
    {
        // Destroy the highest-index (most recently added) targets so the rest
        // keep their positions and wander patterns.
        for (int i = _targets.Count - 1; i >= desiredCount; i--)
        {
            if (_targets[i] != null)
                Destroy(_targets[i].gameObject);
            _targets.RemoveAt(i);
        }
    }

    private void AddTargets(int amount)
    {
        if (_targetPrefab == null || amount <= 0) return;

        var settings = GameManager.Instance.GetGameSettings();
        float wanderRadius = settings.targetGroupWanderRadius;
        float speedMin = settings.swarmTargetSpeedMin;
        float speedMax = settings.swarmTargetSpeedMax;
        float followSmoothing = settings.swarmTargetFollowSmoothing;

        float goldenAngle = 137.508f * Mathf.Deg2Rad;
        Vector2 center = GetSwarmCenter();

        for (int n = 0; n < amount; n++)
        {
            int i = _spawnIndex++;
            float angle = i * goldenAngle;
            float radius = wanderRadius * Mathf.Sqrt((float)(i + 1) / Mathf.Max(1, _maxTargets));
            Vector2 offset = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);

            Vector3 spawnPos = new Vector3(center.x + offset.x, center.y + offset.y, transform.position.z);

            // Spawn in world space (no parent) so group movement can be followed
            // smoothly with a lag instead of rigidly inherited.
            GameObject go = Instantiate(_targetPrefab, spawnPos, Quaternion.identity);
            var swarmTarget = go.GetComponent<SwarmTarget>();
            if (swarmTarget == null)
                swarmTarget = go.AddComponent<SwarmTarget>();

            float speed = Random.Range(speedMin, speedMax);
            swarmTarget.Initialize(this, wanderRadius, speed, followSmoothing);

            _targets.Add(swarmTarget);
        }
    }

    private void UpdateOrbit()
    {
        var settings = GameManager.Instance != null ? GameManager.Instance.GetGameSettings() : null;
        if (settings == null) return;

        bool idleOnNode = _owner != null
            && _owner.GetState() == BodyState.Idle
            && _owner.GetCurrentNode() != null;

        // Blend orbit strength in/out so transitions (arrive/leave node,
        // group added/removed) are smooth rather than snapping.
        float targetStrength = idleOnNode ? 1f : 0f;
        _orbitStrength = Mathf.MoveTowards(_orbitStrength, targetStrength, Time.deltaTime * 2f);

        _orbitAngle += settings.targetGroupOrbitSpeed * Mathf.Deg2Rad * Time.deltaTime;

        float radius = settings.targetGroupOrbitRadius * _orbitStrength;
        Vector2 desiredOffset = new Vector2(Mathf.Cos(_orbitAngle), Mathf.Sin(_orbitAngle)) * radius;

        // SmoothDamp the offset itself for extra smoothness on top of the strength blend.
        _orbitOffset = Vector2.SmoothDamp(_orbitOffset, desiredOffset, ref _orbitVelocity, 0.25f);
    }

    private int CalculateVisualCount(int units)
    {
        if (units <= 0) return 0;

        int count;
        if (units <= _linearCap)
        {
            count = units;
        }
        else
        {
            int extra = Mathf.CeilToInt((float)(units - _linearCap) / _unitsPerExtraTarget);
            count = _linearCap + extra;
        }

        return Mathf.Clamp(count, 1, _maxTargets);
    }

    private void ClearTargets()
    {
        foreach (var t in _targets)
        {
            if (t != null)
                Destroy(t.gameObject);
        }
        _targets.Clear();
    }

    private void RegisterWithEnemyManager()
    {
        if (_registered) return;
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterTargetGroup(this);
            _registered = true;
        }
    }

    private void UnregisterFromEnemyManager()
    {
        if (!_registered) return;
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.UnregisterTargetGroup(this);
            _registered = false;
        }
    }
}
