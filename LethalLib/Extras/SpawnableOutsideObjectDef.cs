using LethalLib.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLib.Extras
{
    [CreateAssetMenu(menuName = "ScriptableObjects/SpawnableOutsideObject")]
    public class SpawnableOutsideObjectDef : ScriptableObject
    {
        public SpawnableOutsideObjectWithRarity spawnableMapObject;
    }
}
