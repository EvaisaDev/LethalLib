# LethalLib 0.11.2
- (Hopefully) Fixed issue with Terminal, where when a mod was disabling a shop item, all the shop items after it would mess up their orders.

# LethalLib 0.11.1
- Added check to RegisterNetworkPrefab, to prevent a prefab from being registered multiple times.

# LethalLib 0.11.0
- Added module: PrefabUtils
	- Added method: ClonePrefab()
	- Added method: CreatePrefab()
- Added method: NetworkPrefabs.CreateNetworkPrefab()
	- Creates a network prefab programmatically and registers it with the network manager.
- Added method: NetworkPrefabs.CloneNetworkPrefab()
	- Clones a network prefab programmatically and registers it with the network manager.
- Added behaviour for Items module
	- When a scrap item is registered as a shop item, the LethalLib will now automatically create a copy and switch the IsScrap value.
	- When a shop item is registered as a scrap, the LethalLib will now automataically create a copy, assign sell values, set IsScrap to true, and add a scan node.

# LethalLib 0.10.4
- Added additional error logging and prevented an exception when a custom dungeon RandomMapObject had an invalid prefab assigned.
- Removed LethalExpansion soft dependency as it caused more issues than it was worth.

# LethalLib 0.10.3
- Fixed custom dungeon generation breaking because of Lethal Company update.
- Added soft dependency to LethalExpansion which might help compatibility(?

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
