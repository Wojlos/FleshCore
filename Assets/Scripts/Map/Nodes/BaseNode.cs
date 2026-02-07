using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Splines;
using TMPro;

public abstract class BaseNode : MonoBehaviour
{
    [SerializeField] private GameObject _playerUnitCounter;
    [SerializeField] private GameObject _enemyUnitsCounter;

    private List<BaseNode> _connectedNodes = new List<BaseNode>();
    private List<Spline> _splines = new List<Spline>();
    private BaseBuilding _building;

    private int _playerUnits = 0;
    private int _enemyUnits = 0;
    private TMP_Text _playerUnitsCounterText;
    private TMP_Text _enemyUnitsCounterText;


    private void Awake() {
        _playerUnitsCounterText = _playerUnitCounter.GetComponentInChildren<TMP_Text>();
        _enemyUnitsCounterText = _enemyUnitsCounter.GetComponentInChildren<TMP_Text>();
        UpdateCounters();
        HideCounters();
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
            _playerUnitCounter.SetActive(false);
        }
        else
        {
            _playerUnitCounter.SetActive(true);
            _playerUnitsCounterText.text = ""+ _playerUnits;
        }
        if(_enemyUnits <= 0)
        {
            _enemyUnitsCounter.SetActive(false);
        }
        else
        {
            _enemyUnitsCounter.SetActive(true);
            _enemyUnitsCounterText.text = ""+ _enemyUnits;
        }
    }

    private void HideCounters()
    {
        if(_playerUnits == 0) _playerUnitCounter.SetActive(false);
        if(_enemyUnits == 0) _enemyUnitsCounter.SetActive(false);

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
