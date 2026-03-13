using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(HealthSystem))]
public abstract class BaseBuilding : MonoBehaviour
{
    [SerializeField] private GameObject _disabledMarker;
    private BuildingType _type;
    public ConflictSide Side { get; private set; }
    protected BaseNode _node;
    protected int _unitProductionPerSecond = 0;

    protected bool _active = true;
    protected bool _isSieged = false;
    public bool IsSieged => _isSieged;
    
    private float _healingTimer = 0f;

    private Material _material;
    private SpriteRenderer _spriteRenderer;
    private HealthSystem _healthSystem;

    protected BaseBuilding(BuildingType type, ConflictSide side)
    {
        _type = type;
        Side = side;
    }

    protected virtual void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _material = _spriteRenderer.material;    

        UpdateMaterialHealthValues();

        if (_healthSystem != null)
        {
            _healthSystem.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(object sender, System.EventArgs e)
    {
        UpdateMaterialHealthValues();
    }

    private void UpdateMaterialHealthValues()
    {
        if (_spriteRenderer == null || _healthSystem == null) return;
        _material.SetFloat("_CurrentHp", _healthSystem.CurrentHealth);
        _material.SetFloat("_MaxHp", _healthSystem.MaxHealth);
    }

    public void AssignNode(BaseNode node)
    {
        _node = node;
        _node.AssignBuilding(this);
    }

    public BaseNode GetNode() => _node;

    public virtual void Cleanup()
    {
        
    }

    void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
        if (!_isSieged && _healthSystem != null)
        {
            _healingTimer += Time.deltaTime;
            
            if (_healingTimer >= 1f)
            {
                _healingTimer = 0f;
                
                float healingSpeed = GetHealingSpeed();
                if (healingSpeed > 0f && _healthSystem.CurrentHealth < _healthSystem.MaxHealth)
                {
                    _healthSystem.Heal(healingSpeed);
                }
            }
        }
    }
    
    private float GetHealingSpeed()
    {
        var gameSettings = GameManager.Instance?.GetGameSettings();
        if (gameSettings == null) return 0f;
        
        return Side == ConflictSide.Player 
            ? gameSettings.playerBuildingHealingSpeed 
            : gameSettings.immunoBuildingHealingSpeed;
    }

    public virtual void ApplySettings(BuildingSettings settings)
    {
        if (settings == null) return;

        SetUnitProduction(settings.unitProductionPerSecond);
        var hs = GetComponent<HealthSystem>();
        if (hs != null)
        {
            hs.MaxHealth = settings.startHp;
            hs.HealToFull();
        }
    }
    public BuildingType GetBuildingType()=> _type;
    public void SetUnitProduction(int u)
    {
        _unitProductionPerSecond = u;
        
    }
    public virtual void Disable()
    {
        _active = false;
        if (_disabledMarker != null)
            _disabledMarker.SetActive(true);
        
    }
    public virtual void Enable()
    {
        _active = true;
        if (_disabledMarker != null)
            _disabledMarker.SetActive(false);
    }

    public virtual void OnEncounterReported(BaseNode encounterNode, UnitTravelGroup reporter) { }

    public void SetSieged(bool sieged)
    {
        _isSieged = sieged;
        if (sieged)
            Disable();
        else
            Enable();
    }

}
