using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField]
    private List<Resource> income = new List<Resource>();
    [SerializeField]
    private List<Resource> diet = new List<Resource>();

    void Start()
    {
        AddProduction();
        AddUsage();
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
        foreach(var resource in income){
            ResourceManager.Instance.IncrementResourceProduction(resource.resourceType, -resource.initialAmount);
        }
        return;
    }

    private void SubtractUsage(){
        foreach(var resource in diet){
            ResourceManager.Instance.IncrementResourceUsage(resource.resourceType, -resource.initialAmount);
        }
        return;
    }

    void OnDestroy(){
        SubtractProduction();
        SubtractUsage();
    }
}
