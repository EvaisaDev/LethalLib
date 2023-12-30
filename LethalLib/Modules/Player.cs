using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLib.Modules
{
    public class Player
    {
        public static Dictionary<string, GameObject> ragdollRefs = new Dictionary<string, GameObject>();
        public static Dictionary<string, int> ragdollIndexes = new Dictionary<string, int>();

        public static void Init()
        {
            On.StartOfRound.Awake += StartOfRound_Awake;
        }

        private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
        {
            orig(self);

            // loop through ragdollrefs
            foreach (KeyValuePair<string, GameObject> ragdollRef in ragdollRefs)
            {
                self.playerRagdolls.Add(ragdollRef.Value);
                // get index
                int index = self.playerRagdolls.Count - 1;
                // add to ragdollIndexes
                ragdollIndexes.Add(ragdollRef.Key, index);
            }
        }

        public static int GetRagdollIndex(string id)
        {
            return ragdollIndexes[id];
        }

        public static GameObject GetRagdoll(string id)
        {
            return ragdollRefs[id];
        }

        // custom player ragdolls for special deaths.
        public static void RegisterPlayerRagdoll(string id, GameObject ragdoll, bool doNetworkRegistry = false)
        {
            NetworkPrefabs.RegisterNetworkPrefab(ragdoll);
            ragdollRefs.Add(id, ragdoll);
        }
    }
}
