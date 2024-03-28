#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#endregion

namespace LethalLib.Modules;

public class Enemies
{
    private static List<SelectableLevel> levelsAlreadyAddedTo = new();
    public static void Init()
    {
        On.StartOfRound.Awake += RegisterLevelEnemies;
        On.Terminal.Start += Terminal_Start;
        On.QuickMenuManager.Start += QuickMenuManager_Start;
    }

    static bool addedToDebug = false; // This method of initializing can be changed to your liking.
    private static void QuickMenuManager_Start(On.QuickMenuManager.orig_Start orig, QuickMenuManager self)
    {
        if (addedToDebug)
        {
            orig(self);
            return;
        }
        var testLevel = self.testAllEnemiesLevel;
        var inside = testLevel.Enemies;
        var daytime = testLevel.DaytimeEnemies;
        var outside = testLevel.OutsideEnemies;
        foreach (SpawnableEnemy spawnableEnemy in spawnableEnemies)
        {
            if (inside.All(x => x.enemyType == spawnableEnemy.enemy)) continue;
            SpawnableEnemyWithRarity spawnableEnemyWithRarity = new SpawnableEnemyWithRarity
            {
                enemyType = spawnableEnemy.enemy,
                rarity = spawnableEnemy.rarity
            };
            switch (spawnableEnemy.spawnType)
            {
                case SpawnType.Default:
                    if (!inside.Any(x => x.enemyType == spawnableEnemy.enemy))
                        inside.Add(spawnableEnemyWithRarity);
                    break;
                case SpawnType.Daytime:
                    if (!daytime.Any(x => x.enemyType == spawnableEnemy.enemy))
                        daytime.Add(spawnableEnemyWithRarity);
                    break;
                case SpawnType.Outside:
                    if (!outside.Any(x => x.enemyType == spawnableEnemy.enemy))
                        outside.Add(spawnableEnemyWithRarity);
                    break;
            }
            if (Plugin.extendedLogging.Value)
                Plugin.logger.LogInfo($"Added {spawnableEnemy.enemy.enemyName} to DebugList [{spawnableEnemy.spawnType}]");
        }
        addedToDebug = true;
        orig(self);
    }

    public struct EnemyAssetInfo
    {
        public EnemyType EnemyAsset;
        public TerminalKeyword keyword;
    }
    public static Terminal terminal;

    public static List<EnemyAssetInfo> enemyAssetInfos = new List<EnemyAssetInfo>();

    private static void Terminal_Start(On.Terminal.orig_Start orig, Terminal self)
    {
        terminal = self;
        var infoKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
        var addedEnemies = new List<string>();
        foreach (SpawnableEnemy spawnableEnemy in spawnableEnemies)
        {
            // if terminal node is null, create one
            if (addedEnemies.Contains(spawnableEnemy.enemy.enemyName))
            {
                Plugin.logger.LogInfo($"Skipping {spawnableEnemy.enemy.enemyName} because it was already added");
                continue;
            }

            if (spawnableEnemy.terminalNode == null)
            {
                spawnableEnemy.terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
                spawnableEnemy.terminalNode.displayText = $"{spawnableEnemy.enemy.enemyName}\n\nDanger level: Unknown\n\n[No information about this creature was found.]\n\n";
                spawnableEnemy.terminalNode.clearPreviousText = true;
                spawnableEnemy.terminalNode.maxCharactersToType = 35;
                spawnableEnemy.terminalNode.creatureName = spawnableEnemy.enemy.enemyName;
            }

            // if spawnableEnemy terminalnode is already in enemyfiles, skip
            if (self.enemyFiles.Any(x => x.creatureName == spawnableEnemy.terminalNode.creatureName))
            {
                Plugin.logger.LogInfo($"Skipping {spawnableEnemy.enemy.enemyName} because it was already added");
                continue;
            }

            var keyword = spawnableEnemy.infoKeyword != null ? spawnableEnemy.infoKeyword : TerminalUtils.CreateTerminalKeyword(spawnableEnemy.terminalNode.creatureName.ToLowerInvariant().Replace(" ", "-"), defaultVerb: infoKeyword);

            keyword.defaultVerb = infoKeyword;

            var allKeywords = self.terminalNodes.allKeywords.ToList();

            // if doesn't contain keyword, add it
            if (!allKeywords.Any(x => x.word == keyword.word))
            {
                allKeywords.Add(keyword);
                self.terminalNodes.allKeywords = allKeywords.ToArray();
            }

            var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
            // if doesn't contain noun, add it
            if (!itemInfoNouns.Any(x => x.noun.word == keyword.word))
            {
                itemInfoNouns.Add(new CompatibleNoun()
                {
                    noun = keyword,
                    result = spawnableEnemy.terminalNode
                });
            }
            infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
     


            spawnableEnemy.terminalNode.creatureFileID = self.enemyFiles.Count;

            self.enemyFiles.Add(spawnableEnemy.terminalNode);

            var scanNodePropertyComponents = spawnableEnemy.enemy.enemyPrefab.GetComponentsInChildren<ScanNodeProperties>();
            for (int i = 0; i < scanNodePropertyComponents.Length; i++)
            {
                scanNodePropertyComponents[i].creatureScanID = spawnableEnemy.terminalNode.creatureFileID;
            }

            var enemyAssetInfo = new EnemyAssetInfo()
            {
                EnemyAsset = spawnableEnemy.enemy,
                keyword = keyword
            };

            enemyAssetInfos.Add(enemyAssetInfo);
        }
        orig(self);
    }

