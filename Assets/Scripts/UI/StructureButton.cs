using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StructureButton : MonoBehaviour
{
    [SerializeField]
    private GameObject _structurePrefab;
    // [SerializeField]
    // private Transform _structureView;

    public void AttachStructure(){
        InputManager.Instance.AttachStructure( _structurePrefab);
    }

    public void ClearStructure(){
        InputManager.Instance.ClearStructure();
    }
}
