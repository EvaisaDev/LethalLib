using DunGen;
using DunGen.Graph;
using LethalLib.Extras;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace LethalLib.Modules
{
    public class Dungeon
    {
        public static void Init()
        {
            On.RoundManager.GenerateNewFloor += RoundManager_GenerateNewFloor;
        }

        public class CustomDungeonArchetype
        {
            public DungeonArchetype archeType;
            public Levels.LevelTypes LevelTypes;
            public int lineIndex = -1;
        }

        public class CustomGraphLine
        {
            public GraphLine graphLine;
            public Levels.LevelTypes LevelTypes;
        }

        public static List<CustomDungeonArchetype> customDungeonArchetypes = new List<CustomDungeonArchetype>();
        public static List<CustomGraphLine> customGraphLines = new List<CustomGraphLine>();
        public static Dictionary<string, TileSet> extraTileSets = new Dictionary<string, TileSet>();
        public static Dictionary<string, GameObjectChance> extraRooms = new Dictionary<string, GameObjectChance>();

        private static void RoundManager_GenerateNewFloor(On.RoundManager.orig_GenerateNewFloor orig, RoundManager self)
        {
            var name = self.currentLevel.name;
            if (Enum.IsDefined(typeof(Levels.LevelTypes), name))
            {
                var levelEnum = (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);

                var index = 0;
                self.dungeonGenerator.Generator.DungeonFlow.Lines.ForEach((line) =>
                {
                    foreach (var dungeonArchetype in customDungeonArchetypes)
                    {
                        if (dungeonArchetype.LevelTypes.HasFlag(levelEnum))
                        {
                            if (!line.DungeonArchetypes.Contains(dungeonArchetype.archeType) && (dungeonArchetype.lineIndex == -1 || dungeonArchetype.lineIndex == index)) { 
                                line.DungeonArchetypes.Add(dungeonArchetype.archeType);
                                Plugin.logger.LogInfo($"Added {dungeonArchetype.archeType.name} to {name}");
                            }
                        }
                    }

                    foreach (var archetype in line.DungeonArchetypes)
                    {
                        var archetypeName = archetype.name;
                        if (extraTileSets.ContainsKey(archetypeName))
                        {
                            var tileSet = extraTileSets[archetypeName];
                            if (!archetype.TileSets.Contains(tileSet))
                            {
                                archetype.TileSets.Add(tileSet);
                                Plugin.logger.LogInfo($"Added {tileSet.name} to {name}");
                            }
                        }
                        foreach (var tileSet in archetype.TileSets)
                        {
                            var tileSetName = tileSet.name;
                            if (extraRooms.ContainsKey(tileSetName))
                            {
                                var room = extraRooms[tileSetName];
                                if (!tileSet.TileWeights.Weights.Contains(room))
                                {
                                    tileSet.TileWeights.Weights.Add(room);
                                }
                            }
                        }
                    }

                    index++;
                });


                foreach (var graphLine in customGraphLines)
                {
                    if (graphLine.LevelTypes.HasFlag(levelEnum))
                    {
                        if(!self.dungeonGenerator.Generator.DungeonFlow.Lines.Contains(graphLine.graphLine))
                        {
                            self.dungeonGenerator.Generator.DungeonFlow.Lines.Add(graphLine.graphLine);
                           // Plugin.logger.LogInfo($"Added {graphLine.graphLine.name} to {name}");
                        }
                    }
                }
            }
            
            orig(self);
        }

        public static void AddArchetype(DungeonArchetype archetype, Levels.LevelTypes levelFlags, int lineIndex = -1)
        {
            var customArchetype = new CustomDungeonArchetype();
            customArchetype.archeType = archetype;
            customArchetype.LevelTypes = levelFlags;
            customArchetype.lineIndex = lineIndex;
            customDungeonArchetypes.Add(customArchetype);
        }

        public static void AddLine(GraphLine line, Levels.LevelTypes levelFlags)
        {
            var customLine = new CustomGraphLine();
            customLine.graphLine = line;
            customLine.LevelTypes = levelFlags;
            customGraphLines.Add(customLine);
        }

        public static void AddLine(DungeonGraphLineDef line, Levels.LevelTypes levelFlags)
        {
            var customLine = new CustomGraphLine();
            customLine.graphLine = line.graphLine;
            customLine.LevelTypes = levelFlags;
            customGraphLines.Add(customLine);
        }

        public static void AddTileSet(TileSet set, string archetypeName)
        {
            extraTileSets.Add(archetypeName, set);
        }

        public static void AddRoom(GameObjectChance room, string tileSetName)
        {
            extraRooms.Add(tileSetName, room);
        }

        public static void AddRoom(GameObjectChanceDef room, string tileSetName)
        {
            extraRooms.Add(tileSetName, room.gameObjectChance);
        }

    }
}
