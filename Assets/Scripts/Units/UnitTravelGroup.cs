using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;

public enum UnitState
{
    Waiting,
    Moving,
    Fighting
}

public enum UnitMode
{
    Patrol,
    Directed
}

public class UnitTravelGroup : MonoBehaviour
{
    [SerializeField] private GameObject _visualisation;

    private int _units;
    private List<BaseNode> _path = new List<BaseNode>();

    private BaseNode _currentNode;
    private BaseNode _nextNode;
    private BaseNode _originNode;
    private BaseNode _previousNode;

    private SplineAnimate _splineAnimator;
    private Coroutine _moveCoroutine;

    private UnitState _state;
    private UnitMode _mode;
    private ConflictSide _side;

    private float _waitTimer;
    private float _stepDelay;
    private float _movementSpeed;
    private int _unitsBeforeBattle;
    private bool _isRegisteredOnNode;

    private BaseBuilding _originBuilding;
    private bool _tracksOriginBuilding;

    private bool _fleeingEncounter;
    private BaseNode _encounterNode;

    // ──────────────────────────────────────────────
    // Lifecycle
    // ──────────────────────────────────────────────

    private void Awake()
    {
        _splineAnimator = GetComponent<SplineAnimate>();
        if (_splineAnimator == null)
            Debug.LogError($"{name}: Missing SplineAnimate component.");
    }

    private void Update()
    {
        switch (_state)
        {
            case UnitState.Waiting:
                UpdateWaiting();
                break;
            case UnitState.Moving:
                break;
            case UnitState.Fighting:
                UpdateFighting();
                break;
        }
    }

    private void OnDestroy()
    {
        if (_state == UnitState.Moving && _currentNode != null && _units > 0)
        {
            _currentNode.ChangeUnitCount(_units, _side == ConflictSide.Player);
            _currentNode.AddTravelGroup(this, _side);
        }

        if (_isRegisteredOnNode && _currentNode != null)
            _currentNode.RemoveTravelGroup(this, _side);
    }

    // ──────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────

    public void SetupPatrol(int units, BaseNode start, BaseNode destination, ConflictSide side)
    {
        Setup(units, start, destination, side, UnitMode.Patrol);
    }

    public void SetupDirected(int units, BaseNode start, BaseNode destination, ConflictSide side)
    {
        Setup(units, start, destination, side, UnitMode.Directed);
    }

    public void Disband()
    {
        Cleanup();
        Destroy(gameObject);
    }

    public void DisbandAndDeleteUnits()
    {
        if (_currentNode != null && _units > 0)
            _currentNode.DestroyUnits(_units, _side == ConflictSide.Player);
        _units = 0;
        Cleanup();
        Destroy(gameObject);
    }

    public void CancelTravel()
    {
        Disband();
    }

    public int GetUnits() => _units;
    public BaseNode GetCurrentNode() => _currentNode;
    public ConflictSide GetSide() => _side;

    public void RedirectTo(BaseNode destination)
    {
        _mode = UnitMode.Directed;

        BaseNode fromNode = (_state == UnitState.Moving && _nextNode != null) ? _nextNode : _currentNode;
        if (fromNode == null) return;

        var newPath = PathfindingManager.Instance.GetPath(fromNode, destination);
        if (newPath == null || newPath.Count == 0)
            return;

        _path = newPath;
        DequeueNextNode();
    }

    // ──────────────────────────────────────────────
    // Setup
    // ──────────────────────────────────────────────

    private void Setup(int units, BaseNode start, BaseNode destination, ConflictSide side, UnitMode mode)
    {
        StopMoveCoroutine();
        _splineAnimator.Pause();

        _units = units;
        _side = side;
        _originNode = start;
        _mode = mode;

        var settings = GameManager.Instance.GetGameSettings();
        if (side == ConflictSide.Player)
        {
            _stepDelay = Random.Range(settings.playerStepDelayMin, settings.playerStepDelayMax);
            _movementSpeed = Random.Range(settings.playerSpeedMin, settings.playerSpeedMax);
        }
        else
        {
            _stepDelay = Random.Range(settings.enemyStepDelayMin, settings.enemyStepDelayMax);
            _movementSpeed = Random.Range(settings.enemySpeedMin, settings.enemySpeedMax);
        }

        var building = start.GetBuilding();
        _originBuilding = (building != null && building.Side == side) ? building : null;
        _tracksOriginBuilding = _originBuilding != null;

        var newPath = PathfindingManager.Instance.GetPath(start, destination);
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("Setup: Path is empty or null.");
            Disband();
            return;
        }

        _path = newPath;
        _currentNode = DequeueNextNode();

        if (_currentNode != null)
        {
            transform.position = _currentNode.transform.position;
            RegisterOnNode();
        }

