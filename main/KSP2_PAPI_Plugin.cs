using BepInEx;
using HarmonyLib;
using KSP.Modding;
using UnityEngine;

namespace ksp2_papi
{
    [BepInPlugin("ksp2-papi", "KSP2 PAPI", MyPluginInfo.PLUGIN_VERSION)]
    public class KSP2_PAPI_Plugin : BaseUnityPlugin
    {
        public static KSP2_PAPI_Plugin Instance { get; protected set; }

        private void Awake()
        {
            Instance ??= this;
            var harmony = new Harmony("ksp2-papi");
            harmony.PatchAll(typeof(Patches));
        }
    }

    public static class Patches
    {
        [HarmonyPatch(typeof(KSP2ModManager), nameof(KSP2ModManager.LoadAllMods))]
        [HarmonyPostfix]
        public static void LoadAllMods()
        {
            GameObject o = new GameObject("Codenade.ksp2-papi", typeof(PapiManager));
            Object.DontDestroyOnLoad(o);
        }
    }
}
