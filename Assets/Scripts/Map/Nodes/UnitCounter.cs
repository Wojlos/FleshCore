using UnityEngine;
using TMPro;

public class UnitCounter : MonoBehaviour
{
    private TMP_Text _textField;
    public bool _isPlayer = false;
    private bool _isSelected = false;
    private int _selectedUnits = 0;
    private int _totalUnits = 0;

    private SpriteRenderer _spriteRenderer;
    private Color _defaultColor = Color.blue;
    private Color _selectedColor = Color.yellow;
    private BaseNode _node;

     void Awake()
    {
        _textField = GetComponentInChildren<TMP_Text>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
            _defaultColor = _spriteRenderer.color;
    }

    public void ConnectNode(BaseNode n)
    {
        _node = n;
    }
    
    public BaseNode GetConnectedNode() => _node;

    public void SetText(int i)
    {
        _totalUnits = i;
        UpdateText();
    }       
    
    public void SelectUnits()
    {
        if (!_isPlayer) return;

        _isSelected = true;
        _selectedUnits = _totalUnits;
        HighlightSelection();
        UpdateText();
    }

    public void SelectPartialUnits(int selectedCount, int totalCount)
    {
        if (!_isPlayer) return;

        _isSelected = true;
        _selectedUnits = selectedCount;
        _totalUnits = totalCount;
        HighlightSelection();
        UpdateText();
    }

    public void DeselectUnits()
    {
        if (!_isPlayer) return;

        _isSelected = false;
        _selectedUnits = 0;
        HighlightSelection();
        UpdateText();
    }

    private void UpdateText()
    {
        if (_textField == null) return;

        if (_isSelected && _selectedUnits < _totalUnits)
        {
            _textField.text = $"{_selectedUnits}/{_totalUnits}";
        }
        else
        {
            _textField.text = _totalUnits.ToString();
        }
    }

    public int GetSelectedUnits() => _isSelected ? _selectedUnits : 0;
    public int GetTotalUnits() => _totalUnits;

    private void HighlightSelection()
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.color = _isSelected ? _selectedColor : _defaultColor;
    }
}
