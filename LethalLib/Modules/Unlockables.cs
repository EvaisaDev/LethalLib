using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LethalLib.Modules.Items;
using UnityEngine;
using System.Reflection;
using LethalLib.Extras;
using static LethalLib.Extras.UnlockableItemDef;
using static LethalLib.Modules.Unlockables;
using static LethalLib.Modules.ContentLoader;

namespace LethalLib.Modules
{
    public enum StoreType
    {
        None,
        ShipUpgrade,
        Decor
    }
    public class Unlockables
    {
        public class RegisteredUnlockable
        {
            public UnlockableItem unlockable;
            public StoreType StoreType;
            public TerminalNode buyNode1;
            public TerminalNode buyNode2;
            public TerminalNode itemInfo;
            public int price;
            public string modName;
            public bool disabled = false;

            public RegisteredUnlockable(UnlockableItem unlockable, TerminalNode buyNode1 = null, TerminalNode buyNode2 = null, TerminalNode itemInfo = null, int price = -1)
            {
                this.unlockable = unlockable;
                this.buyNode1 = buyNode1;
                this.buyNode2 = buyNode2;
                this.itemInfo = itemInfo;
                this.price = price;
            }
        }

        public static List<RegisteredUnlockable> registeredUnlockables = new List<RegisteredUnlockable>();

        public static void Init()
        {
            On.Terminal.Awake += Terminal_Awake;
            On.Terminal.TextPostProcess += Terminal_TextPostProcess;
        }

        private static string Terminal_TextPostProcess(On.Terminal.orig_TextPostProcess orig, Terminal self, string modifiedDisplayText, TerminalNode node)
        {
            if (modifiedDisplayText.Contains("[buyableItemsList]") && modifiedDisplayText.Contains("[unlockablesSelectionList]"))
            {
                // create new line after first colon
                var index = modifiedDisplayText.IndexOf(@":");

                // example: "* Loud horn    //    Price: $150"
                foreach (var unlockable in registeredUnlockables)
                {
                    if (unlockable.StoreType == StoreType.ShipUpgrade && !unlockable.disabled)
                    {

                        var unlockableName = unlockable.unlockable.unlockableName;
                        var unlockablePrice = unlockable.price;

                        var newLine = $"\n* {unlockableName}    //    Price: ${unlockablePrice}";

                        modifiedDisplayText = modifiedDisplayText.Insert(index + 1, newLine);
                    }
                    
                }

            }


            return orig(self, modifiedDisplayText, node);
        }

        public struct BuyableUnlockableAssetInfo
        {
            public UnlockableItem itemAsset;
            public TerminalKeyword keyword;
        }

        public static List<BuyableUnlockableAssetInfo> buyableUnlockableAssetInfos = new List<BuyableUnlockableAssetInfo>();

