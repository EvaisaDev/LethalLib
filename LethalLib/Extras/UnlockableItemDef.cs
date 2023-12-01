using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static LethalLib.Modules.Unlockables;

namespace LethalLib.Extras
{
    [CreateAssetMenu(menuName = "ScriptableObjects/UnlockableItem")]
    public class UnlockableItemDef : ScriptableObject
    {
        public StoreType storeType = StoreType.None;
        public UnlockableItem unlockable;
    }
}
