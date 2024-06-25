using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructuresManager : MonoBehaviour
{
    private static StructuresManager _instance;

    public static StructuresManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<StructuresManager>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject(typeof(StructuresManager).Name);
                    _instance = singleton.AddComponent<StructuresManager>();
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

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