        private static void Terminal_Awake(On.Terminal.orig_Awake orig, Terminal self)
        {
            var startOfRound = StartOfRound.Instance;

            Plugin.logger.LogInfo($"Adding {registeredUnlockables.Count} unlockables to unlockables list");
            foreach (var unlockable in registeredUnlockables)
            {
                if (startOfRound.unlockablesList.unlockables.Any((UnlockableItem x) => x.unlockableName == unlockable.unlockable.unlockableName))
                {
                    Plugin.logger.LogInfo((object)("Unlockable " + unlockable.unlockable.unlockableName + " already exists in unlockables list, skipping"));
                    continue;
                }


                if (unlockable.unlockable.prefabObject != null)
                {
                    var placeable = unlockable.unlockable.prefabObject.GetComponentInChildren<PlaceableShipObject>();
                    if (placeable != null)
                    {
                        placeable.unlockableID = startOfRound.unlockablesList.unlockables.Count;
                    }
                }
                startOfRound.unlockablesList.unlockables.Add(unlockable.unlockable);
            }

            var buyKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
            var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
            var infoKeyword = self.terminalNodes.allKeywords.First(keyword => keyword.word == "info");

            var shopItems = registeredUnlockables.FindAll(unlockable => unlockable.price != -1).ToList();

            Plugin.logger.LogInfo($"Adding {shopItems.Count} items to terminal");
            foreach (var item in shopItems)
            {
                string itemName = item.unlockable.unlockableName;

                var keyword = TerminalUtils.CreateTerminalKeyword(itemName.ToLowerInvariant().Replace(" ", "-"), defaultVerb: buyKeyword);


                if (self.terminalNodes.allKeywords.Any((TerminalKeyword kw) => kw.word == keyword.word))
                {
                    Plugin.logger.LogInfo((object)("Keyword " + keyword.word + " already registed, skipping."));
                    continue;
                }



                var itemIndex = StartOfRound.Instance.unlockablesList.unlockables.FindIndex(unlockable => unlockable.unlockableName == item.unlockable.unlockableName);

                var wah = StartOfRound.Instance;

                if(wah == null)
                {
                    Debug.Log("STARTOFROUND INSTANCE NOT FOUND");
                }

                item.disabled = false;
                
               if (item.price == -1 && item.buyNode1 != null)
               {
                   item.price = item.buyNode1.itemCost;
               }

               var lastChar = itemName[itemName.Length - 1];
               var itemNamePlural = itemName;

              var buyNode2 = item.buyNode2;

              if (buyNode2 == null)
              {
                  buyNode2 = ScriptableObject.CreateInstance<TerminalNode>();

                  buyNode2.name = $"{itemName.Replace(" ", "-")}BuyNode2";
                  buyNode2.displayText = $"Ordered [variableAmount] {itemNamePlural}. Your new balance is [playerCredits].\n\nOur contractors enjoy fast, free shipping while on the job! Any purchased items will arrive hourly at your approximate location.\r\n\r\n";
                  buyNode2.clearPreviousText = true;
                  buyNode2.maxCharactersToType = 15;


              }
              buyNode2.buyItemIndex = -1;
              buyNode2.shipUnlockableID = itemIndex;
              buyNode2.buyUnlockable = true;
              buyNode2.creatureName = itemName;
              buyNode2.isConfirmationNode = false;
              buyNode2.itemCost = item.price;
              buyNode2.playSyncedClip = 0;

              var buyNode1 = item.buyNode1;
              if (buyNode1 == null)
              {
                  buyNode1 = ScriptableObject.CreateInstance<TerminalNode>();
                  buyNode1.name = $"{itemName.Replace(" ", "-")}BuyNode1";
                  buyNode1.displayText = $"You have requested to order {itemNamePlural}. Amount: [variableAmount].\nTotal cost of items: [totalCost].\n\nPlease CONFIRM or DENY.\r\n\r\n";
                  buyNode1.clearPreviousText = true;
                  buyNode1.maxCharactersToType = 35;
              }

              buyNode1.buyItemIndex = -1;
              buyNode1.shipUnlockableID = itemIndex;
              buyNode1.creatureName = itemName;
              buyNode1.isConfirmationNode = true;
              buyNode1.overrideOptions = true;
              buyNode1.itemCost = item.price;
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

              if (item.StoreType == StoreType.Decor)
              {
                  item.unlockable.shopSelectionNode = buyNode1;
              }
              else
              {
                  item.unlockable.shopSelectionNode = null;
              }

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
              }

              self.terminalNodes.allKeywords = allKeywords.ToArray();

              var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
              itemInfoNouns.Add(new CompatibleNoun()
              {
                  noun = keyword,
                  result = itemInfo
              });
              infoKeyword.compatibleNouns = itemInfoNouns.ToArray();

                var buyableItemAssetInfo = new BuyableUnlockableAssetInfo()
                {
                    itemAsset = item.unlockable,
                    keyword = keyword
                };

                buyableUnlockableAssetInfos.Add(buyableItemAssetInfo);

                Plugin.logger.LogInfo($"{item.modName} registered item: {item.unlockable.unlockableName}");
            }

            orig(self);
        }



