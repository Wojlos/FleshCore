using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class BuildingSelectionTooltip : MonoBehaviour
{
    [Header("Section Containers")]
    [SerializeField] private Transform _costContainer;
    [SerializeField] private Transform _upkeepContainer;
    [SerializeField] private Transform _productionContainer;

    [Header("Description")]
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Prefab")]
    [SerializeField] private GameObject _iconTextElementPrefab;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private float _hoverTimer;
    private float _hoverDelay;
    private bool _isWaiting;
    private bool _isVisible;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _hoverDelay = GameManager.Instance.GetGameSettings().tooltipHoverDelay;
        SetVisible(false);
    }

    public void Show(BuildingSettings settings)
    {
        PopulateSection(_costContainer, settings.buildCost);
        PopulateSection(_upkeepContainer, settings.upkeep);
        PopulateProduction(settings);

        if (_descriptionText != null)
            _descriptionText.text = settings.description;

        _hoverTimer = 0f;
        _isWaiting = true;
        _isVisible = false;
        SetVisible(false);
    }

    public void Hide()
    {
        _isWaiting = false;
        _isVisible = false;
        SetVisible(false);
    }

    private void Update()
    {
        if (_isWaiting)
        {
            _hoverTimer += Time.unscaledDeltaTime;
            if (_hoverTimer >= _hoverDelay)
            {
                _isWaiting = false;
                _isVisible = true;
                SetVisible(true);
            }
        }

        if (_isVisible)
        {
            FollowMouse();
        }
    }

    private void SetVisible(bool visible)
    {
        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    private void FollowMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        _rectTransform.position = mousePos;
    }

    private void PopulateSection(Transform container, List<ResourceCost> resources)
    {
        ClearContainer(container);

        foreach (var rc in resources)
        {
            Sprite icon = ResourceManager.Instance.GetResourceIcon(rc.type);
            var element = Instantiate(_iconTextElementPrefab, container).GetComponent<IconTextElement>();
            element.Setup(icon, rc.cost.ToString());
        }
    }

    private void PopulateProduction(BuildingSettings settings)
    {
        ClearContainer(_productionContainer);

        foreach (var rc in settings.production)
        {
            Sprite icon = ResourceManager.Instance.GetResourceIcon(rc.type);
            var element = Instantiate(_iconTextElementPrefab, _productionContainer).GetComponent<IconTextElement>();
            element.Setup(icon, rc.cost.ToString());
        }

        if (settings.unitProductionPerSecond > 0)
        {
            Sprite icon = ResourceManager.Instance.GetUnitsIcon();
            var element = Instantiate(_iconTextElementPrefab, _productionContainer).GetComponent<IconTextElement>();
            element.Setup(icon, settings.unitProductionPerSecond.ToString());
        }

        if (settings.maxUnitsIncrease > 0)
        {
            Sprite icon = ResourceManager.Instance.GetUnitsIcon();
            var element = Instantiate(_iconTextElementPrefab, _productionContainer).GetComponent<IconTextElement>();
            element.Setup(icon, "+" + settings.maxUnitsIncrease.ToString());
        }

        if (settings.type == BuildingType.BloodPump)
        {
            int maxBuildingIncrease = GameManager.Instance.GetGameSettings().pumpMaxBuildingIncrease;
            Sprite icon = ResourceManager.Instance.GetMaxBuildingsIcon();
            var element = Instantiate(_iconTextElementPrefab, _productionContainer).GetComponent<IconTextElement>();
            element.Setup(icon, "+" + maxBuildingIncrease.ToString());
        }
    }

    private void ClearContainer(Transform container)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var child = container.GetChild(i);
            if (child.GetComponent<IconTextElement>() != null)
                Destroy(child.gameObject);
        }
    }
}
