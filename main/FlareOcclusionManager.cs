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

        public const float TEXOFFSET = -6.56E-07f;

        internal LensFlare[] Flares => _flares.ToArray();
        internal Camera flareCamera;
#if INCLUDE_DEBUG_GUI
        internal bool debug;
        internal LensFlare debugfl;
        internal CSFlareUtil.Result resultdebug;
        internal float visibility;
        internal RenderTexture dbgMask;
        internal RenderTexture dbgFT;
        internal float tex1offset = TEXOFFSET;
        internal float tex2offset = TEXOFFSET;
        internal bool skipped;
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
        private Material _conservativeFill;
        private MethodInfo _pqsRenderPrepass;
        private FieldInfo _pqsRT0;
        private Mesh _flareMesh;
        private Vector2Int _lastCameraRes;
        private int _idTex1;
        private int _idTex2;
        private int _idTexOffset1;
        private int _idTexOffset2;

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
            _flareMesh = null;
            _lastCameraRes = Vector2Int.zero;
            if (AssetUtils.NeverLoaded)
                AssetUtils.AssetsLoaded += AssetsLoaded;
            else
                AssetsLoaded();
#if INCLUDE_DEBUG_GUI
            resultdebug = new CSFlareUtil.Result();
#endif
            _pqsRenderPrepass = typeof(PQSRenderer).GetMethod(nameof(PQSRenderer.RenderPrepass), BindingFlags.NonPublic | BindingFlags.Instance);
            _pqsRT0 = typeof(PQSRenderer).GetField(nameof(PQSRenderer.PqsPrepassRenderTarget0), BindingFlags.NonPublic | BindingFlags.Instance);
            _idTex1 = Shader.PropertyToID("_Tex1");
            _idTex2 = Shader.PropertyToID("_Tex2");
            _idTexOffset1 = Shader.PropertyToID("_Tex1_Offset");
            _idTexOffset2 = Shader.PropertyToID("_Tex2_Offset");
        }

        internal void UpdateResolution(int x, int y)
        {
            _lastCameraRes = new Vector2Int(x, y);
            _cameraTexture?.Release();
            _occludedTexture?.Release();
            _cameraTexture = new RenderTexture(x, y, 32)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            _occludedTexture = new RenderTexture(_cameraTexture)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
#if INCLUDE_DEBUG_GUI
            dbgMask?.Release();
            dbgFT?.Release();
            dbgMask = new RenderTexture(x, y, 32)
            {
                filterMode = FilterMode.Bilinear
            };
            dbgFT = new RenderTexture(dbgMask);
#endif
            flareCamera.targetTexture = _cameraTexture;
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
            _conservativeFill = new Material((Shader)AssetUtils.Assets[AssetUtils.Keys.shader_conservativefill])
            {
                enableInstancing = true,
            };
            _addMaterial = new Material(_addShader);
            _flareMesh = ((GameObject)AssetUtils.Assets[AssetUtils.Keys.papi_single]).transform.Find("papi_head/flare").gameObject.GetComponent<MeshFilter>().sharedMesh;
            _addMaterial.SetFloat(_idTexOffset1, TEXOFFSET);
            _addMaterial.SetFloat(_idTexOffset2, TEXOFFSET);
        }

        private void LateUpdate()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;
            if ((mainCamera.scaledPixelWidth != _lastCameraRes.x) || (mainCamera.scaledPixelHeight != _lastCameraRes.y))
                UpdateResolution(mainCamera.scaledPixelWidth, mainCamera.scaledPixelHeight);
            if (_pqsRenderer == null)
                _pqsRenderer = GameManager.Instance.Game.CameraManager?.CurrentPQS?.PQSRenderer;
            else
                CalculateOcclusionAll(mainCamera);
        }

        private void CalculateOcclusionAll(Camera mainCamera)
        {
            if (_flares.Count == 0)
                return;

            _cmdBfr.Clear();

            var viewMatrix = mainCamera.worldToCameraMatrix;
            var projMatrix = mainCamera.projectionMatrix;
            var cameraPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

            flareCamera.worldToCameraMatrix = viewMatrix;
            flareCamera.projectionMatrix = projMatrix;
            flareCamera.RenderWithShader(_renderShader, "RenderType");

            _pqsRenderPrepass.Invoke(_pqsRenderer, null);

            _addMaterial.SetTexture(_idTex1, _cameraTexture, RenderTextureSubElement.Depth);
            _addMaterial.SetTexture(_idTex2, (RenderTexture)_pqsRT0.GetValue(_pqsRenderer), RenderTextureSubElement.Depth);
#if INCLUDE_DEBUG_GUI
            _addMaterial.SetFloat(_idTexOffset1, tex1offset);
            _addMaterial.SetFloat(_idTexOffset2, tex2offset);     
#endif
            Graphics.Blit(null, _occludedTexture, _addMaterial);

            _cmdBfr.SetViewProjectionMatrices(viewMatrix, projMatrix);
            _cmdBfr.SetRenderTarget(_occludedTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            var matrices = new Matrix4x4[_flares.Count];
            for (var i = 0; i < _flares.Count; i++)
            {
                var r = _flares[i].gameObject.GetComponent<Renderer>();
                matrices[i] = r.localToWorldMatrix;
            }
            _cmdBfr.DrawMeshInstanced(_flareMesh, 0, _conservativeFill, -1, matrices, matrices.Length, null);

            SingleCalcContext[] ctxt = new SingleCalcContext[_flares.Count];
            for (int i = 0; i < _flares.Count; i++)
                ctxt[i] = PreCalculateOcclusionSingle(_flares[i], cameraPlanes);

            Graphics.ExecuteCommandBuffer(_cmdBfr);

            foreach (var i in ctxt)
                PostCalculateOcclusionSingle(i);

#if INCLUDE_DEBUG_GUI
            if (debug)
                Graphics.CopyTexture(_occludedTexture, dbgFT);
#endif
        }

        private SingleCalcContext PreCalculateOcclusionSingle(LensFlare flare, Plane[] cameraPlanes)
        {
            var renderer = flare.GetComponent<Renderer>();
            if ((PapiManager.Instance.UseFrustumCulling && !GeometryUtility.TestPlanesAABB(cameraPlanes, renderer.bounds)) ||
                (PapiManager.Instance.UseBackfaceCulling && (Vector3.Angle(flare.transform.forward, cameraPlanes[4].normal) > 90f)))
            {
#if INCLUDE_DEBUG_GUI
                if (debug && debugfl == flare)
                    skipped = true;
#endif
                flare.enabled = false;
                return SingleCalcContext.Skip;
            }
#if INCLUDE_DEBUG_GUI
            else if (debug && debugfl == flare)
                skipped = false;
#endif

            var mask = RenderTexture.GetTemporary(_occludedTexture.width, _occludedTexture.height, 32);
            mask.anisoLevel = 0;
            mask.filterMode = FilterMode.Point;
            _cmdBfr.SetRenderTarget(mask, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            _cmdBfr.ClearRenderTarget(true, true, Color.clear);
            _cmdBfr.DrawRenderer(renderer, _conservativeFill);

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
#if INCLUDE_DEBUG_GUI
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
                context.lflare.color *= result.total != 0 ? result.visible / (float)result.total : 0f;
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

        private void OnEnable()
        {
            flareCamera = new GameObject("FlareCamera").AddComponent<Camera>();
            flareCamera.gameObject.transform.SetParent(transform);
            flareCamera.renderingPath = RenderingPath.Forward;
            flareCamera.clearFlags = CameraClearFlags.SolidColor;
            flareCamera.backgroundColor = Color.clear;
            flareCamera.allowMSAA = false;
            flareCamera.allowHDR = false;
            flareCamera.depthTextureMode = DepthTextureMode.Depth;
            // --- cullingMask ---
            // Flight camera: 753731 -> 0: Default, 1: TransparentFX, 6: UI.ResearchDevelopment, 15: Local.Scenery, 16: Internal.Scenery, 17: Local.Characters, 18: Render.Skybox
            flareCamera.cullingMask = (int)(753731 & (0xFFFFFFFF ^ (1 << 1)));
            flareCamera.useOcclusionCulling = true;
            flareCamera.enabled = false;
            var mainCamera = Camera.main;
            if (mainCamera != null)
                UpdateResolution(mainCamera.scaledPixelWidth, mainCamera.scaledPixelWidth);

            _cmdBfr = new CommandBuffer();
        }

        private void OnDisable()
        {
            _lastCameraRes = Vector2Int.zero;
            _cameraTexture.Release();
            _occludedTexture.Release();
#if INCLUDE_DEBUG_GUI
            dbgFT.Release();
            dbgMask.Release();
#endif
            _cmdBfr.Dispose();
            Destroy(flareCamera.gameObject);
        }
    }
}
