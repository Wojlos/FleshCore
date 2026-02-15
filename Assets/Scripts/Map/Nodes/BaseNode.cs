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


    private void Awake() {
        _playerUnitCounter.ConnectNode(this);
        _enemyUnitCounter.ConnectNode(this);

        UpdateCounters();
        HideCounters();
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
        {
            _playerUnits += change;
        }
        else
        {
            _enemyUnits += change;
        }
        HideCounters();
        UpdateCounters();
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
