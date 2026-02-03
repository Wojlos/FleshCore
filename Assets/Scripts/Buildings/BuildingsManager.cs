using UnityEngine;
using System.Collections.Generic;

public class BuildingsManager : MonoBehaviour
{
    public static BuildingsManager Instance;
    
    [SerializeField] private List<BaseBuilding> _allBuildings = new List<BaseBuilding>();
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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

            // building.Cleanup();
            Destroy(building.gameObject);

            _allBuildings.RemoveAt(i);
        }

        _allBuildings.Clear();
    }


}
