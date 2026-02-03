using System;
using UnityEngine;

public enum BuildingType
{
    BloodPump,
    Harvester,
    DefenceOutpost
}
public class BuildingSelectionManager : MonoBehaviour
{
    public static BuildingSelectionManager Instance;

    [SerializeField] private GameObject _buildingSelectionPopup;

    [Header("Building Prefab Pairing")]
    [SerializeField] private GameObject _bloodPumpPrefab;
    [SerializeField] private GameObject _harvesterPrefab;
    [SerializeField] private GameObject _defenceOutpostPrefab;


    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void SelectBuilding(string buildingType)
    {
        Debug.Log(buildingType);

        if (!BuildingType.TryParse(buildingType, ignoreCase: true, out BuildingType type))
        {
            throw new ArgumentException($"Unknown building type '{buildingType}'.");
        }
        Debug.Log(type);

        switch (type)
        {
            case BuildingType.BloodPump:
                Debug.Log("aaa");
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
    
    public void Hello()
    {
        Debug.Log("Hello");
    }
    public void OpenBuildingSelection()
    {
        _buildingSelectionPopup.SetActive(true);
        
    }

    public void CloseBuildingSelection()
    {
        _buildingSelectionPopup.SetActive(false);
        
    }
}
