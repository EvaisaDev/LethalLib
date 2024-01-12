#region

using System;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace LethalLib.Modules;

public class PrefabUtils
{
    internal static Lazy<GameObject> _prefabParent;
    internal static GameObject prefabParent { get { return _prefabParent.Value; } }

    static PrefabUtils()
    {
        _prefabParent = new Lazy<GameObject>(() =>
        {
            var parent = new GameObject("LethalLibGeneratedPrefabs");
            parent.hideFlags = HideFlags.HideAndDontSave;
            parent.SetActive(false);

            return parent;
        });
    }

    /// <summary>
    /// Clones a prefab and returns the clone.
    /// </summary>
    public static GameObject ClonePrefab(GameObject prefabToClone, string newName = null)
    {
        var prefab = Object.Instantiate(prefabToClone, prefabParent.transform);
        prefab.hideFlags = HideFlags.HideAndDontSave;

        if (newName != null)
        {
            prefab.name = newName;
        }
        else
        {
            prefab.name = prefabToClone.name;
        }

        return prefab;
    }

    /// <summary>
    /// Creates a prefab and returns it.
    /// </summary>
    public static GameObject CreatePrefab(string name)
    {
        var prefab = new GameObject(name);
        prefab.hideFlags = HideFlags.HideAndDontSave;

        prefab.transform.SetParent(prefabParent.transform);

        return prefab;
    }
}