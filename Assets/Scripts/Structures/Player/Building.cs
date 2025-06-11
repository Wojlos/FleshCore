using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class Building : MonoBehaviour
{
    [SerializeField]
    private List<Resource> income = new List<Resource>();
    [SerializeField]
    private List<Resource> diet = new List<Resource>();
    
    public Dictionary<ResourceType, float> _netResources = new Dictionary<ResourceType, float>();

    [SerializeField]
    private ResourceTilemapType buildOnTilemapType = ResourceTilemapType.Any;

    private VeinEntrance _input, _output;

    private Tilemap buildOnTilemap;
    private Color defaultColor;
    private Material material;
    private HealthSystem _healthSystem;

    

    void Awake()
    {
        VeinEntrance[] connectedEntrances = GetComponentsInChildren<VeinEntrance>();
        // if(connectedEntrances.Count != 0){
        foreach(VeinEntrance ve in connectedEntrances){
            if(ve.GetType() == EntranceType.Output){
                _output = ve;
            }else{
                _input = ve;
            }
        }
    

        if(_input != null){
            _input.SetBuilding(this);
        }
        if(_output != null){
            _output.SetBuilding(this);
        }
        UpdateNetResources();

        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDie += HealthSystem_OnDie;
        material = GetComponent<Renderer>().material;
        if(buildOnTilemapType != ResourceTilemapType.Any){
            buildOnTilemap = StructuresManager.Instance.GetTilemapByType(buildOnTilemapType);
        }
    }

    private void UpdateNetResources(){
        foreach(Resource r in income){
            if(!_netResources.ContainsKey(r.resourceType)){
                _netResources[r.resourceType] = r.initialAmount;
            }else{
                _netResources[r.resourceType] += r.initialAmount;
            }
        }
        foreach(Resource r in diet){
            if(!_netResources.ContainsKey(r.resourceType)){
                _netResources[r.resourceType] = -r.initialAmount;
            }else{
                _netResources[r.resourceType] -= r.initialAmount;
            }
        }

        _output.SetNetResources(_netResources);
    }

    [ContextMenu("Check resources")]
    public void CheckResources(){
        foreach (KeyValuePair<ResourceType, float> entry in _netResources){
            Debug.Log(entry.Key +" "+ entry.Value);
        }
    }
    private void AddProduction(){
        foreach(var resource in income){
            ResourceManager.Instance.IncrementResourceProduction(resource.resourceType, resource.initialAmount);
        }
        return;
    }

    private void AddUsage(){
        foreach(var resource in diet){
            ResourceManager.Instance.IncrementResourceUsage(resource.resourceType, resource.initialAmount);
        }
        // ResourceManager.Instance.IncrementResource(_resource, _income);
        return;
    }

    private void SubtractProduction(){
        if(income.Count == 0) return;
        foreach(var resource in income){
            ResourceManager.Instance.IncrementResourceProduction(resource.resourceType, -resource.initialAmount);
        }
    }

    private void SubtractUsage(){
        if(diet.Count == 0) return;
        foreach(var resource in diet){
            ResourceManager.Instance.IncrementResourceUsage(resource.resourceType, -resource.initialAmount);
        }
    }

    private void ChangeColor(Color c){
        material.color = c;
    }

    public void SetPreview(bool canBuild){
        ChangeColor(StructuresManager.Instance.GetPreviewColor(canBuild));
    }

    public void Build(){
        AddProduction();
        AddUsage();
        PathfindingManager.Instance.ChangeNode(transform.position, 0f);
    }

    public void Cleanup(){
        SubtractProduction();
        SubtractUsage();
        PathfindingManager.Instance.ChangeNode(transform.position, 1f);
        StructuresManager.Instance.RemoveBuilding(gameObject);
    }

    public Tilemap GetBuildOnTilemap(){
        return buildOnTilemap;
    }

    void HealthSystem_OnDie(object sender, System.EventArgs e){
        Cleanup();
    }
}
