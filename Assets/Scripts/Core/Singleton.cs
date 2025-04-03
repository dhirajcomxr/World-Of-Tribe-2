
using UnityEngine;
using Unity.Netcode;

public abstract class Singleton<T> : NetworkBehaviour
                where T : Component
{
    private static T _Instance = default;
    public static T Instance
    {
        get
        {
            if (_Instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                    _Instance = objs[0];
                if (objs.Length > 0)
                {
                    Debug.Log($"There is more then one {typeof(T).Name} in the scene.");
                }
                if (_Instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = string.Format("_{0}", typeof(T).Name);
                    _Instance = obj.AddComponent<T>();
                }
            }

            return _Instance;
        }
    }
}

