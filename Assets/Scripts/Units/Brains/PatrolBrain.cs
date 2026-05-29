using UnityEngine;
using System.Collections.Generic;

public class PatrolBrain : IEnemyUnitBrain
{
    private enum PatrolState
    {
        Patrolling,
        Fighting,
        Fleeing,
        Recalling,
        Reinforcing
    }

    private EnemyTravelGroup _body;
    private BaseNode _originNode;
    private BaseBuilding _originBuilding;
    private BaseNode _previousNode;
    private BaseNode _encounterNode;
    private PatrolState _state = PatrolState.Patrolling;
    private List<BaseNode> _recallPath = new List<BaseNode>();

    public void Initialize(EnemyTravelGroup body, BaseNode startNode, BaseBuilding originBuilding)
    {
        _body = body;
        _originNode = startNode;
        _originBuilding = originBuilding;
    }

    public BrainOrder DecideNext(BaseNode currentNode)
    {
        _previousNode = currentNode;

        switch (_state)
        {
            case PatrolState.Patrolling:
                return DecidePatrol(currentNode);

            case PatrolState.Fighting:
                // After fighting, flee home
                _state = PatrolState.Fleeing;
                return ComputeRecallOrder(currentNode);

            case PatrolState.Reinforcing:
                return FollowReinforcePath(currentNode);

            case PatrolState.Fleeing:
            case PatrolState.Recalling:
                return FollowRecallPath(currentNode);

            default:
                return BrainOrder.WaitOrder();
        }
    }

    public BrainOrder OnEncounterUpdate(EncounterInfo info)
    {
        bool opposingUnitsPresent = info.Node != null && info.Node.GetOpposingUnits(ConflictSide.Immuno) > 0;

        // Opposing units on the node = always stay and fight, regardless of state
        if (info.Type == EncounterType.Battle || opposingUnitsPresent)
        {
            if (_encounterNode == null)
                _encounterNode = info.Node;
            _state = PatrolState.Fighting;
            return BrainOrder.WaitOrder();
        }

        // No opposing units from here on — handle Siege/Conquest by state
        if (_state == PatrolState.Fleeing || _state == PatrolState.Recalling)
        {
            return FollowRecallPathImmediate();
        }

        if (_state == PatrolState.Reinforcing)
        {
            return BrainOrder.WaitOrder();
        }

        if (info.Type == EncounterType.Siege || info.Type == EncounterType.NodeConquest)
        {
            if (_encounterNode == null)
                _encounterNode = info.Node;
            _state = PatrolState.Fleeing;
            return ComputeRecallOrder(info.Node);
        }

        return BrainOrder.WaitOrder();
    }

    public BrainOrder OnEncounterEnded(BaseNode currentNode)
    {
        if (_state == PatrolState.Fighting || _state == PatrolState.Reinforcing)
        {
            _state = PatrolState.Fleeing;
            return ComputeRecallOrder(currentNode);
        }

        return BrainOrder.WaitOrder();
    }

    public void OnRedirected(BaseNode newDestination)
    {
        _state = PatrolState.Reinforcing;
        _encounterNode = newDestination;
        var currentNode = _body.GetCurrentNode();
        if (currentNode == null) return;

        var path = PathfindingManager.Instance.GetPath(currentNode, newDestination);
        if (path != null && path.Count > 0)
        {
            _recallPath = path;
            if (_recallPath.Count > 0 && _recallPath[0] == currentNode)
                _recallPath.RemoveAt(0);
        }
    }

    public void OnBodyDestroyed(BaseNode lastNode)
    {
        if ((_state == PatrolState.Fleeing || _state == PatrolState.Fighting) 
            && _encounterNode != null && _originBuilding != null)
        {
            _originBuilding.OnEncounterReported(_encounterNode, _body);
        }
    }

    private BrainOrder DecidePatrol(BaseNode currentNode)
    {
        // Check if origin is under attack
        if (_originNode != null && _originNode.GetOpposingUnits(ConflictSide.Immuno) > 0)
        {
            _state = PatrolState.Recalling;
            return ComputeRecallOrder(currentNode);
        }

        // Check if on opposing territory (not at origin)
        if (currentNode != _originNode && currentNode.GetOwner() != ConflictSide.Immuno)
        {
            // If opposing units are present, stay and fight
            if (currentNode.GetOpposingUnits(ConflictSide.Immuno) > 0)
            {
                _state = PatrolState.Fighting;
                _encounterNode = currentNode;
                return BrainOrder.WaitOrder();
            }

            // No opposing units - flee and alert
            _encounterNode = currentNode;
            _state = PatrolState.Fleeing;
            return ComputeRecallOrder(currentNode);
        }

        // Pick random neighbor for patrol
        var neighbours = currentNode.GetConnectedNodes();
        if (neighbours == null || neighbours.Count == 0)
            return BrainOrder.WaitOrder();

        var preferred = neighbours.FindAll(n => n != _previousNode);
        var candidates = preferred.Count > 0 ? preferred : neighbours;
        var next = candidates[Random.Range(0, candidates.Count)];

        return BrainOrder.MoveOrder(next);
    }

    private BrainOrder ComputeRecallOrder(BaseNode currentNode)
    {
        if (_originNode == null || currentNode == _originNode)
        {
            ReportAndDisband();
            return BrainOrder.WaitOrder();
        }

        var path = PathfindingManager.Instance.GetPath(currentNode, _originNode);
        if (path == null || path.Count == 0)
        {
            _body.Disband();
            return BrainOrder.WaitOrder();
        }

        _recallPath = path;
        if (_recallPath.Count > 0 && _recallPath[0] == currentNode)
            _recallPath.RemoveAt(0);

        return FollowRecallPath(currentNode);
    }

    private BrainOrder FollowRecallPath(BaseNode currentNode)
    {
        if (currentNode == _originNode)
        {
            ReportAndDisband();
            return BrainOrder.WaitOrder();
        }

        if (_recallPath.Count > 0)
        {
            var next = _recallPath[0];
            _recallPath.RemoveAt(0);
            return BrainOrder.MoveOrder(next);
        }

        // Path exhausted but not at origin - recompute
        return ComputeRecallOrder(currentNode);
    }

    private BrainOrder FollowRecallPathImmediate()
    {
        if (_recallPath.Count > 0)
        {
            var next = _recallPath[0];
            _recallPath.RemoveAt(0);
            return BrainOrder.MoveOrder(next);
        }

        return BrainOrder.WaitOrder();
    }

    private BrainOrder FollowReinforcePath(BaseNode currentNode)
    {
        // Arrived at encounter destination
        if (_recallPath.Count == 0)
        {
            // Stay here - we're reinforcing this position
            // When encounter ends, we'll switch to fleeing home
            return BrainOrder.WaitOrder();
        }

        var next = _recallPath[0];
        _recallPath.RemoveAt(0);
        return BrainOrder.MoveOrder(next);
    }

    private void ReportAndDisband()
    {
        if (_encounterNode != null && _originBuilding != null)
            _originBuilding.OnEncounterReported(_encounterNode, _body);

        _body.Disband();
    }
}
