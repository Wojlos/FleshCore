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

    private BaseNode _currentNode;
    private BaseNode _nextNode;
    private BaseNode _destination;

    private SplineAnimate _splineAnimator;
    private Coroutine _travelRoutine;
    private bool _travelCanceled = false;

    private void Awake()
    {
        _splineAnimator = GetComponent<SplineAnimate>();
        if (_splineAnimator == null)
            Debug.LogError($"{name}: Missing SplineAnimate component.");
    }

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
        if (_splineAnimator == null)
            yield break;

        while (_path.Count > 0)
        {
            if (_travelCanceled)
            {
                Destroy(gameObject);
                yield break;    
            }
                
            _nextNode = DequeueNextNode();
            if (_nextNode == null)
                yield break;

            var container = FindCommonSplineContainer(_currentNode, _nextNode);

            _currentNode.ChangeUnitCount(-_units, true);
            _currentNode.RemovePlayerTravelGroup(this);

            if (container == null)
            {
                Debug.LogWarning($"No common SplineContainer between {_currentNode.name} and {_nextNode.name}. Teleporting.");
                transform.position = _nextNode.transform.position;
            }
            else
            {
                _splineAnimator.Container = container;

                SetStartTimeByEndpoints(container, _currentNode.transform.position, _nextNode.transform.position);

                _splineAnimator.Play();
                while (_splineAnimator.IsPlaying)
                    yield return null;

                transform.position = _nextNode.transform.position;
            }

            _currentNode = _nextNode;
            _currentNode.ChangeUnitCount(_units, true);
            _currentNode.AddPlayerTravelGroup(this);

            if (_stepDelaySeconds > 0f)
                yield return new WaitForSeconds(_stepDelaySeconds);
        }
        _currentNode.RemovePlayerTravelGroup(this);
        Destroy(gameObject);
        _travelRoutine = null;
    }

    public void CancelTravel()
    {
        _travelCanceled = true;
    }

    private SplineContainer FindCommonSplineContainer(BaseNode a, BaseNode b)
    {
        if (a == null || b == null) return null;

        List<SplineContainer> aContainers = a.GetSplineContainers();
        List<SplineContainer> bContainers = b.GetSplineContainers();

        if (aContainers == null || bContainers == null) return null;

        var set = new HashSet<SplineContainer>(aContainers);
        foreach (var c in bContainers)
        {
            if (c != null && set.Contains(c))
                return c;
        }

        return null;
    }
    private void SetStartTimeByEndpoints(SplineContainer container, Vector3 fromPos, Vector3 toPos)
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
        {
            container.ReverseFlow(0);
        }
        _splineAnimator.NormalizedTime = 0f;
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
