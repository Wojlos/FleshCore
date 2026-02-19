using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum BuildingType
{
    BloodPump,
    Harvester,
    DefenceOutpost,
    LymphNode,
}
public class BuildingSelectionManager : MonoBehaviour
{
    public static BuildingSelectionManager Instance;

    [SerializeField] private GameObject _buildingSelectionPopup;
    [SerializeField] private Button _removeButton;

    [SerializeField] private GameObject _buildingSelectionGroup;
    [SerializeField] private GameObject _buildingButtonPrefab;


    [Header("Building Prefab Pairing")]
    [SerializeField] private GameObject _bloodPumpPrefab;
    [SerializeField] private GameObject _harvesterPrefab;
    [SerializeField] private GameObject _defenceOutpostPrefab;

    private GameSettings _settings;
    private List<BuildingButton> _buildingButtons = new List<BuildingButton>();
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _settings = GameManager.Instance.GetGameSettings();
        PopulateButtons();

    }
    public void SelectBuilding(string buildingType)
    {

        if (!BuildingType.TryParse(buildingType, ignoreCase: true, out BuildingType type))
        {
            throw new ArgumentException($"Unknown building type '{buildingType}'.");
        }

        switch (type)
        {
            case BuildingType.BloodPump:
                GameManager.Instance.BuildBuilding(_bloodPumpPrefab);
                break;

            case BuildingType.Harvester:
                GameManager.Instance.BuildBuilding(_harvesterPrefab);
                break;

            case BuildingType.DefenceOutpost:
                GameManager.Instance.BuildBuilding(_defenceOutpostPrefab);
                break;

            default:
                throw new NotImplementedException($"No implementation for building of type '{type}'.");
        }
    }
    
    public void OpenBuildingSelection()
    {
        _buildingSelectionPopup.SetActive(true);
        if(GameManager.Instance.IsNodeEmpty()){
            _removeButton.interactable = false;
        }
        else
        {
            _removeButton.interactable = true;   
        }
    }

    public void CloseBuildingSelection()
    {
        foreach(var b in _buildingButtons)
        {
            b.Enable();
        }
        _buildingSelectionPopup.SetActive(false);
    }

    private void PopulateButtons()
    {
        OpenBuildingSelection();
        var buildingsSettings = _settings.buildingSettings;

        foreach(var bs in buildingsSettings)
        {
            GameObject go = Instantiate(
                _buildingButtonPrefab,
                Vector3.zero,
                Quaternion.identity,
                _buildingSelectionGroup.transform
            );
            var buildingButton = go.GetComponent<BuildingButton>();
            buildingButton.SetupBuilding(bs);
            _buildingButtons.Add(buildingButton);
        }
        CloseBuildingSelection();
    }

    private void Update() {
        if (_buildingSelectionPopup.activeInHierarchy)
        {
            CheckPossibleBuilds();
        }
    }
    private void CheckPossibleBuilds()
    {
        var resources = ResourceManager.Instance.GetCurrentResources();
        var isMax = ResourceManager.Instance.IsMaxBuildingsReached();
        foreach(var b in _buildingButtons)
        {
            if (!GameManager.Instance.IsNodeEmpty())
            {
                b.Disable();
            }
            if (isMax)
            {
                if(b.GetType() != BuildingType.BloodPump)
                {
                    b.Disable();
                    continue;
                }
            }
            var cost = b.GetCost();
            foreach (var r in cost)
            {
                if(r.cost > resources[r.type])
                {
                    b.Disable();
                    break;
                }
            }
            b.Enable();
        }

        
    }

}
