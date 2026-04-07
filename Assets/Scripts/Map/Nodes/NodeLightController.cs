using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class NodeLightController : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private List<Light2D> _lights = new List<Light2D>();

    [Header("Fade")]
    [SerializeField] private float _fadeDuration = 0.3f;

    [Header("Flicker")]
    [SerializeField] private float _flickerAmplitude = 0.1f;
    [SerializeField] private float _flickerSpeed = 2.0f;

    private float _baseIntensity = 1f;
    private float _currentFade = 0f;
    private bool _targetOn = false;
    private float _perlinSeed;

    private void Awake()
    {
        _perlinSeed = transform.position.x * 17.3f + transform.position.y * 31.7f;

        _currentFade = 0f;
        _targetOn = false;
        ApplyIntensity(0f);
    }

    public void TurnOn()
    {
        _targetOn = true;
    }

    public void TurnOff()
    {
        _targetOn = false;
    }

    private void Update()
    {
        float target = _targetOn ? 1f : 0f;

        if (_fadeDuration > 0f)
            _currentFade = Mathf.MoveTowards(_currentFade, target, Time.deltaTime / _fadeDuration);
        else
            _currentFade = target;

        float flicker = 0f;
        if (_currentFade > 0f)
        {
            float noise = Mathf.PerlinNoise(_perlinSeed, Time.time * _flickerSpeed);
            flicker = (noise - 0.5f) * 2f * _flickerAmplitude;
        }

        float intensity = _baseIntensity * _currentFade + flicker * _currentFade;
        intensity = Mathf.Max(intensity, 0f);
        ApplyIntensity(intensity);
    }

    private void ApplyIntensity(float intensity)
    {
        for (int i = 0; i < _lights.Count; i++)
        {
            if (_lights[i] != null)
                _lights[i].intensity = intensity;
        }
    }
}
