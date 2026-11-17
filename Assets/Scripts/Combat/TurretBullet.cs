using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    [SerializeField] private GameObject _hitEffectPrefab;

    private Vector2 _targetPosition;
    private float _speed;
    private bool _initialized = false;

    private UnitTravelGroup _targetOwner;
    private float _damageChance;

    public void Initialize(Vector2 targetPosition, float speed, UnitTravelGroup targetOwner = null, float damageChance = 0f)
    {
        _targetPosition = targetPosition;
        _speed = speed;
        _targetOwner = targetOwner;
        _damageChance = damageChance;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;

        Vector2 currentPos = transform.position;
        Vector2 direction = _targetPosition - currentPos;
        float distanceThisFrame = _speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            transform.position = new Vector3(_targetPosition.x, _targetPosition.y, transform.position.z);
            OnReachedTarget();
        }
        else
        {
            Vector2 newPos = currentPos + direction.normalized * distanceThisFrame;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }

    private void OnReachedTarget()
    {
        if (_hitEffectPrefab != null)
        {
            Instantiate(_hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Probability-based damage on impact, mirroring unit-vs-unit combat.
        if (_targetOwner != null && Random.value <= _damageChance)
        {
            _targetOwner.TakeCombatDamage(1);
        }

        Destroy(gameObject);
    }
}
