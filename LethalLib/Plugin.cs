using BepInEx;
using HarmonyLib;
using LethalLib.Modules;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Enemies;

namespace LethalLib
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGUID = "evaisa.lethallib";
        public const string ModName = "LethalLib";
        public const string ModVersion = "0.4.5";

        public static AssetBundle MainAssets;

        public static BepInEx.Logging.ManualLogSource logger;


        private void Awake()
        {
            logger = Logger;

            Logger.LogInfo($"LethalLib loaded!!");

            new ILHook(typeof(StackTrace).GetMethod("AddFrames", BindingFlags.Instance | BindingFlags.NonPublic), IlHook);

            Enemies.Init();
            Items.Init();
            Unlockables.Init();
            LethalLib.Modules.NetworkPrefabs.Init();

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
}