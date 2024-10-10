using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddNode : MonoBehaviour
{
    private Vector3 nodePos;
    // Start is called before the first frame update
    void Start()
    {
        nodePos = transform.position;
        PathfindingManager.Instance.ChangeNode(nodePos, 0f);
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if(nodePos != transform.position){
    //         nodePos = transform.position;
    //     }
    // }
}
