using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JunctionPreview : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RoadManager.Instance.AddCurrentJunction(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
