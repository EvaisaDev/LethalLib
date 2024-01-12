#region

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Configuration;
using LethalLib.Modules;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

#endregion

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace LethalLib;

[BepInPlugin(ModGUID, ModName, ModVersion)]
//[BepInDependency("LethalExpansion", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public const string ModGUID = "evaisa.lethallib";
    public const string ModName = "LethalLib";
    public const string ModVersion = "0.0.0";

    public static AssetBundle MainAssets;

    public static BepInEx.Logging.ManualLogSource logger;
    public static ConfigFile config;

    public static Plugin Instance;

    private void Awake()
    {
        Instance = this;
        config = Config;
        logger = Logger;

        Logger.LogInfo($"LethalLib loaded!!");

        MainAssets = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Info.Location)!, "lethallib"));

        new ILHook(typeof(StackTrace).GetMethod("AddFrames", BindingFlags.Instance | BindingFlags.NonPublic), IlHook);
        Enemies.Init();
        Items.Init();
        Unlockables.Init();
        MapObjects.Init();
        Dungeon.Init();
        Weathers.Init();
        Player.Init();
        Utilities.Init();
        NetworkPrefabs.Init();
    }

    private void IlHook(ILContext il)
    {
        var cursor = new ILCursor(il);
        cursor.GotoNext(
            x => x.MatchCallvirt(typeof(StackFrame).GetMethod("GetFileLineNumber", BindingFlags.Instance | BindingFlags.Public))
        );
        cursor.RemoveRange(2);
        cursor.EmitDelegate<Func<StackFrame, string>>(GetLineOrIL);
    }

    private static string GetLineOrIL(StackFrame instance)
    {
        var line = instance.GetFileLineNumber();
        if (line == StackFrame.OFFSET_UNKNOWN || line == 0)
        {
            return "IL_" + instance.GetILOffset().ToString("X4");
        }

        return line.ToString();
    }
}
