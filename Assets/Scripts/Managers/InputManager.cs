using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;

    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InputManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(InputManager).Name);
                    _instance = singleton.AddComponent<InputManager>();
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
    }

    [SerializeField] private InputActionReference point, click, move;
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject mainTmContainer;
    [SerializeField] private GameObject movingObject;
    [SerializeField] private float camMovementFactor = 0.1f;
    [SerializeField] private float _snapToVeinDist = 3f;


    private GameObject attachedStructure;
    private GameObject attachedStructureView;

    private Vector2 _pointInput; 
    private float _clickInput; 
    private Vector2 _moveInput;

    private float _clickCoolDown = 1f;
    private float _clickClock = 0f;

    void Update()
    {   

        _clickClock += Time.deltaTime;
        _pointInput = point.action.ReadValue<Vector2>();
        _clickInput = click.action.ReadValue<float>();
        _moveInput = move.action.ReadValue<Vector2>();
        if(_moveInput != Vector2.zero){
            Vector3 currentPos = movingObject.transform.position;
            currentPos.x += (_moveInput.x * camMovementFactor * Time.deltaTime);
            currentPos.y += (_moveInput.y * camMovementFactor * Time.deltaTime);

            movingObject.transform.position = currentPos;
        }
        if(_clickInput == 1 && attachedStructure != null && _clickClock >= _clickCoolDown && !IsPointerOverUIElement()){
            _clickClock = 0f;
            BuildStructure();
        }
        if (attachedStructureView != null)
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(_pointInput);
            Vector3 gridPosition = GetNearestGridPosition(mouseWorldPosition);
            gridPosition = new Vector3(gridPosition.x+(grid.cellSize.x/2f), gridPosition.y+(grid.cellSize.y/2f), 0f);

            Junction closestJunction = RoadManager.Instance.GetClosestJunction(gridPosition);

            if(closestJunction != null){
                Vector3 closestJunctionPos = closestJunction.transform.position;
                float distToJunction = Vector3.Distance(gridPosition, closestJunctionPos);
                
                if(distToJunction <= _snapToVeinDist){
                    gridPosition = closestJunctionPos;
                }else{
                    Vector3 closestVein = RoadManager.Instance.FindClosestSplinePosition(gridPosition);

                    float distToVein = Vector3.Distance(gridPosition, closestVein); 
                    if( distToVein <= _snapToVeinDist){
                        gridPosition = closestVein;
                    }
                }
            }
            

            attachedStructureView.transform.position = gridPosition;
            if(attachedStructureView.TryGetComponent<Building>(out Building building)){
                building.SetPreview(
                    StructuresManager.Instance.IsPositionClear(
                        attachedStructureView.transform.position,
                        building.GetBuildOnTilemap()
                    )
                );
            }
        }
    }

    private Vector3 GetNearestGridPosition(Vector3 position)
    {
        Vector3Int cellPosition = grid.WorldToCell(position);
        return grid.CellToWorld(cellPosition);
    }

    private bool IsPointerOverUIElement()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public void AttachStructure(GameObject str){
        if(attachedStructure == str){
            ClearStructure();
        }else{
            if(attachedStructure != null){
                ClearStructure();
            }
            attachedStructure = str;
            attachedStructureView = Instantiate(str, Camera.main.ScreenToWorldPoint(_pointInput), Quaternion.identity);
            if(attachedStructureView.TryGetComponent<Building>(out Building building)){
                building.SetPreview(true);
            }else if(attachedStructureView.TryGetComponent<Junction>(out Junction junction)){
                // junction.isBase = true;
                RoadManager.Instance.AddJunction(junction, true);
            }
        }

    }

    public void ClearStructure(){
        if(attachedStructureView.TryGetComponent<Junction>(out Junction junction)){
            RoadManager.Instance.StopBuilding();
        }
        Destroy(attachedStructureView);
        attachedStructure = null;
    }

    private void BuildStructure(){
        if(attachedStructureView.TryGetComponent<Building>(out Building building)){
            if(StructuresManager.Instance.IsPositionClear(
                attachedStructureView.transform.position,
                building.GetBuildOnTilemap())
            ){
                Camera.main.ScreenToWorldPoint(Input.mousePosition);
                GameObject newBuilding = Instantiate(attachedStructure, attachedStructureView.transform.position, Quaternion.identity);
                newBuilding.GetComponentInChildren<Building>().Build();
                StructuresManager.Instance.AddBuilding(newBuilding);
            }
        }else if(attachedStructureView.TryGetComponent<Junction>(out Junction junction)){
            Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // GameObject newBuilding = Instantiate(attachedStructure, attachedStructureView.transform.position, Quaternion.identity);
            RoadManager.Instance.AddPreviewKnot();
        }
    }

    public Grid GetGrid(){
        return grid;
    }

    public GameObject GetTmContainer(){
        return mainTmContainer;
    }
}
