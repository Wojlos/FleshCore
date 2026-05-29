using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

public abstract class BaseNode : MonoBehaviour
{
    [SerializeField] private UnitCounter _playerUnitCounter;
    [SerializeField] private UnitCounter _enemyUnitCounter;
    [SerializeField] private GameObject _playerTravelGroupPrefab;
    [SerializeField] private GameObject _enemyTravelGroupPrefab;
    [SerializeField] private NodeLightController _playerLight;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private List<BaseNode> _connectedNodes = new List<BaseNode>();
    private List<SplineContainer> _splineContainers = new List<SplineContainer>();
    private BaseBuilding _building;

    private Dictionary<ResourceType, int> _resourceTileCounts = new Dictionary<ResourceType, int>();

    private ConflictSide _owner;
    private HealthSystem _nodeHealthSystem;
    private int _maxNodeHP;

    private int _playerUnits = 0;
    private int _enemyUnits = 0;
    private List<UnitTravelGroup> _attachedPlayerTravelGroups = new List<UnitTravelGroup>();
    private List<UnitTravelGroup> _attachedEnemyTravelGroups = new List<UnitTravelGroup>();

    private BaseNode _rallyPointNode;
    private NodeEncounterController _encounterController;


    private void Awake() {
        _playerUnitCounter.ConnectNode(this);
        _enemyUnitCounter.ConnectNode(this);

        var settings = GameManager.Instance.GetGameSettings();
        _maxNodeHP = settings.maxNodeHP;

        _nodeHealthSystem = gameObject.AddComponent<HealthSystem>();
        _nodeHealthSystem.MaxHealth = _maxNodeHP;
        _nodeHealthSystem.HealToFull();
        _nodeHealthSystem.OnDie += OnNodeOwnershipChanged;

        _encounterController = gameObject.AddComponent<NodeEncounterController>();
        _encounterController.Initialize(this, _nodeHealthSystem);

        _owner = ConflictSide.Immuno;
        UpdateNodeColor();

        UpdateCounters();
        HideCounters();
    }
    
    public bool IsInBattle() => _encounterController != null && _encounterController.GetCurrentEncounterType() == EncounterType.Battle;
    public int GetEnemyUnits() => _enemyUnits;
    public ConflictSide GetOwner() => _owner;
    public List<UnitTravelGroup> GetTravelGroups() => _attachedPlayerTravelGroups;
    public void AddPlayerTravelGroup(UnitTravelGroup tg)
    {
        _attachedPlayerTravelGroups.Add(tg);
    }
    public void RemovePlayerTravelGroup(UnitTravelGroup tg)
    {
        _attachedPlayerTravelGroups.Remove(tg);
    }
    public void AddTravelGroup(UnitTravelGroup tg, ConflictSide side)
    {
        if (side == ConflictSide.Player)
            AddPlayerTravelGroup(tg);
        else
            _attachedEnemyTravelGroups.Add(tg);
    }
    public void RemoveTravelGroup(UnitTravelGroup tg, ConflictSide side)
    {
        if (side == ConflictSide.Player)
            RemovePlayerTravelGroup(tg);
        else
            _attachedEnemyTravelGroups.Remove(tg);
    }
    public List<UnitTravelGroup> GetEnemyTravelGroups() => _attachedEnemyTravelGroups;
    public int GetUnits(ConflictSide side) => side == ConflictSide.Player ? _playerUnits : _enemyUnits;
    public int GetOpposingUnits(ConflictSide side) => side == ConflictSide.Player ? _enemyUnits : _playerUnits;
    
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
    public EnemyTravelGroup SendPatrol(int units, BaseBuilding originBuilding)
    {
        if (_connectedNodes == null || _connectedNodes.Count == 0) return null;

        GameObject go = Instantiate(_enemyTravelGroupPrefab, transform.position, Quaternion.identity);
        var tg = go.GetComponent<EnemyTravelGroup>();
        var brain = new PatrolBrain();
        tg.Setup(units, this, brain, originBuilding);
        return tg;
    }

    public void SendUnits(int units, BaseNode destination, bool cancelExistingTravelGroups = true)
    {
        if (cancelExistingTravelGroups)
        {
            CancelPlayerTravelGroups();
        }
        
        GameObject go = Instantiate(
            _playerTravelGroupPrefab,
            transform.position,
            Quaternion.identity
        );
        var ptg = go.GetComponent<PlayerTravelGroup>();
        ptg.Setup(units, this, destination);
    }

