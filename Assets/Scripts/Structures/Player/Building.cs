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
    [SerializeField]
    private ResourceTilemapType buildOnTilemapType = ResourceTilemapType.Any;


    private Tilemap buildOnTilemap;
    private Color defaultColor;
    private Material material;
    private HealthSystem _healthSystem;

    void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDie += HealthSystem_OnDie;
        material = GetComponent<Renderer>().material;
        if(buildOnTilemapType != ResourceTilemapType.Any){
            buildOnTilemap = StructuresManager.Instance.GetTilemapByType(buildOnTilemapType);
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
