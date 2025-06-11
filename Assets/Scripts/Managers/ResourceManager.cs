using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ResourceType
    {
        Blood,
        Scabs,
        Lipids,
        Bone
    }

[System.Serializable]
public class Resource
{
    public ResourceType resourceType;
    public int initialAmount;
    public Color color;
    public Sprite sprite;

    [HideInInspector]
    public TextMeshProUGUI uiText;
    [HideInInspector]
    public int currentProduction;
    [HideInInspector]
    public int currentUsage;

    public void UpdateUI()
    {
        if (uiText != null)
        {
            uiText.text = $"{(currentProduction - currentUsage).ToString()}/{currentProduction.ToString()}";
        }
    }
}

public class ResourceManager : MonoBehaviour
{
    private static ResourceManager _instance;

    [SerializeField]
    private int initialBlood = 0;
    [SerializeField]
    private int initialScabs = 0;
    [SerializeField]
    private int initialLipids = 0;
    
    [SerializeField]
    private GameObject resourceUIPrefab;
    [SerializeField]
    private List<Resource> resources = new List<Resource>();
    [SerializeField]
    private Transform gridLayout;

    private Dictionary<ResourceType, Resource> resourceDictionary;

    public static ResourceManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ResourceManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(ResourceManager).Name);
                    _instance = singleton.AddComponent<ResourceManager>();
                    DontDestroyOnLoad(singleton);
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeResourceDictionary();
        PopulateResourceUI();
    }

    private void InitializeResourceDictionary()
    {
        resourceDictionary = new Dictionary<ResourceType, Resource>();
        foreach (var resource in resources)
        {
            resource.currentProduction = resource.initialAmount;
            resource.currentUsage = 0;
            resourceDictionary[resource.resourceType] = resource;
        }
    }

    private void PopulateResourceUI()
    {
        foreach (var resource in resources)
        {
            CreateResourceUI(resource);
        }
    }

    private void CreateResourceUI(Resource resource)
    {
        GameObject resourceUI = Instantiate(resourceUIPrefab, gridLayout);

        Image imageComponent = resourceUI.GetComponentInChildren<Image>();
        imageComponent.color = resource.color;

        TextMeshProUGUI textComponent = resourceUI.GetComponentInChildren<TextMeshProUGUI>();
        textComponent.text = resource.currentProduction.ToString();
        resource.uiText = textComponent;
        resource.UpdateUI();

    }

    public void IncrementResourceProduction(ResourceType type, int amount)
    {   
        if(resourceDictionary == null) return;
        
        
        if (resourceDictionary.ContainsKey(type))
        {
            resourceDictionary[type].currentProduction += amount;
            resourceDictionary[type].UpdateUI();
        }
    }

    public void IncrementResourceUsage(ResourceType type, int amount)
    {
        
        if (resourceDictionary.ContainsKey(type))
        {
            resourceDictionary[type].currentUsage += amount;
            resourceDictionary[type].UpdateUI();
        }
    }
    
    public int GetResourceAmount(ResourceType type)
    {
        if (resourceDictionary.ContainsKey(type))
        {
            return resourceDictionary[type].currentProduction;
        }
        return 0; 
    }

    public Resource GetResourceOfType(ResourceType type){
        foreach(Resource r in resources){
            if(r.resourceType == type){
                return r;
            }
        }
        return null;
    }

    public void AddResource(ResourceType type, int initialAmount, Color color)
    {
        if (!resourceDictionary.ContainsKey(type))
        {
            Resource newResource = new Resource { resourceType = type, initialAmount = initialAmount, color = color, currentProduction = initialAmount };
            resources.Add(newResource);
            resourceDictionary[type] = newResource;
            CreateResourceUI(newResource);
        }
    }

    public void DeleteResource(ResourceType type)
    {
        if (resourceDictionary.ContainsKey(type))
        {
            Resource resourceToRemove = resourceDictionary[type];
            resources.Remove(resourceToRemove);
            resourceDictionary.Remove(type);

            Destroy(resourceToRemove.uiText.transform.parent.gameObject);
        }
    }
}