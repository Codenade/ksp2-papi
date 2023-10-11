using UnityEngine;

namespace ksp2_papi
{
    public class PapiBehaviour : MonoBehaviour
    {
        public Color HighColor { get => _mat.GetColor("_HighColor"); set => _mat.SetColor("_HighColor", value); }
        public Color LowColor { get => _mat.GetColor("_LowColor"); set => _mat.SetColor("_LowColor", value); }
        public Color OffColor { get => _mat.GetColor("_OffColor"); set => _mat.SetColor("_OffColor", value); }
        public float Slope { get => -_headTransform.localRotation.z; set => _headTransform.localRotation = Quaternion.Euler(0f, 0f, -value); }
        public float TransitionRange { get => _mat.GetFloat("_Transition"); set => _mat.SetFloat("_Transition", value); }
        public float CutoffAngle { get; set; }
        public float CutoffMultiplier { get; set; }
        public float MaxDistance { get; set; }
        public float MinDistance { get; set; }
        public float Off { get; set; }

        private Transform _headTransform;
        private Material _mat;
#if !UNITY_BUILD && !UNITY_EDITOR
        private FlareOcclusion occlusion;
        private LensFlare _flare;
#endif

        void Awake()
        {
#if !UNITY_BUILD && !UNITY_EDITOR
            _headTransform = transform.Find("papi_head");
            _mat = _headTransform.GetComponent<MeshRenderer>().material;
            _flare = transform.Find("papi_head/flare").gameObject.AddComponent<LensFlare>();
            occlusion = transform.Find("papi_head/flare").gameObject.AddComponent<FlareOcclusion>();
            occlusion.flare = _flare;
#endif
        }

        void Update()
        {
            var camPos = (Vector3)(_headTransform.worldToLocalMatrix * ToVector4(Camera.main.transform.position, 1));
            var angle = Mathf.Rad2Deg * Mathf.Atan(camPos.y / -camPos.x);
            var off = Mathf.Clamp01(Off + Mathf.Clamp01((camPos.magnitude - MaxDistance) * CutoffMultiplier) + Mathf.Clamp01((Vector3.Angle(Vector3.left, camPos) - CutoffAngle) * CutoffMultiplier));
            _mat.SetFloat("_Angle", angle);
            _mat.SetFloat("_Off", off);
#if !UNITY_BUILD && !UNITY_EDITOR
            _flare.color = Color.Lerp(Color.Lerp(LowColor, HighColor, Mathf.Clamp01(angle / TransitionRange + 0.5f)), OffColor, off);
            _flare.enabled = off != 1 && camPos.magnitude >= MinDistance;
#endif
        }

        public static Vector4 ToVector4(Vector3 a, float w) => new Vector4(a.x, a.y, a.z, w);
    }
}