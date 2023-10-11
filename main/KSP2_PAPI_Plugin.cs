using BepInEx;
using HarmonyLib;
using KSP.Modding;
using UnityEngine;

namespace ksp2_papi
{
    [BepInDependency("codenade-inputbinder", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("ksp2-papi", "KSP2 PAPI", "0.0.1.0")]//PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class KSP2_PAPI_Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("ksp2-papi");
            harmony.PatchAll(typeof(LoadMod));
        }
    }

    [HarmonyPatch(typeof(KSP2ModManager), nameof(KSP2ModManager.LoadAllMods))]
    public class LoadMod
    {
        public static void Postfix()
        {
            GameObject o = new GameObject("Codenade.ksp2-papi", typeof(PapiManager));
            o.tag = "Game Manager";
            UnityEngine.Object.DontDestroyOnLoad(o);
        }
    }
}
