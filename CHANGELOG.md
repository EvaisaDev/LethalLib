# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Fixed

### Changed

### Removed

## LethalLib [0.10.0]

> [!WARNING]
> Includes breaking changes!

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