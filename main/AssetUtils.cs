using KSP.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ksp2_papi
{
    internal static class AssetUtils
    {
        public static event Action AssetsLoaded;

        public static Dictionary<string, UnityEngine.Object> Assets => _assets;
        public static bool NeverLoaded => _neverLoaded;

        private static readonly Dictionary<string, UnityEngine.Object> _assets = new Dictionary<string, UnityEngine.Object>();
        private static bool _neverLoaded = true;

        public static void LoadAssets() => GameManager.Instance.Game.KSP2ModManager.StartCoroutine(LoadCatalog());

        private static IEnumerator LoadCatalog()
        {
            AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(ModInfo.CatalogPath);
            yield return operation;
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                GameManager.Instance.Assets.RegisterResourceLocator(operation.Result);
                Logger.Log("Catalog loaded");
            }
            else
                Logger.Error("Failed to load addressables catalog");
            foreach (var i in typeof(Keys).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var key = (string)i.GetValue(null);
                var op = GameManager.Instance.Assets.LoadAssetAsync<UnityEngine.Object>(key);
                yield return op;
                if (op.Status == AsyncOperationStatus.Succeeded)
                    _assets.Add(key, op.Result);
                else
                    Logger.Error("Failed to load " + key);
            }
            _neverLoaded = false;
            AssetsLoaded?.Invoke();
            yield break;
        }

        public static class Keys
        {
            public static readonly string compute_px_count = "ksp2-papi/px-count.compute";
            public static readonly string shader_unlit = "ksp2-papi/unlit.shader";
            public static readonly string papi_single = "ksp2-papi/papi_single.prefab";
            public static readonly string papi_x2 = "ksp2-papi/papi_x2.prefab";
            public static readonly string papi_x4 = "ksp2-papi/papi_x4.prefab";
            public static readonly string arrow_2 = "ksp2-papi/arrow_2.prefab";
            public static readonly string arrow_10 = "ksp2-papi/arrow_10.prefab";
            public static readonly string flare = "ksp2-papi/flare.flare";
            public static readonly string shader_conservativefill = "ksp2-papi/conservativefill.shader";
            public static readonly string shader_copy_depth = "ksp2-papi/DepthCopy.shader";
            public static readonly string shader_add = "ksp2-papi/add.shader";
        };
    }
}
