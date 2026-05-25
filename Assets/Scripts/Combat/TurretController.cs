using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class TurretController : MonoBehaviour
{
    public enum TurretState { Deactivated, Searching, Engaged }

    [Header("State")]
    [SerializeField] private TurretState _currentState = TurretState.Deactivated;

    [Header("Searching")]
    [SerializeField] private float _searchRotationSpeed = 45f;
    [SerializeField] private AnimationCurve _searchEasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float _searchPauseMin = 0.5f;
    [SerializeField] private float _searchPauseMax = 2f;

    [Header("Engaged")]
    [SerializeField] private float _engagedRotationSpeed = 180f;
    [SerializeField] private AnimationCurve _engagedEasingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Firing")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private GameObject _muzzleFlashPrefab;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private int _burstCount = 4;
    [SerializeField] private float _burstInterval = 0.15f;
    [SerializeField] private float _reloadDuration = 1f;
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private float _fireAngleThreshold = 5f;

    [Header("Debug")]
    [SerializeField] private bool _constantFire = false;

    private List<Transform> _targets = new List<Transform>();
    private Transform _currentTarget;
    private Coroutine _searchCoroutine;
    private Coroutine _fireCoroutine;
    private float _currentZRotation;
    private SpriteRenderer _spriteRenderer;

    // Debug mock targets
    private GameObject _mockTargetA;
    private GameObject _mockTargetB;
    private float _mockSwitchTimer;
    private bool _mockUsingA = true;

    private void OnEnable()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentZRotation = transform.eulerAngles.z;

        if (_constantFire)
            SetupMockTargets();
    }

    private void OnDisable()
    {
        CleanupMockTargets();
    }

    private void Update()
    {
        if (_constantFire)
            UpdateMockTargets();

        switch (_currentState)
        {
            case TurretState.Deactivated:
                break;
            case TurretState.Searching:
                break;
            case TurretState.Engaged:
                UpdateEngaged();
                break;
        }

        UpdateSpriteFlip();
    }

    #region Public API

    public void Activate()
    {
        StopAllBehavior();
        TransitionToSearching();
    }

    public void Deactivate()
    {
        StopAllBehavior();
        _currentState = TurretState.Deactivated;
    }

    public void SetTargets(List<Transform> targets)
    {
        _targets = targets ?? new List<Transform>();
        EvaluateState();
    }

    public void AddTarget(Transform target)
    {
        if (target != null && !_targets.Contains(target))
        {
            _targets.Add(target);
            EvaluateState();
        }
    }

    public void RemoveTarget(Transform target)
    {
        _targets.Remove(target);
        EvaluateState();
    }

    #endregion

    #region State Transitions

    private void EvaluateState()
    {
        if (_currentState == TurretState.Deactivated) return;

        // Clean null refs
        _targets.RemoveAll(t => t == null);

        if (_targets.Count > 0 && _currentState != TurretState.Engaged)
            TransitionToEngaged();
        else if (_targets.Count == 0 && _currentState != TurretState.Searching)
            TransitionToSearching();
    }

    private void TransitionToSearching()
    {
        StopFireCoroutine();
        _currentTarget = null;
        _currentState = TurretState.Searching;
        _searchCoroutine = StartCoroutine(SearchRoutine());
    }

    private void TransitionToEngaged()
    {
        StopSearchCoroutine();
        _currentTarget = _targets[0];
        _currentState = TurretState.Engaged;
        _fireCoroutine = StartCoroutine(FireRoutine());
    }

    private void StopAllBehavior()
    {
        StopSearchCoroutine();
        StopFireCoroutine();
    }

    private void StopSearchCoroutine()
    {
        if (_searchCoroutine != null)
        {
            StopCoroutine(_searchCoroutine);
            _searchCoroutine = null;
        }
    }

    private void StopFireCoroutine()
    {
        if (_fireCoroutine != null)
        {
            StopCoroutine(_fireCoroutine);
            _fireCoroutine = null;
        }
    }

    #endregion

    #region Searching

    private IEnumerator SearchRoutine()
    {
        while (true)
        {
            float targetAngle = Random.Range(0f, 180f);
            float startAngle = _currentZRotation;
            float angleDiff = Mathf.DeltaAngle(startAngle, targetAngle);
            float duration = Mathf.Abs(angleDiff) / _searchRotationSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = _searchEasingCurve.Evaluate(t);
                _currentZRotation = startAngle + angleDiff * easedT;
                ApplyRotation();
                yield return null;
            }

            _currentZRotation = startAngle + angleDiff;
            ApplyRotation();

            float pause = Random.Range(_searchPauseMin, _searchPauseMax);
            yield return new WaitForSeconds(pause);
        }
    }

    #endregion

    #region Engaged

    private void UpdateEngaged()
    {
        if (_currentTarget == null)
        {
            EvaluateState();
            return;
        }

        Vector2 direction = ((Vector2)_currentTarget.position - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float angleDiff = Mathf.DeltaAngle(_currentZRotation, targetAngle);
        float maxStep = _engagedRotationSpeed * Time.deltaTime;
        float step = Mathf.Clamp(angleDiff, -maxStep, maxStep);

        _currentZRotation += step;
        ApplyRotation();
    }

    private IEnumerator FireRoutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => _currentTarget != null && IsAlignedWithTarget());

            for (int i = 0; i < _burstCount; i++)
            {
                if (_currentTarget == null) yield break;
                if (!IsAlignedWithTarget()) break;
                FireBullet();
                if (i < _burstCount - 1)
                    yield return new WaitForSeconds(_burstInterval);
            }

            yield return new WaitForSeconds(_reloadDuration);
        }
    }

    private bool IsAlignedWithTarget()
    {
        if (_currentTarget == null) return false;

        Vector2 dirToTarget = ((Vector2)_currentTarget.position - (Vector2)transform.position).normalized;
        float targetAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        float angleDiff = Mathf.Abs(Mathf.DeltaAngle(_currentZRotation, targetAngle));
        return angleDiff <= _fireAngleThreshold;
    }

    private void FireBullet()
    {
        if (_firePoint == null || _currentTarget == null) return;

        Vector2 targetPos = _currentTarget.position;

        // Muzzle flash
        if (_muzzleFlashPrefab != null)
        {
            Instantiate(_muzzleFlashPrefab, _firePoint.position, _firePoint.rotation);
        }

        // Bullet
        if (_bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(_bulletPrefab, _firePoint.position, Quaternion.identity);
            TurretBullet bullet = bulletObj.GetComponent<TurretBullet>();
            if (bullet != null)
            {
                bullet.Initialize(targetPos, _bulletSpeed);
            }
        }
    }

    #endregion

    #region Rotation Helpers

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, _currentZRotation);
    }

    private void UpdateSpriteFlip()
    {
        float z = _currentZRotation % 360f;
        if (z < 0f) z += 360f;

        bool facingLeft = z > 90f && z < 270f;
        _spriteRenderer.flipY = facingLeft;
    }

    #endregion

    #region Debug Mock Targets

    private void SetupMockTargets()
    {
        float angleA = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float angleB = angleA + Mathf.PI; // opposite side
        float distA = Random.Range(1f, 2f);
        float distB = Random.Range(1f, 2f);

        _mockTargetA = new GameObject("MockTargetA");
        _mockTargetA.transform.position = (Vector2)transform.position + new Vector2(Mathf.Cos(angleA), Mathf.Sin(angleA)) * distA;

        _mockTargetB = new GameObject("MockTargetB");
        _mockTargetB.transform.position = (Vector2)transform.position + new Vector2(Mathf.Cos(angleB), Mathf.Sin(angleB)) * distB;

        _mockSwitchTimer = 0f;
        _mockUsingA = true;

        // Activate turret with first mock target
        _targets = new List<Transform> { _mockTargetA.transform };
        Activate();
        TransitionToEngaged();
    }

    private void UpdateMockTargets()
    {
        if (_mockTargetA == null || _mockTargetB == null) return;

        _mockSwitchTimer += Time.deltaTime;
        if (_mockSwitchTimer >= 3f)
        {
            _mockSwitchTimer = 0f;
            _mockUsingA = !_mockUsingA;

            _currentTarget = _mockUsingA ? _mockTargetA.transform : _mockTargetB.transform;
            _targets = new List<Transform> { _currentTarget };
        }
    }

    private void CleanupMockTargets()
    {
        if (_mockTargetA != null) Destroy(_mockTargetA);
        if (_mockTargetB != null) Destroy(_mockTargetB);
    }

    #endregion
}
