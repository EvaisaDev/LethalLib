#region

using UnityEngine;

#endregion

namespace LethalLib.Extras;

[CreateAssetMenu(menuName = "ScriptableObjects/SpawnableOutsideObject")]
public class SpawnableOutsideObjectDef : ScriptableObject
{
    public SpawnableOutsideObjectWithRarity spawnableMapObject;
}