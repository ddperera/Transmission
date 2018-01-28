using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        public OrbitCamera OrbitCamera;
        public Transform CameraFollowPoint;
        public BikeCharacterController Character;
        public float MouseSensitivity = 1f;
        public Collider[] IgnoredColliders;

        private Vector3 _moveInputVector = Vector3.zero;
        private Vector3 _lookInputVector = Vector3.zero;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            Character.IgnoredColliders = IgnoredColliders;

            if (OrbitCamera) 
            {
                OrbitCamera.SetFollowTransform(CameraFollowPoint);
                OrbitCamera.IgnoredColliders = new Collider[IgnoredColliders.Length + 1];
                for (int i = 0; i < IgnoredColliders.Length; i++)
                {
                    OrbitCamera.IgnoredColliders[i] = IgnoredColliders[i];
                }
                OrbitCamera.IgnoredColliders[IgnoredColliders.Length] = Character.GetComponentInChildren<Collider>();
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            // Gather input
			float moveAxisForward = Character.KinematicCharacterMotor.IsStableOnGround ? 1.0f : 0.0f;
			float moveAxisRight = 0f;
            float mouseLookAxisUp = Input.GetAxisRaw("Mouse Y");
			float mouseLookAxisRight = Input.GetAxis("Horizontal");
            _moveInputVector = new Vector3(moveAxisRight, 0f, moveAxisForward);
            _moveInputVector = Vector3.ClampMagnitude(_moveInputVector, 1f);

            // Apply mouse sensitivity
            _lookInputVector = new Vector3(mouseLookAxisRight * MouseSensitivity, mouseLookAxisUp * MouseSensitivity, 0f);

            if (Cursor.lockState != CursorLockMode.Locked)
            {
                //_lookInputVector = Vector3.zero;
            }

            if (Character && OrbitCamera)
            {
                // Apply move input to character
                Vector3 cameraOrientedInput = Quaternion.LookRotation(OrbitCamera.PlanarDirection, OrbitCamera.transform.up) * _moveInputVector;
                Character.SetInputs(cameraOrientedInput, OrbitCamera.PlanarDirection);

                // Apply input to camera
                float scrollInput = -Input.GetAxis("Mouse ScrollWheel");
#if UNITY_WEBGL
                scrollInput = 0f;
#endif
                OrbitCamera.SetInputs(scrollInput, _lookInputVector);

                if(Input.GetMouseButtonDown(1))
                {
                    OrbitCamera.TargetDistance = (OrbitCamera.TargetDistance == 0f) ? OrbitCamera.DefaultDistance : 0f;
                }
            }
        }
    }
}