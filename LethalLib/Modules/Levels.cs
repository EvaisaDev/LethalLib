using System;
using System.Collections.Generic;
using System.Text;

namespace LethalLib.Modules
{
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
            /// This includes modded levels!!
            /// </summary>
            All = ~0
        }

    }
}
