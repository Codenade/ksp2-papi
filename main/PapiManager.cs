using KSP.Game;
using UnityEngine;
using KSP.IO;
using KSP.Messages;
using System.Collections.Generic;
using System;
using RTG;
using UnityEngine.InputSystem;
using BepInEx.Configuration;

namespace ksp2_papi
{
    public class PapiManager : MonoBehaviour
    {
        public static PapiManager Instance => _instance;
        public event Action ReloadedConfig;
        public Dictionary<string, GameObject> AllLightsInstances { get; set; }
        public Dictionary<string, Tuple<string, Vector3, Quaternion>> Data { get; set; }
        public GameObject PapiPrefab { get => _papiPrefab; }
        public bool UseFrustumCulling => _useFrustumCulling;
        public bool UseBackfaceCulling => _useBackfaceCulling;
        public bool UsePixelCounting => _usePixelCounting;
        public bool DisableConfigGui => _disableConfigGui;
        private static PapiManager _instance;
        private GameObject _papiPrefab;
        private ConfigGui _configGui;
        private InputAction _configGuiToggle;
#if INCLUDE_DEBUG_GUI
        private InputAction _debugGuiToggle;
        private DebugGui _debugGui;
#endif
        private bool _useFrustumCulling;
        private bool _useBackfaceCulling;
        private bool _usePixelCounting;
        private bool _disableConfigGui;

        private void Awake()
        {
            if (_instance is null)
                _instance = this;
            else
                DestroyImmediate(this);
            AllLightsInstances = new Dictionary<string, GameObject>();
            Data = new Dictionary<string, Tuple<string, Vector3, Quaternion>>();
            ReloadPapiDefinitions();
            _configGuiToggle = new InputAction("ksp2-papi.ConfigGuiToggle", InputActionType.Value, "<Keyboard>/p", null, null, "Button");
#if INCLUDE_DEBUG_GUI
            _debugGuiToggle = new InputAction("ksp2-papi.DebugGuiToggle", InputActionType.Value, "<Keyboard>/o", null, null, "Button");
#endif
            _configGui = gameObject.AddComponent<ConfigGui>();
            _configGui.enabled = false;
            gameObject.AddComponent<FlareOcclusionManager>();
#if INCLUDE_DEBUG_GUI
            _debugGui = gameObject.AddComponent<DebugGui>();
            _debugGui.enabled = false;
#endif
            AssetUtils.LoadAssets();
            AssetUtils.AssetsLoaded += OnAssetsLoaded;
            InitConfig();
            KSP2_PAPI_Plugin.Instance.Config.SettingChanged += OnConfigUpdated;
        }

        private void InitConfig()
        {
            var cfgfile = KSP2_PAPI_Plugin.Instance.Config;
            _usePixelCounting = cfgfile.Bind(ConfigSections.General.Name, ConfigSections.General.UsePixelCounting, true, 
                "Users who are experiencing performance problems with this mod can disable this setting." + Environment.NewLine +
                "HOWEVER disabling the pixel counting mechanism WILL also make the flares visible through terrain.").Value;
            _useFrustumCulling = cfgfile.Bind(ConfigSections.General.Name, ConfigSections.General.UseFrustumCulling, true,
                "Enable frustum culling. Enhances performance by skipping lens flares outside the camera view.").Value;
            _useBackfaceCulling = cfgfile.Bind(ConfigSections.General.Name, ConfigSections.General.UseBackfaceCulling, true,
                "Enable backface culling. Enhances performance by skipping lens flares facing away from the camera.").Value;
            _disableConfigGui = cfgfile.Bind(ConfigSections.General.Name, ConfigSections.General.DisableConfigGui, true,
                "Disables the included placement utility for papi lights. This is used for fine placement.").Value;
        }

