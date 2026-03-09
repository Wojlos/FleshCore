using UnityEngine;
using System.Collections.Generic;

public class LymphNode : EnemyBuilding
{
    private List<UnitTravelGroup> _currentTravelGroups = new List<UnitTravelGroup>();
    private int _maxPatrolGroups;
    private int _minPatrolSize;
    private bool _patrolsBlocked;
    private int _immunoUnitsToSpawnLymphNode;
    private float _alertDuration;
    private float _alertStartTime;

    public LymphNode() : base(BuildingType.LymphNode)
    {
    }

    protected override void Awake()
    {
        base.Awake();
        var settings = GameManager.Instance.GetGameSettings();
        _maxPatrolGroups = settings.maxImmunoPatroleGroups;
        _minPatrolSize = settings.minPatroleSize;
        _immunoUnitsToSpawnLymphNode = settings.immunoUnitsToSpawnLymphNode;
        _alertDuration = settings.lymphNodeAlertDuration;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        _currentTravelGroups.RemoveAll(tg => tg == null);

        if (_patrolsBlocked)
        {
            CheckRallyPointSpawn();
            return;
        }

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

    private void CheckRallyPointSpawn()
    {
        if (Time.time - _alertStartTime >= _alertDuration)
        {
            Dealert();
            return;
        }

        var rallyNode = _node.GetRallyPoint();
        if (rallyNode == null)
        {
            Dealert();
            return;
        }

        var existingBuilding = rallyNode.GetBuilding();
        if (existingBuilding != null)
        {
            if (existingBuilding.Side == ConflictSide.Immuno)
                Dealert();
            return;
        }

        if (rallyNode.GetUnits(ConflictSide.Player) > 0) return;

        if (rallyNode.GetUnits(ConflictSide.Immuno) < _immunoUnitsToSpawnLymphNode) return;

        if (EnemyManager.Instance.TrySpawnLymphNode(rallyNode, _immunoUnitsToSpawnLymphNode))
            Dealert();
    }

    private void Dealert()
    {
        _patrolsBlocked = false;
        _node.ClearRallyPoint();
    }

    public override void OnEncounterReported(BaseNode encounterNode, UnitTravelGroup reporter)
    {
        _patrolsBlocked = true;
        _alertStartTime = Time.time;

        int unitsInLocalPatrols = 0;
        foreach (var tg in _currentTravelGroups)
        {
            if (tg == null || tg == reporter) continue;
            tg.RedirectTo(encounterNode);
            if (tg.GetCurrentNode() == _node)
                unitsInLocalPatrols += tg.GetUnits();
        }

        int localUnits = _node.GetUnits(ConflictSide.Immuno);
        int freeUnits = localUnits - unitsInLocalPatrols;
        if (freeUnits > 0)
        {
            var tg = _node.SendUnitsDirected(freeUnits, encounterNode, ConflictSide.Immuno);
            if (tg != null)
                _currentTravelGroups.Add(tg);
        }

        _node.SetRallyPoint(encounterNode);
    }
}
