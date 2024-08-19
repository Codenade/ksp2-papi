using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using KSP.Rendering.Planets;
using KSP.Game;
using System.Reflection;

namespace ksp2_papi
{
    // TODO: Add note about trees not being accounted for!

    public class FlareOcclusionManager : MonoBehaviour
    {
        public static FlareOcclusionManager Instance => _instance;

        internal LensFlare[] Flares => _flares.ToArray();
        internal bool debug;
        internal LensFlare debugfl;
        internal CSFlareUtil.Result resultdebug;
        internal float visibility;
        internal Camera flareCamera;
#if DEBUG
        internal RenderTexture dbgMask;
        internal RenderTexture dbgFT;
#endif

        private static FlareOcclusionManager _instance;
        private List<LensFlare> _flares;
        private ComputeShader _cs;
        private RenderTexture _cameraTexture;
        private RenderTexture _occludedTexture;
        private CommandBuffer _cmdBfr;
        private PQSRenderer _pqsRenderer;
        private Shader _renderShader;
        private Shader _copyDepthShader;
        private Shader _addShader;
        private Material _rm;
        private Material _copyDepthMat;
        private Material _addMaterial;
        
        public void RegisterFlare(LensFlare flare)
        {
            if (!_flares.Contains(flare))
                _flares.Add(flare);
        }

        public void UnregisterFlare(LensFlare flare)
        {
            _flares.Remove(flare);
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
            else
                DestroyImmediate(this);
            _flares = new List<LensFlare>();
            _pqsRenderer = null;
            UpdateResolution(new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height));
            flareCamera = new GameObject("FlareCamera").AddComponent<Camera>();
            flareCamera.gameObject.transform.SetParent(transform);
            flareCamera.enabled = false;
            flareCamera.renderingPath = RenderingPath.Forward;
            flareCamera.clearFlags = CameraClearFlags.SolidColor;
            flareCamera.backgroundColor = Color.clear;
            flareCamera.allowMSAA = false;
            flareCamera.allowHDR = false;
            flareCamera.targetTexture = _cameraTexture;
            flareCamera.depthTextureMode = DepthTextureMode.Depth;
            flareCamera.cullingMask = -1 ^ (1 << 10);
            flareCamera.useOcclusionCulling = true;
            resultdebug = new CSFlareUtil.Result();
            _cmdBfr = new CommandBuffer();
            if (AssetUtils.NeverLoaded)
                AssetUtils.AssetsLoaded += AssetsLoaded;
            else
                AssetsLoaded();
        }

        internal void UpdateResolution(Vector2Int size)
        {
            _cameraTexture = new RenderTexture(size.x, size.y, 24);
            _occludedTexture = new RenderTexture(size.x, size.y, 24);
#if DEBUG
            dbgMask = new RenderTexture(size.x, size.y, 24);
            dbgFT = new RenderTexture(size.x, size.y, 24);
#endif
        }

        private void AssetsLoaded()
        {
            AssetUtils.AssetsLoaded -= AssetsLoaded;
            _renderShader = (Shader)AssetUtils.Assets[AssetUtils.Keys.shader_unlit];
            _cs = (ComputeShader)AssetUtils.Assets[AssetUtils.Keys.compute_px_count];
            _copyDepthShader = (Shader)AssetUtils.Assets[AssetUtils.Keys.shader_copy_depth];
            _addShader = (Shader)AssetUtils.Assets[AssetUtils.Keys.shader_add];
            _copyDepthMat = new Material(_copyDepthShader);
            _rm = new Material(_renderShader);
            _addMaterial = new Material(_addShader);
        }

        private void LateUpdate()
        {
            if (_pqsRenderer == null)
                _pqsRenderer = GameManager.Instance.Game.CameraManager.CurrentPQS.PQSRenderer;
            CalculateOcclusionAll();
        }

