using UnityEngine;

namespace ksp2_papi
{
    public class PapiBehaviour : MonoBehaviour
    {
        public Color HighColor { get => _mat.GetColor(_idHighColor); set => _mat.SetColor(_idHighColor, value); }
        public Color LowColor { get => _mat.GetColor(_idLowColor); set => _mat.SetColor(_idLowColor, value); }
        public Color OffColor { get => _mat.GetColor(_idOffColor); set => _mat.SetColor(_idOffColor, value); }
        public float Slope { get => -_headTransform.localRotation.z; set => _headTransform.localRotation = Quaternion.Euler(0f, 0f, -value); }
        public float TransitionRange { get => _mat.GetFloat(_idTransition); set => _mat.SetFloat(_idTransition, value); }
        public float CutoffAngle { get; set; }
        public float CutoffMultiplier { get; set; }
        public float MaxDistance { get; set; }
        public float MinDistance { get; set; }
        public float Off { get; set; }

        private Transform _headTransform;
        private Material _mat;
        private int _idHighColor;
        private int _idLowColor;
        private int _idOffColor;
        private int _idTransition;
        private int _idAngle;
        private int _idOff;
#if !UNITY_BUILD && !UNITY_EDITOR
        private LensFlare _flare;
#endif

        private void Awake()
        {
#if !UNITY_BUILD && !UNITY_EDITOR
            _idHighColor = Shader.PropertyToID("_HighColor");
            _idLowColor = Shader.PropertyToID("_LowColor");
            _idOffColor = Shader.PropertyToID("_OffColor");
            _idTransition = Shader.PropertyToID("_Transition");
            _idAngle = Shader.PropertyToID("_Angle");
            _idOff = Shader.PropertyToID("_Off");
            _headTransform = transform.Find("papi_head");
            _mat = _headTransform.GetComponent<MeshRenderer>().material;
            _flare = transform.Find("papi_head/flare").gameObject.AddComponent<LensFlare>();
            _flare.flare = (Flare)AssetUtils.Assets[AssetUtils.Keys.flare];
            FlareOcclusionManager.Instance.RegisterFlare(_flare);
#endif
        }

#if !UNITY_BUILD && !UNITY_EDITOR
        private void OnDestroy()
        {
            FlareOcclusionManager.Instance.UnregisterFlare(_flare);
        }
#endif

        private void Update()
        {
#if !UNITY_BUILD && !UNITY_EDITOR
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;
            var camPos = (Vector3)(_headTransform.worldToLocalMatrix * mainCamera.transform.position.ToVector4(1));
            var angle = Mathf.Rad2Deg * Mathf.Atan(camPos.y / -camPos.x);
            var off = Mathf.Clamp01(Off + Mathf.Clamp01((camPos.magnitude - MaxDistance) * CutoffMultiplier) + Mathf.Clamp01((Vector3.Angle(Vector3.left, camPos) - CutoffAngle) * CutoffMultiplier));
            _mat.SetFloat(_idAngle, angle);
            _mat.SetFloat(_idOff, off);
            _flare.color = Color.Lerp(Color.Lerp(LowColor, HighColor, Mathf.Clamp01(angle / TransitionRange + 0.5f)), OffColor, off);
            _flare.enabled = off != 1 && camPos.magnitude >= MinDistance;
#endif
        }
    }
}