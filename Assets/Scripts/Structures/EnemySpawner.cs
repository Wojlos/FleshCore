using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _spawnedUnitPrefab;
    [SerializeField] private float _spawnInterval;

    private float _clock = 0f;

    void Update()
    {
        _clock += Time.deltaTime;
        if(_clock >= _spawnInterval){
            SpawnUnit();
            _clock = 0f;
        }   
    }

    private void SpawnUnit(){
        Instantiate(_spawnedUnitPrefab, transform.position, Quaternion.identity);
    }
}
