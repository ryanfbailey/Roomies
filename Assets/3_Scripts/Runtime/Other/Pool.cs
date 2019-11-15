using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{
    // Prefabs
    private List<GameObject> _prefabs = new List<GameObject>();

    // Instance items
    private List<GameObject> _instances = new List<GameObject>();
    private List<int> _instancePrefabs = new List<int>();
    private Dictionary<int, List<int>> _available = new Dictionary<int, List<int>>();
    private static bool _destroyed = false;

    // Get instance
    public static Pool instance
    {
        get
        {
            if (_instance == null && Application.isPlaying && !_destroyed)
            {
                _instance = GameObject.FindObjectOfType<Pool>();
                if (_instance == null)
                {
                    _instance = new GameObject("GAMEOBJECT_POOL").AddComponent<Pool>();
                }
            }
            return _instance;
        }
    }
    private static Pool _instance;

    // On awake, deactivate
    private void Awake()
    {
        _destroyed = false;
        gameObject.SetActive(false);
        DontDestroyOnLoad(transform.root.gameObject);
    }
    private void OnDestroy()
    {
        _destroyed = true;
    }

    // Get prefab index from prefab
    private int GetPrefabIndex(GameObject prefab)
    {
        // Ensure prefab exists
        if (prefab == null)
        {
            Debug.LogError("POOL - CANNOT ADD NULL PREFAB");
            return -1;
        }

        // Find index
        int index = _prefabs.IndexOf(prefab);

        // Add to prefab list
        if (index == -1)
        {
            index = _prefabs.Count;
            _prefabs.Add(prefab);
        }

        // Return index
        return index;
    }

    // Load instance with an actual prefab
    public GameObject Load(GameObject prefab)
    {
        // Get index
        int prefabIndex = GetPrefabIndex(prefab.gameObject);
        if (prefabIndex == -1)
        {
            Debug.LogError("POOL - CANNOT LOAD NULL PREFAB: " + prefabIndex);
            return null;
        }

        // The instance to return
        GameObject inst = null;

        // Look for available indices
        if (_available.ContainsKey(prefabIndex))
        {
            List<int> available = _available[prefabIndex];
            if (available.Count > 0)
            {
                int index = available[0];
                inst = _instances[index];
                available.RemoveAt(0);
                _available[prefabIndex] = available;
            }
        }

        // Not found, instantiate
        if (inst == null)
        {
            // Instantiate
            inst = Instantiate<GameObject>(prefab.gameObject);
            inst.gameObject.name = prefab.name;
            inst.transform.SetParent(transform);
            _instances.Add(inst.gameObject);
            _instancePrefabs.Add(prefabIndex);
        }

        // Remove parent
        inst.transform.SetParent(null);

        // Return loaded instance
        return inst;
    }

    // Unload instance
    public void Unload(GameObject inst)
    {
        // Unload instance
        if (inst == null)
        {
            Debug.LogError("POOL - CANNOT UNLOAD NULL INSTANCE");
            return;
        }

        // Get index
        int instIndex = _instances.IndexOf(inst);
        if (instIndex == -1)
        {
            Debug.LogError("POOL - CANNOT UNLOAD INSTANCE NOT INSTANTIATED IN POOL");
            return;
        }

        // Add to available list
        int prefabIndex = _instancePrefabs[instIndex];
        List<int> available = _available.ContainsKey(prefabIndex) ? _available[prefabIndex] : new List<int>();
        available.Add(instIndex);
        _available[prefabIndex] = available;

        // Add into transform
        inst.transform.SetParent(transform);
    }

    // Preload
    public void Preload(GameObject prefab, int count)
    {
        // Get index
        int prefabIndex = GetPrefabIndex(prefab.gameObject);
        if (prefabIndex != -1)
        {
            Debug.LogError("POOL - CANNOT PRELOAD NULL PREFAB: " + prefabIndex);
            return;
        }

        // Load
        List<GameObject> insts = new List<GameObject>();
        for (int c = 0; c < count; c++)
        {
            insts.Add(Load(prefab));
        }

        // Unload
        for (int c = 0; c < count; c++)
        {
            Unload(insts[c]);
        }
    }
}
