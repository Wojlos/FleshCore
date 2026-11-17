using UnityEngine;
using System;
using System.Collections.Generic;

public class NodeEncounterController : MonoBehaviour
{
    private BaseNode _node;
    private EncounterType _currentEncounter = EncounterType.None;

    private float _battleTimer = 0f;
    private float _battleTickInterval;
    private float _playerBattleDamageChance;
    private float _enemyBattleDamageChance;
    private float _playerSiegeDamageChance;
    private float _enemySiegeDamageChance;
    private float _nodeConquestTickInterval;
    private float _playerConquestDamageChance;
    private float _immunoConquestDamageChance;
    private float _conquestTimer = 0f;

    private HealthSystem _nodeHealthSystem;
    private int _maxNodeHP;

    public event Action<EncounterInfo> OnEncounterChanged;

    private List<UnitTravelGroup> _registeredPlayerGroups = new List<UnitTravelGroup>();
    private List<UnitTravelGroup> _registeredEnemyGroups = new List<UnitTravelGroup>();

    public void Initialize(BaseNode node, HealthSystem nodeHealthSystem)
    {
        _node = node;
        _nodeHealthSystem = nodeHealthSystem;

        var settings = GameManager.Instance.GetGameSettings();
        _battleTickInterval = settings.battleTickInterval;
        _playerBattleDamageChance = settings.playerBattleDamageChance;
        _enemyBattleDamageChance = settings.enemyBattleDamageChance;
        _playerSiegeDamageChance = settings.playerSiegeDamageChance;
        _enemySiegeDamageChance = settings.enemySiegeDamageChance;
        _nodeConquestTickInterval = settings.nodeConquestTickInterval;
        _playerConquestDamageChance = settings.playerConquestDamageChance;
        _immunoConquestDamageChance = settings.immunoConquestDamageChance;
        _maxNodeHP = settings.maxNodeHP;
    }

    private void Update()
    {
        if (_node == null) return;
        EvaluateAndProcess();
    }

    public void RegisterGroup(UnitTravelGroup group, ConflictSide side)
    {
        if (side == ConflictSide.Player)
            _registeredPlayerGroups.Add(group);
        else
            _registeredEnemyGroups.Add(group);
    }

    public void UnregisterGroup(UnitTravelGroup group, ConflictSide side)
    {
        if (side == ConflictSide.Player)
            _registeredPlayerGroups.Remove(group);
        else
            _registeredEnemyGroups.Remove(group);
    }

    public EncounterInfo GetCurrentEncounterInfo()
    {
        return BuildEncounterInfo(_currentEncounter);
    }

    public EncounterType GetCurrentEncounterType() => _currentEncounter;

    private void EvaluateAndProcess()
    {
        int playerUnits = _node.GetUnits(ConflictSide.Player);
        int enemyUnits = _node.GetUnits(ConflictSide.Immuno);
        var building = _node.GetBuilding();

        bool bothSidesPresent = playerUnits > 0 && enemyUnits > 0;
        bool playerSiegesEnemyBuilding = playerUnits > 0 && building != null && building.Side == ConflictSide.Immuno;
        bool enemySiegesPlayerBuilding = enemyUnits > 0 && building != null && building.Side == ConflictSide.Player;

        if (bothSidesPresent)
        {
            SetEncounterType(EncounterType.Battle);

            // Friendly defenders are present, so the building is NOT sieged
            // during a battle. It only gets sieged when left undefended below.
            if (building != null && building.IsSieged)
                building.SetSieged(false);

            _conquestTimer = 0f;
            _battleTimer += Time.deltaTime;
            if (_battleTimer >= _battleTickInterval)
            {
                _battleTimer = 0f;
                ProcessBattleTick(playerUnits, enemyUnits);
            }
        }
        else if (playerSiegesEnemyBuilding || enemySiegesPlayerBuilding)
        {
            SetEncounterType(EncounterType.Siege);

            if (building != null)
                building.SetSieged(true);

            _conquestTimer = 0f;
            _battleTimer += Time.deltaTime;
            if (_battleTimer >= _battleTickInterval)
            {
                _battleTimer = 0f;
                ProcessSiegeTick(building, playerSiegesEnemyBuilding, playerUnits, enemyUnits);
            }
        }
        else
        {
            if (building != null && building.IsSieged)
                building.SetSieged(false);

            _battleTimer = 0f;

            bool playerConquersNode = playerUnits > 0 && _node.GetOwner() == ConflictSide.Immuno;
            bool immunoConquersNode = enemyUnits > 0 && _node.GetOwner() == ConflictSide.Player;

            if (playerConquersNode || immunoConquersNode)
            {
                SetEncounterType(EncounterType.NodeConquest);

                _conquestTimer += Time.deltaTime;
                if (_conquestTimer >= _nodeConquestTickInterval)
                {
                    _conquestTimer = 0f;
                    ProcessConquestTick(playerConquersNode, playerUnits, enemyUnits);
                }
            }
            else
            {
                _conquestTimer = 0f;
                SetEncounterType(EncounterType.None);
            }
        }
    }

