using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupFlesh : MonoBehaviour
{
    [SerializeField]
    private float darken = 0.129f;
	[SerializeField]
	private float gNoiseScale = 2f;
	[SerializeField]
    private float speed = 0.3f;
    
    private Material material;

    void Awake(){
        material = GetComponent<Renderer>().material;
    }

     void FixedUpdate()
    {
        material.SetFloat("_Darken", darken);
		material.SetFloat("_GNoiseScale", gNoiseScale);
		material.SetFloat("_Speed", speed);

        
    }
}
