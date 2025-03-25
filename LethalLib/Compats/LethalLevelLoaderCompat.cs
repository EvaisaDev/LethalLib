using BepInEx.Bootstrap;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LethalLib.Compats;
internal static class LethalLevelLoaderCompat
{
    public static bool LethalLevelLoaderExists => Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader");

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static List<string> TryGetLLLTagsFromLevels(SelectableLevel level)
    {
        if (LethalLevelLoaderExists)
        {
            return GetLLLTagsFromLevel(level); // do i need to make another method? i forgor
        }
        return new();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static List<string> GetLLLTagsFromLevel(SelectableLevel level)
    {
        List<string> tagsForLevel = [];
        foreach (LethalLevelLoader.ExtendedLevel extendedLevel in LethalLevelLoader.PatchedContent.CustomExtendedLevels)
        {
            if (extendedLevel.SelectableLevel != level) continue;
            foreach (LethalLevelLoader.ContentTag tag in extendedLevel.ContentTags)
            {
                tagsForLevel.Add(tag.contentTagName.Trim().ToLowerInvariant());
            }
            break;
        }

        foreach (LethalLevelLoader.ExtendedLevel extendedLevel in LethalLevelLoader.PatchedContent.VanillaExtendedLevels)
        {
            if (extendedLevel.SelectableLevel != level) continue;
            foreach (LethalLevelLoader.ContentTag tag in extendedLevel.ContentTags)
            {
                tagsForLevel.Add(tag.contentTagName.Trim().ToLowerInvariant());
            }
            break;
        }

        return tagsForLevel;
    }
}