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
    
    private int _production = 2;
    private int _consumption = 2;


    void Awake(){
        RoadManager.Instance.AddEntrance(this);
    }

    public void SetJunction(Junction j){
        _junction = j;
        Debug.Log(this);
    }

    public Junction GetJunction(){
        return _junction;
    } 

    void OnDestroy(){
        RoadManager.Instance.RemoveEntrance(this);
    }
}
