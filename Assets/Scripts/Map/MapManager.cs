using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

}