        private void CalculateOcclusionAll()
        {
            typeof(PQSRenderer).GetMethod(nameof(_pqsRenderer.RenderPrepass), BindingFlags.NonPublic | BindingFlags.Instance).Invoke(_pqsRenderer, null);
            
            flareCamera.worldToCameraMatrix = Camera.main.worldToCameraMatrix;
            flareCamera.projectionMatrix = Camera.main.projectionMatrix;
            flareCamera.RenderWithShader(_renderShader, "RenderType");

            _addMaterial.SetTexture("_Tex1", _cameraTexture, RenderTextureSubElement.Depth);
            _addMaterial.SetTexture("_Tex2", (RenderTexture)typeof(PQSRenderer).GetField("PqsPrepassRenderTarget0", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_pqsRenderer), RenderTextureSubElement.Depth);
            Graphics.Blit(null, _occludedTexture, _addMaterial);

            // _copyDepthMat.SetTexture("_MyDepthTex", _cameraTexture, RenderTextureSubElement.Depth);
            // Graphics.Blit(null, dbgMask, _copyDepthMat);

            _cmdBfr.SetViewProjectionMatrices(Camera.main.worldToCameraMatrix, Camera.main.projectionMatrix);
            _cmdBfr.SetRenderTarget(_occludedTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            foreach (var f in _flares)
            {
                Renderer r = f.GetComponent<Renderer>();
                _cmdBfr.DrawRenderer(r, r.material);
            }

            List<SingleCalcContext> ctxt = new List<SingleCalcContext>();
            foreach (var i in _flares)
                ctxt.Add(PreCalculateOcclusionSingle(i));

            Graphics.ExecuteCommandBuffer(_cmdBfr);

            foreach (var i in ctxt)
                PostCalculateOcclusionSingle(i);

            _cmdBfr.Clear();
#if DEBUG
            if (debug)
                Graphics.CopyTexture(_occludedTexture, dbgFT);
#endif
        }

        private SingleCalcContext PreCalculateOcclusionSingle(LensFlare flare)
        {
            var renderer = flare.GetComponent<Renderer>();
            if (PapiManager.Instance.UseViewFrustrumCulling && !GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(flareCamera), renderer.bounds))
            {
                flare.enabled = false;
                return SingleCalcContext.Skip;
            }

            var mask = RenderTexture.GetTemporary(_occludedTexture.width, _occludedTexture.height, 24);
            mask.anisoLevel = 0;
            mask.filterMode = FilterMode.Point;
            _cmdBfr.SetRenderTarget(mask, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            _cmdBfr.ClearRenderTarget(true, true, Color.clear);
            _cmdBfr.DrawRenderer(renderer, renderer.material);

            return new SingleCalcContext()
            {
                skip = false,
                txtmask = mask,
                lflare = flare
            };
        }

        private void PostCalculateOcclusionSingle(SingleCalcContext context)
        {
            if (context.skip)
                return;
            var result = CSFlareUtil.AnalyzeImage(_cs, _occludedTexture, context.txtmask);
#if DEBUG
            if (debug && debugfl == context.lflare)
                Graphics.CopyTexture(context.txtmask, dbgMask);
            if (debug && debugfl == context.lflare)
            {
                resultdebug = result;
                visibility = result.total != 0 ? result.visible / (float)result.total : 0f;
            }
#endif
            if (result.total != 0 && result.visible != 0)
            {
                context.lflare.enabled = true;
                context.lflare.color *= result.visible / (float)result.total;
            }
            else
                context.lflare.enabled = false;
            RenderTexture.ReleaseTemporary(context.txtmask);
        }

        internal struct SingleCalcContext
        {
            internal bool skip;
            internal RenderTexture txtmask;
            internal LensFlare lflare;

            internal static readonly SingleCalcContext Skip = new SingleCalcContext()
            {
                skip = true
            };
        }

        private void OnDestroy()
        {
            _cameraTexture.Release();
            _occludedTexture.Release();
#if DEBUG
            dbgFT.Release();
            dbgMask.Release();
#endif
            _cmdBfr.Dispose();
            Destroy(flareCamera.gameObject);
        }
    }
}