        ///<summary>
        ///Registers a unlockable.
        /// </summary>
        public static void RegisterUnlockable(UnlockableItemDef unlockable, int price = -1, StoreType storeType = StoreType.None)
        {
            RegisterUnlockable(unlockable.unlockable, storeType, null, null, null, price);
        }

        ///<summary>
        ///Registers a unlockable.
        /// </summary>
        public static void RegisterUnlockable(UnlockableItem unlockable, int price = -1, StoreType storeType = StoreType.None)
        {
            RegisterUnlockable(unlockable, storeType, null, null, null, price);
        }

        ///<summary>
        ///Registers a unlockable.
        /// </summary>
        public static void RegisterUnlockable(UnlockableItemDef unlockable, StoreType storeType = StoreType.None, TerminalNode buyNode1 = null, TerminalNode buyNode2 = null, TerminalNode itemInfo = null, int price = -1)
        {
            RegisterUnlockable(unlockable.unlockable, storeType, buyNode1, buyNode2, itemInfo, price);
        }

        ///<summary>
        ///Registers a unlockable.
        /// </summary>
        public static void RegisterUnlockable(UnlockableItem unlockable, StoreType storeType = StoreType.None, TerminalNode buyNode1 = null, TerminalNode buyNode2 = null, TerminalNode itemInfo = null, int price = -1)
        {
            var unlock = new RegisteredUnlockable(unlockable, buyNode1, buyNode2, itemInfo, price);
            var callingAssembly = Assembly.GetCallingAssembly();
            var modDLL = callingAssembly.GetName().Name;
            unlock.modName = modDLL;
            unlock.StoreType = storeType;

            registeredUnlockables.Add(unlock);
        }

        ///<summary>
        ///Removes a unlockable.
        ///This needs to be called after StartOfRound.Awake, can be used for config sync.
        ///This is prone to breaking saves.
        /// </summary>
        public static void DisableUnlockable(UnlockableItemDef unlockable)
        {
            DisableUnlockable(unlockable.unlockable);
        }

        ///<summary>
        ///Removes a unlockable.
        ///This needs to be called after StartOfRound.Awake, can be used for config sync.
        ///This is prone to breaking saves.
        /// </summary>
        public static void DisableUnlockable(UnlockableItem unlockable)
        {
            if (StartOfRound.Instance != null)
            {
                var allKeywords = terminal.terminalNodes.allKeywords.ToList();
                var infoKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "info");
                var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
                var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
                var nouns = buyKeyword.compatibleNouns.ToList();
                var itemInfoNouns = infoKeyword.compatibleNouns.ToList();
                RegisteredUnlockable registeredUnlockable = registeredUnlockables.Find(unlock => unlock.unlockable == unlockable);

                registeredUnlockable.disabled = true;

                if (buyableUnlockableAssetInfos.Any(x => x.itemAsset == unlockable))
                {


                    var asset = buyableUnlockableAssetInfos.First(x => x.itemAsset == unlockable);
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
        ///Updates the price of an already registered unlockable
        ///This needs to be called after StartOfRound.Awake.
        ///Only works for items registered by LethalLib.
        ///</summary>
        public static void UpdateUnlockablePrice(UnlockableItem shopItem, int price)
        {
            if (StartOfRound.Instance != null)
            {
                var buyKeyword = terminal.terminalNodes.allKeywords.First(keyword => keyword.word == "buy");
                var cancelPurchaseNode = buyKeyword.compatibleNouns[0].result.terminalOptions[1].result;
                var nouns = buyKeyword.compatibleNouns.ToList();
                RegisteredUnlockable registeredUnlockable = registeredUnlockables.Find(unlock => unlock.unlockable == shopItem);

                if(registeredUnlockable != null && registeredUnlockable.price != -1)
                {
                    registeredUnlockable.price = price;
                }

                if (buyableUnlockableAssetInfos.Any(x => x.itemAsset == shopItem))
                {
                    var asset = buyableUnlockableAssetInfos.First(x => x.itemAsset == shopItem);

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
                            foreach (var option in node.terminalOptions)
                            {
                                if (option.result != null && option.result.buyItemIndex != -1)
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
}
