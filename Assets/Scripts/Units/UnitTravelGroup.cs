using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines;

public class UnitTravelGroup : MonoBehaviour
{
    [SerializeField] private GameObject _visualisation;
    [SerializeField] private float _stepDelaySeconds = 1f;

    private int _units;
    private List<BaseNode> _path = new List<BaseNode>();

    private BaseNode _nextNode;
    private BaseNode _currentNode;
    private BaseNode _destination;
    private SplineAnimate splineTravelAnimator;

    private Coroutine _travelRoutine;

    public void SetupTravel(int units, BaseNode start, BaseNode destination)
    {
        _units = units;
        _destination = destination;

        var newPath = PathfindingManager.Instance.GetPath(start, destination);
        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning("SetupTravel: Path is empty or null.");
            return;
        }

        if (_travelRoutine != null)
        {
            StopCoroutine(_travelRoutine);
            _travelRoutine = null;
        }

        _path = newPath;

        _currentNode = DequeueNextNode();
        if (_currentNode != null)
            transform.position = _currentNode.transform.position;

        _travelRoutine = StartCoroutine(TravelPathRoutine());
    }

    private IEnumerator TravelPathRoutine()
    {
        while (_path.Count > 0)
        {
            _nextNode = DequeueNextNode();
            if (_nextNode == null)
                yield break;

            _currentNode = _nextNode;
            transform.position = _currentNode.transform.position;

            yield return new WaitForSeconds(_stepDelaySeconds);
        }

        _travelRoutine = null;
    }

    private BaseNode DequeueNextNode()
    {
        if (_path == null || _path.Count == 0)
            return null;

        BaseNode node = _path[0];
        _path.RemoveAt(0);
        return node;
    }
    
}
