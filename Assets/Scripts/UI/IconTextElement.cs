using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IconTextElement : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _text;

    public void Setup(Sprite icon, string text)
    {
        if (_icon != null)
            _icon.sprite = icon;

        if (_text != null)
            _text.text = text;
    }
}
