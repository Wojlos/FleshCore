using UnityEngine;
using System.Collections.Generic;

public class BuildingsManager : MonoBehaviour
{
    public static BuildingsManager Instance;
    
    [SerializeField] private List<BaseBuilding> _allBuildings = new List<BaseBuilding>();
    private GameSettings _settings;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _settings = GameManager.Instance.GetGameSettings();
    }

    public void BuildBuilding(GameObject prefab, BaseNode node)
    {
        GameObject go = Instantiate(
                prefab,
                node.transform.position,
                Quaternion.identity,
                node.transform
            );
        go.transform.localScale = Vector3.one;
        var building = go.GetComponent<BaseBuilding>();
        building.AssignNode(node);
        _allBuildings.Add(building);
        
        _settings = GameManager.Instance.GetGameSettings();
        var buildingSettings = _settings.playerBuildingSettings.Find(b => b.type == building.GetBuildingType());
        building.ApplySettings(buildingSettings);
        
        ResourceManager.Instance.ChangeBuiltBuildings(1);
        ResourceManager.Instance.TryChangeResourcesCurrent(buildingSettings.buildCost, false);
        ResourceManager.Instance.ChangeResourcesPerSecond(buildingSettings.upkeep, false);
        ResourceManager.Instance.ChangeMaxUnits(buildingSettings.maxUnitsIncrease);
    }

    public void RemoveBuilding(BaseNode node)
    {
        ResourceManager.Instance.ChangeBuiltBuildings(-1);
        _allBuildings.Remove(node.GetBuilding());
        node.RemoveBuilding();
    }

    public void ClearAllBuildings()
    {
        for (int i = _allBuildings.Count - 1; i >= 0; i--)
        {
            var building = _allBuildings[i];
            if (building == null)
            {
                _allBuildings.RemoveAt(i);
                continue;
            }

            Destroy(building.gameObject);

            _allBuildings.RemoveAt(i);
        }

        _allBuildings.Clear();
    }

    public List<BaseBuilding> GetAllBuildings() => _allBuildings;

}
