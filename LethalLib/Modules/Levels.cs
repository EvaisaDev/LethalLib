using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLib.Modules
{
    public static class Levels
    {

        [Flags]
        public enum LevelTypes
        {
            None = 1 << 0,
            ExperimentationLevel = 1 << 1,
            AssuranceLevel = 1 << 2,
            VowLevel = 1 << 3,
            OffenseLevel = 1 << 4,
            MarchLevel = 1 << 5,
            RendLevel = 1 << 6,
            DineLevel = 1 << 7,
            TitanLevel = 1 << 8,
            All = ExperimentationLevel | AssuranceLevel | VowLevel | OffenseLevel | MarchLevel | RendLevel | DineLevel | TitanLevel
        }

        public static bool IsSingleLevel(this LevelTypes levelTypes)
        {
            int levelTypesRaw = (int) levelTypes;
            if (levelTypesRaw == 0)
            {
                return false;
            }

            // https://graphics.stanford.edu/~seander/bithacks.html#DetermineIfPowerOf2
            return (levelTypesRaw & (levelTypesRaw - 1)) == 0;
        }

        public static List<LevelTypes> ToList(this LevelTypes levelTypes)
        {
            List<LevelTypes> types = new List<LevelTypes>();
            foreach (LevelTypes levelType in Enum.GetValues(typeof(LevelTypes)))
            {
                if (levelType == LevelTypes.None || levelType == LevelTypes.All)
                {
                    continue;
                }

                if ((levelTypes & levelType) == levelType)
                {
                    types.Add(levelType);
                }
            }

            return types;
        }

        public static LevelTypes FromList(List<LevelTypes> levelTypes)
        {
            LevelTypes types = LevelTypes.None;
            foreach (LevelTypes levelType in levelTypes)
            {
                types |= levelType;
            }

            return types;
        }
    }
}
