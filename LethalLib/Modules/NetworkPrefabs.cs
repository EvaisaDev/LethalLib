#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace LethalLib.Modules;

public class NetworkPrefabs
{


    private static List<GameObject> _networkPrefabs = new List<GameObject>();
    internal static void Init()
    {
        On.GameNetworkManager.Start += GameNetworkManager_Start;
    }

    /// <summary>
    /// Registers a prefab to be added to the network manager.
    /// </summary>
    public static void RegisterNetworkPrefab(GameObject prefab)
    {
        if (prefab is null)
            throw new ArgumentNullException(nameof(prefab), $"The given argument for {nameof(RegisterNetworkPrefab)} is null!");
        if (!_networkPrefabs.Contains(prefab))
            _networkPrefabs.Add(prefab);
    }

    /// <summary>
    /// Creates a network prefab programmatically and registers it with the network manager.
    /// Credit to Day and Xilo.
    /// </summary>
    public static GameObject CreateNetworkPrefab(string name)
    {
        var prefab = PrefabUtils.CreatePrefab(name);
        prefab.AddComponent<NetworkObject>();

        var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + name));

        prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);

        RegisterNetworkPrefab(prefab);
        return prefab;
    }

    /// <summary>
    /// Clones a network prefab programmatically and registers it with the network manager.
    /// Credit to Day and Xilo.
    /// </summary>
    public static GameObject CloneNetworkPrefab(GameObject prefabToClone, string newName = null)
    {
        var prefab = PrefabUtils.ClonePrefab(prefabToClone, newName);

        var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetCallingAssembly().GetName().Name + prefab.name));

        prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = BitConverter.ToUInt32(hash, 0);

        RegisterNetworkPrefab(prefab);
        return prefab;
    }


    private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
    {
        orig(self);

        foreach (GameObject obj in _networkPrefabs)
        {
            if (!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(obj))
                NetworkManager.Singleton.AddNetworkPrefab(obj);
        }

    }
}
