using KSP.Game;
using UnityEngine;
using KSP.IO;
using KSP.Messages;
using System.Collections.Generic;
using System;
using RTG;
using UnityEngine.InputSystem;

namespace ksp2_papi
{
    public class PapiManager : MonoBehaviour
    {
        public static PapiManager Instance => _instance;
        public event Action ReloadedConfig;
        public Dictionary<string, GameObject> AllLightsInstances { get; set; }
        public Dictionary<string, Tuple<string, Vector3, Quaternion>> Data { get; set; }
        public GameObject PapiPrefab { get => _papiPrefab; }
        public bool UseViewFrustrumCulling => _useViewFrustrumCulling;
        public bool UseViewDirectionCulling => _useViewDirectionCulling;
        private static PapiManager _instance;
        private GameObject _papiPrefab;
        private ConfigGui _configGui;
        private InputAction _configGuiToggle;
#if DEBUG
        private InputAction _debugGuiToggle;
        private DebugGui _debugGui;
#endif
        private bool _useViewFrustrumCulling;
        private bool _useViewDirectionCulling;

        private void Awake()
        {
            if (_instance is null)
                _instance = this;
            AllLightsInstances = new Dictionary<string, GameObject>();
            Data = new Dictionary<string, Tuple<string, Vector3, Quaternion>>();
            ReloadConfig();
            _configGuiToggle = new InputAction("ksp2-papi.ConfigGuiToggle", InputActionType.Value, "<Keyboard>/p", null, null, "Button");
#if DEBUG
            _debugGuiToggle = new InputAction("ksp2-papi.DebugGuiToggle", InputActionType.Value, "<Keyboard>/o", null, null, "Button");
#endif
            _configGui = gameObject.AddComponent<ConfigGui>();
            _configGui.enabled = false;
            gameObject.AddComponent<FlareOcclusionManager>();
#if DEBUG
            _debugGui = gameObject.AddComponent<DebugGui>();
            _debugGui.enabled = false;
#endif
            AssetUtils.LoadAssets();
            AssetUtils.AssetsLoaded += OnAssetsLoaded;
        }

        private void OnEnable()
        {
            GameManager.Instance.Game.Messages.PersistentSubscribe<FloatingOriginSnappedMessage>(OnFloatingOriginSnapped);
            _configGuiToggle.performed += ToggleConfigMenuAction;
            _configGuiToggle.Enable();
#if DEBUG
            _debugGuiToggle.performed += ToggleDebugMenu;
            _debugGuiToggle.Enable();
#endif
        }

        private void OnDisable()
        {
            GameManager.Instance.Game.Messages.Unsubscribe<FloatingOriginSnappedMessage>(OnFloatingOriginSnapped);
            _configGuiToggle.performed -= ToggleConfigMenuAction;
            _configGuiToggle.Disable();
#if DEBUG
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

#if DEBUG
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

        public void ReloadConfig(bool autoCreatePapis = false)
        {
            Logger.Log($"Reloading config");
            DestroyAllLights();
            var configData = IOProvider.FromJsonFile<ConfigData>(ModInfo.PapisJsonPath);
            AllLightsInstances.Clear();
            Data.Clear();
            _useViewFrustrumCulling = configData.UseViewFrustrumCulling;
            _useViewDirectionCulling = configData.UseViewDirectionCulling;
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
