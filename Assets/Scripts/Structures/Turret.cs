using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private float _range = 5f;

    private Gun[] _guns;
    private GameObject _target;

    void Start()
    {
        _guns = GetComponentsInChildren<Gun>();   
    }

    void Update()
    {
        _target = EnemyManager.Instance.GetClosestEnemyInRange(transform.position, _range);
        if(_target != null){
            ActivateGuns();
            TrackTarget();
        }
        if(_target == null){
            DeactivateGuns();
        }
    }

    private void ActivateGuns(){
        foreach(Gun g in _guns){
            g.StartShooting();
        }
    }
    
    private void DeactivateGuns(){
        foreach(Gun g in _guns){
            g.StopShooting();
        }
    }

    private void TrackTarget(){
        Vector3 difference = _target.transform.position - transform.position;
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
    }
}
