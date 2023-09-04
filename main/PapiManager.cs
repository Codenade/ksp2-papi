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
        private static PapiManager _instance;
        private GameObject _papiPrefab;
        private ConfigGui _configGui;
        private InputAction _configGuiToggle;

        public void Awake()
        {
            if (_instance is null)
                _instance = this;
            AllLightsInstances = new Dictionary<string, GameObject>();
            Data = new Dictionary<string, Tuple<string, Vector3, Quaternion>>();
            ReloadConfig();
            _configGuiToggle = new InputAction("ksp2-papi.ConfigGuiToggle", InputActionType.Value, "<Keyboard>/p", null, null, "Button");
            _configGui = gameObject.AddComponent<ConfigGui>();
            _configGui.enabled = false;
            AssetUtils.LoadAssets();
            AssetUtils.CatalogLoaded += OnCatalogLoaded;
            //if (Harmony.HasAnyPatches("codenade-inputbinder"))
            //    if (Codenade.Inputbinder.Inputbinder.Instance is object)
            //        if (Codenade.Inputbinder.Inputbinder.Instance.IsInitialized)
            //            Inputbinder_Initialized();
        }

        //public void Inputbinder_Initialized()
        //{
        //    if (Harmony.HasAnyPatches("codenade-inputbinder"))
        //    {
        //        if (Codenade.Inputbinder.Inputbinder.Instance.ActionManager.Actions.TryGetValue(_configGuiToggle.name, out var act))
        //        {
        //            _configGuiToggle.performed -= OnToggleConfigMenuAction;
        //            _configGuiToggle.Disable();
        //            _configGuiToggle.Dispose();
        //            _configGuiToggle = act.Action;
        //            _configGuiToggle.performed += OnToggleConfigMenuAction;
        //            _configGuiToggle.Enable();
        //        }
        //        else
        //        {
        //            Codenade.Inputbinder.Inputbinder.Instance.ActionManager.AddAction(_configGuiToggle);
        //        }
        //    }
        //}

        public void OnEnable()
        {
            GameManager.Instance.Game.Messages.PersistentSubscribe<FloatingOriginSnappedMessage>(OnFloatingOriginSnapped);
            _configGuiToggle.performed += OnToggleConfigMenuAction;
            _configGuiToggle.Enable();
            //if (Harmony.HasAnyPatches("codenade-inputbinder"))
            //    Codenade.Inputbinder.Inputbinder.Initialized += Inputbinder_Initialized;
        }

        public void OnDisable()
        {
            GameManager.Instance.Game.Messages.Unsubscribe<FloatingOriginSnappedMessage>(OnFloatingOriginSnapped);
            _configGuiToggle.performed -= OnToggleConfigMenuAction;
            _configGuiToggle.Disable();
            //if (Harmony.HasAnyPatches("codenade-inputbinder"))
            //    Codenade.Inputbinder.Inputbinder.Initialized -= Inputbinder_Initialized;
        }

        private void OnCatalogLoaded()
        {
            GameManager.Instance.Assets.Load<GameObject>(AssetUtils.Keys.papi_x4, result => _papiPrefab = result);
        }

        public void UpdatePapi()
        {
            var tempInstances = new Dictionary<string, GameObject>(AllLightsInstances);
            foreach (var i in tempInstances)
                if (i.Value == null)
                    AllLightsInstances[i.Key] = CreatePAPI(GameObject.Find(Data[i.Key].Item1)?.transform, Data[i.Key].Item2, Data[i.Key].Item3, i.Key);
        }

        private void OnToggleConfigMenuAction(InputAction.CallbackContext _)
        {
            ToggleConfigMenu();
        }

        public void ToggleConfigMenu()
        {
            if (_configGui.enabled)
                _configGui.enabled = false;
            else
                _configGui.enabled = true;
        }

        public void ReloadConfig(bool autoReload = false)
        {
            Logger.Log($"Reloading PAPI data");
            DestroyAllLights();
            var configData = IOProvider.FromJsonFile<ConfigData>(ModInfo.PapisJsonPath);
            AllLightsInstances.Clear();
            Data.Clear();
            foreach (var d in configData.PapiData)
            {
                AllLightsInstances.Add(d.ID, null);
                Data.Add(d.ID, new Tuple<string, Vector3, Quaternion>(d.ParentName, d.LocalPosition, Quaternion.Euler(d.LocalRotation)));
            }
            if (autoReload)
                UpdatePapi();
            ReloadedConfig?.Invoke();
        }

        public void DestroyAllLights()
        {
            foreach (var i in AllLightsInstances)
                if (i.Value != null)
                    Destroy(i.Value);
        }

        void OnFloatingOriginSnapped(MessageCenterMessage msg)
        {
            UpdatePapi();
        }

        GameObject CreatePAPI(Transform parent, Vector3 position, Quaternion rotation, string name)
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
