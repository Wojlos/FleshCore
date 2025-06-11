using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.Linq;



public class PathfindingManager : MonoBehaviour
{
	[SerializeField] private int gridSize = 100;
	[SerializeField] private float cellSize = 1.0f;

    [SerializeField] private bool drawDebug = false;

	[SerializeField] private Tilemap mainTilemap;
    [SerializeField] private float _maxTime = 1f;


    private float[,] costArr;
    private float[,] integratedArr;
    private float[,] occupiedArr;

    private List<Vector2Int>[,] vectorArr;
    private bool recalcVectorField = false;
    
    private static PathfindingManager _instance;

    private float _counter = 0f;

    public static PathfindingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PathfindingManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(PathfindingManager).Name);
                    _instance = singleton.AddComponent<PathfindingManager>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        costArr = new float[gridSize,gridSize];
        integratedArr = new float[gridSize,gridSize];
        occupiedArr = new float[gridSize,gridSize];

        GenerateCostField();
        IntegrateCostField();
        CalculateVectorField();

    }

    public List<Vector2Int> GetNextMoves(int x, int y){
        return(vectorArr[x,y]);
    }

    void Update(){
        // _counter += Time.deltaTime;
        // if(_counter >= _maxTime){
        //     _counter = 0f;
        //     IntegrateCostField();
        //     CalculateVectorField();            
        //     recalcVectorField = false;
        // }
    }

    private void GenerateCostField(){
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 tilePos = new Vector3(x * cellSize, y * cellSize, 0f);
                Vector3Int cellPosition = mainTilemap.WorldToCell(tilePos);

                if (mainTilemap.GetTile(cellPosition) == null)
                {
                    costArr[x,y] = 1f;
                }else{
                    costArr[x,y] = 999f;
                }
            }
        }
    }

    private void IntegrateCostField(){
        List<Vector2Int> posToCheck = new List<Vector2Int>();
        float[,] newIntArr = new float[gridSize,gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if(costArr[x,y] == 0f){
                    posToCheck.Add(new Vector2Int(x,y));
                }
                if(costArr[x,y] == 999f){
                    newIntArr[x,y] = 999f;
                }
            }
        }
		List<Vector2Int> directions = new List<Vector2Int>(){ Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };
        while(posToCheck.Count != 0){
			var checking = posToCheck[0];
			posToCheck.RemoveAt(0);

			foreach(var direction in directions){
				var checkingDir = checking + direction;
				if((checkingDir.x < 0 || checkingDir.y < 0 ) || (checkingDir.x >= gridSize || checkingDir.y >= gridSize)){
					continue;
				}else{
					if(newIntArr[checkingDir.x, checkingDir.y] == 0 &&
                        costArr[checkingDir.x, checkingDir.y] != 0
                    ){
                        if(costArr[checkingDir.x, checkingDir.y] == 999f){
						    newIntArr[checkingDir.x, checkingDir.y] = costArr[checkingDir.x, checkingDir.y] ;
                        }else{
                            newIntArr[checkingDir.x, checkingDir.y] = newIntArr[checking.x, checking.y] + costArr[checkingDir.x, checkingDir.y] ;
						    posToCheck.Add(checkingDir);
                        }
					}
				}
			}
		}
        integratedArr = newIntArr;
    }


    private void CalculateVectorField(){
        List<Vector2Int>[,] newVArr = new List<Vector2Int>[gridSize,gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                List<Vector2Int> directions = new List<Vector2Int>(){ 
                    Vector2Int.left,
                    Vector2Int.right,
                    Vector2Int.down,
                    Vector2Int.up,
                    new Vector2Int(1,1),
                    new Vector2Int(1,-1),
                    new Vector2Int(-1,1),
                    new Vector2Int(-1,-1)
                };
                float minValue = integratedArr[x,y];
    			var checking = new Vector2Int(x,y);
                List<Vector2Int> directionsToRemove = new List<Vector2Int>();
                foreach(var direction in directions){
                    var checkingDir = checking + direction;
                    if((checkingDir.x < 0 || checkingDir.y < 0 ) || (checkingDir.x >= gridSize || checkingDir.y >= gridSize)){
                        continue;
                    }else{
                        if(integratedArr[checkingDir.x, checkingDir.y] >= minValue){
                            // directions.Remove(direction);
                            directionsToRemove.Add(direction);
                        }
                    }
			    }
                 foreach (var direction in directionsToRemove)
                {
                    directions.Remove(direction);
                }

                newVArr[x, y] = directions;
                // newVArr[x,y] = directions;

            }
        }
        vectorArr = newVArr;
        
    }

    public void ChangeNode(Vector3 pos, float value){
        Vector3Int cellPosition = mainTilemap.WorldToCell(pos);
        costArr[cellPosition.x, cellPosition.y] = value;
        // RecalcVectorField();
    }
    
    public void SetOccupied(Vector3 pos, float value){
        Vector3Int cellPosition = mainTilemap.WorldToCell(pos);
        // Debug.Log("aaa");
        occupiedArr[cellPosition.x, cellPosition.y] = value;
    }
    public float GetOccupied(Vector3 pos){
        Vector3Int cellPosition = mainTilemap.WorldToCell(pos);
        return occupiedArr[cellPosition.x, cellPosition.y];
    }

    public float GetNodeCost(Vector3Int position){
        // Vector3Int cellPosition = mainTilemap.WorldToCell(position);
        return costArr[position.x, position.y];    
    }

    public float GetIntegratedCost(Vector3 pos){
        Vector3Int cellPosition = mainTilemap.WorldToCell(pos);
		
        return integratedArr[cellPosition.x, cellPosition.y];
    }

    void OnDrawGizmos()
    {
        if(!drawDebug){
            return;
        }
        Gizmos.color = Color.grey;

        for (int x = 0; x <= gridSize; x++)
        {
            Vector3 startPos = new Vector3(x * cellSize, 0, 0);
            Vector3 endPos = new Vector3(x * cellSize,gridSize * cellSize, 0 );
            Gizmos.DrawLine(startPos, endPos);
        }

        for (int y = 0; y <= gridSize; y++)
        {
            Vector3 startPos = new Vector3(0, y * cellSize,0 );
            Vector3 endPos = new Vector3(gridSize * cellSize,y * cellSize, 0);
            Gizmos.DrawLine(startPos, endPos);
        }
         // Draw numbers inside each cell
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.yellow;
        labelStyle.fontSize = 9;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector3 cellCenter = new Vector3(x * cellSize + (cellSize / 2), y * cellSize + (cellSize / 2), 0);
                Handles.Label(cellCenter, $"{occupiedArr[x,y]}", labelStyle);
            }
        }

        // Gizmos.color = Color.yellow;
        // for (int x = 0; x < gridSize; x++)
        // {
        //     for (int y = 0; y < gridSize; y++)
        //     {   
		// 		Vector3 tilePos = new Vector3(x, y, 0f);
		// 		Vector3Int cellPosition = mainTilemap.WorldToCell(tilePos);
        //         if(mainTilemap.GetTile(cellPosition) == null){
        //             var arrowRotation = 0f;
        //             Vector3 cellCenter = new Vector3(x * cellSize + (cellSize / 2), y * cellSize + (cellSize / 2), 0);
        //             Vector3 arrowShaftEnd = cellCenter + Quaternion.Euler(0, 0, arrowRotation) *Vector3.up * (cellSize * 0.4f);
        //             Gizmos.DrawLine(cellCenter, arrowShaftEnd);

        //             Vector3 arrowHeadLeft = arrowShaftEnd + Quaternion.Euler(0, 0, arrowRotation + 225) * Vector3.right * 0.2f;
        //             Vector3 arrowHeadRight = arrowShaftEnd + Quaternion.Euler(0, 0, arrowRotation + 315) * Vector3.right * 0.2f;
        //             Gizmos.DrawLine(arrowShaftEnd, arrowHeadLeft);
        //             Gizmos.DrawLine(arrowShaftEnd, arrowHeadRight);
        //         }
        //     }
        // }
    }
   
}
