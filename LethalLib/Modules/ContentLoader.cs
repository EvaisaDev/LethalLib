#region

using System;
using System.Collections.Generic;
using BepInEx;
using LethalLib.Extras;
using UnityEngine;

#endregion

namespace LethalLib.Modules;

/// <summary>
/// Content loading module, for easier content loading.
/// Not feature complete. If someone wants to add to this feel free to do so.
/// </summary>
public class ContentLoader
{
    // getsetter
    public Dictionary<string, CustomContent> LoadedContent { get; } = new();

    // Main stuff
    public PluginInfo modInfo;
    AssetBundle modBundle;
    public string modName => modInfo.Metadata.Name;

    public string modVersion => modInfo.Metadata.Version.ToString();

    public string modGUID => modInfo.Metadata.GUID;

    public Action<CustomContent, GameObject> prefabCallback = (content, prefab) => { };

    public ContentLoader(PluginInfo modInfo, AssetBundle modBundle, Action<CustomContent, GameObject> prefabCallback = null)
    {
        this.modInfo = modInfo;
        this.modBundle = modBundle;

        if (prefabCallback != null)
        {
            this.prefabCallback = prefabCallback;
        }
    }
    public ContentLoader Create(PluginInfo modInfo, AssetBundle modBundle, Action<CustomContent, GameObject> prefabCallback = null)
    {
        return new ContentLoader(modInfo, modBundle, prefabCallback);
    }


