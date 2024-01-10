# LethalLib  

[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/lordfirespeed/lethallib/build.yml?style=for-the-badge&logo=github)](https://github.com/Lordfirespeed/lethallib/actions/workflows/build.yml)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Evaisa/LethalLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Evaisa/LethalLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/)

**A library for adding new content to Lethal Company, mainly for personal use.**

## Features

Currently includes:   
- Custom Scrap Item API  
- Custom Shop Item API  
- Unlockables API  
- Map Objects API
- Dungeon API
- Custom Enemy API  
- Network Prefab API  
- Weather API  

## Changes

See the [changelog](https://github.com/EvaisaDev/LethalLib/blob/main/CHANGELOG.md) for changes by-version and unreleased changes.

## Contributing 

You will need to create a `LethalLib/LethalLib.csproj.user` file to provide your Lethal Company game directory path.

### Example `LethalLib.csproj.user`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <PropertyGroup>
        <LETHAL_COMPANY_DIR>C:/Program Files (x86)/Steam/steamapps/common/Lethal Company</LETHAL_COMPANY_DIR>
        <TEST_PROFILE_DIR>$(APPDATA)/r2modmanPlus-local/LethalCompany/profiles/Test LethalLib</TEST_PROFILE_DIR>
    </PropertyGroup>

    <!-- Create your 'Test Profile' using your modman of choice before enabling this. 
    Enable by setting the Condition attribute to "true". *nix users should switch out `copy` for `cp`. -->
    <Target Name="CopyToTestProfile" DependsOnTargets="NetcodePatch" AfterTargets="PostBuildEvent" Condition="false">
        <MakeDir
                Directories="$(TEST_PROFILE_DIR)/BepInEx/plugins/Evaisa-LethalLib/LethalLib"
                Condition="Exists('$(TEST_PROFILE_DIR)') And !Exists('$(TEST_PROFILE_DIR)/BepInEx/plugins/Evaisa-LethalLib/LethalLib')"
        />
        <Exec Command="copy &quot;$(TargetPath)&quot; &quot;$(TEST_PROFILE_DIR)/BepInEx/plugins/Evaisa-LethalLib/LethalLib/&quot;" />
    </Target>
</Project>
```