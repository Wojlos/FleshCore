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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_tooltip != null)
            _tooltip.Hide();
    }
}