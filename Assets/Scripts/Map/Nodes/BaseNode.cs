using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;

public abstract class BaseNode : MonoBehaviour
{
    [SerializeField] private UnitCounter _playerUnitCounter;
    [SerializeField] private UnitCounter _enemyUnitCounter;

    private List<BaseNode> _connectedNodes = new List<BaseNode>();
    private List<Spline> _splines = new List<Spline>();
    private BaseBuilding _building;

    private int _playerUnits = 0;
    private int _enemyUnits = 0;


    private void Awake() {
        _playerUnitCounter.ConnectNode(this);
        _enemyUnitCounter.ConnectNode(this);

        UpdateCounters();
        HideCounters();
    }
    public UnitCounter GetUnitCounter() => _playerUnitCounter;
    public int GetAllPlayerUnits()
    {
        var tempUnits = _playerUnits;
        _playerUnits = 0;
        UpdateCounters();
        HideCounters();
        return tempUnits;

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

    public void AddSpline(Spline s)
    {
        _splines.Add(s);
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
