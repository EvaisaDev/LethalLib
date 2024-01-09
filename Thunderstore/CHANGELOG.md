# LethalLib 0.10.1  
- Fixed issue with Ragdolls system where ragdolls got registered multiple times.  

# LethalLib 0.10.0
- [**BREAKING CHANGE**] Added save system patch which attempts to keep the items array in the same order, so that items don't change when you load an old save after mods have updated.  
	- This is experimental and currently locked behind a config setting, may break old saves.
- Added Intellisense comments to all API functions.
- Added method: Enemies.RemoveEnemyFromLevels()
- Added method: Items.RemoveScrapFromLevels()
- Added method: Items.RemoveShopItem()
- Added method: Items.UpdateShopItemPrice()
- Added method: Unlockables.DisableUnlockable()
- Added method: Unlockables.UpdateUnlockablePrice()
- Added method: Weathers.RemoveWeather()
- Added method: MapObjects.RemoveMapObject()
- Added method: MapObjects.RemoveOutsideObject()
- Added Module: ContentLoader
	- This acts as an alternative way to register content, abstracting some extra stuff away such as network registry and asset loading.  
- Added Module: Player  
	- Added method: RegisterPlayerRagdoll()  
	- Added method: GetRagdollIndex()  
	- Added method: GetRagdoll()  
- Added Module: Utilities
	- Added method: FixMixerGroups()
	
# LethalLib 0.9.0  
- "All" levels enum now includes modded maps.  
- Added "Vanilla" levels enum.  
- Added overflow for levelOverrides to Dungeon API, Enemies API, Items API, Map Objects API, and Weathers API  
	- which can be used to add to specific levels by name rather than enum.  
