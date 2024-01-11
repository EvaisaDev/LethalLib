#region

using LethalLib.Modules;
using UnityEngine;

#endregion

namespace LethalLib.Extras
{
    [CreateAssetMenu(menuName = "ScriptableObjects/WeatherDef")]
    public class WeatherDef : ScriptableObject
    {
        public string weatherName;
        public Levels.LevelTypes levels = Levels.LevelTypes.None;
        public string[] levelOverrides;
        public int weatherVariable1;
        public int weatherVariable2;
        public WeatherEffect weatherEffect;

    }
}
