#region

using UnityEngine;

#endregion

namespace LethalLib.Extras;

[CreateAssetMenu(menuName = "ScriptableObjects/SpawnableMapObject")]
public class SpawnableMapObjectDef : ScriptableObject
{
    public SpawnableMapObject spawnableMapObject;
}