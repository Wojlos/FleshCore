using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private GameObject _bullet;
    [SerializeField] private GameObject _shellDrop;
    [SerializeField] private Transform _bulletSpawnPoint;
    
    [SerializeField] private float _reloadTime = 0.2f;
    [SerializeField] private float _range = 5f;
    [SerializeField] private float _bulletSpeed = 5f;

    private float _shootClock = 0f;
    private bool _shooting = false;

    void Start(){
        _shootClock = Random.Range(0f, _reloadTime);
    }
    // Update is called once per frame
    void Update()
    {
        if(_shooting){
            _shootClock += Time.deltaTime;
            if(_shootClock >= _reloadTime){
                _shootClock = 0f;
                Destroy(Instantiate(_muzzleFlash, _bulletSpawnPoint.position, transform.rotation), 0.1f);
                Bullet bullet = Instantiate(_bullet, _bulletSpawnPoint.position, transform.rotation).GetComponent<Bullet>();
                bullet.Setup(transform.right, _bulletSpeed, 10f);
            }
        }
    }

    public void StartShooting(){
        _shellDrop.SetActive(true);
        _shooting = true;
    }

    public void StopShooting(){
        _shellDrop.SetActive(false);
        _shooting = false;
    }
}
