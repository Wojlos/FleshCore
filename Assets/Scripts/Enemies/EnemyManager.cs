using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private List<BaseBuilding> _enemyBuildings = new List<BaseBuilding>();
    private int _maxBuildings = 1;
    private int _currentUnits = 0;
    private int _maxUnits = 30;

    [SerializeField] private GameObject _buildingPrefab;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        var settings = GameManager.Instance.GetGameSettings();
        _maxBuildings = settings.maxImmunoBuildings;
        _maxUnits = settings.maxStartImmunoUnits;
    }

    void Update()
    {
        if (_enemyBuildings.Count <= _maxBuildings)
        {
            var node = SelectRandomNode();
            if (node != null)
                SpawnBuilding(_buildingPrefab, node);
        }
    }

    private void SpawnBuilding(GameObject prefab, BaseNode node)
    {
        GameObject go = Instantiate(
                prefab,
                node.transform.position,
                Quaternion.identity,
                node.transform
            );
        go.transform.localScale = Vector3.one;
        var building = go.GetComponent<BaseBuilding>();
        _enemyBuildings.Add(building);
        building.AssignNode(node);

        var hs = building.GetComponent<HealthSystem>();
        var settings = GameManager.Instance.GetGameSettings();
        var buildingSettings = settings.immunoBuildingSettings.Find(s => s.type == building.GetBuildingType());
        
        if (hs != null)
        {
            hs.OnDie += (_, _) => {
                _enemyBuildings.Remove(building);
                if (buildingSettings != null)
                    _maxUnits -= buildingSettings.maxUnitsIncrease;
            };
        }

        ((EnemyBuilding)building).ApplySettings(buildingSettings);

        if (buildingSettings != null)
            _maxUnits += buildingSettings.maxUnitsIncrease;
    }

    private BaseNode SelectRandomNode()
    {
        var allNodes = MapManager.Instance.GetAllNodes();
        if (allNodes.Count == 0) return null;
        int attempts = 0;
        while (attempts < allNodes.Count * 2)
        {
            var tempSpot = allNodes[Random.Range(0, allNodes.Count)];
            if (tempSpot.GetBuilding() == null)
                return tempSpot;
            attempts++;
        }
        return null;
    }

    public bool TrySpawnLymphNode(BaseNode node, int unitCost)
    {
        if (node.GetBuilding() != null) return false;
        if (node.GetUnits(ConflictSide.Immuno) < unitCost) return false;

        node.DestroyUnits(unitCost, false);
        SpawnBuilding(_buildingPrefab, node);
        return true;
    }

    public int TryChangeUnits(int change)
    {
        int tempCount = _currentUnits + change;
        if (tempCount < 0)
        {
            int actual = _currentUnits;
            _currentUnits = 0;
            return -actual;
        }
        if (tempCount > _maxUnits)
        {
            int actual = _maxUnits - _currentUnits;
            _currentUnits = _maxUnits;
            return actual;
        }
        _currentUnits = tempCount;
        return change;
    }
}
