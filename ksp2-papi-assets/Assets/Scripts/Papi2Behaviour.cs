using System.Collections.Generic;
using UnityEngine;

namespace ksp2_papi
{
    public class Papi2Behaviour : MonoBehaviour
    {
        public readonly Color HighColor = Color.white;
        public readonly Color LowColor = Color.red;
        public readonly Color OffColor = Color.black;
        public readonly float[] Slopes = new float[2] { 2.8f, 3.2f };
        public const float TransitionRange = 0.1f;
        public const float CutoffAngle = 10f;
        public const float CutoffMultiplier = 0.1f;
        public const float MaxDistance = 30000f;
        public const float MinDistance = 50f;
        public const float HorizontalSeparation = 9f;
        public const float Off = 0f;

        private List<PapiBehaviour> _papis;

        private void Start()
        {
            _papis = new List<PapiBehaviour>
            {
                transform.Find("papi_l").gameObject.AddComponent<PapiBehaviour>(),
                transform.Find("papi_r").gameObject.AddComponent<PapiBehaviour>()
            };
            ApplyFieldChanges();
        }

        public void ApplyFieldChanges()
        {
            for (var i = 0; i < _papis.Count; i++)
            {
                _papis[i].Slope = Slopes[i];
                _papis[i].transform.localPosition = new Vector3(0f, 0f, 0.5f * HorizontalSeparation - i * HorizontalSeparation);
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