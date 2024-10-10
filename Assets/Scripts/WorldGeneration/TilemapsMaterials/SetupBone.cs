using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupBone : MonoBehaviour
{
    [SerializeField]
    private float darken = 0.129f;
	[SerializeField]
	private float cellDensity = 2f;
    [SerializeField]
	private Vector3 boneBorders = new Vector3(2f,4f,6f);
    
    private Material material;

    void Awake(){
        material = GetComponent<Renderer>().material;
    }

     void FixedUpdate()
    {
        material.SetFloat("_Darken", darken);
		material.SetFloat("_CellDensity", cellDensity);
		material.SetVector("_BoneBorders", boneBorders);

        
    }
}
