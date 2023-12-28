using LethalLib.Extras;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LethalLib.Modules.Items;

namespace LethalLib.Modules
{
    public class Weathers
    {
        public class CustomWeather
        {
            public string name;
            public int weatherVariable1;
            public int weatherVariable2;
            public WeatherEffect weatherEffect;
            public Levels.LevelTypes levels;
            public string[] spawnLevelOverrides;

            public CustomWeather(string name, WeatherEffect weatherEffect, Levels.LevelTypes levels = Levels.LevelTypes.None, string[] spawnLevelOverrides = null, int weatherVariable1 = 0, int weatherVariable2 = 0 )
            {
                this.name = name;
                this.weatherVariable1 = weatherVariable1;
                this.weatherVariable2 = weatherVariable2;
                this.weatherEffect = weatherEffect;
                this.levels = levels;
                this.spawnLevelOverrides = spawnLevelOverrides;
            }
        }

        public static Dictionary<int, CustomWeather> customWeathers = new Dictionary<int, CustomWeather>();
        public static int numCustomWeathers = 0;
       // public static Array newWeatherValuesArray;
        //public static string[] newWeatherNamesArray;

        public static void Init()
        {


            //public static Array GetValues(Type enumType);
            /*new Hook(typeof(Enum).GetMethod("GetValues", new Type[]
            {
            typeof(Type)
            }), typeof(Weathers).GetMethod("GetValuesHook"));

            //public static string[] GetNames(Type enumType);
            new Hook(typeof(Enum).GetMethod("GetNames", new Type[]
            {
            typeof(Type)
            }), typeof(Weathers).GetMethod("GetNamesHook"));*/

            //public override string ToString();
            new Hook(typeof(Enum).GetMethod("ToString", new Type[]
            {
            }), typeof(Weathers).GetMethod("ToStringHook"));

            On.TimeOfDay.Awake += TimeOfDay_Awake;
            On.StartOfRound.Awake += StartOfRound_Awake;
        }

