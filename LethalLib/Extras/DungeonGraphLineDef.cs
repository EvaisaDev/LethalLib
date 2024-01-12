#region

using DunGen.Graph;
using UnityEngine;

#endregion

namespace LethalLib.Extras;

[CreateAssetMenu(menuName = "ScriptableObjects/DungeonGraphLine")]
public class DungeonGraphLineDef : ScriptableObject
{
    public GraphLine graphLine;
}