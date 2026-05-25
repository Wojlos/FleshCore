using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private Light2D _light;
    [SerializeField] private float _lightDuration = 0.15f;
    [SerializeField] private AnimationCurve _lightIntensityCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
    [SerializeField] private float _maxIntensity = 2f;

    private float _timer;
    private bool _active = false;

    private void OnEnable()
    {
        if (_light != null)
        {
            _active = true;
            _timer = 0f;
            _light.enabled = true;
            _light.intensity = _maxIntensity;
        }
    }

    private void Update()
    {
        if (!_active || _light == null) return;

        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / _lightDuration);
        float curveValue = _lightIntensityCurve.Evaluate(t);
        _light.intensity = curveValue * _maxIntensity;

        if (t >= 1f)
        {
            _light.enabled = false;
            _active = false;
        }
    }
}