    /// <summary>
    /// Loads and registers custom content.
    /// Handles everything for you including network registry.
    /// </summary>
    public void Register(CustomContent content)
    {
        if(LoadedContent.ContainsKey(content.ID))
        {
            Debug.LogError($"[LethalLib] {modName} tried to register content with ID {content.ID} but it already exists!");
            return;
        }

        if(content is CustomItem item)
        {
            var itemAsset = modBundle.LoadAsset<Item>(item.contentPath);
            item.item = itemAsset;
            NetworkPrefabs.RegisterNetworkPrefab(itemAsset.spawnPrefab);
            Utilities.FixMixerGroups(itemAsset.spawnPrefab);
            prefabCallback(item, itemAsset.spawnPrefab);
            item.registryCallback(itemAsset);

            if(content is ShopItem shopItem)
            {
                TerminalNode buyNode1 = null;
                TerminalNode buyNode2 = null;
                TerminalNode itemInfo = null;
                if(shopItem.buyNode1Path != null)
                {
                    buyNode1 = modBundle.LoadAsset<TerminalNode>(shopItem.buyNode1Path);
                }
                if(shopItem.buyNode2Path != null)
                {
                    buyNode2 = modBundle.LoadAsset<TerminalNode>(shopItem.buyNode2Path);
                }
                if(shopItem.itemInfoPath != null)
                {
                    itemInfo = modBundle.LoadAsset<TerminalNode>(shopItem.itemInfoPath);
                }

                Items.RegisterShopItem(itemAsset, buyNode1, buyNode2, itemInfo, shopItem.initPrice);
            }
            else if(content is ScrapItem scrapItem)
            {
                Items.RegisterScrap(itemAsset, scrapItem.levelRarities, scrapItem.customLevelRarities);
            }
            else
            {
                Items.RegisterItem(itemAsset);
            }


        }
        else if (content is Unlockable unlockable)
        {
            var unlockableAsset = modBundle.LoadAsset<UnlockableItemDef>(unlockable.contentPath);
            if(unlockableAsset.unlockable.prefabObject != null)
            {
                NetworkPrefabs.RegisterNetworkPrefab(unlockableAsset.unlockable.prefabObject);
                prefabCallback(content, unlockableAsset.unlockable.prefabObject);
                Utilities.FixMixerGroups(unlockableAsset.unlockable.prefabObject);
            }
            unlockable.unlockable = unlockableAsset.unlockable;
            unlockable.registryCallback(unlockableAsset.unlockable);


            TerminalNode buyNode1 = null;
            TerminalNode buyNode2 = null;
            TerminalNode itemInfo = null;
            if(unlockable.buyNode1Path != null)
            {
                buyNode1 = modBundle.LoadAsset<TerminalNode>(unlockable.buyNode1Path);
            }
            if(unlockable.buyNode2Path != null)
            {
                buyNode2 = modBundle.LoadAsset<TerminalNode>(unlockable.buyNode2Path);
            }
            if(unlockable.itemInfoPath != null)
            {
                itemInfo = modBundle.LoadAsset<TerminalNode>(unlockable.itemInfoPath);
            }

            Unlockables.RegisterUnlockable(unlockableAsset, unlockable.storeType, buyNode1, buyNode2, itemInfo, unlockable.initPrice);

        }
        else if (content is CustomEnemy enemy)
        {
            var enemyAsset = modBundle.LoadAsset<EnemyType>(enemy.contentPath);
            NetworkPrefabs.RegisterNetworkPrefab(enemyAsset.enemyPrefab);
            Utilities.FixMixerGroups(enemyAsset.enemyPrefab);
            enemy.enemy = enemyAsset;
            prefabCallback(content, enemyAsset.enemyPrefab);
            enemy.registryCallback(enemyAsset);

            TerminalNode infoNode = null;
            TerminalKeyword infoKeyword = null;
            if(enemy.infoNodePath != null)
            {
                infoNode = modBundle.LoadAsset<TerminalNode>(enemy.infoNodePath);
            }
            if(enemy.infoKeywordPath != null)
            {
                infoKeyword = modBundle.LoadAsset<TerminalKeyword>(enemy.infoKeywordPath);
            }

            if ((int)(enemy.spawnType) == -1)
            {
                Enemies.RegisterEnemy(enemyAsset, enemy.rarity, enemy.LevelTypes, enemy.levelOverrides, infoNode, infoKeyword);
            }
            else
            {
                Enemies.RegisterEnemy(enemyAsset, enemy.rarity, enemy.LevelTypes, enemy.spawnType, enemy.levelOverrides, infoNode, infoKeyword);
            }

        }
        else if (content is MapHazard mapObject)
        {
            var mapObjectAsset = modBundle.LoadAsset<SpawnableMapObjectDef>(mapObject.contentPath);
            mapObject.hazard = mapObjectAsset;
            NetworkPrefabs.RegisterNetworkPrefab(mapObjectAsset.spawnableMapObject.prefabToSpawn);
            Utilities.FixMixerGroups(mapObjectAsset.spawnableMapObject.prefabToSpawn);
            prefabCallback(content, mapObjectAsset.spawnableMapObject.prefabToSpawn);
            mapObject.registryCallback(mapObjectAsset);

            MapObjects.RegisterMapObject(mapObjectAsset, mapObject.LevelTypes, mapObject.levelOverrides, mapObject.spawnRateFunction);

        }
        else if (content is OutsideObject outsideObject)
        {
            var mapObjectAsset = modBundle.LoadAsset<SpawnableOutsideObjectDef>(outsideObject.contentPath);
            outsideObject.mapObject = mapObjectAsset;
            NetworkPrefabs.RegisterNetworkPrefab(mapObjectAsset.spawnableMapObject.spawnableObject.prefabToSpawn);
            Utilities.FixMixerGroups(mapObjectAsset.spawnableMapObject.spawnableObject.prefabToSpawn);
            prefabCallback(content, mapObjectAsset.spawnableMapObject.spawnableObject.prefabToSpawn);
            outsideObject.registryCallback(mapObjectAsset);

            MapObjects.RegisterOutsideObject(mapObjectAsset, outsideObject.LevelTypes, outsideObject.levelOverrides, outsideObject.spawnRateFunction);

        }

        LoadedContent.Add(content.ID, content);
    }

    /// <summary>
    /// Loads and registers an entire array of custom content.
    /// Handles everything for you including network registry.
    /// </summary>
    public void RegisterAll(CustomContent[] content)
    {
        Plugin.logger.LogInfo($"[LethalLib] {modName} is registering {content.Length} content items!");
        foreach(CustomContent c in content)
        {
            Register(c);
        }
    }

