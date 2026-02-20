using UnityEngine;
using System.Collections.Generic;

public enum StructureType
{
    LymphNode
}

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;
    private List<BaseStructure> _enemyStructures = new List<BaseStructure>();
    private int _maxStructures = 1;
    private int _currentUnits = 0;
    private int _maxUnits = 30;

    [SerializeField] private GameObject _structurePrefab;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        var settings = GameManager.Instance.GetGameSettings();
        _maxStructures = settings.maxImmunoBuildings;
        _maxUnits = settings.maxImmunoUnits;
    }

    void Update()
    {
        if(_enemyStructures.Count <= _maxStructures)
        {
            var node = SelectRandomNode();
            SpawnStructure(_structurePrefab, node);
        }
    }

    private void SpawnStructure(GameObject prefab, BaseNode node)
    {
        GameObject go = Instantiate(
                prefab,
                node.transform.position,
                Quaternion.identity,
                node.transform
            );
        go.transform.localScale = Vector3.one;
        var structure = go.GetComponent<BaseStructure>();
        _enemyStructures.Add(structure);
        structure.AssignNode(node);

        var settings = GameManager.Instance.GetGameSettings();
        var structureSettings = settings.immunoBuildingSettings.Find(s => s.type == structure.GetStructureType());
        structure.ApplySettings(structureSettings);
    }

    private BaseNode SelectRandomNode()
    {
        var allNodes = MapManager.Instance.GetAllNodes();
        var allPlayerBuildings = BuildingsManager.Instance.GetAllBuildings();
        if(allNodes.Count == allPlayerBuildings.Count)
        {
            return null;
        }
        bool spotFound = false;
        while (!spotFound)
        {
            var tempSpot = allNodes[Random.Range(0, allNodes.Count)];
            if (tempSpot.GetBuilding() == null)
            {
                return tempSpot;
            }
        }
        return null;
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