        private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
        {
            foreach (KeyValuePair<int, CustomWeather> entry in customWeathers)
            {
                foreach (SelectableLevel level in self.levels)
                {
                    var name = level.name;

                    var alwaysValid = entry.Value.levels.HasFlag(Levels.LevelTypes.All) || (entry.Value.spawnLevelOverrides != null && entry.Value.spawnLevelOverrides.Any(item => item.ToLowerInvariant() == name.ToLowerInvariant()));

                    if (Enum.IsDefined(typeof(Levels.LevelTypes), name) || alwaysValid)
                    {
                        var levelEnum = alwaysValid ? Levels.LevelTypes.All : (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
                        var weathers = level.randomWeathers.ToList();
                        // loop through custom weathers
        
                        // if the custom weather has the level
                        if (alwaysValid || entry.Value.levels.HasFlag(levelEnum))
                        {
                            // add it to the level
                            weathers.Add(new RandomWeatherWithVariables()
                            {
                                weatherType = (LevelWeatherType)entry.Key,
                                weatherVariable = entry.Value.weatherVariable1,
                                weatherVariable2 = entry.Value.weatherVariable2
                            });
                            
                            Plugin.logger.LogInfo($"Added weather {entry.Value.name} to level {level.name} at weather index: {entry.Key}");
                        }
                    
                        level.randomWeathers = weathers.ToArray();

                    }
                }
            }


            orig(self);
        }

        private static void TimeOfDay_Awake(On.TimeOfDay.orig_Awake orig, TimeOfDay self)
        {
            List<WeatherEffect> weatherList = self.effects.ToList();
            
            // we want to insert things at the right index, but there might be gaps, in which case we need to fill it with nulls
            // first we find our highest index
            int highestIndex = 0;
            foreach (KeyValuePair<int, CustomWeather> entry in customWeathers)
            {
                if (entry.Key > highestIndex)
                {
                    highestIndex = entry.Key;
                }
            }

            // then we fill the list with nulls until we reach the highest index
            while (weatherList.Count <= highestIndex)
            {
                weatherList.Add(null);
            }

            // thne we set the custom weathers at their index
            foreach (KeyValuePair<int, CustomWeather> entry in customWeathers)
            {
                weatherList[entry.Key] = entry.Value.weatherEffect;
            }

            // then we set the list
            self.effects = weatherList.ToArray();

            orig(self);
        }

        /*
        public static Array GetValuesHook(Func<Type, Array> orig, Type enumType)
        {
            if(enumType == typeof(LevelWeatherType))
            {

                
            }
            return orig(enumType);
        }


        public static string[] GetNamesHook(Func<Type, string[]> orig, Type enumType)
        {
            if (enumType == typeof(LevelWeatherType))
            {
                
            }
            return orig(enumType);
        }*/

        public static string ToStringHook(Func<Enum, string> orig, Enum self)
        {
            if (self.GetType() == typeof(LevelWeatherType))
            {
                if (customWeathers.ContainsKey((int)(LevelWeatherType)self))
                {
                    return customWeathers[(int)(LevelWeatherType)self].name;
                }
            }

            return orig(self);
        }


        ///<summary>
        ///Register a weather with the game.
        ///</summary>
        public static void RegisterWeather(WeatherDef weather)
        {
            RegisterWeather(weather.weatherName, weather.weatherEffect, weather.levels, weather.levelOverrides, weather.weatherVariable1, weather.weatherVariable2);
        }

        ///<summary>
        ///Register a weather with the game, which are able to show up on the specified levels.
        ///</summary>
        public static void RegisterWeather(string name, WeatherEffect weatherEffect, Levels.LevelTypes levels = Levels.LevelTypes.None, int weatherVariable1 = 0, int weatherVariable2 = 0)
        {
            var origValues = Enum.GetValues(typeof(LevelWeatherType));
            int num = origValues.Length - 1;

            num += numCustomWeathers;

            // add our numcustomweathers
            numCustomWeathers++;

            Plugin.logger.LogInfo($"Registering weather {name} at index {num - 1}");

            // add to dictionary at next value
            customWeathers.Add(num, new CustomWeather(name, weatherEffect, levels, null, weatherVariable1, weatherVariable2));
        }

        ///<summary>
        ///Register a weather with the game, which are able to show up on the specified levels.
        ///</summary>
        public static void RegisterWeather(string name, WeatherEffect weatherEffect, Levels.LevelTypes levels = Levels.LevelTypes.None, string[] spawnLevelOverrides = null, int weatherVariable1 = 0, int weatherVariable2 = 0)
        {
            var origValues = Enum.GetValues(typeof(LevelWeatherType));
            int num = origValues.Length - 1;

            num += numCustomWeathers;

            // add our numcustomweathers
            numCustomWeathers++;

            Plugin.logger.LogInfo($"Registering weather {name} at index {num - 1}");

            // add to dictionary at next value
            customWeathers.Add(num, new CustomWeather(name, weatherEffect, levels, spawnLevelOverrides, weatherVariable1, weatherVariable2));
        }

        ///<summary>
        ///Removes a weather from the specified levels.
        ///This needs to be called after StartOfRound.Awake.
        ///Only works for weathers registered by LethalLib.
        ///</summary>
        public static void RemoveWeather(string levelName, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null)
        {
            foreach (KeyValuePair<int, CustomWeather> entry in customWeathers)
            {
                if (entry.Value.name == levelName && StartOfRound.Instance != null)
                {

                    foreach (SelectableLevel level in StartOfRound.Instance.levels)
                    {
                        var name = level.name;

                        var alwaysValid = levelFlags.HasFlag(Levels.LevelTypes.All) || (levelOverrides != null && levelOverrides.Any(item => item.ToLowerInvariant() == name.ToLowerInvariant()));

                        if (Enum.IsDefined(typeof(Levels.LevelTypes), name) || alwaysValid)
                        {
                            var levelEnum = alwaysValid ? Levels.LevelTypes.All : (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
                            if (alwaysValid || levelFlags.HasFlag(levelEnum))
                            {
                                var weathers = level.randomWeathers.ToList();

                                weathers.RemoveAll(item => item.weatherType == (LevelWeatherType)entry.Key);

                                level.randomWeathers = weathers.ToArray();
                            }
                        }
                    }
                }
            }
        }

    }

}