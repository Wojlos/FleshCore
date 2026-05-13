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
    [SerializeField] private TextMeshProUGUI _buildingText;
    [SerializeField] private TextMeshProUGUI _unitText;


    [Header("resources")]
    [SerializeField] private List<ResourceData> _resourceData = new List<ResourceData>();

    [Header("Additional Production Icons")]
    [SerializeField] private Sprite _unitsIcon;
    [SerializeField] private Sprite _maxBuildingsIcon;
    

    private int _maxBuildings = 0;
    private int _builtBuildings = 0;
    
    private int _maxUnits = 0;
    private int _currentUnits = 0;
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
        _maxUnits = _settings.startingMaxUnits;
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
        TryChangeResourcesCurrent(res);

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
    public void ChangeMaxUnits(int a)
    {
        _maxUnits += a;
        UpdateCounters();
    
    }
    public void ChangeCurrentUnits(int a)
    {
        _currentUnits += a;
        UpdateCounters();
    }

    private void UpdateCounters()
    {
        _buildingText.text = ""+_builtBuildings+"/"+_maxBuildings;
        _unitText.text = ""+_currentUnits+"/"+_maxUnits;
    }

    public int GetMaxBuildings()=> _maxBuildings;
    public int GetMaxUnits()=> _maxBuildings;

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

    public bool TryChangeResourcesCurrent(Dictionary<ResourceType, int> change, bool add = true)
    {
        int factor = add ? 1 : -1;

        foreach (var kvp in change)
        {
            int current = _resourcePanels[kvp.Key].GetCurrent();
            int newValue = current + kvp.Value * factor;

            if (newValue < 0)
                return false;
        }

        foreach (var kvp in change)
        {
            _resourcePanels[kvp.Key].ChangeCurrent(kvp.Value * factor);
        }

        return true;
    }

    public bool TryChangeResourcesCurrent(List<ResourceCost> change, bool add = true)
    {
        int factor = add ? 1 : -1;

        foreach (var cost in change)
        {
            int current = _resourcePanels[cost.type].GetCurrent();
            int newValue = current + cost.cost * factor;

            if (newValue < 0)
                return false;
        }

        foreach (var cost in change)
        {
            _resourcePanels[cost.type].ChangeCurrent(cost.cost * factor);
        }

        return true;
    }

    public int TryChangeUnits(int change)
    {
        if (change > 0)
        {
            if (_currentUnits >= _maxUnits)
                return 0;

            int actual = Mathf.Min(change, _maxUnits - _currentUnits);
            _currentUnits += actual;
            UpdateCounters();
            return actual;
        }
        else if (change < 0)
        {
            int actual = Mathf.Max(change, -_currentUnits);
            _currentUnits += actual;
            UpdateCounters();
            return actual;
        }
        return 0;
    }


    public bool IsMaxBuildingsReached()
    {
        return _builtBuildings >= _maxBuildings;
    }
    public bool IsMaxUnitsReached()
    {
        return _currentUnits >= _maxUnits;
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

    public Sprite GetResourceIcon(ResourceType type)
    {
        foreach (var rd in _resourceData)
        {
            if (rd.type == type) return rd.icon;
        }
        return null;
    }

    public Sprite GetUnitsIcon() => _unitsIcon;
    public Sprite GetMaxBuildingsIcon() => _maxBuildingsIcon;
}