        private void OnConfigUpdated(object sender, SettingChangedEventArgs e)
        {
            switch (e.ChangedSetting.Definition.Section)
            {
                case ConfigSections.General.Name:
                    switch (e.ChangedSetting.Definition.Key)
                    {
                        case ConfigSections.General.UsePixelCounting:
                            _usePixelCounting = (bool)e.ChangedSetting.BoxedValue;
                            FlareOcclusionManager.Instance.enabled = _usePixelCounting;
                            break;
                        case ConfigSections.General.UseFrustumCulling:
                            _useFrustumCulling = (bool)e.ChangedSetting.BoxedValue;
                            break;
                        case ConfigSections.General.UseBackfaceCulling:
                            _useBackfaceCulling = (bool)e.ChangedSetting.BoxedValue;
                            break;
                        case ConfigSections.General.DisableConfigGui:
                            _disableConfigGui = (bool)e.ChangedSetting.BoxedValue;
                            if (enabled)
                            {
                                if (_disableConfigGui)
                                    _configGuiToggle.Disable();
                                else
                                    _configGuiToggle.Enable();
                            }
                            break;
                    }
                    break;
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.Game.Messages.PersistentSubscribe<FloatingOriginSnappedMessage>(OnFloatingOriginSnapped);
            _configGuiToggle.performed += ToggleConfigMenuAction;
            if (!_disableConfigGui)
                _configGuiToggle.Enable();
#if INCLUDE_DEBUG_GUI
            _debugGuiToggle.performed += ToggleDebugMenu;
            _debugGuiToggle.Enable();
#endif
        }

        private void OnDisable()
        {
            GameManager.Instance.Game.Messages.Unsubscribe<FloatingOriginSnappedMessage>(OnFloatingOriginSnapped);
            _configGuiToggle.performed -= ToggleConfigMenuAction;
            _configGuiToggle.Disable();
#if INCLUDE_DEBUG_GUI
            _debugGuiToggle.performed -= ToggleDebugMenu;
            _debugGuiToggle.Disable();
#endif
        }

        private void OnAssetsLoaded()
        {
            _papiPrefab = (GameObject)AssetUtils.Assets[AssetUtils.Keys.papi_x4];
        }

        public void UpdatePapi()
        {
            var tempInstances = new Dictionary<string, GameObject>(AllLightsInstances);
            foreach (var i in tempInstances)
                if (i.Value == null)
                    AllLightsInstances[i.Key] = CreatePAPI(GameObject.Find(Data[i.Key].Item1)?.transform, Data[i.Key].Item2, Data[i.Key].Item3, i.Key);
        }

#if INCLUDE_DEBUG_GUI
        public void ToggleDebugMenu(InputAction.CallbackContext _ = default)
        {
            if (_debugGui.enabled)
                _debugGui.enabled = false;
            else
                _debugGui.enabled = true;
        }
#endif

        public void ToggleConfigMenuAction(InputAction.CallbackContext _ = default)
        {
            if (_configGui.enabled)
                _configGui.enabled = false;
            else
                _configGui.enabled = true;
        }

        public void ReloadPapiDefinitions(bool autoCreatePapis = false)
        {
            Logger.Log($"Reloading config");
            DestroyAllLights();
            var configData = IOProvider.FromJsonFile<ConfigData>(ModInfo.PapisJsonPath);
            AllLightsInstances.Clear();
            Data.Clear();
            foreach (var d in configData.PapiData)
            {
                AllLightsInstances.Add(d.ID, null);
                Data.Add(d.ID, new Tuple<string, Vector3, Quaternion>(d.ParentName, d.LocalPosition, Quaternion.Euler(d.LocalRotation)));
            }
            if (autoCreatePapis)
                UpdatePapi();
            ReloadedConfig?.Invoke();
        }

        public void DestroyAllLights()
        {
            foreach (var i in AllLightsInstances)
                if (i.Value != null)
                    Destroy(i.Value);
        }

        private void OnFloatingOriginSnapped(MessageCenterMessage msg)
        {
            UpdatePapi();
        }

        public GameObject CreatePAPI(Transform parent, Vector3 position, Quaternion rotation, string name)
        {
            if (parent != null)
            {
                GameObject o = Instantiate(_papiPrefab, parent);
                o.name = name;
                o.transform.localPosition = position;
                o.transform.localRotation = rotation;
                o.transform.SetWorldScale(Vector3.one);
                return o;
            }
            return null;
        }
    }
}