    private static void RegisterLevelEnemies(On.StartOfRound.orig_Awake orig, StartOfRound self)
    {
        orig(self);

        RegisterLethalLibEnemiesForAllLevels();

        if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader") || // currently has typo
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("iambatby.lethallevelloader")) // might be changed to this
            On.RoundManager.Start += RegisterLevelEnemiesforLLL_RoundManager_Start;
        
        if(BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("LethalExpansion"))
            On.Terminal.Start += RegisterLevelEnemiesforLE_Terminal_Start;
    }

    private static void RegisterLevelEnemiesforLLL_RoundManager_Start(On.RoundManager.orig_Start orig, RoundManager self)
    {
        orig(self);
        RegisterLethalLibEnemiesForAllLevels();
    }

    private static void RegisterLevelEnemiesforLE_Terminal_Start(On.Terminal.orig_Start orig, Terminal self)
    {
        orig(self);
        RegisterLethalLibEnemiesForAllLevels();
    }

    private static void RegisterLethalLibEnemiesForAllLevels()
    {

        foreach (SelectableLevel level in StartOfRound.Instance.levels)
        {
            if(levelsAlreadyAddedTo.Contains(level))
                continue;
            foreach (SpawnableEnemy spawnableEnemy in spawnableEnemies)
            {
                AddEnemyToLevel(spawnableEnemy, level);
            }
            levelsAlreadyAddedTo.Add(level);
        }
    }

    private static void AddEnemyToLevel(SpawnableEnemy spawnableEnemy, SelectableLevel level)
    {
        var name = level.name;

        //var alwaysValid = spawnableEnemy.spawnLevels.HasFlag(Levels.LevelTypes.All) || (spawnableEnemy.spawnLevelOverrides != null && spawnableEnemy.spawnLevelOverrides.Any(item => item.ToLowerInvariant() == name.ToLowerInvariant()));
        //var isModded = spawnableEnemy.spawnLevels.HasFlag(Levels.LevelTypes.Modded) && !Enum.IsDefined(typeof(Levels.LevelTypes), name);
        var alwaysValid = spawnableEnemy.levelRarities.ContainsKey(Levels.LevelTypes.All) || (spawnableEnemy.customLevelRarities != null && spawnableEnemy.customLevelRarities.ContainsKey(name));
        var isModded = spawnableEnemy.levelRarities.ContainsKey(Levels.LevelTypes.Modded) && !Enum.IsDefined(typeof(Levels.LevelTypes), name);


        if (isModded)
        {
            alwaysValid = true;
        }

        var realLevelEnum = Levels.LevelTypes.None;
        bool isVanilla = false;

        if(Enum.IsDefined(typeof(Levels.LevelTypes), name)){
            realLevelEnum = (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
            isVanilla = true;
        }
        else
            name = Levels.Compatibility.GetLLLNameOfLevel(name);

        if (isVanilla || alwaysValid)
        {

            // if modded, alwaysValid is true anyways
            var levelEnum = alwaysValid ? Levels.LevelTypes.All : realLevelEnum;

            if (alwaysValid || spawnableEnemy.levelRarities.ContainsKey(levelEnum))
            {
                // find rarity
                int rarity = 0;

                if (isVanilla && spawnableEnemy.levelRarities.ContainsKey(realLevelEnum))
                    rarity = spawnableEnemy.levelRarities[realLevelEnum];
                
                else if (!isVanilla && spawnableEnemy.customLevelRarities != null && spawnableEnemy.customLevelRarities.ContainsKey(name))
                    rarity = spawnableEnemy.customLevelRarities[name];

                else if (!isVanilla && spawnableEnemy.levelRarities.ContainsKey(Levels.LevelTypes.Modded))
                    rarity = spawnableEnemy.levelRarities[Levels.LevelTypes.Modded];
                
                else if (spawnableEnemy.levelRarities.ContainsKey(Levels.LevelTypes.All))
                    rarity = spawnableEnemy.levelRarities[Levels.LevelTypes.All];
                

                var spawnableEnemyWithRarity = new SpawnableEnemyWithRarity()
                {
                    enemyType = spawnableEnemy.enemy,
                    rarity = rarity
                };

                // make sure spawnableScrap does not already contain item
                //Plugin.logger.LogInfo($"Checking if {spawnableEnemy.enemy.name} is already in {name}");

                /*
                if (!level.spawnableEnemies.Any(x => x.spawnableEnemy == spawnableEnemy.enemy))
                {
                    level.spawnableEnemies.Add(spawnableEnemyWithRarity);
                    Logger.LogInfo($"Added {spawnableEnemy.enemy.name} to {name}");
                }*/

                switch (spawnableEnemy.spawnType)
                {
                    case SpawnType.Default:
                        if (!level.Enemies.Any(x => x.enemyType == spawnableEnemy.enemy))
                        {
                            level.Enemies.Add(spawnableEnemyWithRarity);
                            if (Plugin.extendedLogging.Value)
                                Plugin.logger.LogInfo($"To {name} added {spawnableEnemy.enemy.name} with weight {rarity} and SpawnType [Default]");
                        }
                        break;
                    case SpawnType.Daytime:
                        if (!level.DaytimeEnemies.Any(x => x.enemyType == spawnableEnemy.enemy))
                        {
                            level.DaytimeEnemies.Add(spawnableEnemyWithRarity);
                            if (Plugin.extendedLogging.Value)
                                Plugin.logger.LogInfo($"To {name} added {spawnableEnemy.enemy.name} with weight {rarity} andSpawnType [Daytime]");
                        }
                        break;
                    case SpawnType.Outside:
                        if (!level.OutsideEnemies.Any(x => x.enemyType == spawnableEnemy.enemy))
                        {
                            level.OutsideEnemies.Add(spawnableEnemyWithRarity);
                            if (Plugin.extendedLogging.Value)
                                Plugin.logger.LogInfo($"To {name} added {spawnableEnemy.enemy.name} with weight {rarity} and SpawnType [Outside]");
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public enum SpawnType
    {
        Default,
        Daytime,
        Outside
    }

    public class SpawnableEnemy
    {
        public EnemyType enemy;
        public SpawnType spawnType;
        public TerminalNode terminalNode;
        public TerminalKeyword infoKeyword;
        public string modName;

        /// <summary>
        /// Deprecated
        /// This is never set or used, use levelRarities and customLevelRarities instead.
        /// </summary>
        public int rarity;

        /// <summary>
        /// Deprecated
        /// This is never set or used, use levelRarities and customLevelRarities instead.
        /// </summary>
        public Levels.LevelTypes spawnLevels;

        /// <summary>
        /// Deprecated
        /// This is never set or used, use levelRarities and customLevelRarities instead.
        /// </summary>
        public string[] spawnLevelOverrides;

        public Dictionary<string, int> customLevelRarities = new Dictionary<string, int>();
        public Dictionary<Levels.LevelTypes, int> levelRarities = new Dictionary<Levels.LevelTypes, int>();
        public SpawnableEnemy(EnemyType enemy, int rarity, Levels.LevelTypes spawnLevels, SpawnType spawnType, string[] spawnLevelOverrides = null)
        {
            this.enemy = enemy;
            //this.rarity = rarity;
            this.spawnLevels = spawnLevels;
            this.spawnType = spawnType;
            //this.spawnLevelOverrides = spawnLevelOverrides;

            if (spawnLevelOverrides != null)
            {
                foreach (var level in spawnLevelOverrides)
                {
                    customLevelRarities.Add(Levels.Compatibility.GetLLLNameOfLevel(level), rarity);
                }
            }

            if (spawnLevels != Levels.LevelTypes.None)
            {
                foreach (Levels.LevelTypes level in Enum.GetValues(typeof(Levels.LevelTypes)))
                {
                    if (spawnLevels.HasFlag(level))
                    {
                        levelRarities.Add(level, rarity);
                    }
                }
            }
        }

        public SpawnableEnemy(EnemyType enemy, SpawnType spawnType, Dictionary<Levels.LevelTypes, int>? levelRarities = null, Dictionary<string, int>? customLevelRarities = null)
        {
            this.enemy = enemy;
            this.spawnType = spawnType;

            if (levelRarities != null)
            {
                this.levelRarities = levelRarities;
            }

            if (customLevelRarities != null)
            {
                this.customLevelRarities = Levels.Compatibility.LLLifyLevelRarityDictionary(customLevelRarities);
            }
        }
    }

    public static List<SpawnableEnemy> spawnableEnemies = new List<SpawnableEnemy>();

    /// <summary>
    /// Registers a enemy to be added to the given levels.
    /// </summary>
    public static void RegisterEnemy(EnemyType enemy, int rarity, Levels.LevelTypes levelFlags, SpawnType spawnType, TerminalNode infoNode = null, TerminalKeyword infoKeyword = null)
    {
        RegisterEnemy(enemy, rarity, levelFlags, spawnType, null, infoNode, infoKeyword);
    }

    /// <summary>
    /// Registers a enemy to be added to the given levels.
    /// </summary>
    public static void RegisterEnemy(EnemyType enemy, int rarity, Levels.LevelTypes levelFlags, SpawnType spawnType, string[] spawnLevelOverrides = null, TerminalNode infoNode = null, TerminalKeyword infoKeyword = null)
    {
        EnemyNullCheck(enemy);
        // if already registered, add rarity to levelRarities
        var spawnableEnemy = spawnableEnemies.FirstOrDefault(x => x.enemy == enemy && x.spawnType == spawnType);

        if (spawnableEnemy != null)
        {
            if (levelFlags != Levels.LevelTypes.None)
            {
                spawnableEnemy.levelRarities.Add(levelFlags, rarity);
            }

            if (spawnLevelOverrides != null)
            {
                foreach (var level in spawnLevelOverrides)
                {
                    spawnableEnemy.customLevelRarities.Add(Levels.Compatibility.GetLLLNameOfLevel(level), rarity);
                }
            }
            return;
        }

        spawnableEnemy = new SpawnableEnemy(enemy, rarity, levelFlags, spawnType, spawnLevelOverrides);

        spawnableEnemy.terminalNode = infoNode;
        spawnableEnemy.infoKeyword = infoKeyword;

        FinalizeRegisterEnemy(spawnableEnemy);
    }

    /// <summary>
    /// Registers a enemy to be added to the given levels, However it allows you to pass rarity tables, instead of just a single rarity
    /// </summary>
    public static void RegisterEnemy(EnemyType enemy, SpawnType spawnType, Dictionary<Levels.LevelTypes, int>? levelRarities = null, Dictionary<string, int>? customLevelRarities = null, TerminalNode infoNode = null, TerminalKeyword infoKeyword = null)
    {
        EnemyNullCheck(enemy);
        // if already registered, add rarity to levelRarities
        var spawnableEnemy = spawnableEnemies.FirstOrDefault(x => x.enemy == enemy && x.spawnType == spawnType);

        if (spawnableEnemy != null)
        {
            if (levelRarities != null)
            {
                foreach (var level in levelRarities)
                {
                    spawnableEnemy.levelRarities.Add(level.Key, level.Value);
                }
            }

            if (customLevelRarities != null)
            {
                foreach (var level in customLevelRarities)
                {
                    spawnableEnemy.customLevelRarities.Add(Levels.Compatibility.GetLLLNameOfLevel(level.Key), level.Value);
                }
            }
            return;
        }

        spawnableEnemy = new SpawnableEnemy(enemy, spawnType, levelRarities, customLevelRarities);

        spawnableEnemy.terminalNode = infoNode;
        spawnableEnemy.infoKeyword = infoKeyword;

        FinalizeRegisterEnemy(spawnableEnemy);
    }

    private static void FinalizeRegisterEnemy(SpawnableEnemy spawnableEnemy)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        var modDLL = callingAssembly.GetName().Name;
        spawnableEnemy.modName = modDLL;

        if (spawnableEnemy.enemy.enemyPrefab is null) {
            throw new NullReferenceException($"Cannot register enemy '{spawnableEnemy.enemy.enemyName}', because enemy.enemyPrefab is null!");
        }

        var collisionDetectComponents = spawnableEnemy.enemy.enemyPrefab.GetComponentsInChildren<EnemyAICollisionDetect>();
        foreach (var collisionDetectComponent in collisionDetectComponents)
        {
            if (collisionDetectComponent.mainScript is null) {
                Plugin.logger.LogWarning($"An Enemy AI Collision Detect Script on GameObject '{collisionDetectComponent.gameObject.name}' of enemy '{spawnableEnemy.enemy.enemyName}' does not reference a 'Main Script', and could cause Null Reference Exceptions.");
            }
        }

        spawnableEnemies.Add(spawnableEnemy);
    }

    private static void EnemyNullCheck(EnemyType enemy)
    {
        if (enemy is null) {
            throw new ArgumentNullException(nameof(enemy), $"The first argument of {nameof(RegisterEnemy)} was null!");
        }
    }

    /// <summary>
    /// Registers a enemy to be added to the given levels.
    /// Automatically sets the spawnType based on the enemy's isDaytimeEnemy and isOutsideEnemy properties.
    /// </summary>
    public static void RegisterEnemy(EnemyType enemy, int rarity, Levels.LevelTypes levelFlags, TerminalNode infoNode = null, TerminalKeyword infoKeyword = null)
    {
        EnemyNullCheck(enemy);
        var spawnType = enemy.isDaytimeEnemy ? SpawnType.Daytime : enemy.isOutsideEnemy ? SpawnType.Outside : SpawnType.Default;

        RegisterEnemy(enemy, rarity, levelFlags, spawnType, null, infoNode, infoKeyword);
    }

    /// <summary>
    /// Registers a enemy to be added to the given levels.
    /// Automatically sets the spawnType based on the enemy's isDaytimeEnemy and isOutsideEnemy properties.
    /// </summary>
    public static void RegisterEnemy(EnemyType enemy, int rarity, Levels.LevelTypes levelFlags, string[] spawnLevelOverrides = null, TerminalNode infoNode = null, TerminalKeyword infoKeyword = null)
    {
        EnemyNullCheck(enemy);
        var spawnType = enemy.isDaytimeEnemy ? SpawnType.Daytime : enemy.isOutsideEnemy ? SpawnType.Outside : SpawnType.Default;

        RegisterEnemy(enemy, rarity, levelFlags, spawnType, spawnLevelOverrides, infoNode, infoKeyword);
    }

    /// <summary>
    /// Registers a enemy to be added to the given levels, However it allows you to pass rarity tables, instead of just a single rarity
    /// Automatically sets the spawnType based on the enemy's isDaytimeEnemy and isOutsideEnemy properties.
    /// </summary>
    public static void RegisterEnemy(EnemyType enemy, Dictionary<Levels.LevelTypes, int>? levelRarities = null, Dictionary<string, int>? customLevelRarities = null, TerminalNode infoNode = null, TerminalKeyword infoKeyword = null)
    {
        EnemyNullCheck(enemy);
        var spawnType = enemy.isDaytimeEnemy ? SpawnType.Daytime : enemy.isOutsideEnemy ? SpawnType.Outside : SpawnType.Default;

        RegisterEnemy(enemy, spawnType, levelRarities, customLevelRarities, infoNode, infoKeyword);
    }

    ///<summary>
    ///Removes a enemy from the given levels.
    ///This needs to be called after StartOfRound.Awake, can be used for config sync.
    ///Only works for enemies added by LethalLib.
    /// </summary>
    public static void RemoveEnemyFromLevels(EnemyType enemyType, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null)
    {
        if (StartOfRound.Instance != null)
        {
            foreach (SelectableLevel level in StartOfRound.Instance.levels)
            {
                var name = level.name;

                if(!Enum.IsDefined(typeof(Levels.LevelTypes), name))
                    name = Levels.Compatibility.GetLLLNameOfLevel(name);
                                
                var alwaysValid = levelFlags.HasFlag(Levels.LevelTypes.All) || (levelOverrides != null && levelOverrides.Any(item => Levels.Compatibility.GetLLLNameOfLevel(item).ToLowerInvariant() == name.ToLowerInvariant()));
                var isModded = levelFlags.HasFlag(Levels.LevelTypes.Modded) && !Enum.IsDefined(typeof(Levels.LevelTypes), name);

                if (isModded)
                {
                    alwaysValid = true;
                }

                if (Enum.IsDefined(typeof(Levels.LevelTypes), name) || alwaysValid)
                {
                    var levelEnum = alwaysValid ? Levels.LevelTypes.All : (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
                    if (alwaysValid || levelFlags.HasFlag(levelEnum))
                    {

                        var enemies = level.Enemies;
                        var daytimeEnemies = level.DaytimeEnemies;
                        var outsideEnemies = level.OutsideEnemies;

                            
                        enemies.RemoveAll(x => x.enemyType == enemyType);
                        daytimeEnemies.RemoveAll(x => x.enemyType == enemyType);
                        outsideEnemies.RemoveAll(x => x.enemyType == enemyType);
                        if (Plugin.extendedLogging.Value)
                            Plugin.logger.LogInfo("Removed Enemy " + enemyType.name + " from Level " + name);
                    }
                }
            }
        }
    }

}
