//#define POSITIONING // define for in game keybinds to bring the papi into position
//#define LIGHTSADJUST // define for in game keybinds to change different properties of the attached lights
//#define ON_SCREEN_LOG

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

    public class KSP_PAPI_Loader : KerbalMonoBehaviour
    {
        // TODO: not urgent: do some cleanup
        GameObject ksc_papi_09_a;
        GameObject ksc_papi_27_a;
        GameObject ksc_papi_09_b;
        GameObject ksc_papi_27_b;

        string runway_a_name = "col_box_runwayA";
        string runway_b_name = "col_box_runwayB";
#if POSITIONING || LIGHTSADJUST
        Dictionary<string, KeyBinding> keyBindings;
#endif
        Vector3    ksc_papi_27_a_position;
        Quaternion ksc_papi_27_a_rotation;
        Vector3    ksc_papi_09_a_position;
        Quaternion ksc_papi_09_a_rotation;
        Vector3    ksc_papi_27_b_position;
        Quaternion ksc_papi_27_b_rotation;
        Vector3    ksc_papi_09_b_position;
        Quaternion ksc_papi_09_b_rotation;

        public void Awake()
        {
            Log(MethodBase.GetCurrentMethod());
            //GameManager.Instance.Game.Input.Global.ToggleDebugWindow.ChangeCompositeBinding("OneModifier");
            //GameManager.Instance.Game.Input.Global.ToggleDebugWindow.ChangeBinding("Binding")
            //    .WithPath("<Keyboard>/f12");
            //GameManager.Instance.Game.Input.Global.ToggleDebugWindow.ChangeBinding("Modifier")
            //    .WithPath("<Keyboard>/leftShift");

#if POSITIONING || LIGHTSADJUST
            keyBindings = new Dictionary<string, KeyBinding>();
#endif
            ksc_papi_27_a_position = new Vector3(-2207, 960, 6);
            ksc_papi_27_a_rotation = Quaternion.Euler(0, 270, 270);
            ksc_papi_09_a_position   = new Vector3(-1991, -4305, 6);
            ksc_papi_09_a_rotation   = Quaternion.Euler(0, 90, 90);
            ksc_papi_27_b_position   = new Vector3(-2449, 960, 6);
            ksc_papi_27_b_rotation   = Quaternion.Euler(0, 270, 270);
            ksc_papi_09_b_position   = new Vector3(-2228, -4305, 6);
            ksc_papi_09_b_rotation   = Quaternion.Euler(0, 90, 90);
        }

        public void Start()
        {
            Log(MethodBase.GetCurrentMethod());

            DontDestroyOnLoad(this);
#if ON_SCREEN_LOG
            //InvokeRepeating("FindObjectAtCursor", .5f, .5f);
            gameObject.AddComponent<ZzzLog>();
#endif

#if POSITIONING
            keyBindings.Add("mod_x", new KeyBinding(KeyCode.J));
            keyBindings.Add("mod_y", new KeyBinding(KeyCode.K));
            keyBindings.Add("mod_z", new KeyBinding(KeyCode.L));
            keyBindings.Add("mod_n", new KeyBinding(KeyCode.I));
            keyBindings.Add("mod_r", new KeyBinding(KeyCode.O));
            keyBindings.Add("mod_c", new KeyBinding(KeyCode.H));
            keyBindings.Add("mod_rst", new KeyBinding(KeyCode.P));
#endif
#if LIGHTSADJUST
            keyBindings.Add("light_size+", new KeyBinding(KeyCode.J));
            keyBindings.Add("light_size-", new KeyBinding(KeyCode.K));
            keyBindings.Add("light_show", new KeyBinding(KeyCode.H));
#endif
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

        public void Update()
        {
            if (ksc_papi_27_a == null)
            {
                CreatePAPI(ref ksc_papi_27_a, GameObject.Find(runway_a_name), ksc_papi_27_a_position, ksc_papi_27_a_rotation);
            }
            if (ksc_papi_09_a == null)
            {
                CreatePAPI(ref ksc_papi_09_a, GameObject.Find(runway_a_name), ksc_papi_09_a_position, ksc_papi_09_a_rotation);
            }
            if (ksc_papi_27_b == null)
            {
                CreatePAPI(ref ksc_papi_27_b, GameObject.Find(runway_b_name), ksc_papi_27_b_position, ksc_papi_27_b_rotation);
            }
            if (ksc_papi_09_b == null)
            {
                CreatePAPI(ref ksc_papi_09_b, GameObject.Find(runway_b_name), ksc_papi_09_b_position, ksc_papi_09_b_rotation);
            }
        }

        public void FixedUpdate()
        {
#if POSITIONING
            GameObject movethis = ksc_papi_09_b;
            Vector3 movethis_position = ksc_papi_09_b_position;
            Quaternion movethis_rotation = ksc_papi_09_b_rotation;
            if (movethis != null)
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
#endif
#if LIGHTSADJUST
            if (keyBindings["light_size+"].GetKey(true) && testPapi != null)
            {
                testPapi.GetComponent<KSP_PAPI>().LightSize += 0.05f;
            }
            else if (keyBindings["light_size-"].GetKey(true) && testPapi != null)
            {
                testPapi.GetComponent<KSP_PAPI>().LightSize -= 0.05f;
            }
            else if (keyBindings["light_show"].GetKeyDown(true))
            {
                Log(testPapi.GetComponent<KSP_PAPI>().LightSize);
            }
#endif
        }

        void CreatePAPI(ref GameObject papi, GameObject parent, Vector3 position, Quaternion rotation)
        {
            Log(MethodBase.GetCurrentMethod());
            if (parent != null)
            {
                AsyncOperationHandle<GameObject> operation = Addressables.InstantiateAsync("Assets/KSP_PAPI/KSP_PAPI.prefab");
                operation.WaitForCompletion();
                if (operation.Status == AsyncOperationStatus.Failed)
                {
                    LogError("Failed to load prefab \"Assets/KSP_PAPI/KSP_PAPI.prefab\"");
                }
                else
                {
                    papi = operation.Result;
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
            Debug.Log("[KSP_PAPI]: " + o);
        }

        void LogError(object o)
        {
            Debug.LogError("[KSP_PAPI]: (ERROR) -> " + o);
        }
    }
}
