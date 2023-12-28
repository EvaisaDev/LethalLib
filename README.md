# LethalLib  
**A library for adding new content to Lethal Company, mainly for personal use.**
  
https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/
  
Currently includes:   
- Custom Scrap Item API  
- Custom Shop Item API  
- Unlockables API  
- Map Objects API
- Dungeon API
- Custom Enemy API  
- Network Prefab API  
- Weather API  

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
- Added method: MapObjects.RemoveMapObject()
- Added method: MapObjects.RemoveOutsideObject()