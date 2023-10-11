using System.Collections.Generic;
using UnityEngine;

namespace ksp2_papi
{
    public class Papi4Behaviour : MonoBehaviour
    {
        public Color HighColor = Color.white;
        public Color LowColor = Color.red;
        public Color OffColor = Color.black;
        public float[] Slopes = new float[4] { 2.5f, 2.8f, 3.2f, 3.5f };
        public float TransitionRange = 0.1f;
        public float CutoffAngle = 10f;
        public float CutoffMultiplier = 0.1f;
        public float MaxDistance = 50000f;
        public float MinDistance = 50f;
        public float HorizontalSeparation = 9f;
        public float Off = 0f;

        private List<PapiBehaviour> _papis;

        void Start()
        {
            _papis = new List<PapiBehaviour>
            {
                transform.Find("papi_ll").gameObject.AddComponent<PapiBehaviour>(),
                transform.Find("papi_lm").gameObject.AddComponent<PapiBehaviour>(),
                transform.Find("papi_rm").gameObject.AddComponent<PapiBehaviour>(),
                transform.Find("papi_rr").gameObject.AddComponent<PapiBehaviour>()
            };
            ApplyFieldChanges();
        }

        public void ApplyFieldChanges()
        {
            for (var i = 0; i < _papis.Count; i++)
            {
                _papis[i].Slope = Slopes[i];
                _papis[i].transform.localPosition = new Vector3(0f, 0f, 1.5f * HorizontalSeparation - i * HorizontalSeparation);
            }
            foreach (var i in _papis)
            {
                i.HighColor = HighColor;
                i.LowColor = LowColor;
                i.OffColor = OffColor;
                i.CutoffAngle = CutoffAngle;
                i.CutoffMultiplier = CutoffMultiplier;
                i.TransitionRange = TransitionRange;
                i.MaxDistance = MaxDistance;
                i.MinDistance = MinDistance;
                i.Off = Off;
            }
        }
    }
}