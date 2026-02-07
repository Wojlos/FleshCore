using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
public class InputHandler : MonoBehaviour
{

    private Camera _mainCamera;


    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        var rayHit = Physics2D.GetRayIntersection(_mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));
        if (!rayHit.collider) {
            GameManager.Instance.ClickedAnywhere();   
            return;
        }
        var go = rayHit.collider.gameObject;


        if (go.TryGetComponent<BaseNode>(out var baseNode))
        {
            GameManager.Instance.ClickedOnNode(baseNode);
        }
        else
        {

            GameManager.Instance.ClickedAnywhere();   
        }
        
    }
}