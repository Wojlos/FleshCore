using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;

public enum BodyState
{
    Idle,
    Moving
}

public abstract class UnitTravelGroup : MonoBehaviour
{
    [SerializeField] private GameObject _visualisation;

    protected int _units;
    protected ConflictSide _side;
    protected BaseNode _currentNode;
    protected BaseNode _previousNode;

    private BaseNode _nextNode;
    private SplineAnimate _splineAnimator;
    private Coroutine _moveCoroutine;
    private float _movementSpeed;
    protected float _stepDelay;
    private float _waitTimer;
    private bool _waitingForReady;

    private BodyState _state;
    private bool _isRegisteredOnNode;

    private MaterialPropertyBlock _materialPropertyBlock;
    private MeshRenderer _sphereRenderer;

    private NodeEncounterController _currentEncounterController;

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────

    private void Awake()
    {
        _splineAnimator = GetComponent<SplineAnimate>();
        if (_splineAnimator == null)
            Debug.LogError($"{name}: Missing SplineAnimate component.");

        _materialPropertyBlock = new MaterialPropertyBlock();

        if (_visualisation != null)
            _sphereRenderer = _visualisation.GetComponent<MeshRenderer>();

        OnAwake();
    }

    private void Update()
    {
        if (_units <= 0)
        {
            OnBodyDying();
            Cleanup();
            Destroy(gameObject);
            return;
        }

        switch (_state)
        {
            case BodyState.Idle:
                UpdateIdle();
                break;
            case BodyState.Moving:
                break;
        }
    }

    private void OnDestroy()
    {
        if (_state == BodyState.Moving && _currentNode != null && _units > 0)
        {
            _currentNode.ChangeUnitCount(_units, _side);
            _currentNode.AddTravelGroup(this, _side);
        }

        UnsubscribeFromEncounter();

        if (_isRegisteredOnNode && _currentNode != null)
            _currentNode.RemoveTravelGroup(this, _side);
    }

    // ──────────────────────────────────────────────
    // Abstract / Virtual Hooks
    // ──────────────────────────────────────────────

    protected virtual void OnAwake() { }
    protected abstract void OnArrivedAtNode();
    protected abstract void OnReadyToMove();
    protected virtual void OnEncounterChanged(EncounterInfo info) { }
    protected virtual void OnBodyDying() { }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public int GetUnits() => _units;
    public BaseNode GetCurrentNode() => _currentNode;
    public ConflictSide GetSide() => _side;
    public BodyState GetState() => _state;

    public void ApplyDamage(int amount)
    {
        _units -= amount;
        if (_units < 0) _units = 0;
    }

    public void Disband()
    {
        Cleanup();
        Destroy(gameObject);
    }

    public void DisbandAndDeleteUnits()
    {
        if (_currentNode != null && _units > 0)
            _currentNode.DestroyUnits(_units, _side);
        _units = 0;
        Cleanup();
        Destroy(gameObject);
    }

    public void CancelTravel()
    {
        Disband();
    }

    // ──────────────────────────────────────────────
    // Protected Setup (called by subclasses)
    // ──────────────────────────────────────────────

    protected void SetupBase(int units, BaseNode startNode, ConflictSide side, float speed, float stepDelay, Color color)
    {
        StopMoveCoroutine();
        if (_splineAnimator != null)
            _splineAnimator.Pause();

        _units = units;
        _side = side;
        _movementSpeed = speed;
        _stepDelay = stepDelay;
        _currentNode = startNode;

        SetSphereColor(color);

        if (_currentNode != null)
        {
            transform.position = _currentNode.transform.position;
            RegisterOnNode();
            SubscribeToEncounter();
        }

        EnterIdle(true);
    }

    // ──────────────────────────────────────────────
    // Protected Movement Commands
    // ──────────────────────────────────────────────

    protected void MoveTo(BaseNode target)
    {
        if (target == null || _currentNode == null) return;

        // Cannot leave a node while opposing units are present
        if (_currentNode.GetOpposingUnits(_side) > 0)
        {
            StayIdle();
            return;
        }

        _nextNode = target;
        EnterMoving();
    }

    protected void StayIdle()
    {
        _waitingForReady = false;
        _waitTimer = _stepDelay;
    }

    // ──────────────────────────────────────────────
    // Idle State
    // ──────────────────────────────────────────────

    private void EnterIdle(bool isInitial = false)
    {
        _state = BodyState.Idle;
        _waitTimer = _stepDelay;
        _waitingForReady = false;

        if (!isInitial)
            OnArrivedAtNode();
    }

    private void UpdateIdle()
    {
        if (_waitingForReady) return;

        // Don't tick timer while a unit-vs-unit battle is active on this node
        if (_currentEncounterController != null &&
            _currentEncounterController.GetCurrentEncounterType() == EncounterType.Battle)
            return;

        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        _waitingForReady = true;
        OnReadyToMove();
    }

    // ──────────────────────────────────────────────
    // Moving State
    // ──────────────────────────────────────────────

