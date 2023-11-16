using BepInEx.Logging;
using LethalThings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static LethalLib.Modules.Enemies;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System.Diagnostics;
using System.Reflection;

namespace LethalLib.Modules
{
    public class Items
    {
        public static void Init()
        {
            On.StartOfRound.Awake += RegisterLevelScrap;
        }

        public static List<ScrapItem> scrapItems = new List<ScrapItem>();

        private static void RegisterLevelScrap(On.StartOfRound.orig_Awake orig, StartOfRound self)
        {

            foreach (SelectableLevel level in self.levels)
            {
                var name = level.name;

                if (Enum.IsDefined(typeof(Levels.LevelTypes), name))
                {
                    var levelEnum = (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);
                    foreach (ScrapItem scrapItem in scrapItems)
                    {
                        if (scrapItem.spawnLevels.HasFlag(levelEnum))
                        {
                            var spawnableÍtemWithRarity = new SpawnableItemWithRarity()
                            {
                                spawnableItem = scrapItem.item,
                                rarity = scrapItem.rarity
                            };

                            // make sure spawnableScrap does not already contain item
                            Plugin.logger.LogInfo($"Checking if {scrapItem.item.name} is already in {name}");

                            if (!level.spawnableScrap.Any(x => x.spawnableItem == scrapItem.item))
                            {
                                level.spawnableScrap.Add(spawnableÍtemWithRarity);
                                Plugin.logger.LogInfo($"Added {scrapItem.item.name} to {name}");
                            }
                        }
                    }

                }
            }



            foreach (ScrapItem scrapItem in scrapItems)
            {
                if (!self.allItemsList.itemsList.Contains(scrapItem.item))
                {
                    Plugin.logger.LogInfo($"Item registered: {scrapItem.item.name}");
                    self.allItemsList.itemsList.Add(scrapItem.item);
                }
            }

        }

        public class ScrapItem
        {
            public Item item;
            public int rarity;
            public Levels.LevelTypes spawnLevels;

            public ScrapItem(Item item, int rarity, Levels.LevelTypes spawnLevels)
            {
                this.item = item;
                this.rarity = rarity;
                this.spawnLevels = spawnLevels;
            }
        }


        public static void RegisterScrap(Item spawnableItem, int rarity, Levels.LevelTypes levelFlags)
        {
            var scrapItem = new ScrapItem(spawnableItem, rarity, levelFlags);

            scrapItems.Add(scrapItem);
        }

    }
}
