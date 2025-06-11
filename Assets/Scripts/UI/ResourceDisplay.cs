using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceDisplay : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _uiText;
    [SerializeField] private Image _image;

    public void SetupDisplay(Sprite sprite, float value){
        _uiText.text = value.ToString();
        _image.sprite = sprite;
    }
}
