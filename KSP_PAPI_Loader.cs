//#define POSITIONING // define for in game keybinds to bring the papi into position
//#define LIGHTSADJUST // define for in game keybinds to change different properties of the attached lights
//#define ON_SCREEN_LOG
//#define DBG_OBJECT_HOVER

using System;
using System.Collections.Generic;
using System.IO;
using KSP.Game;
using KSP.Input;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;
using KSP.Modding;
using KSP.IO;
using KSP.Messages;
using KSP.Sim;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace KSP2_PAPI
{
#if ON_SCREEN_LOG
    public class ZzzLog : MonoBehaviour
    {
        uint qsize = 15;  // number of messages to keep
        Queue myLogQueue = new Queue();

        void Start()
        {
            Debug.Log("Started up logging.");
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            myLogQueue.Enqueue("[" + type + "] : " + logString);
            if (type == LogType.Exception)
                myLogQueue.Enqueue(stackTrace);
            while (myLogQueue.Count > qsize)
                myLogQueue.Dequeue();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
            GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
            GUILayout.EndArea();
        }
    }
#endif

#if DBG_OBJECT_HOVER
    public class DbgObjHover : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
            Ray cursorRay = Camera.main.ScreenPointToRay(Mouse.Position);
            string objName = "";
            if (Physics.Raycast(cursorRay, out RaycastHit hitInfo))
            {
                objName = hitInfo.transform.gameObject.name;
            }
            GUILayout.Label(objName);
            GUILayout.EndArea();
        }
    }
#endif
#if POSITIONING
    public class DbgPapiPos : MonoBehaviour
    {
        public GameObject TrackedObj { get; set; }
        
        void OnGUI()
        {
            if (!(TrackedObj is null))
            {
                GUILayout.BeginArea(new Rect(Screen.width/2, 200, 400, 400));
                GUILayout.Label($"Name: {TrackedObj.name}" +
                                $"Position: {TrackedObj.transform.localPosition}" +
                                $"Rotation: {TrackedObj.transform.localRotation.eulerAngles}");
                GUILayout.EndArea();
            }
        }
    }
