using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;

    private Vector2 _moveInput;

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        if (_moveInput == Vector2.zero) return;

        Vector3 move = new Vector3(_moveInput.x, _moveInput.y, 0f);
        transform.position += move * moveSpeed * Time.deltaTime;
    }
}