using RTG;
using System.Linq;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ksp2_papi
{
    internal class DebugGui : MonoBehaviour
    {
        public PapiManager Manager { get; private set; }
        public FlareOcclusionManager FlareOcclusionManager { get; private set; }

        private Rect _windowRect;
        private int _selectedIdx;
        private int _cameraLayerIdx;
        private bool _changeCameraLayer;

#if DEBUG
        private void Awake()
        {
            Manager = PapiManager.Instance;
            FlareOcclusionManager = FlareOcclusionManager.Instance;
            _windowRect = new Rect(1300f, 100f, 200f, 150f);
            _selectedIdx = 0;
            _cameraLayerIdx = 0;
            FlareOcclusionManager.flareCamera.cullingMask = 1 << _cameraLayerIdx;
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
            using (new VerticalScope())
            {
                if (!_changeCameraLayer)
                    _changeCameraLayer = Toggle(_changeCameraLayer, "Change camera layer?");
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
                            Label($"Used camera layer: {_cameraLayerIdx}");
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
                    Label($"{c.transform.parent.parent.parent.gameObject.name}/{c.transform.parent.parent.gameObject.name}");
                    FlexibleSpace();
                    if (Button(">", Width(20)))
                        _selectedIdx = _selectedIdx >= FlareOcclusionManager.Flares.Length - 1 ? _selectedIdx : _selectedIdx + 1;
                    FlareOcclusionManager.debugfl = FlareOcclusionManager.Flares[_selectedIdx];
                }
                Space(12);
                Label($"Camera info {Camera.main.transform.name} {Camera.main.transform.position}");
                if (c != default(LensFlare))
                {
                    Label("path: " + c.gameObject.transform.PathTo());
                    Space(8);
                    Label($"Pixels visible: {FlareOcclusionManager.resultdebug.visible}");
                    Label($"Pixels total  : {FlareOcclusionManager.resultdebug.total}");
                    Label($"Visibility    : {FlareOcclusionManager.visibility}");
                    Label($"is visible: {GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(FlareOcclusionManager.flareCamera), c.GetComponent<Renderer>().bounds)}");

                    Box(FlareOcclusionManager.dbgMask, Width(512), Height(288));
                    Box(FlareOcclusionManager.dbgFT, Width(512), Height(288));
                }
                else
                    Label("Does not exist yet");
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
#else
        private void OnEnable()
        {
            Logger.Error("Tried to open DebugGui in non-debug build!");
        }
#endif
    }
}