        EnterWaiting();
    }

    // ──────────────────────────────────────────────
    // Waiting State
    // ──────────────────────────────────────────────

    private void EnterWaiting()
    {
        _state = UnitState.Waiting;
        _waitTimer = _stepDelay;
    }

    private void UpdateWaiting()
    {
        if (IsOriginBuildingDestroyed())
        {
            DisbandAndDeleteUnits();
            return;
        }

        if (!_fleeingEncounter && HasConflict())
        {
            bool hasOpposingUnits = _currentNode.GetOpposingUnits(_side) > 0;

            if (!hasOpposingUnits && _mode == UnitMode.Patrol && _side == ConflictSide.Immuno
                && _currentNode != _originNode)
            {
                _encounterNode = _currentNode;
                _fleeingEncounter = true;
                SwitchToDirectedRecall();
                return;
            }

            if (_encounterNode == null && _mode == UnitMode.Patrol && _side == ConflictSide.Immuno
                && _currentNode != _originNode)
            {
                _encounterNode = _currentNode;
            }

            EnterFighting();
            return;
        }

        if (!_fleeingEncounter && HasOpposingNodeOwnership())
        {
            if (_mode == UnitMode.Patrol && _side == ConflictSide.Immuno && _currentNode != _originNode)
            {
                _encounterNode = _currentNode;
                _fleeingEncounter = true;
                SwitchToDirectedRecall();
                return;
            }
        }

        if (_mode == UnitMode.Patrol && IsOriginUnderAttack())
        {
            SwitchToDirectedRecall();
            return;
        }

        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            return;
        }

        if (_path.Count > 0)
        {
            _nextNode = DequeueNextNode();
            if (_nextNode != null)
            {
                EnterMoving();
                return;
            }
        }

        HandlePathEnd();
    }

    // ──────────────────────────────────────────────
    // Moving State
    // ──────────────────────────────────────────────

    private void EnterMoving()
    {
        if (!_fleeingEncounter && HasConflict())
        {
            EnterFighting();
            return;
        }

        _state = UnitState.Moving;
        _currentNode.ChangeUnitCount(-_units, _side == ConflictSide.Player);
        UnregisterFromNode();
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

        _currentNode.ChangeUnitCount(_units, _side == ConflictSide.Player);
        RegisterOnNode();

        EnterWaiting();
    }

    // ──────────────────────────────────────────────
    // Fighting State
    // ──────────────────────────────────────────────

    private void EnterFighting()
    {
        _state = UnitState.Fighting;
        _unitsBeforeBattle = _currentNode.GetUnits(_side);

        if (_mode == UnitMode.Patrol)
            SwitchToDirectedRecall();
    }

    private void UpdateFighting()
    {
        bool hasOpposingUnits = _currentNode.GetOpposingUnits(_side) > 0;
        bool hasOpposingBuilding = HasOpposingBuilding(_currentNode);
        bool hasOwnUnits = _currentNode.GetUnits(_side) > 0;

        if ((hasOpposingUnits || hasOpposingBuilding) && hasOwnUnits)
            return;

        if (!hasOwnUnits)
        {
            Cleanup();
            Destroy(gameObject);
            return;
        }

        ApplyBattleDamage();

        if (_units <= 0)
        {
            Cleanup();
            Destroy(gameObject);
            return;
        }

        if (_encounterNode != null && !_fleeingEncounter)
            _fleeingEncounter = true;

        EnterWaiting();
    }

    private void ApplyBattleDamage()
    {
        int unitsAfter = _currentNode.GetUnits(_side);
        int totalDamage = _unitsBeforeBattle - unitsAfter;

        if (totalDamage > 0 && _unitsBeforeBattle > 0)
        {
            float damageRatio = (float)totalDamage / _unitsBeforeBattle;
            _units -= Mathf.RoundToInt(_units * damageRatio);
        }
    }

    // ──────────────────────────────────────────────
    // Patrol Recall
    // ──────────────────────────────────────────────

    private void SwitchToDirectedRecall()
    {
        if (_originNode == null || _currentNode == _originNode)
            return;

        _mode = UnitMode.Directed;

        var recallPath = PathfindingManager.Instance.GetPath(_currentNode, _originNode);
        if (recallPath == null || recallPath.Count == 0)
        {
            Disband();
            return;
        }

        _path = recallPath;
        DequeueNextNode();
    }

    // ──────────────────────────────────────────────
    // Path End
    // ──────────────────────────────────────────────

    private void HandlePathEnd()
    {
        if (_mode == UnitMode.Patrol)
        {
            BaseNode next = PickPatrolNode();
            if (next != null)
            {
                _path.Clear();
                _path.Add(next);
                _nextNode = DequeueNextNode();
                EnterMoving();
            }
            else
            {
                Disband();
            }
        }
        else
        {
            if (_fleeingEncounter && _encounterNode != null && _originBuilding != null)
                _originBuilding.OnEncounterReported(_encounterNode, this);

            Disband();
        }
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
    // Conflict Checks
    // ──────────────────────────────────────────────

    private bool HasConflict()
    {
        if (_currentNode == null) return false;
        if (_currentNode.GetOpposingUnits(_side) > 0) return true;
        if (HasOpposingBuilding(_currentNode)) return true;
        return false;
    }

    private bool HasOpposingBuilding(BaseNode node)
    {
        if (node == null) return false;
        var building = node.GetBuilding();
        return building != null && building.Side != _side;
    }

    private bool IsOriginUnderAttack()
    {
        if (_originNode == null) return false;
        return _originNode.GetOpposingUnits(_side) > 0;
    }

    private bool IsOriginBuildingDestroyed()
    {
        return _tracksOriginBuilding && _originBuilding == null;
    }

    private bool HasOpposingNodeOwnership()
    {
        if (_currentNode == null) return false;
        return _currentNode.GetOwner() != _side;
    }

    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private BaseNode DequeueNextNode()
    {
        if (_path == null || _path.Count == 0)
            return null;

        BaseNode node = _path[0];
        _path.RemoveAt(0);
        return node;
    }

    private BaseNode PickPatrolNode()
    {
        var neighbours = _currentNode.GetConnectedNodes();
        if (neighbours == null || neighbours.Count == 0) return null;

        var preferred = neighbours.FindAll(n => n != _previousNode);
        var candidates = preferred.Count > 0 ? preferred : neighbours;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private SplineContainer FindCommonSplineContainer(BaseNode a, BaseNode b)
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
