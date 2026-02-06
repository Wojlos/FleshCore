using UnityEngine;
using TMPro;
using System.Collections.Generic;

public enum ResourceType {
    Blood,
    Lipids,
    Bone
    
}
[System.Serializable]
public struct ResourceData
{
    public ResourceType type;
    public Sprite icon;
}


public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("UI elements")]
    [SerializeField] private GameObject _resourceBar;
    [SerializeField] private GameObject _resourcePanelPrefab;

    
    [Header("counters")]
    [SerializeField] private TextMeshProUGUI buildingText;

    [Header("resources")]
    [SerializeField] private List<ResourceData> _resourceData = new List<ResourceData>();
    

    private int _maxBuildings = 0;
    private int _builtBuildings = 0;
    private GameSettings _settings;
    private Dictionary<ResourceType, ResourcePanel> _resourcePanels =  new Dictionary<ResourceType, ResourcePanel>();


    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetupGame();
    }
    private void SetupGame()
    {
        _settings = GameManager.Instance.GetGameSettings();
        _maxBuildings = _settings.startingMaxBuildings;
        UpdateCounters();
        PopulateResourceCounters();
        SetupStartingResources();

    }

    private void PopulateResourceCounters()
    {
        foreach(var r in _resourceData)
        {
            GameObject go = Instantiate(
                _resourcePanelPrefab,
                Vector3.zero,
                Quaternion.identity,
                _resourceBar.transform
            );
            var panel = go.GetComponent<ResourcePanel>();
            panel.SetupPanel(r.icon);
            _resourcePanels[r.type] = panel;
        }
    }
    private void SetupStartingResources()
    {
        var res = new Dictionary<ResourceType, int>();
        foreach (var r in _settings.startingResources)
        {
            res[r.type] = r.cost;
        }
        ChangeResourcesCurrent(res);

    }
    
    public void ChangeMaxBuildings(int a)
    {
        _maxBuildings += a;
        UpdateCounters();
    
    }
    public void ChangeBuiltBuildings(int a)
    {
        _builtBuildings += a;
        UpdateCounters();
    }

    private void UpdateCounters()
    {
        buildingText.text = ""+_builtBuildings+"/"+_maxBuildings;
    }

    public int GetMaxBuildings()=> _maxBuildings;

    public void ChangeResourcesPerSecond(Dictionary<ResourceType, int> change, bool add = true)
    {
        int factor = add ? 1 : -1;

        foreach (var kvp in change)
        {
            _resourcePanels[kvp.Key].ChangePerSecond(kvp.Value * factor);
        }
    }

    public void ChangeResourcesPerSecond(List<ResourceCost> change, bool add = true)
    {
        int factor = add ? 1 : -1;

        foreach (var cost in change)
        {
            _resourcePanels[cost.type].ChangePerSecond(cost.cost * factor);
        }
    }

    public void ChangeResourcesCurrent(Dictionary<ResourceType, int> change, bool add = true)
    {
        int factor = add ? 1 : -1;

        foreach (var kvp in change)
        {
            _resourcePanels[kvp.Key].ChangeCurrent(kvp.Value * factor);
        }
    }

    public void ChangeResourcesCurrent(List<ResourceCost> change, bool add = true)
    {
        int factor = add ? 1 : -1;

        foreach (var cost in change)
        {
            _resourcePanels[cost.type].ChangeCurrent(cost.cost * factor);
        }
    }

    public bool IsMaxBuildingsReached()
    {
        return _builtBuildings >= _maxBuildings;
    }
    public Dictionary<ResourceType, int> GetCurrentResources()
    {
        var res = new Dictionary<ResourceType, int>();

        foreach (KeyValuePair<ResourceType, ResourcePanel> kvp in _resourcePanels)
        {
            res[kvp.Key] = kvp.Value.GetCurrent();
        }
        return res;
    }


}
