using KSP.Game;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace ksp2_papi
{
    internal class ConfigGui : MonoBehaviour
    {
        public PapiManager Manager { get; private set; }

        private Rect _windowRect;
        private int _selectedIdx;
        private string _posStepSize;
        private string _rotStepSize;
        private string _localPosStepSize;
        private string _localRotStepSize;
        private GameObject _arrow2Prefab;
        private GameObject _arrow10Prefab;

        private void Awake()
        {
            Manager = PapiManager.Instance;
            _windowRect = new Rect(300f, 300f, 200f, 150f);
            _posStepSize = "1";
            _rotStepSize = "10";
            _localPosStepSize = "1";
            _localRotStepSize = "10";
            _selectedIdx = 0;
            AssetUtils.AssetsLoaded += OnCatalogLoaded;
        }

        private void OnCatalogLoaded()
        {
            AssetUtils.AssetsLoaded -= OnCatalogLoaded;
            StartCoroutine(LoadAssets());
        }

        private IEnumerator LoadAssets()
        {
            var operation = GameManager.Instance.Assets.LoadAssetAsync<GameObject>("ksp2-papi/arrow_2.prefab");
            yield return operation;
            if (operation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                _arrow2Prefab = operation.Result;
            else
                Logger.Error("Failed to load ksp2-papi/arrow_2.prefab");
            operation = GameManager.Instance.Assets.LoadAssetAsync<GameObject>("ksp2-papi/arrow_10.prefab");
            yield return operation;
            if (operation.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                _arrow10Prefab = operation.Result;
            else
                Logger.Error("Failed to load ksp2-papi/arrow_10.prefab");
            yield break;
        }

        private void OnEnable()
        {
            Manager.ReloadedConfig += OnConfigReloaded;
        }

        private void OnDisable()
        {
            Manager.ReloadedConfig -= OnConfigReloaded;
        }

        private void OnConfigReloaded()
        {
            _selectedIdx = 0;
        }

        private void OnGUI()
        {
            if (Manager.DisableConfigGui)
                enabled = false;
            _windowRect = Window(1000, _windowRect, OnWindow, "Papi placement utility");
        }

        private void OnWindow(int id)
        {
            if (id != 1000)
                return;
            var c = Manager.AllLightsInstances.ElementAtOrDefault(_selectedIdx);
            using (new VerticalScope())
            {
                if (Button($"Reload PAPI Definitions ({Path.GetFileName(ModInfo.PapisJsonPath)})"))
                    Manager.ReloadPapiDefinitions(true);
                if (Toggle(Manager.AllLightsInstances.ContainsKey("debug_arrow"), "Create arrow"))
                {
                    if (!Manager.AllLightsInstances.ContainsKey("debug_arrow"))
                    {
                        Manager.AllLightsInstances.Add("debug_arrow", Instantiate(_arrow10Prefab, Vector3.zero, Quaternion.Euler(Vector3.zero), null));
                        _selectedIdx = Manager.AllLightsInstances.Count - 1;
                    }
                }
                else if (Manager.AllLightsInstances.ContainsKey("debug_arrow"))
                {
                    Destroy(Manager.AllLightsInstances["debug_arrow"]);
                    Manager.AllLightsInstances.Remove("debug_arrow");
                    if (_selectedIdx >= Manager.AllLightsInstances.Count)
                        _selectedIdx = Manager.AllLightsInstances.Count - 1;
                }
                using (new HorizontalScope())
                {
                    if (Button("<", Width(20)))
                        _selectedIdx = _selectedIdx > 0 ? _selectedIdx - 1 : _selectedIdx;
                    Label(c.Key);
                    if (Button(">", Width(20)))
                        _selectedIdx = _selectedIdx >= Manager.AllLightsInstances.Count - 1 ? _selectedIdx : _selectedIdx + 1;
                }
                Label($"Camera info {Camera.main.transform.name} {Camera.main.transform.position}");
                Label($"gravity?: {Physics.gravity}");
                if (c.Value != null)
                {
                    if (Button("Rotate Object to align with gravity!"))
                    {
                        c.Value.transform.LookAt(c.Value.transform.position + Physics.gravity);
                        //Manager.AllLightsInstances["debug_arrow"].transform.rotation = Quaternion.LookRotation(Manager.AllLightsInstances["debug_arrow"].transform.position - GameManager.Instance.Game.UniverseView.PhysicsSpace.PositionToPhysics(_simObject.Position));
                    }
                    Label("path: " + c.Value.transform.PathTo());
                    Label("pos: " + c.Value.transform.position);
                    using (new HorizontalScope())
                    {
                        bool stepSizeValid = float.TryParse(_posStepSize, out var parsedStepSize);
                        if (Button("-z", Width(20)) && stepSizeValid)
                            c.Value.transform.position += new Vector3(0f, 0f, -parsedStepSize);
                        if (Button("-y", Width(20)) && stepSizeValid)
                            c.Value.transform.position += new Vector3(0f, -parsedStepSize, 0f);
                        if (Button("-x", Width(20)) && stepSizeValid)
                            c.Value.transform.position += new Vector3(-parsedStepSize, 0f, 0f);
                        _posStepSize = TextField(_posStepSize);
                        if (Button("+x", Width(20)) && stepSizeValid)
                            c.Value.transform.position += new Vector3(parsedStepSize, 0f, 0f);
                        if (Button("+y", Width(20)) && stepSizeValid)
                            c.Value.transform.position += new Vector3(0f, parsedStepSize, 0f);
                        if (Button("+z", Width(20)) && stepSizeValid)
                            c.Value.transform.position += new Vector3(0f, 0f, parsedStepSize);
                    }
                    Label("rot: " + c.Value.transform.rotation.eulerAngles);
                    using (new HorizontalScope())
                    {
                        bool stepSizeValid = float.TryParse(_rotStepSize, out var parsedStepSize);
                        if (Button("-z", Width(20)) && stepSizeValid)
                            c.Value.transform.rotation = Quaternion.Euler(c.Value.transform.rotation.eulerAngles + new Vector3(0f, 0f, -parsedStepSize));
                        if (Button("-y", Width(20)) && stepSizeValid)
                            c.Value.transform.rotation = Quaternion.Euler(c.Value.transform.rotation.eulerAngles + new Vector3(0f, -parsedStepSize, 0f));
                        if (Button("-x", Width(20)) && stepSizeValid)
                            c.Value.transform.rotation = Quaternion.Euler(c.Value.transform.rotation.eulerAngles + new Vector3(-parsedStepSize, 0f, 0f));
                        _rotStepSize = TextField(_rotStepSize);
                        if (Button("+x", Width(20)) && stepSizeValid)
                            c.Value.transform.rotation = Quaternion.Euler(c.Value.transform.rotation.eulerAngles + new Vector3(parsedStepSize, 0f, 0f));
                        if (Button("+y", Width(20)) && stepSizeValid)
                            c.Value.transform.rotation = Quaternion.Euler(c.Value.transform.rotation.eulerAngles + new Vector3(0f, parsedStepSize, 0f));
                        if (Button("+z", Width(20)) && stepSizeValid)
                            c.Value.transform.rotation = Quaternion.Euler(c.Value.transform.rotation.eulerAngles + new Vector3(0f, 0f, parsedStepSize));
                    }
                    Label("local pos: " + c.Value.transform.localPosition);
                    using (new HorizontalScope())
                    {
                        bool stepSizeValid = float.TryParse(_localPosStepSize, out var parsedStepSize);
                        if (Button("-z", Width(20)) && stepSizeValid)
                            c.Value.transform.localPosition += new Vector3(0f, 0f, -parsedStepSize);
                        if (Button("-y", Width(20)) && stepSizeValid)
                            c.Value.transform.localPosition += new Vector3(0f, -parsedStepSize, 0f);
                        if (Button("-x", Width(20)) && stepSizeValid)
                            c.Value.transform.localPosition += new Vector3(-parsedStepSize, 0f, 0f);
                        _localPosStepSize = TextField(_localPosStepSize);
                        if (Button("+x", Width(20)) && stepSizeValid)
                            c.Value.transform.localPosition += new Vector3(parsedStepSize, 0f, 0f);
                        if (Button("+y", Width(20)) && stepSizeValid)
                            c.Value.transform.localPosition += new Vector3(0f, parsedStepSize, 0f);
                        if (Button("+z", Width(20)) && stepSizeValid)
                            c.Value.transform.localPosition += new Vector3(0f, 0f, parsedStepSize);
                    }
                    Label("local rot: " + c.Value.transform.localRotation.eulerAngles);
                    using (new HorizontalScope())
                    {
                        bool stepSizeValid = float.TryParse(_localRotStepSize, out var parsedStepSize);
                        if (Button("-z", Width(20)) && stepSizeValid)
                            c.Value.transform.localRotation = Quaternion.Euler(c.Value.transform.localRotation.eulerAngles + new Vector3(0f, 0f, -parsedStepSize));
                        if (Button("-y", Width(20)) && stepSizeValid)
                            c.Value.transform.localRotation = Quaternion.Euler(c.Value.transform.localRotation.eulerAngles + new Vector3(0f, -parsedStepSize, 0f));
                        if (Button("-x", Width(20)) && stepSizeValid)
                            c.Value.transform.localRotation = Quaternion.Euler(c.Value.transform.localRotation.eulerAngles + new Vector3(-parsedStepSize, 0f, 0f));
                        _localRotStepSize = TextField(_localRotStepSize);
                        if (Button("+x", Width(20)) && stepSizeValid)
                            c.Value.transform.localRotation = Quaternion.Euler(c.Value.transform.localRotation.eulerAngles + new Vector3(parsedStepSize, 0f, 0f));
                        if (Button("+y", Width(20)) && stepSizeValid)
                            c.Value.transform.localRotation = Quaternion.Euler(c.Value.transform.localRotation.eulerAngles + new Vector3(0f, parsedStepSize, 0f));
                        if (Button("+z", Width(20)) && stepSizeValid)
                            c.Value.transform.localRotation = Quaternion.Euler(c.Value.transform.localRotation.eulerAngles + new Vector3(0f, 0f, parsedStepSize));
                    }
                }
                else
                    Label("Does not exist yet");
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}
