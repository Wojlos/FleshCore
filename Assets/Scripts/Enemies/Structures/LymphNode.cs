using UnityEngine;
using System.Collections.Generic;

public class LymphNode : EnemyBuilding
{
    private List<UnitTravelGroup> _currentTravelGroups = new List<UnitTravelGroup>();
    private int _maxPatrolGroups;
    private int _minPatrolSize;

    public LymphNode() : base(BuildingType.LymphNode)
    {
    }

    private void Awake()
    {
        var settings = GameManager.Instance.GetGameSettings();
        _maxPatrolGroups = settings.maxImmunoPatroleGroups;
        _minPatrolSize = settings.minPatroleSize;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        _currentTravelGroups.RemoveAll(tg => tg == null);

        if (_currentTravelGroups.Count < _maxPatrolGroups && _node.GetUnits(ConflictSide.Immuno) >= _minPatrolSize)
            SpawnPatrolGroup();
    }

    private void SpawnPatrolGroup()
    {
        var tg = _node.SendPatrol(_minPatrolSize, ConflictSide.Immuno);
        if (tg != null)
            _currentTravelGroups.Add(tg);
    }

    public override void Cleanup()
    {
        base.Cleanup();
        foreach (var tg in _currentTravelGroups)
        {
            if (tg != null)
                tg.DisbandAndDeleteUnits();
        }
        _currentTravelGroups.Clear();
    }
}
