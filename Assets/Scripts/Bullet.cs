using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float _speed;
    private float _dmg;
    private Vector3 _direction;
    // private AudioSource _audioSource;

    [SerializeField] private GameObject _hitVfx;
    [SerializeField] private LayerMask layerMask;

    void Awake()
    {
        Destroy(gameObject, 5f);
        // _audioSource = GetComponent<AudioSource>();

    }

    void Update()
    {
        // RaycastHit hit;
        // if (Physics.Raycast(transform.position, _direction, out hit, _speed * Time.deltaTime))
        // {
        //     Debug.Log("aaa");
        //     HandleCollision();
        // }
        var target = EnemyManager.Instance.GetClosestEnemyInRange(transform.position, 0.5f);
        if(target != null){
            HandleCollision(target);
        }
        transform.position += _direction * _speed * Time.deltaTime;
    }
    
    void OnCollisionEnter2D(Collision2D coll){
        HandleCollision(coll.collider.gameObject);
    }

    private void HandleCollision(GameObject collider){
        Destroy(gameObject);
        if(collider.TryGetComponent<HealthSystem>(out var healthSystem)){
            healthSystem.DoDamage(_dmg);
        }
        Destroy(Instantiate(_hitVfx, transform.position, transform.rotation), 0.2f);
    }

    public void Setup(Vector3 dir, float speed, float dmg)
    {
        _direction = dir;
        _speed = speed;
        _dmg = dmg;
    }
}