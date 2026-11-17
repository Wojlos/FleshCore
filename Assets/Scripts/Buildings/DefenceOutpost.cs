using UnityEngine;
using System.Collections.Generic;

public class DefenceOutpost : PlayerBuilding
{
    [SerializeField] private TurretController _turret;

    private CircleCollider2D _rangeTrigger;
    private HashSet<TargetGroup> _inRangeGroups = new HashSet<TargetGroup>();
    private TargetGroup _activeGroup;

    public DefenceOutpost() : base(BuildingType.DefenceOutpost)
    {
        
    }
    private float _unitProductionTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        SetupRangeTrigger();
        if (_turret != null)
            _turret.Activate();
    }

    private void SetupRangeTrigger()
    {
        var settings = GameManager.Instance.GetGameSettings();

        var rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;

        _rangeTrigger = gameObject.AddComponent<CircleCollider2D>();
        _rangeTrigger.isTrigger = true;
        _rangeTrigger.radius = settings.turretRange;
    }

    protected override void OnUpdate()
    {

        base.OnUpdate();

        UpdateTurretTargets();

        if (!_active) return;

        if (_unitProductionPerSecond <= 0) return;

        float spawnInterval = 1f / _unitProductionPerSecond;
        _unitProductionTimer += Time.deltaTime;
        
        if (_unitProductionTimer >= spawnInterval)
        {
            _unitProductionTimer -= spawnInterval;
            _node.CreateUnits(1, ConflictSide.Player);
        }
        
    }

    private void UpdateTurretTargets()
    {
        if (_turret == null)
        {
            return;
        }

        _inRangeGroups.RemoveWhere(g => g == null);

        if (_inRangeGroups.Count == 0)
        {
            if (_activeGroup != null)
            {
                _activeGroup = null;
                _turret.SetTargets(null);
            }
            return;
        }

        TargetGroup closest = null;
        float closestDist = float.MaxValue;
        foreach (var group in _inRangeGroups)
        {
            float dist = Vector2.Distance(transform.position, group.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = group;
            }
        }

        if (closest != _activeGroup)
        {
            _activeGroup = closest;
            if (_activeGroup == null)
            {
                _turret.SetTargets(null);
                return;
            }
        }

        if (_activeGroup != null)
            _turret.SetTargets(_activeGroup.GetTargetTransforms());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var targetGroup = other.GetComponent<TargetGroup>();
        if (targetGroup != null)
        {
            _inRangeGroups.Add(targetGroup);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var targetGroup = other.GetComponent<TargetGroup>();
        if (targetGroup != null)
        {
            _inRangeGroups.Remove(targetGroup);
            if (targetGroup == _activeGroup)
            {
                _activeGroup = null;
                if (_turret != null)
                    _turret.SetTargets(null);
            }
        }
    }

    public override void Enable()
    {
        base.Enable();
        if (_turret != null)
            _turret.Activate();
    }

    public override void Disable()
    {
        base.Disable();
        if (_turret != null)
            _turret.Deactivate();
        _activeGroup = null;
        _inRangeGroups.Clear();
    }

    public float GetTurretRange()
    {
        var settings = GameManager.Instance?.GetGameSettings();
        return settings != null ? settings.turretRange : 5f;
    }
}
