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

        var movedUnits =0;
        foreach(var c in _selectedCounters)
        {
            var n = c.GetConnectedNode();
            var unitsMoved = n.GetAllPlayerUnits();
            n.SendUnits(unitsMoved, node);
        }        
        // node.ChangeUnitCount(movedUnits, true);
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

        var nodes = MapManager.Instance.GetAllNodesInArea(area);

        foreach(var n in nodes)
        {
            var counter = n.GetPlayerUnitCounter();
            if (counter){
                _selectedCounters.Add(counter);
                counter.SelectUnits();    
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
    public bool IsNodeEmpty()
    {
        return _selectedNode != null && _selectedNode.GetBuilding() == null;
    }

}