    private void EnterMoving()
    {
        _state = BodyState.Moving;
        _currentNode.ChangeUnitCount(-_units, _side);
        UnregisterFromNode();
        UnsubscribeFromEncounter();
        _moveCoroutine = StartCoroutine(PlayMoveAnimation());
    }

    private IEnumerator PlayMoveAnimation()
    {
        var container = FindCommonSplineContainer(_currentNode, _nextNode);

        if (container == null)
        {
            Debug.LogWarning($"No common SplineContainer between {_currentNode.name} and {_nextNode.name}. Teleporting.");
            transform.position = _nextNode.transform.position;
        }
        else
        {
            _splineAnimator.Container = container;
            SetSplineDirection(container, _currentNode.transform.position, _nextNode.transform.position);
            _splineAnimator.AnimationMethod = SplineAnimate.Method.Speed;
            _splineAnimator.MaxSpeed = _movementSpeed;
            _splineAnimator.Play();

            while (_splineAnimator.IsPlaying)
                yield return null;

            transform.position = _nextNode.transform.position;
        }

        ArriveAtNode();
    }

    private void ArriveAtNode()
    {
        _previousNode = _currentNode;
        _currentNode = _nextNode;
        _nextNode = null;
        _moveCoroutine = null;

        _currentNode.ChangeUnitCount(_units, _side);
        RegisterOnNode();
        SubscribeToEncounter();

        EnterIdle(false);

        // Notify about existing encounter AFTER state is Idle
        // so any MoveTo triggered by the brain won't conflict with EnterIdle
        NotifyExistingEncounter();
    }

    // ──────────────────────────────────────────────
    // Encounter Subscription
    // ──────────────────────────────────────────────

    private void SubscribeToEncounter()
    {
        if (_currentNode == null) return;

        _currentEncounterController = _currentNode.GetComponent<NodeEncounterController>();
        if (_currentEncounterController != null)
        {
            _currentEncounterController.RegisterGroup(this, _side);
            _currentEncounterController.OnEncounterChanged += HandleEncounterChanged;
        }
    }

    private void NotifyExistingEncounter()
    {
        if (_currentEncounterController == null) return;
        var info = _currentEncounterController.GetCurrentEncounterInfo();
        if (info.Type != EncounterType.None)
            OnEncounterChanged(info);
    }

    private void UnsubscribeFromEncounter()
    {
        if (_currentEncounterController != null)
        {
            _currentEncounterController.UnregisterGroup(this, _side);
            _currentEncounterController.OnEncounterChanged -= HandleEncounterChanged;
            _currentEncounterController = null;
        }
    }

    private void HandleEncounterChanged(EncounterInfo info)
    {
        OnEncounterChanged(info);
    }

    // ──────────────────────────────────────────────
    // Node Registration
    // ──────────────────────────────────────────────

    private void RegisterOnNode()
    {
        if (_currentNode == null || _isRegisteredOnNode) return;
        _currentNode.AddTravelGroup(this, _side);
        _isRegisteredOnNode = true;
    }

    private void UnregisterFromNode()
    {
        if (_currentNode == null || !_isRegisteredOnNode) return;
        _currentNode.RemoveTravelGroup(this, _side);
        _isRegisteredOnNode = false;
    }

    private void Cleanup()
    {
        StopMoveCoroutine();
        UnsubscribeFromEncounter();
        UnregisterFromNode();
    }

    private void StopMoveCoroutine()
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
            _moveCoroutine = null;
        }
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private void SetSphereColor(Color color)
    {
        if (_sphereRenderer == null || _materialPropertyBlock == null)
            return;

        _materialPropertyBlock.SetColor("_FaceColor", color);
        _materialPropertyBlock.SetColor("_Color", color);
        _sphereRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    protected SplineContainer FindCommonSplineContainer(BaseNode a, BaseNode b)
    {
        if (a == null || b == null) return null;

        var aContainers = a.GetSplineContainers();
        var bContainers = b.GetSplineContainers();
        if (aContainers == null || bContainers == null) return null;

        var set = new HashSet<SplineContainer>(aContainers);
        foreach (var c in bContainers)
        {
            if (c != null && set.Contains(c))
                return c;
        }

        return null;
    }

    private void SetSplineDirection(SplineContainer container, Vector3 fromPos, Vector3 toPos)
    {
        if (container == null || container.Splines.Count == 0)
        {
            _splineAnimator.NormalizedTime = 0f;
            return;
        }

        Spline spline = container.Splines[0];
        if (spline == null || spline.Count < 2)
        {
            _splineAnimator.NormalizedTime = 0f;
            return;
        }

        Vector3 p0 = spline[0].Position;
        Vector3 p1 = spline[spline.Count - 1].Position;

        float dFrom0 = (p0 - fromPos).sqrMagnitude;
        float dFrom1 = (p1 - fromPos).sqrMagnitude;
        float dTo0 = (p0 - toPos).sqrMagnitude;
        float dTo1 = (p1 - toPos).sqrMagnitude;

        bool forward = (dFrom0 + dTo1) <= (dFrom1 + dTo0);
        if (!forward)
            container.ReverseFlow(0);

        _splineAnimator.NormalizedTime = 0f;
    }
}
