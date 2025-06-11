using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

using UnityEngine;

public enum ResourceTilemapType
    {
        Any,
        Flesh,
        Adipose,
        Bone
    }

[System.Serializable]
public class ResourceTilemap
{
    public Tilemap tilemap;
    public ResourceTilemapType type;
}

public class StructuresManager : MonoBehaviour
{
    private static StructuresManager _instance;

    public static StructuresManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StructuresManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(StructuresManager).Name);
                    _instance = singleton.AddComponent<StructuresManager>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    private List<GameObject> allBuildings = new List<GameObject>();
    private Tilemap[] tilemaps;
    [SerializeField]
    private Color buildingPreviewColorYes, buildingPreviewColorNo;
    [SerializeField] 
    private List<ResourceTilemap> resourceTilemaps = new List<ResourceTilemap>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        tilemaps = (InputManager.Instance.GetTmContainer().GetComponentsInChildren<Tilemap>());
    }

    public void AddBuilding(GameObject go){
        allBuildings.Add(go);
    }

    public void RemoveBuilding(GameObject go){
        allBuildings.Remove(go);
    }

    public Color GetPreviewColor(bool canBuild){
        if(canBuild){
            return buildingPreviewColorYes;
        }
        return buildingPreviewColorNo;
    }

    public bool IsPositionClear(Vector3 position, Tilemap tilemap = null){
        foreach(GameObject building in allBuildings){
            if(building.transform.position == position){
                return false;
            }
        }
        foreach(Tilemap tm in tilemaps){
            Vector3Int tmPosition = tm.WorldToCell(position);
            TileBase tile = tm.GetTile(tmPosition);
            if(tile != null){
                return false;
            }
        }
        if( tilemap != null){
            Vector3Int tmPosition = tilemap.WorldToCell(position);
            if(!IsResourceNextTo(tilemap, tmPosition)){
                return false;
            }
        }
        return true;
    }

    private bool IsResourceNextTo(Tilemap tilemap, Vector3Int tmPosition){
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),  
            new Vector3Int(0, -1, 0), 
            new Vector3Int(-1, 0, 0), 
            new Vector3Int(1, 0, 0)
        };
        foreach (Vector3Int direction in directions)
        {
            Vector3Int checkPosition = tmPosition + direction;
            TileBase checkTile = tilemap.GetTile(checkPosition);
            if (checkTile != null)
            {
                return true;
            }
        }
        return false;
    }

    public Tilemap GetTilemapByType(ResourceTilemapType tilemapType){
        foreach(ResourceTilemap rtm in resourceTilemaps){
            if(rtm.type == tilemapType){
                return rtm.tilemap;
                }
        }
        return null;
    }

    public List<GameObject> GetAllBuildings(){
        return allBuildings;
    } 

    // public bool IsPositionClear(){
    //     return false;
    // }
}
