#region

using LethalLib.Modules;
using UnityEngine;

#endregion

namespace LethalLib.Extras
{
    [CreateAssetMenu(menuName = "ScriptableObjects/UnlockableItem")]
    public class UnlockableItemDef : ScriptableObject
    {
        // storeType is not really used, but it is still here for compatibility.
        public StoreType storeType = StoreType.None;
        public UnlockableItem unlockable;
    }
}
