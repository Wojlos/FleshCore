using UnityEngine;

public class EnemyTravelGroup : UnitTravelGroup
{
    [SerializeField] private GameObject _targetGroupPrefab;

    private IEnemyUnitBrain _brain;
    private BaseBuilding _originBuilding;
    private bool _tracksOriginBuilding;
    private TargetGroup _targetGroup;

    public void Setup(int units, BaseNode start, IEnemyUnitBrain brain, BaseBuilding originBuilding = null)
    {
        var settings = GameManager.Instance.GetGameSettings();
        float speed = Random.Range(settings.enemySpeedMin, settings.enemySpeedMax);
        float stepDelay = Random.Range(settings.enemyStepDelayMin, settings.enemyStepDelayMax);
        Color color = settings.immunoUnitGroupColor;

        _originBuilding = originBuilding;
        _tracksOriginBuilding = _originBuilding != null;

        _brain = brain;
        _brain.Initialize(this, start, originBuilding);

        SetupBase(units, start, ConflictSide.Immuno, speed, stepDelay, color);
        SetupTargetGroup(units);
    }

    private void SetupTargetGroup(int units)
    {
        if (_targetGroupPrefab == null) return;

        GameObject go = Instantiate(_targetGroupPrefab, transform.position, Quaternion.identity, transform);
        _targetGroup = go.GetComponent<TargetGroup>();
        if (_targetGroup == null)
            _targetGroup = go.AddComponent<TargetGroup>();

        _targetGroup.SetOwner(this);
        _targetGroup.SetUnitCount(units);
        _targetGroup.SpawnTargets();
    }

    public override void ApplyDamage(int amount)
    {
        base.ApplyDamage(amount);

        if (_targetGroup != null)
        {
            _targetGroup.SetUnitCount(_units);
            _targetGroup.SpawnTargets();
        }
    }

    public TargetGroup GetTargetGroup() => _targetGroup;

    public BaseBuilding GetOriginBuilding() => _originBuilding;

    public void RedirectTo(BaseNode destination)
    {
        if (_brain == null) return;
        _brain.OnRedirected(destination);

        // If idle, ask brain immediately for new order
        if (GetState() == BodyState.Idle)
            ExecuteOrder(_brain.DecideNext(_currentNode));
    }

    protected override void OnArrivedAtNode()
    {
        if (IsOriginDestroyed())
        {
            DisbandAndDeleteUnits();
            return;
        }
    }

    protected override void OnReadyToMove()
    {
        if (IsOriginDestroyed())
        {
            DisbandAndDeleteUnits();
            return;
        }

        if (_brain == null)
        {
            Disband();
            return;
        }

        ExecuteOrder(_brain.DecideNext(_currentNode));
    }

    protected override void OnEncounterChanged(EncounterInfo info)
    {
        if (_brain == null) return;

        if (info.Type == EncounterType.None)
        {
            ExecuteOrder(_brain.OnEncounterEnded(_currentNode));
        }
        else
        {
            ExecuteOrder(_brain.OnEncounterUpdate(info));
        }
    }

    protected override void OnBodyDying()
    {
        _brain?.OnBodyDestroyed(_currentNode);
    }

    private void ExecuteOrder(BrainOrder order)
    {
        switch (order.Instruction)
        {
            case BrainInstruction.Wait:
                StayIdle();
                break;
            case BrainInstruction.Move:
                if (order.TargetNode != null)
                    MoveTo(order.TargetNode);
                else
                    StayIdle();
                break;
        }
    }

    private bool IsOriginDestroyed()
    {
        return _tracksOriginBuilding && _originBuilding == null;
    }
}
