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
        <LethalCompanyDir>C:/Program Files (x86)/Steam/steamapps/common/Lethal Company/</LethalCompanyDir>
        <TestProfileDir>$(APPDATA)/r2modmanPlus-local/LethalCompany/profiles/Test LethalLib/</TestProfileDir>
    </PropertyGroup>

    <!-- Enable by setting the Condition attribute to "true". *nix users should switch out `copy` for `cp`. -->
    <Target Name="CopyToTestProfile" DependsOnTargets="NetcodePatch" AfterTargets="PostBuildEvent" Condition="false">
        <MakeDir
                Directories="$(TestProfileDir)BepInEx/plugins/Evaisa-LethalLib/LethalLib"
                Condition="!Exists('$(TestProfileDir)BepInEx/plugins/Evaisa-LethalLib/LethalLib')"
        />
        <Exec Command="copy &amp;quot;$(TargetPath)&amp;quot; &amp;quot;$(TestProfileDir)BepInEx/plugins/Evaisa-LethalLib/LethalLib/&amp;quot;" />
    </Target>
</Project>
```