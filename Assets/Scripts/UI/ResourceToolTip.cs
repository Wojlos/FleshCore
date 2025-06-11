using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceToolTip : MonoBehaviour
{
    private Dictionary<ResourceType, float> _resources = new Dictionary<ResourceType, float>();
    [SerializeField] private RectTransform _gridLayout;
    [SerializeField] private GameObject _resourceDisplayPrefab; 

    private List<GameObject> _displayed = new List<GameObject>();

    void Start()
    {
        CreateResourceUI();
    }

    private void CreateResourceUI()
    {
        var res = _resources.Count;
        var height = 0f;
        
        if(res > 0){
            height = 5 + (res*25);
        }

        _gridLayout.sizeDelta = new Vector2 (_gridLayout.sizeDelta.x, height);;

        foreach(KeyValuePair<ResourceType, float> entry in _resources){
            GameObject resourceUI = Instantiate(_resourceDisplayPrefab, _gridLayout);
            _displayed.Add(resourceUI);
            resourceUI.GetComponent<ResourceDisplay>().SetupDisplay(ResourceManager.Instance.GetResourceOfType(entry.Key).sprite, entry.Value);
        }
    }

    private void ClearDisplay(){
        for(int i = 0; i<_displayed.Count; i++){
            Destroy(_displayed[i]);
        }
        _displayed.Clear();
    }

    public void SetResources(Dictionary<ResourceType, float> newResources){
        ClearDisplay();
        _resources.Clear();
        foreach(KeyValuePair<ResourceType, float> entry in newResources){
            _resources[entry.Key] = entry.Value;
        }
        CreateResourceUI();
    }
}
