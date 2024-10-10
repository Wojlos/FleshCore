using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupAdipose : MonoBehaviour
{
    [SerializeField]
	private float darken = 0.129f;
    [SerializeField]
	private float cellDensity = 1.2f;
    [SerializeField]
    private float speed = 10f;

    private Material material;

    void Awake(){
        material = GetComponent<Renderer>().material;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        material.SetFloat("_Darken", darken);
        material.SetFloat("_CellDensity", cellDensity);
        material.SetFloat("_Speed", speed);
    }
}
