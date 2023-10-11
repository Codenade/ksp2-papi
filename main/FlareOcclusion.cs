using KSP.Game;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ksp2_papi
{
    public class FlareOcclusion : MonoBehaviour
    {
        public LensFlare flare;
        public float visibility;
        public bool debug;
        public int[] resultdebug;

        private ComputeShader _cs;
        private RenderTexture _flareTexture;
        private RenderTexture _mask;
        private Camera _flareCamera;
        private Renderer _renderer;
        private Shader _renderShader;
        private int _cullingMask;

        void Awake()
        {
            _flareTexture = new RenderTexture(64, 64, 16);
            _flareCamera = new GameObject("FlareCamera").AddComponent<Camera>();
            _flareCamera.gameObject.transform.SetParent(transform);
            _flareCamera.enabled = false;
            _flareCamera.renderingPath = RenderingPath.Forward;
            _flareCamera.targetTexture = _flareTexture;
            _flareCamera.clearFlags = CameraClearFlags.SolidColor;
            _flareCamera.backgroundColor = Color.clear;
            _renderer = GetComponent<Renderer>();
            _mask = new RenderTexture(64, 64, 16);
            _cullingMask = LayerMask.GetMask(LayerMask.LayerToName(30));
            if (AssetUtils.NeverLoaded)
                AssetUtils.CatalogLoaded += CatalogLoaded;
            else
                StartCoroutine(LoadAssets());
            enabled = false;
        }

        private void CatalogLoaded()
        {
            AssetUtils.CatalogLoaded -= CatalogLoaded;
            StartCoroutine(LoadAssets());
            enabled = false;
        }

        IEnumerator LoadAssets()
        {
            var op_1 = GameManager.Instance.Assets.LoadAssetAsync<Shader>(AssetUtils.Keys.unlit);
            yield return op_1;
            if (op_1.Status == AsyncOperationStatus.Succeeded)
                _renderShader = op_1.Result;
            else
            {
                Logger.Error("Failed to load " + AssetUtils.Keys.unlit);
                enabled = false;
                yield break;
            }
            var op_2 = GameManager.Instance.Assets.LoadAssetAsync<ComputeShader>(AssetUtils.Keys.px_count);
            yield return op_2;
            if (op_2.Status == AsyncOperationStatus.Succeeded)
                _cs = op_2.Result;
            else
            {
                Logger.Error("Failed to load " + AssetUtils.Keys.px_count);
                enabled = false;
                yield break;
            }
            var op_3 = GameManager.Instance.Assets.LoadAssetAsync<Flare>(AssetUtils.Keys.flare);
            yield return op_3;
            if (op_3.Status == AsyncOperationStatus.Succeeded)
                flare.flare = op_3.Result;
            else
            {
                Logger.Error("Failed to load " + AssetUtils.Keys.flare);
                enabled = false;
                yield break;
            }
            enabled = true;
            yield break;
        }

        void LateUpdate()
        {
            var flareBounds = _renderer.bounds;
            var flareDistance = (flareBounds.center - Camera.main.transform.position).magnitude;
            var flareHalfSize = Mathf.Max(flareBounds.extents.x, Mathf.Max(flareBounds.extents.y, flareBounds.extents.z));
            _flareCamera.nearClipPlane = Camera.main.nearClipPlane;
            _flareCamera.farClipPlane = flareDistance + flareHalfSize + 10;
            _flareCamera.fieldOfView = 2f * Mathf.Atan(flareHalfSize / flareDistance) * Mathf.Rad2Deg;
            _flareCamera.gameObject.transform.position = Camera.main.gameObject.transform.position;
            _flareCamera.gameObject.transform.LookAt(transform, Vector3.up);
            _flareCamera.targetTexture = _flareTexture;
            _flareCamera.cullingMask = -1;
            _flareCamera.RenderWithShader(_renderShader, "RenderType");
            _flareCamera.cullingMask = _cullingMask;
            _flareCamera.targetTexture = _mask;
            _flareCamera.RenderWithShader(_renderShader, "RenderType");
            int[] result = CSFlareUtil.AnalyzeImage(_cs, _flareTexture, _mask);
            if (debug) resultdebug = result;
            float finalResult = (float)result[0] / result[1];
            visibility = finalResult;
            flare.color *= finalResult;
        }

        void OnGUI()
        {
            if (!debug) return;
            GUILayout.Box(_flareTexture);
            GUILayout.Box(_mask);
            GUILayout.Label(visibility.ToString());
            GUILayout.Label(resultdebug[0].ToString());
            GUILayout.Label(resultdebug[1].ToString());
        }
    }
}
