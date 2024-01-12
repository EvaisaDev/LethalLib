#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using LethalLib.Extras;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace LethalLib.Modules;

public class Items
{
    public static ConfigEntry<bool> useSavedataFix;
    public static GameObject scanNodePrefab;

    public static void Init()
    {
        useSavedataFix = Plugin.config.Bind<bool>("Items", "EnableItemSaveFix", false, "Allow for LethalLib to store/reorder the item list, which should fix issues where items get reshuffled when loading an old save. This is experimental and may cause save corruptions occasionally.");

        scanNodePrefab = Plugin.MainAssets.LoadAsset<GameObject>("Assets/Custom/ItemScanNode.prefab");

        On.StartOfRound.Start += StartOfRound_Start;
        On.Terminal.Awake += Terminal_Awake;
        On.Terminal.TextPostProcess += Terminal_TextPostProcess;
    }

    private static string Terminal_TextPostProcess(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifiedDisplayText, TerminalNode node)
    {
        var oldItemList = self.buyableItemsList.ToList();
        var itemList = self.buyableItemsList.ToList();

        // remove any disabled items, this is horrific for performance but i do not have a better solution rn
        itemList.RemoveAll(x => {
            var actualItem = shopItems.FirstOrDefault(item => item.origItem == x || item.item == x);
            if (actualItem != null)
            {
                return actualItem.wasRemoved;
            }
            return false;
        });

        self.buyableItemsList = itemList.ToArray();
        var output = orig(self, modifiedDisplayText, node);

        self.buyableItemsList = oldItemList.ToArray();

        return output;
    }

    public struct ItemSaveOrderData
    {
        public int itemId;
        public string itemName;
        public string assetName;
    }

    public struct BuyableItemAssetInfo
    {
        public Item itemAsset;
        public TerminalKeyword keyword;
    }

    public static List<Item> LethalLibItemList = new List<Item>();
    public static List<BuyableItemAssetInfo> buyableItemAssetInfos = new List<BuyableItemAssetInfo>();
    public static Terminal terminal;

    private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        // Savedata fix, not sure if this works properly because my savegames have been randomly getting corrupted.
        if (useSavedataFix.Value && self.IsHost)
        {
            Plugin.logger.LogInfo($"Fixing Item savedata!!");

            List<ItemSaveOrderData> itemList = new List<ItemSaveOrderData>();

            StartOfRound.Instance.allItemsList.itemsList.ForEach(item =>
            {
                itemList.Add(new ItemSaveOrderData()
                {
                    itemId = item.itemId,
                    itemName = item.itemName,
                    assetName = item.name
                });
            });



            // load itemlist from es3
            if (ES3.KeyExists("LethalLibAllItemsList", GameNetworkManager.Instance.currentSaveFileName))
            {
                // load itemsList
                itemList = ES3.Load<List<ItemSaveOrderData>>("LethalLibAllItemsList", GameNetworkManager.Instance.currentSaveFileName);
            }

            // sort so that items are in the same order as they were when the game was saved
            // if item is not in list, add it at the end
            List<Item> list = StartOfRound.Instance.allItemsList.itemsList;

            List<Item> newList = new List<Item>();

            foreach (ItemSaveOrderData item in itemList)
            {
                var itemInList = list.FirstOrDefault(x => x.itemId == item.itemId && x.itemName == item.itemName && item.assetName == x.name);

                // add in correct place, if there is a gap, we want to add an empty Item scriptable object
                if (itemInList != null)
                {
                    newList.Add(itemInList);
                }
                else
                {
                    newList.Add(ScriptableObject.CreateInstance<Item>());
                }
            }

            foreach (Item item in list)
            {
                if (!newList.Contains(item))
                {
                    newList.Add(item);
                }
            }

            StartOfRound.Instance.allItemsList.itemsList = newList;

            // save itemlist to es3
            ES3.Save<List<ItemSaveOrderData>>("LethalLibAllItemsList", itemList, GameNetworkManager.Instance.currentSaveFileName);

            // loop and print
            /*for (int i = 0; i < StartOfRound.Instance.allItemsList.itemsList.Count; i++)
            {
                var item = StartOfRound.Instance.allItemsList.itemsList[i];
                Plugin.logger.LogInfo($"Item {i}: Name: {item.itemName} - ItemID: {item.itemId} - AssetName: {item.name}");
            }*/
        }

