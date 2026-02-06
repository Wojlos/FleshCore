using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private BaseNode _selectedNode;
    [SerializeField] private GameSettings _gameSettings;


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
        SelectNode(node);
        BuildingSelectionManager.Instance.OpenBuildingSelection();
    }
    public void ClickedAnywhere()
    {
        DeselectNode();
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
