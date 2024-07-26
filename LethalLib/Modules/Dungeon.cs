#region

using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using DunGen.Graph;
using LethalLib.Extras;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace LethalLib.Modules;

public class Dungeon
{
    public static void Init()
    {
        On.RoundManager.GenerateNewFloor += RoundManager_GenerateNewFloor;
        On.RoundManager.Start += RoundManager_Start;
        // On.StartOfRound.Start += StartOfRound_Start;
    }
    /*
    private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
    {


        Plugin.logger.LogInfo("Added custom dungeons to levels");
        orig(self);
    }
    */

    private static void RoundManager_Start(On.RoundManager.orig_Start orig, RoundManager self)
    {
        /*foreach(var dungeon in customDungeons)
        {
            if (!self.dungeonFlowTypes.Contains(dungeon.dungeonFlow))
            {
                var flowTypes = self.dungeonFlowTypes.ToList();
                flowTypes.Add(dungeon.dungeonFlow);
                self.dungeonFlowTypes = flowTypes.ToArray();

                var newDungeonIndex = self.dungeonFlowTypes.Length - 1;
                dungeon.dungeonIndex = newDungeonIndex;

                var firstTimeDungeonAudios = self.firstTimeDungeonAudios.ToList();
                // check if the indexes match
                if (firstTimeDungeonAudios.Count != self.dungeonFlowTypes.Length - 1)
                {
                    // add nulls until they do
                    while (firstTimeDungeonAudios.Count < self.dungeonFlowTypes.Length - 1)
                    {
                        firstTimeDungeonAudios.Add(null);
                    }
                }
                firstTimeDungeonAudios.Add(dungeon.firstTimeDungeonAudio);
                self.firstTimeDungeonAudios = firstTimeDungeonAudios.ToArray();
            }
        }


        var startOfRound = StartOfRound.Instance;
        foreach (var dungeon in customDungeons)
        {
            foreach (var level in startOfRound.levels)
            {
                var name = level.name;
                var alwaysValid = dungeon.LevelTypes.HasFlag(Levels.LevelTypes.All) || (dungeon.levelOverrides != null && dungeon.levelOverrides.Any(item => item.ToLowerInvariant() == name.ToLowerInvariant()));
                var isModded = dungeon.LevelTypes.HasFlag(Levels.LevelTypes.Modded) && !EnumUtils.IsDefined<Levels.LevelTypes>(name);

                if (isModded)
                {
                    alwaysValid = true;
                }

                if (EnumUtils.IsDefined<Levels.LevelTypes>(name) || alwaysValid)
                {
                    var levelEnum = alwaysValid ? Levels.LevelTypes.All : EnumUtils.Parse<Levels.LevelTypes>(name);

                    if ((alwaysValid || dungeon.LevelTypes.HasFlag(levelEnum)) && !level.dungeonFlowTypes.Any(rarityInt => rarityInt.id == dungeon.dungeonIndex))
                    {
                        var flowTypes = level.dungeonFlowTypes.ToList();
                        flowTypes.Add(new IntWithRarity { id = dungeon.dungeonIndex, rarity = dungeon.rarity });
                        level.dungeonFlowTypes = flowTypes.ToArray();
                    }
                }
            }
        }
        */
        orig(self);
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

    public class CustomDungeon
    {
        public int rarity;
        public DungeonFlow dungeonFlow;
        public Levels.LevelTypes LevelTypes;
        public string[] levelOverrides;
        public int dungeonIndex = -1;
        public AudioClip firstTimeDungeonAudio;
    }

    public static List<CustomDungeonArchetype> customDungeonArchetypes = new List<CustomDungeonArchetype>();
    public static List<CustomGraphLine> customGraphLines = new List<CustomGraphLine>();
    public static Dictionary<string, TileSet> extraTileSets = new Dictionary<string, TileSet>();
    public static Dictionary<string, GameObjectChance> extraRooms = new Dictionary<string, GameObjectChance>();
    public static List<CustomDungeon> customDungeons = new List<CustomDungeon>();

    private static void RoundManager_GenerateNewFloor(On.RoundManager.orig_GenerateNewFloor orig, RoundManager self)
    {
        var name = self.currentLevel.name;
        if (EnumUtils.IsDefined<Levels.LevelTypes>(name))
        {
            var levelEnum = EnumUtils.Parse<Levels.LevelTypes>(name);

            var index = 0;
            self.dungeonGenerator.Generator.DungeonFlow.Lines.ForEach((line) =>
            {
                foreach (var dungeonArchetype in customDungeonArchetypes)
                {
                    if (dungeonArchetype.LevelTypes.HasFlag(levelEnum))
                    {
                        if (!line.DungeonArchetypes.Contains(dungeonArchetype.archeType) && (dungeonArchetype.lineIndex == -1 || dungeonArchetype.lineIndex == index)) { 
                            line.DungeonArchetypes.Add(dungeonArchetype.archeType);
                            if (Plugin.extendedLogging.Value)
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
                            if (Plugin.extendedLogging.Value)
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
                    if (!self.dungeonGenerator.Generator.DungeonFlow.Lines.Contains(graphLine.graphLine))
                    {
                        self.dungeonGenerator.Generator.DungeonFlow.Lines.Add(graphLine.graphLine);
                        // Plugin.logger.LogInfo($"Added {graphLine.graphLine.name} to {name}");
                    }
                }
            }
        }

        orig(self);

        // debug copy of GenerateNewFloor
        /*
        if (self.currentLevel.dungeonFlowTypes != null && self.currentLevel.dungeonFlowTypes.Length != 0)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < self.currentLevel.dungeonFlowTypes.Length; i++)
            {
                list.Add(self.currentLevel.dungeonFlowTypes[i].rarity);
            }
            int id = self.currentLevel.dungeonFlowTypes[self.GetRandomWeightedIndex(list.ToArray(), self.LevelRandom)].id;

            Plugin.logger.LogInfo($"Dungeon flow id: {id}");
            Plugin.logger.LogInfo($"Dungeon flow count: {self.dungeonFlowTypes.Length}");
            Plugin.logger.LogInfo($"Dungeon flow name: {self.dungeonFlowTypes[id].name}");

            self.dungeonGenerator.Generator.DungeonFlow = self.dungeonFlowTypes[id];
            if (id < self.firstTimeDungeonAudios.Length && self.firstTimeDungeonAudios[id] != null)
            {
                EntranceTeleport[] array = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>();
                if (array != null && array.Length != 0)
                {
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (array[j].isEntranceToBuilding)
                        {
                            array[j].firstTimeAudio = self.firstTimeDungeonAudios[id];
                            array[j].dungeonFlowId = id;
                        }
                    }
                }
            }
        }
        self.dungeonGenerator.Generator.ShouldRandomizeSeed = false;
        self.dungeonGenerator.Generator.Seed = self.LevelRandom.Next();
        Debug.Log($"GenerateNewFloor(). Map generator's random seed: {self.dungeonGenerator.Generator.Seed}");
        float lengthMultiplier = self.currentLevel.factorySizeMultiplier * self.mapSizeMultiplier;
        self.dungeonGenerator.Generator.LengthMultiplier = lengthMultiplier;
        self.dungeonGenerator.Generate();*/


        // register prefabs

        var networkManager = UnityEngine.Object.FindObjectOfType<NetworkManager>();

        RandomMapObject[] objarray = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();

        foreach (RandomMapObject randomMapObject in objarray)
        {
            // loop through
            for(int i = 0; i < randomMapObject.spawnablePrefabs.Count; i++)
            {
                // get prefab name
                var prefabName = randomMapObject.spawnablePrefabs[i].name;

                var prefab = networkManager.NetworkConfig.Prefabs.m_Prefabs.FirstOrDefault(x => x.Prefab.name == prefabName);

                if (prefab != default(NetworkPrefab) && prefab.Prefab != randomMapObject.spawnablePrefabs[i])
                {
                    randomMapObject.spawnablePrefabs[i] = prefab.Prefab;
                    //Plugin.logger.LogInfo($"DungeonGeneration - Remapped prefab ({prefabName})!");
                }
                else if(prefab == default(NetworkPrefab))
                {
                    //Plugin.logger.LogInfo($"DungeonGeneration - Could not find network prefab ({prefabName})!");
                    Plugin.logger.LogError($"DungeonGeneration - Could not find network prefab ({prefabName})! Make sure your assigned prefab is registered with the network manager, or named identically to the vanilla prefab you are referencing.");
                }
                /*else
                {
                    Plugin.logger.LogInfo($"DungeonGeneration - Prefab ({prefabName}) was already correctly mapped!");
                }*/
            }
        }

    }

    /// <summary>
    /// Registers a custom archetype to a level.
    /// </summary>
    public static void AddArchetype(DungeonArchetype archetype, Levels.LevelTypes levelFlags, int lineIndex = -1)
    {
        var customArchetype = new CustomDungeonArchetype();
        customArchetype.archeType = archetype;
        customArchetype.LevelTypes = levelFlags;
        customArchetype.lineIndex = lineIndex;
        customDungeonArchetypes.Add(customArchetype);
    }

    /// <summary>
    /// Registers a dungeon graphline to a level.
    /// </summary>
    public static void AddLine(GraphLine line, Levels.LevelTypes levelFlags)
    {
        var customLine = new CustomGraphLine();
        customLine.graphLine = line;
        customLine.LevelTypes = levelFlags;
        customGraphLines.Add(customLine);
    }

    /// <summary>
    /// Registers a dungeon graphline to a level.
    /// </summary>
    public static void AddLine(DungeonGraphLineDef line, Levels.LevelTypes levelFlags)
    {
        AddLine(line.graphLine, levelFlags);
    }

    /// <summary>
    /// Adds a tileset to a dungeon archetype
    /// </summary>
    public static void AddTileSet(TileSet set, string archetypeName)
    {
        extraTileSets.Add(archetypeName, set);
    }

    /// <summary>
    /// Adds a room to a tileset with the given name.
    /// </summary>
    public static void AddRoom(GameObjectChance room, string tileSetName)
    {
        extraRooms.Add(tileSetName, room);
    }

    /// <summary>
    /// Adds a room to a tileset with the given name.
    /// </summary>
    public static void AddRoom(GameObjectChanceDef room, string tileSetName)
    {
        AddRoom(room.gameObjectChance, tileSetName);
    }

    /// <summary>
    /// Adds a dungeon to the given levels.
    /// </summary>
    public static void AddDungeon(DungeonDef dungeon, Levels.LevelTypes levelFlags)
    {
        AddDungeon(dungeon.dungeonFlow, dungeon.rarity, levelFlags, dungeon.firstTimeDungeonAudio); 
    }

    /// <summary>
    /// Adds a dungeon to the given levels.
    /// </summary>
    public static void AddDungeon(DungeonDef dungeon, Levels.LevelTypes levelFlags, string[] levelOverrides)
    {
        AddDungeon(dungeon.dungeonFlow, dungeon.rarity, levelFlags, levelOverrides, dungeon.firstTimeDungeonAudio);
    }

    /// <summary>
    /// Adds a dungeon to the given levels.
    /// </summary>
    public static void AddDungeon(DungeonFlow dungeon, int rarity, Levels.LevelTypes levelFlags, AudioClip firstTimeDungeonAudio = null)
    {
        customDungeons.Add(new CustomDungeon
        {
            dungeonFlow = dungeon,
            rarity = rarity,
            LevelTypes = levelFlags,
            firstTimeDungeonAudio = firstTimeDungeonAudio
        });
    }

    /// <summary>
    /// Adds a dungeon to the given levels.
    /// </summary>
    public static void AddDungeon(DungeonFlow dungeon, int rarity, Levels.LevelTypes levelFlags, string[] levelOverrides = null, AudioClip firstTimeDungeonAudio = null)
    {
        customDungeons.Add(new CustomDungeon
        {
            dungeonFlow = dungeon,
            rarity = rarity,
            LevelTypes = levelFlags,
            firstTimeDungeonAudio = firstTimeDungeonAudio,
            levelOverrides = levelOverrides
        });
    }

    // TODO: Allow runtime removal to let people have synced configs (I do not want to implement this because it is a hassle.)
}
