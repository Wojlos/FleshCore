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
    private BaseStructure _structure;


    private int _playerUnits = 0;
    private int _enemyUnits = 0;
    private List<UnitTravelGroup> _attachedPlayerTravelGroups = new List<UnitTravelGroup>();

    private bool _inBattle = false;
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
        bool playerSiegesStructure = _playerUnits > 0 && _enemyUnits == 0 && _structure != null;
        bool enemySiegesBuilding = _enemyUnits > 0 && _playerUnits == 0 && _building != null;

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
        else if (playerSiegesStructure || enemySiegesBuilding)
        {
            if (_inBattle)
            {
                FinishBattle();
                _inBattle = false;
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
            _battleTimer = 0f;
        }
    }

    private void FinishBattle()
    {
    }

    private void ProcessBattleTick()
    {
        int playerKills = 0;
        int enemyKills = 0;

        for (int i = 0; i < _playerUnits; i++)
        {
            if (Random.value <= _damageChance)
            {
                playerKills++;
            }
        }

        for (int i = 0; i < _enemyUnits; i++)
        {
            if (Random.value <= _damageChance)
            {
                enemyKills++;
            }
        }

        playerKills = Mathf.Min(playerKills, _enemyUnits);
        enemyKills = Mathf.Min(enemyKills, _playerUnits);

        if (playerKills > 0)
        {
            DestroyUnits(playerKills, false);
        }
        if (enemyKills > 0)
        {
            DestroyUnits(enemyKills, true);
        }

    }

    private void ProcessSiegeTick()
    {
        if (_playerUnits > 0 && _structure != null)
        {
            int structureHits = 0;
            for (int i = 0; i < _playerUnits; i++)
            {
                if (Random.value <= _damageChance)
                    structureHits++;
            }
            if (structureHits > 0)
                _structure.GetComponent<HealthSystem>().ApplyDamage(structureHits);
        }
        else if (_enemyUnits > 0 && _building != null)
        {
            int buildingHits = 0;
            for (int i = 0; i < _enemyUnits; i++)
            {
                if (Random.value <= _damageChance)
                    buildingHits++;
            }
            if (buildingHits > 0)
                _building.GetComponent<HealthSystem>().ApplyDamage(buildingHits);
        }
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
    
    public UnitCounter GetUnitCounter(){
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

    public BaseStructure GetStructure()=> _structure;

    public void AssignStructure(BaseStructure structure)
    {
        _structure = structure;
        var hs = _structure.GetComponent<HealthSystem>();
        if (hs != null)
            hs.OnDie += (_, _) => RemoveStructure();
    }

    public void RemoveStructure()
    {
        var s = _structure;
        _structure = null;
        s.Cleanup();
        Destroy(s.gameObject);
        
    }
}
