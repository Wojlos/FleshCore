using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetupBone : MonoBehaviour
{
    [SerializeField]
    private float darken = 0.129f;
	[SerializeField]
	private float borderTilling = 2f;
    [SerializeField]
	private float mainTilling = 2f;
    
    private Material material;

    void Awake(){
        material = GetComponent<Renderer>().material;
    }

     void FixedUpdate()
    {
        material.SetFloat("_Darken", darken);
		material.SetFloat("_MainTiling", mainTilling);
		material.SetFloat("_BorderTiling", borderTilling);
        
    }
}
