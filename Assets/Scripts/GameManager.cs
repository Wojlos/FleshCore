using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private BaseNode _selectedNode;
    [SerializeField] private GameSettings _gameSettings;
    private List<UnitCounter> _selectedCounters = new List<UnitCounter>();

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        MapGenerator.Instance.GenerateMap();
        TilemapsManager.Instance.GenerateResources(MapManager.Instance.GetAllNodes());
        TilemapsManager.Instance.CalculateResourceTileCounts(MapManager.Instance.GetAllNodes());
    }

    private void Start()
    {
        InitializePlayerStart();
    }

    public void ClickedOnNode(BaseNode node){
        DeselectCounters();
        SelectNode(node);
        BuildingSelectionManager.Instance.OpenBuildingSelection();
    }
    public void RightClickedOnNode(BaseNode node){
        if (_selectedNode != null && _selectedNode.GetBuilding() != null && _selectedNode.GetBuilding().Side == ConflictSide.Player)
        {
            if (_selectedNode == node)
            {
                _selectedNode.ClearRallyPoint();
            }
            else
            {
                _selectedNode.SetRallyPoint(node);
            }
            return;
        }

        foreach(var c in _selectedCounters)
        {
            var n = c.GetConnectedNode();
            var unitsMoved = c.GetSelectedUnits();
            var totalUnits = c.GetTotalUnits();
            
            if (unitsMoved > 0)
            {
                bool isFullSelection = (unitsMoved == totalUnits);
                n.SendUnits(unitsMoved, node, cancelExistingTravelGroups: isFullSelection);
            }
        }
        DeselectCounters();

    }
    public void ClickedOnUnitCounter(UnitCounter counter){
        DeselectCounters();
        counter.SelectUnits();
        _selectedCounters.Add(counter);
    }

    public void SelectUnitsInArea(Rect area)
    {
        DeselectCounters();

        var nodesWithCoverage = MapManager.Instance.GetAllNodesInAreaWithCoverage(area);

        foreach(var data in nodesWithCoverage)
        {
            var counter = data.node.GetPlayerUnitCounter();
            if (counter)
            {
                int totalUnits = data.node.GetAllPlayerUnits();
                int selectedUnits = Mathf.CeilToInt(totalUnits * data.coveragePercentage);
                selectedUnits = Mathf.Clamp(selectedUnits, 0, totalUnits);

                if (selectedUnits > 0)
                {
                    _selectedCounters.Add(counter);
                    counter.SelectPartialUnits(selectedUnits, totalUnits);
                }
            }
        }
    }
    private void DeselectCounters()
    {
        foreach(var c in _selectedCounters)
        {
            c.DeselectUnits();
        }
        _selectedCounters.Clear();
    }
    public void ClickedAnywhere()
    {
        DeselectNode();
        DeselectCounters();
    }

    public void BuildBuilding(GameObject prefab)
    {
        BuildingsManager.Instance.BuildBuilding(prefab, _selectedNode);
        DeselectNode();
    }   
    public void RemoveBuilding()
    {
        BuildingsManager.Instance.RemoveBuilding(_selectedNode);
        DeselectNode();
    }

    public void Cleanup()
    {
        BuildingsManager.Instance.ClearAllBuildings();
    }

    private void SelectNode(BaseNode n)
    {
        _selectedNode = n;
    }
    private void DeselectNode()
    {
        _selectedNode = null;
        BuildingSelectionManager.Instance.CloseBuildingSelection();   
    }

    public GameSettings GetGameSettings() => _gameSettings;
    public BaseNode GetSelectedNode() => _selectedNode;
    public bool IsNodeEmpty()
    {
        return _selectedNode != null && _selectedNode.GetBuilding() == null;
    }

    private void InitializePlayerStart()
    {
        var allNodes = MapManager.Instance.GetAllNodes();
        if (allNodes == null || allNodes.Count == 0)
        {
            Debug.LogError("GameManager: No nodes available for player start initialization.");
            return;
        }

        BaseNode startNode = allNodes[Random.Range(0, allNodes.Count)];
        if (startNode == null)
        {
            Debug.LogError("GameManager: Selected start node is null.");
            return;
        }

        startNode.SetOwnerDirectly(ConflictSide.Player);

        var settings = _gameSettings;
        var pumpSettings = settings.playerBuildingSettings.Find(b => b.type == BuildingType.BloodPump);
        if (pumpSettings == null)
        {
            Debug.LogError("GameManager: BloodPump settings not found in GameSettings.");
            return;
        }

        GameObject pumpPrefab = BuildingSelectionManager.Instance.GetBuildingPrefab(BuildingType.BloodPump);
        if (pumpPrefab == null)
        {
            Debug.LogError("GameManager: BloodPump prefab not found.");
            return;
        }

        BuildingsManager.Instance.BuildBuilding(pumpPrefab, startNode);

        int unitsToSpawn = settings.startingPlayerUnits;
        startNode.CreateUnits(unitsToSpawn, ConflictSide.Player);

        CenterCameraOnNode(startNode);
    }

    private void CenterCameraOnNode(BaseNode node)
    {
        if (node == null) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 nodePosition = node.transform.position;
        mainCamera.transform.position = new Vector3(nodePosition.x, nodePosition.y, mainCamera.transform.position.z);
    }

}
