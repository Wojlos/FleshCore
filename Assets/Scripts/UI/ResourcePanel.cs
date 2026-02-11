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
    private float _updateTimer = 0f;

    private void Update() {
        _updateTimer += Time.deltaTime;
        if(_updateTimer >= 1f)
        {
            _updateTimer = 0f;
            UpdateText();
        }
    }
    public void SetupPanel(Sprite sprite)
    {
        _resourceImage.sprite = sprite;
        UpdateText();
    }
    
    public void ChangeCurrent(int c)
    {
        _current += c;
    }

    public void ChangePerSecond(int c)
    {
        _perSecond += c;
    }

    private void UpdateText()
    {
        _perSecondText.text = ""+_perSecond;
        _currentText.text = ""+_current;

    }

    public int GetCurrent()=> _current;

}
