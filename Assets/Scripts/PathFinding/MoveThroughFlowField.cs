using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveThroughFlowField : MonoBehaviour
{
    [SerializeField] private float _speedFactor = 0.1f;
    [SerializeField] private float _turnspeed = 0.1f;
    [SerializeField] private float _range = 2f;
    private Grid _grid;
    private Vector3Int _currentCellPos;
    private float _currentCellCost;

    // private Vector3Int _currentCellPos;
    private Vector2 _nextMove;
    private Vector2 lastMove = Vector2.zero;
    private Vector3 _nextTarget; 

    private bool _isTrigered = false;

    void Start(){
        _grid = InputManager.Instance.GetGrid();
        _nextTarget = _grid.WorldToCell(transform.position);
    }



    void Update()
    {   
        _isTrigered = false;
        if(PathfindingManager.Instance.GetIntegratedCost(transform.position) != 0f){
            _isTrigered = true;
        }

        if(_isTrigered){
            if(_nextTarget == transform.position){
                // PathfindingManager.Instance.SetOccupied(transform.position, 0f);
                SetNextMove();
                // PathfindingManager.Instance.SetOccupied(_nextTarget, 1f);
            }
        }else{
            if(_nextTarget == transform.position){
                // PathfindingManager.Instance.SetOccupied(transform.position, 0f);
                SetNextRandomMove();
                // PathfindingManager.Instance.SetOccupied(_nextTarget, 1f);
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, _nextTarget, _speedFactor*Time.deltaTime);
    
        foreach(var target in StructuresManager.Instance.GetAllBuildings())
        {
            Vector3 normPos = transform.position;  
            normPos.z = target.transform.position.z;
            if(Vector3.Distance(normPos, target.transform.position) <= _range){
                if(target.TryGetComponent(out HealthSystem hs)){
                    hs.DoDamage(2f);
                    return;
                }
            }
        } 
    }

    private void SetNextRandomMove(){
        List<Vector2Int> directions = new List<Vector2Int>(){ 
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
            // new Vector2Int(1,1),
            // new Vector2Int(1,-1),
            // new Vector2Int(-1,1),
            // new Vector2Int(-1,-1)
        };
        List<Vector2Int> posibleDirs = new List<Vector2Int>();
        
        foreach(var dir in directions){
            Vector3Int currentCell = _grid.WorldToCell(transform.position);
            currentCell.x += dir.x;
            currentCell.y += dir.y;
            if(PathfindingManager.Instance.GetNodeCost(currentCell) <= 1f ){
                posibleDirs.Add(dir);
            }
        }

        if(posibleDirs.Count == 0){
            _nextMove = Vector2Int.zero;
            return;
        }

        Vector2Int nm = posibleDirs[Random.Range(0, posibleDirs.Count)];
        
        Vector3Int cellTarget  = _grid.WorldToCell(transform.position);
        cellTarget.x += nm.x;
        cellTarget.y += nm.y; 
        Vector3 newTarget = _grid.GetCellCenterWorld(cellTarget);
        if(PathfindingManager.Instance.GetOccupied(newTarget) != 0f){
            return;
        }
        _nextTarget = newTarget; 

    }

    private void SetNextMove(){
        List<Vector2Int> nextMoves = PathfindingManager.Instance.GetNextMoves((int)transform.position.x, (int)transform.position.y);
        if(nextMoves.Count == 0){
            _nextMove = Vector2Int.zero;
            return;
        }
        Vector2Int nm = nextMoves[Random.Range(0,nextMoves.Count)];
        // _nextMove.x = nm.x;
        // _nextMove.y = nm.y;
        
        Vector3Int cellTarget  = _grid.WorldToCell(transform.position);
        cellTarget.x += nm.x;
        cellTarget.y += nm.y; 
        Vector3 newTarget = _grid.GetCellCenterWorld(cellTarget);
        if(PathfindingManager.Instance.GetOccupied(newTarget) != 0f){
            return;
        }
        _nextTarget = newTarget; 

    }
}
