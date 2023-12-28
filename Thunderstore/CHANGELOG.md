# LethalLib 0.10.0
- [**BREAKING CHANGE**] Added save system patch which attempts to keep the items array in the same order, so that items don't change when you load an old save after mods have updated.  
	- This will likely break all existing saves.
- Added Intellisense comments to all API functions.
- Added method: Enemies.RemoveEnemyFromLevel()
- Added method: Items.RemoveScrapFromLevel()
- Added method: Items.RemoveShopItem()
- Added method: Items.UpdateShopItemPrice()
- Added method: Unlockables.DisableUnlockable()
- Added method: Unlockables.UpdateUnlockablePrice()
- Added method: Weathers.RemoveWeather()


# LethalLib 0.9.0  
- "All" levels enum now includes modded maps.  
- Added "Vanilla" levels enum.  
- Added overflow for levelOverrides to Dungeon API, Enemies API, Items API, Map Objects API, and Weathers API  
	- which can be used to add to specific levels by name rather than enum.  
