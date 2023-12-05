using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLib.Extras
{
    [CreateAssetMenu(menuName = "ScriptableObjects/DungeonGraphLine")]
    public class DungeonGraphLineDef : ScriptableObject
    {
        public GraphLine graphLine;
    }
}
