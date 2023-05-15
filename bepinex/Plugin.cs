using BepInEx;
using HarmonyLib;
using KSP.IO;
using KSP.Modding;
using System.Reflection;
using UnityEngine;

namespace ksp2_papi_bepinex
{
    [BepInPlugin("ksp2-papi-bepinex", "KSP2 PAPI", "0.0.0.2")]//PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin ksp2-ksp2_papi_bepinex-BepInEx is loaded!");
            var harmony = new Harmony("ksp2-papi-bepinex");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(KSP2Mod), nameof(KSP2Mod.Load))]
    public class Patch1
    {
        static void Postfix(KSP2Mod __instance)
        {
            if (__instance.ModName == "KSP2_PAPI")
            {
                Assembly assembly = Assembly.LoadFrom(__instance.ModRootPath + IOProvider.DirectorySeparatorCharacter.ToString() + "ksp2_papi.dll");
                new GameObject("KSP2_PAPI").AddComponent(assembly.GetType("KSP2_PAPI.KSP_PAPI_Loader"));
            }
        }
    }
}
