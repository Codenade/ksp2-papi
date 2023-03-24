using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using KSP.IO;
using KSP.Modding;
using UnityEngine;

namespace ksp2_papi_bepinplugin
{
    [BepInPlugin("codenade.ksp2-papi", "ksp2-papi", "0.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        void Awake()
        {
            HarmonyFileLog.Enabled = true;
            Debug.Log("codenade.ksp2-papi awake");
            var harmony = new Harmony("codenade.ksp2-papi");
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            Logger.LogDebug("codenade.ksp2-papi loaded");
            
        }
    }

    [HarmonyPatch(typeof(KSP2Mod), nameof(KSP2Mod.Load))]
    public class Patch1
    {
        static void Postfix(KSP2Mod __instance)
        {
            if (__instance.ModName == "KSP_PAPI")
            {
                Assembly assembly = Assembly.LoadFrom(__instance.ModRootPath + IOProvider.DirectorySeparatorCharacter.ToString() + "ksp2_papi.dll");
                new GameObject("KSP_PAPI").AddComponent(assembly.GetType("KSP2_PAPI.KSP_PAPI_Loader"));
            }
        }
    }
}