    private void SetEncounterType(EncounterType newType)
    {
        if (_currentEncounter == newType) return;

        _currentEncounter = newType;
        var info = BuildEncounterInfo(newType);
        OnEncounterChanged?.Invoke(info);
    }

    private EncounterInfo BuildEncounterInfo(EncounterType type)
    {
        var building = _node.GetBuilding();
        int playerUnits = _node.GetUnits(ConflictSide.Player);
        int enemyUnits = _node.GetUnits(ConflictSide.Immuno);

        return new EncounterInfo
        {
            Node = _node,
            Type = type,
            AttackingSide = playerUnits > 0 && enemyUnits > 0
                ? ConflictSide.Player
                : (playerUnits > 0 ? ConflictSide.Player : ConflictSide.Immuno),
            OpposingUnits = playerUnits > 0 ? enemyUnits : playerUnits,
            HasOpposingBuilding = building != null && (
                (playerUnits > 0 && building.Side == ConflictSide.Immuno) ||
                (enemyUnits > 0 && building.Side == ConflictSide.Player))
        };
    }

    private void ProcessBattleTick(int playerUnits, int enemyUnits)
    {
        int killedByPlayer = 0;
        int killedByEnemy = 0;

        for (int i = 0; i < playerUnits; i++)
        {
            if (UnityEngine.Random.value <= _playerBattleDamageChance)
                killedByPlayer++;
        }

        for (int i = 0; i < enemyUnits; i++)
        {
            if (UnityEngine.Random.value <= _enemyBattleDamageChance)
                killedByEnemy++;
        }

        killedByPlayer = Mathf.Min(killedByPlayer, enemyUnits);
        killedByEnemy = Mathf.Min(killedByEnemy, playerUnits);

        if (killedByPlayer > 0)
        {
            DistributeDamage(ConflictSide.Immuno, killedByPlayer);
            _node.DestroyUnits(killedByPlayer, ConflictSide.Immuno);
        }
        if (killedByEnemy > 0)
        {
            DistributeDamage(ConflictSide.Player, killedByEnemy);
            _node.DestroyUnits(killedByEnemy, ConflictSide.Player);
        }
    }

    private void ProcessSiegeTick(BaseBuilding building, bool playerAttacking, int playerUnits, int enemyUnits)
    {
        if (building == null) return;

        int attackerCount = playerAttacking ? playerUnits : enemyUnits;
        float damageChance = playerAttacking ? _playerSiegeDamageChance : _enemySiegeDamageChance;

        int hits = 0;
        for (int i = 0; i < attackerCount; i++)
        {
            if (UnityEngine.Random.value <= damageChance)
                hits++;
        }
        if (hits > 0)
            building.GetComponent<HealthSystem>().ApplyDamage(hits);
    }

    private void ProcessConquestTick(bool playerConquering, int playerUnits, int enemyUnits)
    {
        int attackerCount = playerConquering ? playerUnits : enemyUnits;
        float damageChance = playerConquering ? _playerConquestDamageChance : _immunoConquestDamageChance;

        int hits = 0;
        for (int i = 0; i < attackerCount; i++)
        {
            if (UnityEngine.Random.value <= damageChance)
                hits++;
        }

        if (hits > 0)
            _nodeHealthSystem.ApplyDamage(hits);
    }

    private void DistributeDamage(ConflictSide side, int totalKilled)
    {
        var groups = side == ConflictSide.Player ? _registeredPlayerGroups : _registeredEnemyGroups;
        if (groups.Count == 0) return;

        // Remove nulls
        groups.RemoveAll(g => g == null);
        if (groups.Count == 0) return;

        int totalUnitsInGroups = 0;
        foreach (var g in groups)
            totalUnitsInGroups += g.GetUnits();

        if (totalUnitsInGroups == 0) return;

        int distributed = 0;
        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            int groupUnits = g.GetUnits();
            if (groupUnits <= 0) continue;

            int damage;
            if (i == groups.Count - 1)
                damage = totalKilled - distributed;
            else
                damage = Mathf.RoundToInt((float)groupUnits / totalUnitsInGroups * totalKilled);

            damage = Mathf.Min(damage, groupUnits);
            if (damage > 0)
            {
                g.ApplyDamage(damage);
                distributed += damage;
            }
        }
    }
}