    private void CancelPlayerTravelGroups()
    {
        foreach( var tg in _attachedPlayerTravelGroups.ToArray())
        {
            tg.CancelTravel();
        }
        _attachedPlayerTravelGroups.Clear();
    }
    public void ChangeUnitCount(int change, ConflictSide side)
    {
        if (side == ConflictSide.Player)
            _playerUnits += change;
        else
            _enemyUnits += change;

        HideCounters();
        UpdateCounters();
        UpdatePlayerLight();
    }

    public void CreateUnits(int count, ConflictSide side)
    {
        int actual;
        if (side == ConflictSide.Player)
            actual = ResourceManager.Instance.TryChangeUnits(count);
        else
            actual = EnemyManager.Instance.TryChangeUnits(count);

        if (actual > 0)
        {
            ChangeUnitCount(actual, side);

            if (_rallyPointNode != null && _rallyPointNode != this)
            {
                SendUnitsDirected(actual, _rallyPointNode, side);
            }
        }
    }

    public void DestroyUnits(int count, ConflictSide side)
    {
        int actual;
        if (side == ConflictSide.Player)
            actual = ResourceManager.Instance.TryChangeUnits(-count);
        else
            actual = EnemyManager.Instance.TryChangeUnits(-count);

        if (actual < 0)
            ChangeUnitCount(actual, side);
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

    public UnitTravelGroup SendUnitsDirected(int units, BaseNode destination, ConflictSide side)
    {
        if (side == ConflictSide.Player)
        {
            GameObject go = Instantiate(_playerTravelGroupPrefab, transform.position, Quaternion.identity);
            var ptg = go.GetComponent<PlayerTravelGroup>();
            ptg.Setup(units, this, destination);
            return ptg;
        }
        else
        {
            GameObject go = Instantiate(_enemyTravelGroupPrefab, transform.position, Quaternion.identity);
            var etg = go.GetComponent<EnemyTravelGroup>();
            var brain = new PatrolBrain();
            etg.Setup(units, this, brain, GetBuilding());
            etg.RedirectTo(destination);
            return etg;
        }
    }

    public void SetRallyPoint(BaseNode target) { _rallyPointNode = target; }
    public void ClearRallyPoint() { _rallyPointNode = null; }
    public BaseNode GetRallyPoint() => _rallyPointNode;

    public void AssignBuilding(BaseBuilding building)
    {
        _building = building;
        var hs = _building.GetComponent<HealthSystem>();
        if (hs != null)
            hs.OnDie += (_, _) => RemoveBuilding();
        UpdatePlayerLight();
    }

    public BaseBuilding GetBuilding()=> _building;
    public void RemoveBuilding()
    {
        var b = _building;
        _building = null;
        b.Cleanup();
        Destroy(b.gameObject);
        UpdatePlayerLight();
    }

    private void OnNodeOwnershipChanged(object sender, System.EventArgs e)
    {
        ConflictSide newOwner = _owner == ConflictSide.Player ? ConflictSide.Immuno : ConflictSide.Player;
        _owner = newOwner;
        
        _nodeHealthSystem.HealToFull();
        UpdateNodeColor();
        UpdatePlayerLight();
    }

    private void UpdateNodeColor()
    {
        if (_spriteRenderer == null) return;
        
        var settings = GameManager.Instance.GetGameSettings();
        _spriteRenderer.color = _owner == ConflictSide.Player ? settings.playerNodeColor : settings.immunoNodeColor;
    }

    public void SetOwnerDirectly(ConflictSide newOwner)
    {
        _owner = newOwner;
        if (_nodeHealthSystem != null)
            _nodeHealthSystem.HealToFull();
        UpdateNodeColor();
        UpdatePlayerLight();
    }

    public void SetResourceTileCounts(Dictionary<ResourceType, int> counts)
    {
        _resourceTileCounts = counts;
    }

    public Dictionary<ResourceType, int> GetResourceTileCounts() => _resourceTileCounts;

    public int GetResourceTileCount(ResourceType type)
    {
        return _resourceTileCounts.TryGetValue(type, out int count) ? count : 0;
    }

    private void UpdatePlayerLight()
    {
        if (_playerLight == null) return;

        bool hasPlayerBuilding = _building != null && _building.Side == ConflictSide.Player;
        bool hasPlayerUnits = _playerUnits > 0;
        bool playerOwned = _owner == ConflictSide.Player;

        if (hasPlayerBuilding || hasPlayerUnits || playerOwned)
            _playerLight.TurnOn();
        else
            _playerLight.TurnOff();
    }

}