#endif
    public class ConfigData
    {
        public List<PapiData> PapiData { get; set; }
    }

    public struct Vector3Data
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public static implicit operator Vector3(Vector3Data v3d) => new Vector3(v3d.x, v3d.y, v3d.z);
    }

    public class PapiData
    {
        public string ID { get; set; }
        public string ParentName { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "Position")]
        public Vector3Data LocalPosition { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "Rotation")]
        public Vector3Data LocalRotation { get; set; }
    }

    public class KSP_PAPI_Loader : MonoBehaviour
    {
        ConfigData configData;

#if POSITIONING || LIGHTSADJUST
        Dictionary<string, KeyBinding> keyBindings;
        GameObject movethis;
        int movethisIdx;
        Vector3 papi_pos = Vector3.zero;
        Quaternion papi_rot = Quaternion.Euler(Vector3.zero);
#endif

        public void OnEnable()
        {
            GameManager.Instance.Game.Messages.Subscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        public void OnDisable()
        {
            GameManager.Instance.Game.Messages.Unsubscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        public void UpdatePapi()
        {
            foreach (PapiData papiData in configData.PapiData)
            {
                GameObject newPapi = GameObject.Find(papiData.ID);
                if (newPapi is null)
                    CreatePAPI(ref newPapi, GameObject.Find(papiData.ParentName), papiData.LocalPosition, Quaternion.Euler(papiData.LocalRotation), papiData.ID);
            }
        }

        public void ReloadConfig()
        {
            foreach (PapiData papiData in configData.PapiData)
            {
                GameObject o = GameObject.Find(papiData.ID);
                if (!(o is null))
                    o.DestroyGameObject();
            }
            foreach (KSP2Mod mod in GameManager.Instance.Game.KSP2ModManager.CurrentMods)
            {
                if (mod.ModName == "KSP_PAPI" || mod.ModName == "KSP2_PAPI")
                {
                    Log($"Reloaded Config");
                    configData = IOProvider.FromJsonFile<ConfigData>(Path.Combine(mod.ModRootPath, "config.json"));
                }
            }
            UpdatePapi();
        }

        public void Awake()
        {
            Log(MethodBase.GetCurrentMethod());
            configData = new ConfigData();
            foreach (KSP2Mod mod in GameManager.Instance.Game.KSP2ModManager.CurrentMods)
            {
                if (mod.ModName == "KSP_PAPI" || mod.ModName == "KSP2_PAPI")
                {
                    Log($"Mod root path: {mod.ModRootPath}");
                    Log($"Config path: {Path.Combine(mod.ModRootPath, "config.json")}");
                    configData = IOProvider.FromJsonFile<ConfigData>(Path.Combine(mod.ModRootPath, "config.json"));
                }
            }
            InvokeRepeating(nameof(UpdatePapi), 5, 30);
            //GameManager.Instance.Game.Input.Global.ToggleDebugWindow.ChangeCompositeBinding("OneModifier");
            //GameManager.Instance.Game.Input.Global.ToggleDebugWindow.ChangeBinding("Binding")
            //    .WithPath("<Keyboard>/f12");
            //GameManager.Instance.Game.Input.Global.ToggleDebugWindow.ChangeBinding("Modifier")
            //    .WithPath("<Keyboard>/leftShift");

#if POSITIONING || LIGHTSADJUST
            keyBindings = new Dictionary<string, KeyBinding>();
#endif
        }

        public void Start()
        {
            Log(MethodBase.GetCurrentMethod());

            DontDestroyOnLoad(this);

#if ON_SCREEN_LOG
            gameObject.AddComponent<ZzzLog>();
#endif
#if DBG_OBJECT_HOVER
            gameObject.AddComponent<DbgObjHover>();
#endif

#if POSITIONING
            gameObject.AddComponent<DbgPapiPos>();
            keyBindings.Add("mod_x", new KeyBinding(KeyCode.J));
            keyBindings.Add("mod_y", new KeyBinding(KeyCode.K));
            keyBindings.Add("mod_z", new KeyBinding(KeyCode.L));
            keyBindings.Add("mod_n", new KeyBinding(KeyCode.I));
            keyBindings.Add("mod_r", new KeyBinding(KeyCode.O));
            keyBindings.Add("mod_c", new KeyBinding(KeyCode.H));
            keyBindings.Add("mod_rst", new KeyBinding(KeyCode.P));
            keyBindings.Add("mod_nxt", new KeyBinding(KeyCode.ScrollLock));
            keyBindings.Add("mod_rld", new KeyBinding(KeyCode.F3));
#endif
#if LIGHTSADJUST
            keyBindings.Add("light_size+", new KeyBinding(KeyCode.J));
            keyBindings.Add("light_size-", new KeyBinding(KeyCode.K));
            keyBindings.Add("light_show", new KeyBinding(KeyCode.H));
#endif
#if POSITIONING || LIGHTSADJUST
            movethis = GameObject.Find(configData.PapiData[0].ID);
            movethisIdx = 0;
#endif
        }

        void OnGameStateChanged(MessageCenterMessage msg)
        {
            Log(MethodBase.GetCurrentMethod());
            GameState gameState = GameManager.Instance.Game.GlobalGameState.GetGameState().GameState;
            GameState prevGameState = GameManager.Instance.Game.GlobalGameState.GetLastGameState().GameState;
            bool show = (gameState == GameState.FlightView) || (gameState == GameState.KerbalSpaceCenter);
            bool prevShow = (prevGameState == GameState.FlightView) || (prevGameState == GameState.KerbalSpaceCenter);
            if ((show != prevShow) && show)
            {
                foreach (PapiData papiData in configData.PapiData)
                {
                    GameObject newPapi = null;
                    Log("Parent Name");
                    Log(papiData.ParentName);
                    Log("GameObject");
                    Log(GameObject.Find(papiData.ParentName));
                    CreatePAPI(ref newPapi, GameObject.Find(papiData.ParentName), papiData.LocalPosition, Quaternion.Euler(papiData.LocalRotation), papiData.ID);
                }
            }
        }

#if ON_SCREEN_LOG
        void FindObjectAtCursor()
        {
            Ray cursorRay = Camera.main.ScreenPointToRay(Mouse.Position);
            if (Physics.Raycast(cursorRay, out RaycastHit hitInfo))
            {
                Log(hitInfo.transform.gameObject.name);
            }
        }
#endif

        public void FixedUpdate()
        {
#if POSITIONING
            Vector3 movethis_position = papi_pos;
            Quaternion movethis_rotation = papi_rot;
            if (!(movethis is null))
            {
                Vector3 a = new Vector3(keyBindings["mod_x"].GetKey(true) ? 1 : 0, keyBindings["mod_y"].GetKey(true) ? 1 : 0, keyBindings["mod_z"].GetKey(true) ? 1 : 0);
                if (a != Vector3.zero)
                {
                    if (keyBindings["mod_r"].GetKey(true))
                    {
                        if (keyBindings["mod_n"].GetKey(true))
                        {
                            movethis.transform.localRotation = Quaternion.Euler(movethis.transform.localRotation.eulerAngles - a);
                        }
                        else
                        {
                            movethis.transform.localRotation = Quaternion.Euler(movethis.transform.localRotation.eulerAngles + a);
                        }
                    }
                    else if (keyBindings["mod_n"].GetKey(true))
                    {
                        movethis.transform.localPosition -= a;
                    }
                    else
                    {
                        movethis.transform.localPosition += a;
                    }
                }
                if (keyBindings["mod_c"].GetKeyDown(true))
                {
                    Log($"----- testPapi properties -----{Environment.NewLine}" +
                        $"transform.position = {movethis.transform.position}{Environment.NewLine}" +
                        $"transform.localPosition = {movethis.transform.localPosition}{Environment.NewLine}" +
                        $"transform.rotation.eulerAngles = {movethis.transform.rotation.eulerAngles}{Environment.NewLine}" +
                        $"transform.localRotation.eulerAngles = {movethis.transform.localRotation.eulerAngles}{Environment.NewLine}" +
                        $"distance = {Vector3.Distance(movethis.transform.position, Camera.main.gameObject.transform.position)}{Environment.NewLine}" +
                        $"-------------------------------");
                }
                if (keyBindings["mod_rst"].GetKeyDown(true))
                {
                    movethis.transform.localPosition = movethis_position;
                    movethis.transform.localRotation = movethis_rotation;
                }
            }
            else
            {
                movethis = GameObject.Find(configData.PapiData[movethisIdx].ID);
            }
            if (keyBindings["mod_nxt"].GetKeyDown(true))
            {
                if (!keyBindings["mod_n"].GetKey(true))
                {
                    movethisIdx = movethisIdx + 1 >= configData.PapiData.Count ? configData.PapiData.Count - 1 : movethisIdx + 1;
                    movethis = GameObject.Find(configData.PapiData[movethisIdx].ID);
                    GetComponent<DbgPapiPos>().TrackedObj = movethis;
                }
                else
                {
                    movethisIdx = movethisIdx - 1 < 0 ? 0 : movethisIdx - 1;
                    movethis = GameObject.Find(configData.PapiData[movethisIdx].ID);
                    GetComponent<DbgPapiPos>().TrackedObj = movethis;
                }
            }
            if (keyBindings["mod_rld"].GetKeyDown(true))
            {
                ReloadConfig();
            }
#endif
#if LIGHTSADJUST
            if (keyBindings["light_size+"].GetKey(true) && movethis != null)
            {
                movethis.GetComponent<KSP_PAPI>().LightSize += 0.05f;
            }
            else if (keyBindings["light_size-"].GetKey(true) && movethis != null)
            {
                movethis.GetComponent<KSP_PAPI>().LightSize -= 0.05f;
            }
            else if (keyBindings["light_show"].GetKeyDown(true))
            {
                Log(movethis.GetComponent<KSP_PAPI>().LightSize);
            }
#endif
        }

        void CreatePAPI(ref GameObject papi, GameObject parent, Vector3 position, Quaternion rotation, string name)
        {
            Log(MethodBase.GetCurrentMethod());
            if (parent != null)
            {
                AsyncOperationHandle<GameObject> operation = GameManager.Instance.Game.Assets.CreateAsyncRaw("KSP2_PAPI/KSP_PAPI.prefab");
                operation.WaitForCompletion();
                if (operation.Status == AsyncOperationStatus.Failed)
                {
                    LogError("Failed to load prefab \"KSP2_PAPI/KSP_PAPI.prefab\"");
                }
                else
                {
                    papi = operation.Result;
                    papi.name = name;
                    papi.AddComponent<KSP_PAPI>();
                    papi.transform.parent = parent.transform;
                    papi.transform.localPosition = position;
                    papi.transform.localRotation = rotation;
                    Log("Spawned new PAPI");
                }
            }
            else
            {
                LogError("No parent set: CreatePAPI()");
            }
        }

        void Log(object o)
        {
            Debug.Log("[KSP_PAPI]: " + (o is null ? "null" : o));
        }

        void LogError(object o)
        {
            Debug.LogError("[KSP_PAPI]: (ERROR) -> " + o);
        }

        void LogLine(string msg = "Log", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
        {
            Log(msg + " at line " + lineNumber + " (" + caller + ")");
        }
    }
}