    /// <summary>
    /// Loads and registers an entire list of custom content.
    /// Handles everything for you including network registry.
    /// </summary>
    public void RegisterAll(List<CustomContent> content)
    {
        Plugin.logger.LogInfo($"[LethalLib] {modName} is registering {content.Count} content items!");
        foreach (CustomContent c in content)
        {
            Register(c);
        }
    }

    // Content classes
    public class CustomContent
    {
        private string id = "";
        public string ID => id;

        public CustomContent(string id)
        {
            this.id = id;
        }
    }

    public class CustomItem : CustomContent
    {
        public Action<Item> registryCallback = (item) => { };
        public string contentPath = "";
        internal Item item;
        public Item Item => item;

        public CustomItem(string id, string contentPath, Action<Item> registryCallback = null) : base(id)
        {
            this.contentPath = contentPath;
            if(registryCallback != null)
            {
                this.registryCallback = registryCallback;
            }
        }
    }

    public class ShopItem : CustomItem
    {
        public void RemoveFromShop()
        {
            Items.RemoveShopItem(Item);
        }

        public void SetPrice(int price)
        {
            Items.UpdateShopItemPrice(Item, price);
        }

        public int initPrice = 0;
        public string buyNode1Path = null;
        public string buyNode2Path = null;
        public string itemInfoPath = null;

        public ShopItem(string id, string contentPath, int price = 0, string buyNode1Path = null, string buyNode2Path = null, string itemInfoPath = null, Action<Item> registryCallback = null) : base(id, contentPath, registryCallback)
        {
            this.initPrice = price;
            this.buyNode1Path = buyNode1Path;
            this.buyNode2Path = buyNode2Path;
            this.itemInfoPath = itemInfoPath;
        }
    }

    public class ScrapItem : CustomItem
    {
        public void RemoveFromLevels(Levels.LevelTypes levelFlags)
        {
            Items.RemoveScrapFromLevels(Item, levelFlags);
        }

        /// <summary>
        /// THIS IS NEVER USED, ONLY HERE FOR COMPAT
        /// </summary>
        public int Rarity {
            get => 0;
        }

        public Dictionary<Levels.LevelTypes, int> levelRarities = new Dictionary<Levels.LevelTypes, int>();
        public Dictionary<string, int> customLevelRarities = new Dictionary<string, int>();

        public ScrapItem(string id, string contentPath, int rarity, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null, Action<Item> registryCallback = null) : base(id, contentPath, registryCallback)
        {
            // assign level rarities
            if(levelFlags != Levels.LevelTypes.None)
            {
                levelRarities.Add(levelFlags, rarity);
            }
            else if(levelOverrides != null)
            {
                foreach(string s in levelOverrides)
                {
                    customLevelRarities.Add(s, rarity);
                }
            }
        }

        public ScrapItem(string id, string contentPath, Dictionary<Levels.LevelTypes, int>? levelRarities = null, Dictionary<string, int>? customLevelRarities = null, Action<Item> registryCallback = null) : base(id, contentPath, registryCallback)
        {
            if(levelRarities != null)
            {
                this.levelRarities = levelRarities;
            }
            if(customLevelRarities != null)
            {
                this.customLevelRarities = customLevelRarities;
            }
        }
    }

    public class Unlockable : CustomContent
    {
        public Action<UnlockableItem> registryCallback = (unlockable) => { };
        internal UnlockableItem unlockable;
        public UnlockableItem UnlockableItem => unlockable;
        public string contentPath = "";
        public int initPrice = 0;
        public string buyNode1Path = null;
        public string buyNode2Path = null;
        public string itemInfoPath = null;
        public StoreType storeType = StoreType.None;

        public void RemoveFromShop()
        {
            Unlockables.DisableUnlockable(UnlockableItem);
        }

        public void SetPrice(int price)
        {
            Unlockables.UpdateUnlockablePrice(UnlockableItem, price);
        }

