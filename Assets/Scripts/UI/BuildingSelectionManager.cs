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

    [Header("Tooltip")]
    [SerializeField] private GameObject _tooltipPrefab;

    private GameSettings _settings;
    private List<BuildingButton> _buildingButtons = new List<BuildingButton>();
    private BuildingSelectionTooltip _tooltip;
    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _settings = GameManager.Instance.GetGameSettings();
        InstantiateTooltip();
        PopulateButtons();

    }

    private void InstantiateTooltip()
    {
        if (_tooltipPrefab == null) return;
        GameObject go = Instantiate(_tooltipPrefab, _buildingSelectionPopup.transform.parent);
        _tooltip = go.GetComponent<BuildingSelectionTooltip>();
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
        var selectedNode = GameManager.Instance.GetSelectedNode();
        if (selectedNode == null || selectedNode.GetOwner() != ConflictSide.Player)
        {
            return;
        }

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
        if (_tooltip != null)
            _tooltip.Hide();

        foreach(var b in _buildingButtons)
        {
            b.Enable();
        }
        _buildingSelectionPopup.SetActive(false);
    }

    private void PopulateButtons()
    {
        OpenBuildingSelection();
        var buildingsSettings = _settings.playerBuildingSettings;

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
            buildingButton.SetTooltip(_tooltip);
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
                continue;
            }
            if (isMax)
            {
                if(b.GetBuildingType() != BuildingType.BloodPump)
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

    public GameObject GetBuildingPrefab(BuildingType type)
    {
        switch (type)
        {
            case BuildingType.BloodPump:
                return _bloodPumpPrefab;
            case BuildingType.Harvester:
                return _harvesterPrefab;
            case BuildingType.DefenceOutpost:
                return _defenceOutpostPrefab;
            default:
                return null;
        }
    }

}
