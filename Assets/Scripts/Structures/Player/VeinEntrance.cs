using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EntranceType
{
    Output,
    Input
}

public class VeinEntrance : MonoBehaviour
{
    [SerializeField] private EntranceType _type; 
    private Junction _junction;
    private VeinEntrance _otherEntrance;
    
    [SerializeField] private float _production = 2f;
    [SerializeField] private float _consumption = 2f;
    
    private Dictionary<ResourceType, float> _netResources = new Dictionary<ResourceType, float>();


    private Building _building;



    void Awake(){
        RoadManager.Instance.AddEntrance(this);
        foreach(var ve in transform.parent.GetComponentsInChildren<VeinEntrance>()){
            if(ve != this){
                _otherEntrance = ve;
            }
        }
    }

    public void SetJunction(Junction j){
        _junction = j;
        _junction.SetEntrance(this);
        var otherJunction = _otherEntrance.GetJunction();
        if(otherJunction != null){
            if(_type == EntranceType.Input){
                _junction.AddNext(otherJunction);
                otherJunction.AddPrev(_junction);
            }else{
                otherJunction.AddNext(_junction);
                _junction.AddPrev(otherJunction);
            }
        }
    }

    public void SetBuilding(Building b){
        _building = b;
    }

    public Junction GetJunction(){
        return _junction;
    } 

    public EntranceType GetType(){
        return _type;
    }
    
    public float GetProduction(){
        return _production;
    }

    public float GetNetProfit(){
        return (_production - _consumption);
    }

    public void SetNetResources(Dictionary<ResourceType, float> nR){
        _netResources = nR;
    }

    public Dictionary<ResourceType, float> GetNetResources(){
        return _netResources;
    }

    void OnDestroy(){
        RoadManager.Instance.RemoveEntrance(this);
    }
}
