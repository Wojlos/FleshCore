using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class BuildingButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image _iconImage;
    private Button _button;
    private BuildingType _buildingType;
    private List<ResourceCost> _buildCost = new List<ResourceCost>();
    private List<ResourceCost> _upkeep = new List<ResourceCost>();
    private BuildingSettings _settings;
    private BuildingSelectionTooltip _tooltip;

    private static GameObject _rangeCircle;
    private const int CircleSegments = 64;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_iconImage == null)
            _iconImage = GetComponent<Image>();
    }

    public void SetupBuilding(BuildingSettings settings)
    {
        if (_button == null)
            _button = GetComponent<Button>();

        if (_iconImage == null)
            _iconImage = GetComponent<Image>();

        _buildingType = settings.type;

        if (_iconImage != null)
            _iconImage.sprite = settings.sprite;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();

            _button.onClick.AddListener(() =>
            {
                BuildingSelectionManager.Instance.SelectBuilding(_buildingType.ToString());
            });
        }

        _buildCost = settings.buildCost;
        _upkeep = settings.upkeep;
        _settings = settings;
    }

    public List<ResourceCost> GetCost() => _buildCost;
    public List<ResourceCost> GetUpkeep() => _upkeep;
    public BuildingType GetBuildingType() => _buildingType;

    public void Enable()
    {
        _button.interactable = true;
    }

    public void Disable()
    {
        _button.interactable = false;
    }

    public void SetTooltip(BuildingSelectionTooltip tooltip)
    {
        _tooltip = tooltip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_tooltip != null && _settings != null)
            _tooltip.Show(_settings);

        if (_buildingType == BuildingType.Harvester)
            ShowGatheringRange();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip != null)
            _tooltip.Hide();

        HideGatheringRange();
    }

    private void ShowGatheringRange()
    {
        var mapSettings = TilemapsManager.Instance?.GetMapSettings();
        if (mapSettings == null || !mapSettings.showGatheringRangeOnHover) return;

        BaseNode selectedNode = GameManager.Instance.GetSelectedNode();
        if (selectedNode == null) return;

        HideGatheringRange();

        _rangeCircle = new GameObject("GatheringRangeCircle");
        _rangeCircle.transform.position = selectedNode.transform.position;

        var lr = _rangeCircle.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = CircleSegments;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = mapSettings.gatheringRangeVisualColor;
        lr.endColor = mapSettings.gatheringRangeVisualColor;

        float radius = mapSettings.resourceGatheringRange;
        Vector3 center = selectedNode.transform.position;

        for (int i = 0; i < CircleSegments; i++)
        {
            float angle = (float)i / CircleSegments * Mathf.PI * 2f;
            lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    public static void HideGatheringRange()
    {
        if (_rangeCircle != null)
        {
            Destroy(_rangeCircle);
            _rangeCircle = null;
        }
    }
}