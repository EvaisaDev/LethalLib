#region

using DunGen.Graph;
using UnityEngine;

#endregion

namespace LethalLib.Extras;

[CreateAssetMenu(menuName = "ScriptableObjects/DungeonDef")]
public class DungeonDef : ScriptableObject
{
    public DungeonFlow dungeonFlow;
    [Range(0f, 300f)]
    public int rarity;
    public AudioClip firstTimeDungeonAudio;
}