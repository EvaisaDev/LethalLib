# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## LethalLib [1.1.1]

### Added

- MonoDetour added as a Thunderstore dependency to provide CIL analysis in stack traces when any ILHook (includes HarmonyX transpilers) manipulation target method throws on compilation. LethalLib has no real dependency on it. This change is purely made to make debugging ILHooks/transpilers easier for modders who happen to have LethalLib installed.

## LethalLib [1.1.0]

### Fixed

- Updated mod version to work with V70 Lethal.

## LethalLib [1.0.3]

### Fixed

- Changed the validation to be a clamp and not forced to be 1.

## LethalLib [1.0.2]

### Added

- Validation to scrap and shop items registered with invalid weight values (above 4 and under 1).

## LethalLib [1.0.1]

### Added

- LethalLib NuGet package now ships with xml docs (wow!!)
- Enabled embedded debug symbols for easier to read stacktraces for when LethalLib explodes

### Fixed

- Null checks to avoid errors with loading into lobby with empty MapObjects

## LethalLib [1.0.0]

> [!NOTE]
> Despite the major version jump from 0.16.4 to 1.0.0, no major or breaking changes were made.  
> This change was made to properly follow SemVer to show that LethalLib's public API is stable.  

### Added

- Ability for items, levels, outside and inside mapobjects to register to levels through their LethalLevelLoader content tag.

### Fixed

- mapobjects maybe having the same issues that items and enemies had the previous two versions with case sensitivity and leveltype validation.

## LethalLib [0.16.4]

### Fixed
- `AddEnemyToLevel` needing a `LevelType` to validate custom moon enemy rarities.
- `AddScrapItemToLevel` having the same issue as above.

## LethalLib [0.16.3]

### Fixed
- `GetLLLNameOfLevel` function now returns a lowercase level name so input is no longer case sensitive.

## LethalLib [0.16.2]

### Fixed
- The last return value of `spawnRateFunction` of MapObjects no longer overwrites a map object's spawn curve for each moon.

## LethalLib [0.16.1]

### Fixed
- `Levels.LevelTypes.Vanilla` now works for registering enemies and items on moons.

## LethalLib [0.16.0]

### Added
- Version 50 moons were finally added to the `LevelTypes` enum.
- LethalLib weathers now also get added to LethalLevelLoader moons.

### Changed
- Use `TryGotoNext` instead of `GotoNext` for `StackFrame.AddFrames` ILHook so it doesn't throw if sequence was not found due to another mod patching the method first ([#74](https://github.com/EvaisaDev/LethalLib/pull/74))
- Added a reference to a `ToString` weather enum Hook ([#81](https://github.com/EvaisaDev/LethalLib/pull/81))

### Fixed
- `RemoveWeather`'s first argument was named "levelName", now it is "weatherName".

## LethalLib [0.15.1]

### Fixed
- Custom DungeonFlow registration has been disabled to prevent issues when using mod in current v50 beta versions.

## LethalLib [0.15.0]

### Added
- LethalLib will now also register enemies and items for when LethalLevelLoader adds its moons.

### Changed
- customLevelRarities will now accept the original level name or the level name modified by LethalLevelLoader, meaning enemies and items can target a custom moon using either name

### Fixed
- Enemy and item spawn weights now get applied as one would expect
  - `Levels.LevelTypes.All` no longer overrides all spawn weights
  - `Levels.LevelTypes.Modded` now applies its spawn weights
    - this used to only apply its weight if customLevelRarities contained the level's name
  - customLevelRarities now applies its weights

## LethalLib [0.14.4]

### Fixed
- Added various null checks to prevent crashes and to give better feedback to developers when using custom enemy API.

## LethalLib [0.14.3]

### Fixed
- API for enemy registration with rarity tables works now.

## LethalLib [0.14.2]

### Changed
- Added config option: Extended Logging.
- Reduced the amount of logging LethalLib does by default.

## LethalLib [0.14.1]

### Fixed
- Last update broke the network registry API 💀

## LethalLib [0.14.0]

### Added
- Added enemies to debug menu
 - https://github.com/EvaisaDev/LethalLib/pull/53

## LethalLib [0.13.2]

### Fixed
- Disabled decor was still showing in the shop, added some horrific hax to prevent this.

## LethalLib [0.13.1]

### Fixed
- Map objects were being added every time a lobby was loaded, causing too many to spawn.

## LethalLib [0.13.0]

### Added
- Ability to pass rarity dictionaries for registering enemies.  
- "Modded" LevelTypes flag  

## LethalLib [0.12.1]

### Fixed

- Reverted function signature changes for backwards compatibility reasons.
- Readded some removed properties (These do not do anything now but they are there to prevent old mods from dying.)

## LethalLib [0.12.0]

> [!WARNING]
> Includes potentially breaking changes!

### Added
- Ability to pass rarity dictionaries for registering scrap items.

### Changed
- Cleaned up git repo slightly.  
- Internal changes to the way scrap items are added to levels.  
- When registering the same scrap item multiple times it will be merged with the previous ones.  

## LethalLib [0.11.2]

### Fixed

- (to verify) Issue with Terminal, where when a mod was disabling a shop item,
  all the shop items after it would mess up their orders.

## LethalLib [0.11.1]

### Changed

- RegisterNetworkPrefab now checks prefabs to avoid registering duplicates

## LethalLib [0.11.0]

### Added

- Module: PrefabUtils
  - Method: ClonePrefab()
  - Method: CreatePrefab()
- Method: NetworkPrefabs.CreateNetworkPrefab()
  - Creates a network prefab programmatically and registers it with the network manager.
- Method: NetworkPrefabs.CloneNetworkPrefab()
  - Clones a network prefab programmatically and registers it with the network manager.

### Changed

- Behaviour for Items module
  - When a scrap item is registered as a shop item, the LethalLib
    will now automatically create a copy and switch the IsScrap value.
  - When a shop item is registered as a scrap, the LethalLib will now
    automatically create a copy, assign sell values, set IsScrap to true, and add a scan node.

## LethalLib [0.10.4]

### Added

- Additional error logging and prevented an exception when
  a custom dungeon RandomMapObject had an invalid prefab assigned.

### Removed

- LethalExpansion soft dependency as it caused more issues than it was worth.

## LethalLib [0.10.3]

### Added

- Soft dependency to LethalExpansion which might help compatibility(?)

### Fixed

- Fixed custom dungeon generation breaking because of Lethal Company update.

## LethalLib [0.10.1]

### Fixed

- Fixed issue with Ragdolls system where ragdolls got registered multiple times.

## LethalLib [0.10.0]

> [!WARNING]
> Includes potentially breaking changes!

### Added

- Save system patch which attempts to keep the items array in the same order,
  so that items don't change when you load an old save after mods have updated.
  - This will likely break all existing saves.
- Intellisense comments to all API functions.
- Method: Enemies.RemoveEnemyFromLevels()
- Method: Items.RemoveScrapFromLevels()
- Method: Items.RemoveShopItem()
- Method: Items.UpdateShopItemPrice()
- Method: Unlockables.DisableUnlockable()
- Method: Unlockables.UpdateUnlockablePrice()
- Method: Weathers.RemoveWeather()
- Method: MapObjects.RemoveMapObject()
- Method: MapObjects.RemoveOutsideObject()
- Added Module: ContentLoader
  - This acts as an alternative way to register content, abstracting
    some extra stuff away such as network registry and asset loading.
- Added Module: Player
  - Method: RegisterPlayerRagdoll()
  - Method: GetRagdollIndex()
  - Method: GetRagdoll()
