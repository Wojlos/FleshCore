using UnityEngine;
using TMPro;

public class UnitCounter : MonoBehaviour
{
    private TMP_Text _textField;
    public bool _isPlayer = false;
    private bool _isSelected = false;

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
        _textField.text = i.ToString();
    }       
    public void SelectUnits()
    {
        if (!_isPlayer) return;

        _isSelected = true;
        HighlightSelection();
    }

    public void DeselectUnits()
    {
        if (!_isPlayer) return;

        _isSelected = false;
        HighlightSelection();
    }

    private void HighlightSelection()
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.color = _isSelected ? _selectedColor : _defaultColor;
    }
}
