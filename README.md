# LethalLib

[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/evaisadev/lethallib/build.yml?style=for-the-badge&logo=github)](https://github.com/evaisadev/lethallib/actions/workflows/build.yml)
[![Thunderstore Version](https://img.shields.io/thunderstore/v/Evaisa/LethalLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/Evaisa/LethalLib?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/)
[![NuGet Version](https://img.shields.io/nuget/v/evaisa.lethallib?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Evaisa.LethalLib) 

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
- Prefab Utils  
- Weather API  
- ContentLoader  

## Changes

See the [changelog](https://github.com/EvaisaDev/LethalLib/blob/main/CHANGELOG.md) for changes by-version and unreleased changes.

## Contributing

### Fork & Clone

Fork the repository on GitHub and clone your fork locally.

### Configure Git hooks & `post-checkout`

Configure the Git hooks directory for your local copy of the repository:
```sh
git config core.hooksPath hooks/
```

Alternatively, you can create symbolic links in `.git/hooks/*` that point to `../hooks/*`.

Then re-checkout to trigger the `post-checkout` hook:
```sh
git checkout main
```

### `LethalLib.csproj.user`
You will need to create a `LethalLib/LethalLib.csproj.user` file to provide your Lethal Company game directory path.

#### Template
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
        <Exec Command="copy &quot;$(TargetPath)&quot; &quot;$(TestProfileDir)BepInEx/plugins/Evaisa-LethalLib/LethalLib/&quot;" />
    </Target>
</Project>
```
