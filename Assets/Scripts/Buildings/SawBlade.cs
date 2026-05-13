using UnityEngine;
using System.Collections.Generic;

public class SawBlade : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Speed at which the blade travels between points (world units/sec)")]
    [SerializeField] private float _moveSpeed = 2f;

    [Tooltip("How long the blade stays at a point before moving to the next (seconds)")]
    [SerializeField] private float _dwellTime = 2f;

    [Tooltip("Random extra dwell time added on top of base (0 to this value)")]
    [SerializeField] private float _dwellTimeVariance = 1f;

    [Header("Rotation")]
    [Tooltip("Blade spin speed in degrees per second")]
    [SerializeField] private float _rotationSpeed = 360f;

    [Header("Shake")]
    [Tooltip("How far the blade shakes from the target point while dwelling")]
    [SerializeField] private float _shakeIntensity = 0.05f;

    [Tooltip("How fast the shake oscillates (higher = faster jitter)")]
    [SerializeField] private float _shakeFrequency = 20f;

    [Header("Particles")]
    [Tooltip("The root transform of the excavation particles (positioned at blade edge)")]
    [SerializeField] private Transform _particlesRoot;

    [Tooltip("Shooting particle system")]
    [SerializeField] private ParticleSystem _shootingParticles;

    [Tooltip("Splash particle system")]
    [SerializeField] private ParticleSystem _splashParticles;

    [Tooltip("Blade visual radius used to offset particles to the blade edge")]
    [SerializeField] private float _bladeRadius = 0.5f;

    private enum State { Idle, Moving, Dwelling }

    private State _state = State.Idle;
    private List<TilemapsManager.EdgeTileData> _targetPoints = new List<TilemapsManager.EdgeTileData>();
    private Dictionary<ResourceType, Gradient> _resourceGradients = new Dictionary<ResourceType, Gradient>();
    private Vector2 _buildingPosition;
    private int _currentTargetIndex = -1;
    private TilemapsManager.EdgeTileData _currentTarget;
    private Vector2 _dwellOrigin;
    private float _dwellTimer;
    private float _currentDwellDuration;
    private float _shakeTimer;
    private bool _active;

    public void Initialize(List<TilemapsManager.EdgeTileData> edgePoints, Dictionary<ResourceType, Gradient> resourceGradients, Vector2 buildingPosition)
    {
        _targetPoints = edgePoints ?? new List<TilemapsManager.EdgeTileData>();
        _resourceGradients = resourceGradients ?? new Dictionary<ResourceType, Gradient>();
        _buildingPosition = buildingPosition;

        StopParticles();

        if (_targetPoints.Count == 0)
        {
            _state = State.Idle;
            return;
        }

        PickNextTarget();
        _state = State.Moving;
    }

    public void SetActive(bool active)
    {
        _active = active;
        if (!active)
            StopParticles();
    }

    private void Update()
    {
        if (!_active || _targetPoints.Count == 0)
            return;

        switch (_state)
        {
            case State.Moving:
                UpdateMoving();
                break;
            case State.Dwelling:
                UpdateDwelling();
                break;
        }
    }

    private void UpdateMoving()
    {
        Vector2 pos = transform.position;
        Vector2 dir = _currentTarget.position - pos;
        float dist = dir.magnitude;
        float step = _moveSpeed * Time.deltaTime;

        if (step >= dist)
        {
            transform.position = new Vector3(_currentTarget.position.x, _currentTarget.position.y, transform.position.z);
            StartDwelling();
        }
        else
        {
            Vector2 newPos = pos + dir.normalized * step;
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }

    private void StartDwelling()
    {
        _state = State.Dwelling;
        _dwellOrigin = _currentTarget.position;
        _dwellTimer = 0f;
        _shakeTimer = 0f;
        _currentDwellDuration = _dwellTime + Random.Range(0f, _dwellTimeVariance);

        SetupParticles();
        PlayParticles();
    }

    private void UpdateDwelling()
    {
        _dwellTimer += Time.deltaTime;
        _shakeTimer += Time.deltaTime;

        // Spin only while dwelling
        transform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);

        // Shake around the dwell point
        float offsetX = Mathf.PerlinNoise(_shakeTimer * _shakeFrequency, 0f) * 2f - 1f;
        float offsetY = Mathf.PerlinNoise(0f, _shakeTimer * _shakeFrequency) * 2f - 1f;
        Vector2 shakeOffset = new Vector2(offsetX, offsetY) * _shakeIntensity;
        Vector2 shakenPos = _dwellOrigin + shakeOffset;
        transform.position = new Vector3(shakenPos.x, shakenPos.y, transform.position.z);

        if (_dwellTimer >= _currentDwellDuration)
        {
            StopParticles();
            PickNextTarget();
            _state = State.Moving;
        }
    }

    private void SetupParticles()
    {
        if (_particlesRoot == null) return;

        // Direction from building to blade = general "outward into resource" direction
        Vector2 bladePos = _currentTarget.position;
        Vector2 radialDir = (bladePos - _buildingPosition).normalized;

        // Position particles at blade edge, on the side facing into the resource
        Vector2 particlePos = bladePos + radialDir * _bladeRadius;
        _particlesRoot.position = new Vector3(particlePos.x, particlePos.y, _particlesRoot.position.z);

        // Rotate particles to shoot toward the building
        Vector2 emitDir = -radialDir;
        float angle = Mathf.Atan2(emitDir.y, emitDir.x) * Mathf.Rad2Deg;
        _particlesRoot.rotation = Quaternion.Euler(0f, 0f, angle);

        // Set start color from resource gradient
        if (_resourceGradients.TryGetValue(_currentTarget.resourceType, out Gradient gradient))
        {
            ApplyGradient(_shootingParticles, gradient);
            ApplyGradient(_splashParticles, gradient);
        }
    }

    private void ApplyGradient(ParticleSystem ps, Gradient gradient)
    {
        if (ps == null || gradient == null) return;
        var main = ps.main;
        var minMaxGradient = new ParticleSystem.MinMaxGradient(gradient);
        minMaxGradient.mode = ParticleSystemGradientMode.RandomColor;
        main.startColor = minMaxGradient;
    }

    private void PlayParticles()
    {
        if (_shootingParticles != null && !_shootingParticles.isPlaying) _shootingParticles.Play();
        if (_splashParticles != null && !_splashParticles.isPlaying) _splashParticles.Play();
    }

    private void StopParticles()
    {
        if (_shootingParticles != null && _shootingParticles.isPlaying) _shootingParticles.Stop();
        if (_splashParticles != null && _splashParticles.isPlaying) _splashParticles.Stop();
    }

    private void PickNextTarget()
    {
        if (_targetPoints.Count <= 1)
        {
            _currentTargetIndex = 0;
            _currentTarget = _targetPoints[0];
            return;
        }

        int newIndex;
        do
        {
            newIndex = Random.Range(0, _targetPoints.Count);
        } while (newIndex == _currentTargetIndex);

        _currentTargetIndex = newIndex;
        _currentTarget = _targetPoints[_currentTargetIndex];
    }
}
