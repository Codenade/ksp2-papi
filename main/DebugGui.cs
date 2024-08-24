#if INCLUDE_DEBUG_GUI
using System.Linq;
using static UnityEngine.GUILayout;
#endif
using UnityEngine;

namespace ksp2_papi
{
    internal class DebugGui : MonoBehaviour
    {
#if INCLUDE_DEBUG_GUI
        public PapiManager Manager { get; private set; }
        public FlareOcclusionManager FlareOcclusionManager { get; private set; }

        private Rect _windowRect;
        private int _selectedIdx;
        private int _cameraLayerIdx;
        private bool _changeCameraLayer;

        private void Awake()
        {
            Manager = PapiManager.Instance;
            FlareOcclusionManager = FlareOcclusionManager.Instance;
            _windowRect = new Rect(1300f, 100f, 200f, 150f);
            _selectedIdx = 0;
            _cameraLayerIdx = 0;
        }

        private void OnEnable()
        {
            FlareOcclusionManager.debug = true;
            Manager.ReloadedConfig += OnConfigReloaded;
        }

        private void OnDisable()
        {
            FlareOcclusionManager.debug = false;
            Manager.ReloadedConfig -= OnConfigReloaded;
        }

        private void OnConfigReloaded()
        {
            _selectedIdx = 0;
        }

        private void OnGUI()
        {
            _windowRect = Window(2000, _windowRect, OnWindow, "Papi debug utility");
        }

        private void OnWindow(int id)
        {
            if (id != 2000)
                return;
            var c = FlareOcclusionManager.Flares.ElementAtOrDefault(_selectedIdx);
            var mainCamera = Camera.main;
            using (new VerticalScope())
            {
                if (!_changeCameraLayer)
                {
                    if (Button("Change camera layer (you will need to restart to revert this)"))
                        _changeCameraLayer = true;
                }
                else
                {
                    using (new HorizontalScope())
                    {
                        if (Button("<", Width(20)))
                        {
                            if (_cameraLayerIdx > 0)
                                _cameraLayerIdx--;
                            else
                                _cameraLayerIdx = 31;
                            FlareOcclusionManager.flareCamera.cullingMask = 1 << _cameraLayerIdx;
                        }
                        FlexibleSpace();
                        Label($"Used camera layer: {_cameraLayerIdx}, Layer name: {LayerMask.LayerToName(_cameraLayerIdx)}");
                        FlexibleSpace();
                        if (Button(">", Width(20)))
                        {
                            if (_cameraLayerIdx < 31)
                                _cameraLayerIdx++;
                            else
                                _cameraLayerIdx = 0;
                            FlareOcclusionManager.flareCamera.cullingMask = 1 << _cameraLayerIdx;
                        }
                    }
                }
                using (new HorizontalScope())
                {
                    if (Button("<", Width(20)))
                        _selectedIdx = _selectedIdx > 0 ? _selectedIdx - 1 : _selectedIdx;
                    FlexibleSpace();
                    Label($"{c?.transform.parent?.parent?.parent?.gameObject.name}/{c?.transform.parent?.parent?.gameObject.name}");
                    FlexibleSpace();
                    if (Button(">", Width(20)))
                        _selectedIdx = _selectedIdx >= FlareOcclusionManager.Flares.Length - 1 ? _selectedIdx : _selectedIdx + 1;
                    c = FlareOcclusionManager.debugfl = FlareOcclusionManager.Flares.ElementAtOrDefault(_selectedIdx);
                }
                Space(12);
                Label($"tex1offset: {FlareOcclusionManager.tex1offset}");
                FlareOcclusionManager.tex1offset = HorizontalSlider(FlareOcclusionManager.tex1offset, -0.000001f, 0.000001f);
                Label($"tex2offset: {FlareOcclusionManager.tex2offset}");
                FlareOcclusionManager.tex2offset = HorizontalSlider(FlareOcclusionManager.tex2offset, -0.000001f, 0.000001f);
                Space(12);
                if (mainCamera != null)
                    Label($"Camera info {mainCamera.transform.name} {mainCamera.transform.position}");
                if (c != null)
                {
                    Label("path: " + c.gameObject.transform.PathTo());
                    Space(8);
                    Label($"Pixels visible: {FlareOcclusionManager.resultdebug.visible}");
                    Label($"Pixels total  : {FlareOcclusionManager.resultdebug.total}");
                    Label($"Visibility    : {FlareOcclusionManager.visibility}");
                    Label($"Skipped       : {FlareOcclusionManager.skipped}");
                    if (mainCamera != null)
                        Label($"GeometryUtility.TestPlanesAABB: {GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), c.GetComponent<Renderer>().bounds)}");

                    Box(FlareOcclusionManager.dbgMask, Width(512), Height(288));
                    Box(FlareOcclusionManager.dbgFT, Width(512), Height(288));
                }
                else
                    Label("No data");
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
#else
        private bool startCalledOnce;

        private void Start()
        {
            startCalledOnce = true;
        }

        private void OnEnable()
        {
            if (startCalledOnce)
                Logger.Error("Tried to open DebugGui in a build without INCLUDE_DEBUG_GUI defined!");
        }
#endif
    }
}
