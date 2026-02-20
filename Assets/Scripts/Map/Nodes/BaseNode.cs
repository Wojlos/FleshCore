using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

public abstract class BaseNode : MonoBehaviour
{
    [SerializeField] private UnitCounter _playerUnitCounter;
    [SerializeField] private UnitCounter _enemyUnitCounter;
    [SerializeField] private GameObject _unitTravelGroupPrefab;

    private List<BaseNode> _connectedNodes = new List<BaseNode>();
    private List<SplineContainer> _splineContainers = new List<SplineContainer>();
    private BaseBuilding _building;


    private int _playerUnits = 0;
    private int _enemyUnits = 0;
    private List<UnitTravelGroup> _attachedPlayerTravelGroups = new List<UnitTravelGroup>();

    private bool _inBattle = false;
    private bool _sieging = false;
    private float _battleTimer = 0f;
    private const float _battleTickInterval = 1f;
    private const float _damageChance = 0.3f;


    private void Awake() {
        _playerUnitCounter.ConnectNode(this);
        _enemyUnitCounter.ConnectNode(this);

        UpdateCounters();
        HideCounters();
    }

    private void Update()
    {
        CheckAndRunBattle();
    }

    private void CheckAndRunBattle()
    {
        bool bothSidesPresent = _playerUnits > 0 && _enemyUnits > 0;
        bool playerSiegesEnemyBuilding = _playerUnits > 0 && _enemyUnits == 0 && _building != null && _building.Side == ConflictSide.Immuno;
        bool enemySiegesPlayerBuilding = _enemyUnits > 0 && _playerUnits == 0 && _building != null && _building.Side == ConflictSide.Player;

        if (bothSidesPresent)
        {
            if (!_inBattle)
            {
                _inBattle = true;
                _battleTimer = 0f;
            }

            _battleTimer += Time.deltaTime;
            if (_battleTimer >= _battleTickInterval)
            {
                _battleTimer = 0f;
                ProcessBattleTick();
            }
        }
        else if (playerSiegesEnemyBuilding || enemySiegesPlayerBuilding)
        {
            if (_inBattle)
            {
                FinishBattle();
                _inBattle = false;
            }

            if (!_sieging)
            {
                _sieging = true;
                _building.Disable();
            }

            _battleTimer += Time.deltaTime;
            if (_battleTimer >= _battleTickInterval)
            {
                _battleTimer = 0f;
                ProcessSiegeTick();
            }
        }
        else
        {
            if (_inBattle)
            {
                FinishBattle();
            }
            _inBattle = false;

            if (_sieging)
            {
                _sieging = false;
                if (_building != null)
                    _building.Enable();
            }

            _battleTimer = 0f;
        }
    }

    private void FinishBattle()
    {
    }

    private void ProcessBattleTick()
    {
        int killedByPlayer = 0;
        int killedByEnemy = 0;

        for (int i = 0; i < _playerUnits; i++)
        {
            if (Random.value <= _damageChance)
                killedByPlayer++;
        }

        for (int i = 0; i < _enemyUnits; i++)
        {
            if (Random.value <= _damageChance)
                killedByEnemy++;
        }

        killedByPlayer = Mathf.Min(killedByPlayer, _enemyUnits);
        killedByEnemy = Mathf.Min(killedByEnemy, _playerUnits);

        if (killedByPlayer > 0)
            DestroyUnits(killedByPlayer, false);
        if (killedByEnemy > 0)
            DestroyUnits(killedByEnemy, true);
    }

    private void ProcessSiegeTick()
    {
        if (_building == null) return;

        int attackerCount = _building.Side == ConflictSide.Immuno ? _playerUnits : _enemyUnits;
        int hits = 0;
        for (int i = 0; i < attackerCount; i++)
        {
            if (Random.value <= _damageChance)
                hits++;
        }
        if (hits > 0)
            _building.GetComponent<HealthSystem>().ApplyDamage(hits);
    }
    
    public List<UnitTravelGroup> GetTravelGroups() => _attachedPlayerTravelGroups;
    public void AddPlayerTravelGroup(UnitTravelGroup tg)
    {
        _attachedPlayerTravelGroups.Add(tg);
    }
    public void RemovePlayerTravelGroup(UnitTravelGroup tg)
    {
        _attachedPlayerTravelGroups.Remove(tg);
    }
    
    public UnitCounter GetPlayerUnitCounter(){
        if(_playerUnits != 0)
        {
            return _playerUnitCounter;
        }
        return null;
    }
    public int GetAllPlayerUnits()
    {
        return _playerUnits;

    }
    public List<SplineContainer> GetSplineContainers()=> _splineContainers;
    public void SendUnits(int units, BaseNode destination)
    {
        CancelPlayerTravelGroups();
        GameObject go = Instantiate(
            _unitTravelGroupPrefab,
            destination.transform.position,
            Quaternion.identity
        );
        var utr = go.GetComponent<UnitTravelGroup>();
        utr.SetupTravel(units, this, destination);
    }

    private void CancelPlayerTravelGroups()
    {
        foreach( var tg in _attachedPlayerTravelGroups)
        {
            tg.CancelTravel();
        }
        _attachedPlayerTravelGroups.Clear();
    }
    public void ChangeUnitCount(int change, bool player)
    {
        if (player)
            _playerUnits += change;
        else
            _enemyUnits += change;

        HideCounters();
        UpdateCounters();
    }

    public void CreateUnits(int count, bool player)
    {
        int actual;
        if (player)
            actual = ResourceManager.Instance.TryChangeUnits(count);
        else
            actual = EnemyManager.Instance.TryChangeUnits(count);

        if (actual > 0)
            ChangeUnitCount(actual, player);
    }

    public void DestroyUnits(int count, bool player)
    {
        int actual;
        if (player)
            actual = ResourceManager.Instance.TryChangeUnits(-count);
        else
            actual = EnemyManager.Instance.TryChangeUnits(-count);

        if (actual < 0)
            ChangeUnitCount(actual, player);
    }
    private void UpdateCounters()
    {
        if(_playerUnits <= 0)
        {
            _playerUnitCounter.gameObject.SetActive(false);
        }
        else
        {
            _playerUnitCounter.gameObject.SetActive(true);
            _playerUnitCounter.SetText(_playerUnits);
        }
        if(_enemyUnits <= 0)
        {
            _enemyUnitCounter.gameObject.SetActive(false);
        }
        else
        {
            _enemyUnitCounter.gameObject.SetActive(true);
            _enemyUnitCounter.SetText(_enemyUnits);

        }
    }

    private void HideCounters()
    {
        if(_playerUnits == 0) _playerUnitCounter.gameObject.SetActive(false);
        if(_enemyUnits == 0) _enemyUnitCounter.gameObject.SetActive(false);
    
    }
    public void ConnectNode(BaseNode node)
    {
        if (!_connectedNodes.Contains(node))
        {
            _connectedNodes.Add(node);
            node.ConnectNode(this);
        }

    }
    public List<BaseNode> GetConnectedNodes() => _connectedNodes;

    public void AddSplineContainer(SplineContainer s)
    {
        _splineContainers.Add(s);
    }

    public void AssignBuilding(BaseBuilding building)
    {
        _building = building;
        var hs = _building.GetComponent<HealthSystem>();
        if (hs != null)
            hs.OnDie += (_, _) => RemoveBuilding();
    }

    public BaseBuilding GetBuilding()=> _building;
    public void RemoveBuilding()
    {
        var b = _building;
        _building = null;
        b.Cleanup();
        Destroy(b.gameObject);
        
    }

}
