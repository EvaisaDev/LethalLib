using System;
using System.Collections.Generic;
using System.Text;
using static LethalLib.Modules.Items;
using static LethalLib.Plugin;
using Unity.Netcode;
using UnityEngine;

namespace LethalLib.Modules
{
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
            if(!_networkPrefabs.Contains(prefab))
                _networkPrefabs.Add(prefab);
        }

        private static void GameNetworkManager_Start(On.GameNetworkManager.orig_Start orig, GameNetworkManager self)
        {
            orig(self);

            foreach (GameObject obj in _networkPrefabs)
            {
                if(!NetworkManager.Singleton.NetworkConfig.Prefabs.Contains(obj))
                    NetworkManager.Singleton.AddNetworkPrefab(obj);
            }
            
        }
    }
}