        orig(self);
    }

    private static void Terminal_Awake(On.Terminal.orig_Awake orig, Terminal self)
    {
        var startOfRound = StartOfRound.Instance;

        foreach (ScrapItem scrapItem in scrapItems)
        {

            foreach (SelectableLevel level in startOfRound.levels)
            {
                var name = level.name;

                var alwaysValid = scrapItem.spawnLevels.HasFlag(Levels.LevelTypes.All) || (scrapItem.spawnLevelOverrides != null && scrapItem.spawnLevelOverrides.Any(item => item.ToLowerInvariant() == name.ToLowerInvariant()));

                if (Enum.IsDefined(typeof(Levels.LevelTypes), name) || alwaysValid)
                {
                    var levelEnum = alwaysValid ? Levels.LevelTypes.All : (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);

                    if (alwaysValid || scrapItem.spawnLevels.HasFlag(levelEnum))
                    {
                        var spawnableÍtemWithRarity = new SpawnableItemWithRarity()
                        {
                            spawnableItem = scrapItem.item,
                            rarity = scrapItem.rarity
                        };

                        // make sure spawnableScrap does not already contain item
                        //Plugin.logger.LogInfo($"Checking if {scrapItem.item.name} is already in {name}");

                        if (!level.spawnableScrap.Any(x => x.spawnableItem == scrapItem.item))
                        {
                            level.spawnableScrap.Add(spawnableÍtemWithRarity);
                            //Plugin.logger.LogInfo($"Added {scrapItem.item.name} to {name}");


                        }
                    }
                }
            }
        }

        // startOfRound.allItemsList.itemsList.RemoveAll(x => LethalLibItemList.Contains(x));

        foreach (ScrapItem scrapItem in scrapItems)
        {
            if (!startOfRound.allItemsList.itemsList.Contains(scrapItem.item))
            {
                if (scrapItem.modName != "LethalLib")
                {
                    Plugin.logger.LogInfo($"{scrapItem.modName} registered scrap item: {scrapItem.item.itemName}");
                }
                else
                {
                    Plugin.logger.LogInfo($"Registered scrap item: {scrapItem.item.itemName}");
                }

                LethalLibItemList.Add(scrapItem.item);

                startOfRound.allItemsList.itemsList.Add(scrapItem.item);
            }
        }

        foreach (ShopItem shopItem in shopItems)
        {
            if (!startOfRound.allItemsList.itemsList.Contains(shopItem.item))
            {
                if (shopItem.modName != "LethalLib")
                {
                    Plugin.logger.LogInfo($"{shopItem.modName} registered shop item: {shopItem.item.itemName}");
                }
                else
                {
                    Plugin.logger.LogInfo($"Registered shop item: {shopItem.item.itemName}");
                }

                LethalLibItemList.Add(shopItem.item);

                startOfRound.allItemsList.itemsList.Add(shopItem.item);
            }
        }

        foreach (PlainItem plainItem in plainItems)
        {
            if (!startOfRound.allItemsList.itemsList.Contains(plainItem.item))
            {
                if (plainItem.modName != "LethalLib")
                {
                    Plugin.logger.LogInfo($"{plainItem.modName} registered item: {plainItem.item.itemName}");
                }
                else
                {
                    Plugin.logger.LogInfo($"Registered item: {plainItem.item.itemName}");
                }

                LethalLibItemList.Add(plainItem.item);

                startOfRound.allItemsList.itemsList.Add(plainItem.item);
            }
        }



        terminal = self;
        var itemList = self.buyableItemsList.ToList();

        var buyKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
        var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
        var infoKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "info");



        Plugin.logger.LogInfo($"Adding {shopItems.Count} items to terminal");
        foreach (ShopItem item in shopItems)
        {
            if (itemList.Any((Item x) => x.itemName == item.item.itemName) && !item.wasRemoved)
            {
                Plugin.logger.LogInfo((object)("Item " + item.item.itemName + " already exists in terminal, skipping"));
                continue;
            }

            item.wasRemoved = false;

            if (item.price == -1)
            {
                item.price = item.item.creditsWorth;
            }
            else
            {
                item.item.creditsWorth = item.price;
            }

            var oldIndex = -1;

            if (!itemList.Any((Item x) => x == item.item))
            {
                itemList.Add(item.item);
            }
            else
            {
                oldIndex = itemList.IndexOf(item.item);
            }

            var newIndex = oldIndex == -1 ? itemList.Count - 1 : oldIndex;

            var itemName = item.item.itemName;
            var lastChar = itemName[itemName.Length - 1];
            var itemNamePlural = itemName;

            Plugin.logger.LogInfo($"Adding {itemName} to terminal");

            var buyNode2 = item.buyNode2;

            if(buyNode2 == null)
            {
                buyNode2 = ScriptableObject.CreateInstance<TerminalNode>();

                buyNode2.name = $"{itemName.Replace(" ", "-")}BuyNode2";
                buyNode2.displayText = $"Ordered [variableAmount] {itemNamePlural}. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                buyNode2.clearPreviousText = true;
                buyNode2.maxCharactersToType = 15;

                Plugin.logger.LogInfo($"Generating buynode2");

            }

            buyNode2.buyItemIndex = newIndex;
            buyNode2.isConfirmationNode = false;
            buyNode2.itemCost = item.price;
            buyNode2.playSyncedClip = 0;

            Plugin.logger.LogInfo($"Item price: {buyNode2.itemCost}, Item index: {buyNode2.buyItemIndex}");

            var buyNode1 = item.buyNode1;
            if (buyNode1 == null)
            {
                buyNode1 = ScriptableObject.CreateInstance<TerminalNode>();
                buyNode1.name = $"{itemName.Replace(" ", "-")}BuyNode1";
                buyNode1.displayText = $"You have requested to order {itemNamePlural}. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                buyNode1.clearPreviousText = true;
                buyNode1.maxCharactersToType = 35;

                Plugin.logger.LogInfo($"Generating buynode1");
                    
            }

            buyNode1.buyItemIndex = newIndex;
            buyNode1.isConfirmationNode = true;
            buyNode1.overrideOptions = true;
            buyNode1.itemCost = item.price;

            Plugin.logger.LogInfo($"Item price: {buyNode1.itemCost}, Item index: {buyNode1.buyItemIndex}");

            buyNode1.terminalOptions = new CompatibleNoun[2]
            {
                new CompatibleNoun()
                {
                    noun = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "confirm"),
                    result = buyNode2
                },
                new CompatibleNoun()
                {
                    noun = self.terminalNodes.allKeywords.First(keyword2 => keyword2.word == "deny"),
                    result = cancelPurchaseNode
                }
            };

            var keyword = TerminalUtils.CreateTerminalKeyword(itemName.ToLowerInvariant().Replace(" ", "-"), defaultVerb: buyKeyword);

            Plugin.logger.LogInfo($"Generated keyword: {keyword.word}");

            //self.terminalNodes.allKeywords.AddItem(keyword);
            var allKeywords = self.terminalNodes.allKeywords.ToList();
            allKeywords.Add(keyword);
            self.terminalNodes.allKeywords = allKeywords.ToArray();

            var nouns = buyKeyword.compatibleNouns.ToList();
            nouns.Add(new CompatibleNoun()
            {
                noun = keyword,
                result = buyNode1
            });
            buyKeyword.compatibleNouns = nouns.ToArray();


            var itemInfo = item.itemInfo;
            if (itemInfo == null)
            {
                itemInfo = ScriptableObject.CreateInstance<TerminalNode>();
                itemInfo.name = $"{itemName.Replace(" ", "-")}InfoNode";
                itemInfo.displayText = $"[No information about this object was found.]\n\n";
                itemInfo.clearPreviousText = true;
                itemInfo.maxCharactersToType = 25;

                Plugin.logger.LogInfo($"Generated item info!!");
            }

            self.terminalNodes.allKeywords = allKeywords.ToArray();

            var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
            itemInfoNouns.Add(new CompatibleNoun()
            {
                noun = keyword,
                result = itemInfo
            });
            infoKeyword.compatibleNouns = itemInfoNouns.ToArray();

            BuyableItemAssetInfo buyableItemAssetInfo = new BuyableItemAssetInfo()
            {
                itemAsset = item.item,
                keyword = keyword
            };

            buyableItemAssetInfos.Add(buyableItemAssetInfo);
        }

        self.buyableItemsList = itemList.ToArray();

        orig(self);
    }

    public static List<ScrapItem> scrapItems = new List<ScrapItem>();
    public static List<ShopItem> shopItems = new List<ShopItem>();
    public static List<PlainItem> plainItems = new List<PlainItem>();


    public class ScrapItem
    {
        public Item item;
        public Item origItem;
        public int rarity;
        public Levels.LevelTypes spawnLevels;
        public string[] spawnLevelOverrides;
        public string modName;

        public ScrapItem(Item item, int rarity, Levels.LevelTypes spawnLevels = Levels.LevelTypes.None, string[] spawnLevelOverrides = null)
        {
            origItem = item;
            if (item.isScrap == false)
            {
    
                item = item.Clone();
                item.isScrap = true;
                if(item.maxValue == 0 && item.minValue == 0)
                {
                    item.minValue = 40;
                    item.maxValue = 100;
                }
                else if(item.maxValue == 0)
                {
                    item.maxValue = item.minValue * 2;
                }
                else if(item.minValue == 0)
                {
                    item.minValue = item.maxValue / 2;
                }

                var newPrefab = NetworkPrefabs.CloneNetworkPrefab(item.spawnPrefab);

                if(newPrefab.GetComponent<GrabbableObject>() != null)
                {
                    newPrefab.GetComponent<GrabbableObject>().itemProperties = item;
                }

                if(newPrefab.GetComponentInChildren<ScanNodeProperties>() == null)
                {
                    // add scan node
                    var scanNode = Object.Instantiate(scanNodePrefab, newPrefab.transform);
                    scanNode.name = "ScanNode";
                    scanNode.transform.localPosition = new Vector3(0, 0, 0);
                    var properties = scanNode.GetComponent<ScanNodeProperties>();
                    properties.headerText = item.itemName;
                }

                item.spawnPrefab = newPrefab;
            }
            this.item = item;
            this.rarity = rarity;
            this.spawnLevels = spawnLevels;
            this.spawnLevelOverrides = spawnLevelOverrides;
        }
    }

    public class PlainItem
    {
        public Item item;
        public string modName;

        public PlainItem(Item item)
        {
            this.item = item;
        }
    }

    public class ShopItem
    {
        public Item item;
        public Item origItem;
        public TerminalNode buyNode1;
        public TerminalNode buyNode2;
        public TerminalNode itemInfo;
        public bool wasRemoved = false;
        public int price;
        public string modName;
        public ShopItem(Item item, TerminalNode buyNode1 = null, TerminalNode buyNode2 = null, TerminalNode itemInfo = null, int price = 0)
        {
            origItem = item;
            if (item.isScrap)
            {
                item = item.Clone();
                item.isScrap = false;

                var newPrefab = NetworkPrefabs.CloneNetworkPrefab(item.spawnPrefab);

                if (newPrefab.GetComponent<GrabbableObject>() != null)
                {
                    newPrefab.GetComponent<GrabbableObject>().itemProperties = item;
                }

                if (newPrefab.GetComponentInChildren<ScanNodeProperties>() != null)
                {
                    Object.Destroy(newPrefab.GetComponentInChildren<ScanNodeProperties>().gameObject);
                }

                item.spawnPrefab = newPrefab;
            }
            this.item = item;
            this.price = price;
            if (buyNode1 != null)
            {
                this.buyNode1 = buyNode1;
            }
            if (buyNode2 != null)
            {
                this.buyNode2 = buyNode2;
            }
            if (itemInfo != null)
            {
                this.itemInfo = itemInfo;
            }
        }
    }

    ///<summary>
    ///This method registers a scrap item to the game, making it obtainable in the specified levels.
    ///</summary>
    public static void RegisterScrap(Item spawnableItem, int rarity, Levels.LevelTypes levelFlags)
    {
        var scrapItem = new ScrapItem(spawnableItem, rarity, levelFlags);

        var callingAssembly = Assembly.GetCallingAssembly();
        var modDLL = callingAssembly.GetName().Name;
        scrapItem.modName = modDLL;


        scrapItems.Add(scrapItem);
    }


    ///<summary>
    ///This method registers a scrap item to the game, making it obtainable in the specified levels. With the option to add custom levels to the list.
    ///</summary>
    public static void RegisterScrap(Item spawnableItem, int rarity, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null)
    {
        var scrapItem = new ScrapItem(spawnableItem, rarity, levelFlags, levelOverrides);

        var callingAssembly = Assembly.GetCallingAssembly();
        var modDLL = callingAssembly.GetName().Name;
        scrapItem.modName = modDLL;


        scrapItems.Add(scrapItem);
    }

    ///<summary>
    ///This method registers a shop item to the game.
    ///</summary>
    public static void RegisterShopItem(Item shopItem, TerminalNode buyNode1 = null, TerminalNode buyNode2 = null, TerminalNode itemInfo = null, int price = -1)
    {
        var item = new ShopItem(shopItem, buyNode1, buyNode2, itemInfo, price);
        var callingAssembly = Assembly.GetCallingAssembly();
        var modDLL = callingAssembly.GetName().Name;
        item.modName = modDLL;

        shopItems.Add(item);
    }

    ///<summary>
    ///This method registers a shop item to the game.
    ///</summary>
    public static void RegisterShopItem(Item shopItem, int price = -1)
    {
        var item = new ShopItem(shopItem, null, null, null, price);
        var callingAssembly = Assembly.GetCallingAssembly();
        var modDLL = callingAssembly.GetName().Name;
        item.modName = modDLL;

        shopItems.Add(item);
    }

    ///<summary>
    ///This method registers an item to the game, without making it obtainable in any way.
    ///</summary>
    public static void RegisterItem(Item plainItem)
    {
        var item = new PlainItem(plainItem);
        var callingAssembly = Assembly.GetCallingAssembly();
        var modDLL = callingAssembly.GetName().Name;
        item.modName = modDLL;

        plainItems.Add(item);
    }

    ///<summary>
    ///Removes a scrap from the given levels.
    ///This needs to be called after StartOfRound.Awake.
    /// </summary>
    public static void RemoveScrapFromLevels(Item scrapItem, Levels.LevelTypes levelFlags = Levels.LevelTypes.None, string[] levelOverrides = null)
    {
        if (StartOfRound.Instance != null)
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
                        // find item in scrapItems
                        var actualItem = scrapItems.FirstOrDefault(x => x.origItem == scrapItem || x.item == scrapItem);

                        var spawnableItemWithRarity = level.spawnableScrap.FirstOrDefault(x => x.spawnableItem == actualItem.item);

                        if (spawnableItemWithRarity != null)
                        {
                            level.spawnableScrap.Remove(spawnableItemWithRarity);
                        }

                    }
                }
            }
        }
    }

    ///<summary>
    ///Removes a shop item from the game.
    ///This needs to be called after StartOfRound.Awake.
    ///Only works for items registered by LethalLib.
    ///</summary>
    public static void RemoveShopItem(Item shopItem)
    {
        if (StartOfRound.Instance != null)
        {
            var actualItem = shopItems.FirstOrDefault(x => x.origItem == shopItem || x.item == shopItem);

            // do not remove from list because it fucks up the indexes
            /*
            var itemList = terminal.buyableItemsList.ToList();
            itemList.RemoveAll(x => x == actualItem.item);
            terminal.buyableItemsList = itemList.ToArray();
            */
                
            actualItem.wasRemoved = true;

            var allKeywords = terminal.terminalNodes.allKeywords.ToList();
            var infoKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
            var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
               

            var nouns = buyKeyword.compatibleNouns.ToList();
            var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
            if (buyableItemAssetInfos.Any(x => x.itemAsset == actualItem.item))
            {
                var asset = buyableItemAssetInfos.First(x => x.itemAsset == actualItem.item);
                allKeywords.Remove(asset.keyword);

                nouns.RemoveAll(noun => noun.noun == asset.keyword);
                itemInfoNouns.RemoveAll(noun => noun.noun == asset.keyword);
            }
            terminal.terminalNodes.allKeywords = allKeywords.ToArray();
            buyKeyword.compatibleNouns = nouns.ToArray();
            infoKeyword.compatibleNouns = itemInfoNouns.ToArray();
        }
    }

    ///<summary>
    ///Updates the price of an already registered shop item.
    ///This needs to be called after StartOfRound.Awake.
    ///Only works for items registered by LethalLib.
    ///</summary>
    public static void UpdateShopItemPrice(Item shopItem, int price)
    {
        if (StartOfRound.Instance != null)
        {
            var actualItem = shopItems.FirstOrDefault(x => x.origItem == shopItem || x.item == shopItem);

            actualItem.item.creditsWorth = price;
            var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
            var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            var nouns = buyKeyword.compatibleNouns.ToList();
            if (buyableItemAssetInfos.Any(x => x.itemAsset == actualItem.item))
            {
                var asset = buyableItemAssetInfos.First(x => x.itemAsset == actualItem.item);

                // correct noun
                if (nouns.Any(noun => noun.noun == asset.keyword))
                {
                    var noun = nouns.First(noun => noun.noun == asset.keyword);
                    var node = noun.result;
                    node.itemCost = price;
                    // get buynode 2
                    if (node.terminalOptions.Length > 0)
                    {
                        // loop through terminal options
                        foreach (var option in node.terminalOptions) { 
                            if(option.result != null && option.result.buyItemIndex != -1)
                            {
                                option.result.itemCost = price;
                            }
                        }
                    }
                }
            }
        }
    }

}