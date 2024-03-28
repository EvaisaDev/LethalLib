#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace LethalLib.Modules;

public class Levels
{

    [Flags]
    public enum LevelTypes
    {
        None = 1 << 0,
        ExperimentationLevel = 1 << 2,
        AssuranceLevel = 1 << 3,
        VowLevel = 1 << 4,
        OffenseLevel = 1 << 5,
        MarchLevel = 1 << 6,
        RendLevel = 1 << 7,
        DineLevel = 1 << 8,
        TitanLevel = 1 << 9,
        Vanilla = ExperimentationLevel | AssuranceLevel | VowLevel | OffenseLevel | MarchLevel | RendLevel | DineLevel | TitanLevel,

        /// <summary>
        /// Only modded levels
        /// </summary>
        Modded = 1 << 10,

        /// <summary>
        /// This includes modded levels!
        /// Acts as a global override
        /// </summary>
        All = ~0
    }

    internal static class Compatibility
    {
        /*
        // The following code is from LLL, but is copied here because we need to use it
        // even when LLL is not installed, because LLL alters LE(C) moon names to be
        // usable in e.g. BepInEx configuration files by removing illegal characters.
        //
        // https://github.com/IAmBatby/LethalLevelLoader
        */

        // From LLL, class: ConfigHelper
        private const string illegalCharacters = ".,?!@#$%^&*()_+-=';:'\"";

        // From LLL, class: ExtendedLevel (modified to take a string as input)
        private static string GetNumberlessPlanetName(string planetName)
        {
            if (planetName != null)
                return new string(planetName.SkipWhile(c => !char.IsLetter(c)).ToArray());
            else
                return string.Empty;
        }

        // From LLL, class: Extensions (modified: removed 'this' from string input)
        private static string StripSpecialCharacters(string input)
        {
            string returnString = string.Empty;

            foreach (char charmander in input)
                if ((!illegalCharacters.ToCharArray().Contains(charmander) && char.IsLetterOrDigit(charmander)) || charmander.ToString() == " ")
                    returnString += charmander;

            return returnString;
        }

        // Helper Method for LethalLib
        internal static string GetLLLNameOfLevel(string levelName)
        {
            // -> 10 Example
            string newName = StripSpecialCharacters(GetNumberlessPlanetName(levelName));
            // -> Example
            if (!newName.EndsWith("Level"))
                newName += "Level";
            // -> ExampleLevel
            return newName;
        }

        // Helper Method for LethalLib
        internal static Dictionary<string, int> LLLifyLevelRarityDictionary(Dictionary<string, int> keyValuePairs)
        {
            // LethalLevelLoader changes LethalExpansion level names. By applying the LLL changes always,
            // we can make sure all enemies get added to their target levels whether or not LLL is installed.
            Dictionary<string, int> LLLifiedCustomLevelRarities = new();
            var clrKeys = keyValuePairs.Keys.ToList();
            var clrValues = keyValuePairs.Values.ToList();
            for (int i = 0; i < keyValuePairs.Count; i++)
            {
                LLLifiedCustomLevelRarities.Add(GetLLLNameOfLevel(clrKeys[i]), clrValues[i]);
            }
            return LLLifiedCustomLevelRarities;
        }
    }
}