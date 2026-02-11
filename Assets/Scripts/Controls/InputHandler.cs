using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InputHandler : MonoBehaviour
{
    private Camera _mainCamera;

    [Header("Selection Box UI")]
    [SerializeField] private RectTransform selectionBox; // Assign SelectionBox RectTransform (UI Image)
    [SerializeField] private Canvas canvas;              // Assign the Canvas that contains SelectionBox

    [Header("Drag Select")]
    [SerializeField] private float dragThresholdPixels = 8f;

    private bool _isPointerDown;
    private bool _isDragging;
    private Vector2 _dragStartScreen;
    private Rect _dragRectScreen;

    private void Awake()
    {
        _mainCamera = Camera.main;

        // Safe default: keep it hidden on start
        if (selectionBox != null)
            selectionBox.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!_isPointerDown) return;
        if (Mouse.current == null) return;
        Vector2 current = Mouse.current.position.ReadValue();

        if (!_isDragging)
        {
            if (Vector2.Distance(current, _dragStartScreen) >= dragThresholdPixels)
            {
                _isDragging = true;
                ShowSelectionBox(_dragStartScreen);
            }
        }

        if (_isDragging)
        {
            _dragRectScreen = MakeScreenRect(_dragStartScreen, current);
            UpdateSelectionBox(_dragRectScreen);
        }
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && !_isPointerDown)
            return;
        
        if (Mouse.current == null) return;
        HandleSingleClick(false);
        
    }
    public void OnClick(InputAction.CallbackContext context)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject() && !_isPointerDown)
            return;

        if (Mouse.current == null) return;

        if (context.started)
        {
            _isPointerDown = true;
            _isDragging = false;
            _dragStartScreen = Mouse.current.position.ReadValue();
            HideSelectionBox();
            return;
        }

        if (context.canceled)
        {
            _isPointerDown = false;

            if (_isDragging)
            {
                Vector2 end = Mouse.current.position.ReadValue();
                _dragRectScreen = MakeScreenRect(_dragStartScreen, end);

                GameManager.Instance.SelectUnitsInArea(_dragRectScreen);
                _isDragging = false;
                HideSelectionBox();
                return;
            }

            HideSelectionBox();
            HandleSingleClick();
        }
    }

    private void HandleSingleClick(bool left = true)
    {
        var rayHit = Physics2D.GetRayIntersection(
            _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue())
        );

        if (!rayHit.collider)
        {
            GameManager.Instance.ClickedAnywhere();
            return;
        }

        var go = rayHit.collider.gameObject;

        if (go.TryGetComponent<BaseNode>(out var baseNode))
        {
            if (left)
            {
                GameManager.Instance.ClickedOnNode(baseNode);
            }
            else
            {
                GameManager.Instance.RightClickedOnNode(baseNode);
            }
        }
        else if(go.TryGetComponent<UnitCounter>(out var unitCounter) && left)
        {
            GameManager.Instance.ClickedOnUnitCounter(unitCounter);
        }else
        {
            GameManager.Instance.ClickedAnywhere();
        }

    }

    private static Rect MakeScreenRect(Vector2 a, Vector2 b)
    {
        float xMin = Mathf.Min(a.x, b.x);
        float xMax = Mathf.Max(a.x, b.x);
        float yMin = Mathf.Min(a.y, b.y);
        float yMax = Mathf.Max(a.y, b.y);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private void ShowSelectionBox(Vector2 startScreen)
    {
        if (selectionBox == null || canvas == null) return;

        selectionBox.gameObject.SetActive(true);
        UpdateSelectionBox(MakeScreenRect(startScreen, startScreen));
    }

    private void HideSelectionBox()
    {
        if (selectionBox == null) return;
        selectionBox.gameObject.SetActive(false);
    }

    private void UpdateSelectionBox(Rect screenRect)
    {
        if (selectionBox == null || canvas == null) return;

        RectTransform canvasRect = canvas.transform as RectTransform;

        // For Screen Space Overlay, camera should be null. Otherwise use canvas.worldCamera.
        Camera uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, new Vector2(screenRect.xMin, screenRect.yMin), uiCam, out Vector2 localMin);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, new Vector2(screenRect.xMax, screenRect.yMax), uiCam, out Vector2 localMax);

        // IMPORTANT: Set your SelectionBox Image pivot to (0,0) in the Inspector (bottom-left)
        selectionBox.anchoredPosition = localMin;
        selectionBox.sizeDelta = (localMax - localMin);
    }
}
