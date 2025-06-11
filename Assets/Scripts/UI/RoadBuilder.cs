using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder : MonoBehaviour
{
    // [SerializeField]
    // private GameObject _splinePrefab;
    [SerializeField]
    private GameObject _junctionPrefab;


    public void AttachStructure(){
        InputManager.Instance.AttachStructure( _junctionPrefab);
    }

    public void ClearStructure(){
        InputManager.Instance.ClearStructure();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
