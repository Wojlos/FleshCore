using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private static EnemyManager _instance;

    public static EnemyManager Instance;

    private List<GameObject> _enemies = new List<GameObject>();

    void Awake(){
        if (Instance != null)
        {
            Debug.LogError("There is more then one EnemyManager on the Scene");
            return;
        }
        Instance = this;
    }

    public void AddEnemy(GameObject e){
        _enemies.Add(e);
    
    }

    public void SubEnemy(GameObject e){
        _enemies.Remove(e);
    }

    public GameObject GetClosestEnemyInRange(Vector3 pos, float range){
        GameObject closest = null;
        float minDistance = range;

        foreach(var e in _enemies){
            float distance = Vector3.Distance(e.transform.position, pos);
            if(distance < minDistance){
                closest = e;
            }
        }
        return closest;
    }
}