        public Unlockable(string id, string contentPath, int price = 0, string buyNode1Path = null, string buyNode2Path = null, string itemInfoPath = null, StoreType storeType = StoreType.None, Action<UnlockableItem> registryCallback = null) : base(id)
        {
            this.contentPath = contentPath;
            if(registryCallback != null)
            {
                this.registryCallback = registryCallback;
            }
            this.initPrice = price;
            this.buyNode1Path = buyNode1Path;
            this.buyNode2Path = buyNode2Path;
            this.itemInfoPath = itemInfoPath;
            this.storeType = storeType;
        }
    }

    public class CustomEnemy : CustomContent
    {
        public Action<EnemyType> registryCallback = (enemy) => { };
        public string contentPath = "";
        internal EnemyType enemy;
        public EnemyType Enemy => enemy;
        public string infoNodePath = null;
        public string infoKeywordPath = null;
        public int rarity = 0;

        public void RemoveFromLevels(Levels.LevelTypes levelFlags)
        {
            Enemies.RemoveEnemyFromLevels(Enemy, levelFlags);
        }

        public Levels.LevelTypes LevelTypes = Levels.LevelTypes.None;
        public string[] levelOverrides = null;

        public Enemies.SpawnType spawnType = (Enemies.SpawnType)(-1);

        public CustomEnemy(string id, string contentPath, int rarity = 0, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, Enemies.SpawnType spawnType = (Enemies.SpawnType)(-1), string[] levelOverrides = null, string infoNodePath = null, string infoKeywordPath = null, Action<EnemyType> registryCallback = null) : base(id)
        {
            this.contentPath = contentPath;
            if(registryCallback != null)
            {
                this.registryCallback = registryCallback;
            }
            this.infoNodePath = infoNodePath;
            this.infoKeywordPath = infoKeywordPath;
            this.rarity = rarity;
            this.LevelTypes = levelFlags;
            this.levelOverrides = levelOverrides;
            this.spawnType = spawnType;
        }
    }


    public class MapHazard : CustomContent
    {
        public Action<SpawnableMapObjectDef> registryCallback = (hazard) => { };
        public string contentPath = "";
        internal SpawnableMapObjectDef hazard;
        public SpawnableMapObjectDef Hazard => hazard;
        public Func<SelectableLevel, AnimationCurve> spawnRateFunction;

        public Levels.LevelTypes LevelTypes = Levels.LevelTypes.None;
        public string[] levelOverrides = null;

        public void RemoveFromLevels(Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null)
        {
            MapObjects.RemoveMapObject(Hazard, levelFlags, levelOverrides);
        }

        public MapHazard(string id, string contentPath, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null, Action<SpawnableMapObjectDef> registryCallback = null) : base(id)
        {
            this.contentPath = contentPath;
            if(registryCallback != null)
            {
                this.registryCallback = registryCallback;
            }
            this.LevelTypes = levelFlags;
            this.levelOverrides = levelOverrides;
            this.spawnRateFunction = spawnRateFunction;
        }
    }

    public class OutsideObject : CustomContent
    {
        public Action<SpawnableOutsideObjectDef> registryCallback = (hazard) => { };
        public string contentPath = "";
        internal SpawnableOutsideObjectDef mapObject;
        public SpawnableOutsideObjectDef MapObject => mapObject;
        public Func<SelectableLevel, AnimationCurve> spawnRateFunction;

        public Levels.LevelTypes LevelTypes = Levels.LevelTypes.None;
        public string[] levelOverrides = null;

        public void RemoveFromLevels(Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null)
        {
            MapObjects.RemoveOutsideObject(MapObject, levelFlags, levelOverrides);
        }

        public OutsideObject(string id, string contentPath, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null, Action<SpawnableOutsideObjectDef> registryCallback = null) : base(id)
        {
            this.contentPath = contentPath;
            if (registryCallback != null)
            {
                this.registryCallback = registryCallback;
            }
            this.LevelTypes = levelFlags;
            this.levelOverrides = levelOverrides;
            this.spawnRateFunction = spawnRateFunction;
        }
    }

}
