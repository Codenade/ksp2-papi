using UnityEngine;

namespace KSP2_PAPI
{
    public class KSP_PAPI : MonoBehaviour
    {
        /// <summary>Basically always White</summary>
        public Color HighColor { get; set; } = Color.white;
        /// <summary>Basically always Red</summary>
        public Color LowColor { get; set; } = Color.red;
        /// <summary>Lamp thresholds in degrees { LL, LM, RM, RR }</summary>
        public float[] Slopes { get; set; } = new float[] { 2.5f, 2.8f, 3.2f, 3.5f };
        /// <summary>Area in which the lights transistion between colors. Unit: Degrees</summary>
        public float TransitionRange { get; set; } = 0.1f;
        /// <summary>Adjusts the size of the halo</summary>
        public float LightSize { get; set; } = 6.6f;
        /// <summary>Angle at which the halos disappear (fade)</summary>
        public float CutoffAngle { get; set; } = 50f;
        /// <summary>Set the cutoff range</summary>
        public float CutoffMultiplier { get; set; } = 2f;
        /// <summary>Distance at which the halos disappear (fade)</summary>
        public float MaxDistance { get; set; } = 20000f;

        Material mat;
        Light ll, lm, rm, rr;

        void Start()
        {
            mat = gameObject.GetComponent<MeshRenderer>().material;
            ll = transform.Find("LL").GetComponent<Light>();
            lm = transform.Find("LM").GetComponent<Light>();
            rm = transform.Find("RM").GetComponent<Light>();
            rr = transform.Find("RR").GetComponent<Light>();
            ll.enabled = true;
            lm.enabled = true;
            rm.enabled = true;
            rr.enabled = true;
            ll.range = LightSize;
            lm.range = LightSize;
            rm.range = LightSize;
            rr.range = LightSize;
        }

        void Update()
        {
            Vector3 camPosWorld = Camera.main.transform.position;
            Vector3 camPos = gameObject.transform.worldToLocalMatrix * new Vector4(camPosWorld.x, camPosWorld.y, camPosWorld.z, 1);
            float camAngle = Vector3.Angle(Vector3.left, camPos); // view angle of the camera repecting both the z and the y axis rotation
            float angle = Mathf.Rad2Deg * Mathf.Asin(camPos.y / -camPos.x); // angle on the local z axis
            float fade = (Mathf.Abs(camAngle) / CutoffAngle - 1) * CutoffMultiplier;
            mat.SetFloat("_Angle", angle);
            ll.color = Color.Lerp(LowColor, HighColor, Mathf.Clamp01((angle - Slopes[0] + (TransitionRange / 2.0f)) / TransitionRange));
            lm.color = Color.Lerp(LowColor, HighColor, Mathf.Clamp01((angle - Slopes[1] + (TransitionRange / 2.0f)) / TransitionRange));
            rm.color = Color.Lerp(LowColor, HighColor, Mathf.Clamp01((angle - Slopes[2] + (TransitionRange / 2.0f)) / TransitionRange));
            rr.color = Color.Lerp(LowColor, HighColor, Mathf.Clamp01((angle - Slopes[3] + (TransitionRange / 2.0f)) / TransitionRange));
            float lsize = LightSize - Mathf.Clamp(LightSize * fade, 0, LightSize);
            ll.enabled = fade < 1f;
            lm.enabled = fade < 1f;
            rm.enabled = fade < 1f;
            rr.enabled = fade < 1f;
            ll.range = lsize;
            lm.range = lsize;
            rm.range = lsize;
            rr.range = lsize;
        }
    }
}