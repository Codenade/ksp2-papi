using KSP.Game;
using System;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ksp2_papi
{
    internal static class AssetUtils
    {
        public static event Action CatalogLoaded;
        public static bool NeverLoaded => _neverLoaded;

        private static bool _neverLoaded = true;

        public static void LoadAssets()
        {
            GameManager.Instance.Game.KSP2ModManager.StartCoroutine(LoadCatalog());
        }

        private static IEnumerator LoadCatalog()
        {
            AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(ModInfo.CatalogPath);
            yield return operation;
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                GameManager.Instance.Assets.RegisterResourceLocator(operation.Result);
                Logger.Log("Catalog loaded");
                _neverLoaded = false;
                CatalogLoaded?.Invoke();
            }
            else
                Logger.Error("Failed to load addressables");
            yield break;
        }

        public static class Keys
        {
            public static readonly string px_count = "ksp2-papi/px-count.compute";
            public static readonly string unlit = "ksp2-papi/unlit.shader";
            public static readonly string papi_single = "ksp2-papi/papi_single.prefab";
            public static readonly string papi_x2 = "ksp2-papi/papi_x2.prefab";
            public static readonly string papi_x4 = "ksp2-papi/papi_x4.prefab";
            public static readonly string arrow_2 = "ksp2-papi/arrow_2.prefab";
            public static readonly string arrow_10 = "ksp2-papi/arrow_10.prefab";
            public static readonly string flare = "ksp2-papi/flare.flare";
        };
    }
}
