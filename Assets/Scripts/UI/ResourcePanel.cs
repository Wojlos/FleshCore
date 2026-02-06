using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ResourcePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _currentText;
    [SerializeField] private TextMeshProUGUI _perSecondText;
    [SerializeField] private Image _resourceImage;

    private int _current = 0;
    private int _perSecond = 0;

    public void SetupPanel(Sprite sprite)
    {
        _resourceImage.sprite = sprite;
        UpdateText();
    }
    
    public void ChangeCurrent(int c)
    {
        _current += c;
        UpdateText();
    }

    public void ChangePerSecond(int c)
    {
        _perSecond += c;
        UpdateText();
    }

    private void UpdateText()
    {
        _perSecondText.text = ""+_perSecond;
        _currentText.text = ""+_current;

    }

    public int GetCurrent()=> _current;

}
