using System;
using UnityEngine;

namespace ksp2_papi.Editor
{
    /// <summary>
    /// <b>Intended for editor use only!</b><br/>
    /// Controls the camera in a similar way that many 3D programs do it.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [DisallowMultipleComponent]
    public class CameraController : MonoBehaviour
    {
        [Header("Movement")]
        public float speedMultiplier;
        public float fastSpeedMultiplier;
        public float panMultiplier;
        public float scrollMultiplier;
        public float smoothingTime;
        [Header("Keybinds")]
        public KeyCode Forward;
        public KeyCode Backward;
        public KeyCode Left;
        public KeyCode Right;
        public KeyCode Up;
        public KeyCode Down;
        public KeyCode MoveFast;
        [Header("Rotation")]
        public float sensitivity;
        public LimitFloat verticalRotationLimit;

        private Vector3 _lastMousePos;
        private LimitFloat _vrl;
        private float _lastUpdate;

        private void Update()
        {
            var deltaTime = Time.unscaledTime - _lastUpdate;
            var fwdAxis = Input.GetKey(Forward) ? (Input.GetKey(Backward) ? 0 : 1) : (Input.GetKey(Backward) ? -1 : 0);
            var horAxis = Input.GetKey(Right) ? (Input.GetKey(Left) ? 0 : 1) : (Input.GetKey(Left) ? -1 : 0);
            var vertAxis = Input.GetKey(Up) ? (Input.GetKey(Down) ? 0 : 1) : (Input.GetKey(Down) ? -1 : 0);
            var inputVector = new Vector3(horAxis, vertAxis, fwdAxis);
            var mousePosition = Input.mousePosition;
            var mouseMovement = mousePosition - _lastMousePos;
            inputVector.Normalize();
            var move = (Input.GetKey(MoveFast) ? fastSpeedMultiplier : speedMultiplier) * deltaTime * inputVector;
            var cameraRotation = transform.rotation;
            if (Input.GetMouseButton(1) && !Input.GetMouseButtonDown(1))
            {
                var rotMouseMovement = sensitivity * mouseMovement;
                if (_vrl.Apply)
                {
                    var eulRot = new Vector3(transform.localRotation.eulerAngles.x, 0, 0);
                    eulRot.y += rotMouseMovement.x;
                    eulRot.x = _vrl.Apply ? RestrictOutside(eulRot.x - rotMouseMovement.y, _vrl.Min, _vrl.Max) : eulRot.x - rotMouseMovement.y;
                    cameraRotation *= Quaternion.AngleAxis(eulRot.y, transform.InverseTransformDirection(Vector3.up));
                    cameraRotation *= Quaternion.AngleAxis(transform.localRotation.eulerAngles.x - eulRot.x, transform.InverseTransformVector(Quaternion.AngleAxis(-90, transform.up) * transform.forward));
                }
                else
                {
                    cameraRotation *= Quaternion.AngleAxis(rotMouseMovement.x, transform.InverseTransformDirection(Vector3.up));
                    cameraRotation *= Quaternion.AngleAxis(rotMouseMovement.y, transform.InverseTransformVector(Quaternion.AngleAxis(-90, transform.up) * transform.forward));
                }
            }
            else if (Input.GetMouseButton(2) && !Input.GetMouseButtonDown(2))
            {
                move = deltaTime * panMultiplier * -mouseMovement;
            }
            move.z += Input.mouseScrollDelta.y * scrollMultiplier;
            transform.position += (Vector3)(transform.localToWorldMatrix * move);
            transform.rotation = cameraRotation;
            _lastMousePos = mousePosition;
            _lastUpdate = Time.unscaledTime;
        }

        void OnValidate()
        {
            _vrl.Apply = verticalRotationLimit.Apply;
            _vrl.Min = -verticalRotationLimit.Min;
            _vrl.Max = 360 - verticalRotationLimit.Max;
        }

        float RestrictOutside(float x, float lowerEnd, float upperEnd)
        {
            if (x <= lowerEnd || x >= upperEnd)
                return x;
            if (upperEnd - x < x - lowerEnd)
                return upperEnd;
            return lowerEnd;
        }
    }

    [Serializable]
    public struct LimitFloat
    {
        public bool Apply;
        public float Min;
        public float Max;
    };
}