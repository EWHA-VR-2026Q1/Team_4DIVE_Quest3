#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace HW09.Heejin
{
    [RequireComponent(typeof(Rigidbody))]
    public class EditorTestCamera4B : MonoBehaviour
    {
        public float moveSpeed = 3f;
        public float rotationSensitivity = 0.1f;

        private float _yaw;
        private float _pitch;
        private Rigidbody _rb;
        private Vector3 _moveDir;

        void Start()
        {
            _yaw   = transform.eulerAngles.y;
            _pitch = transform.eulerAngles.x;
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity    = false;
            _rb.freezeRotation = true;
        }

        void Update()
        {
            var mouse    = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null || keyboard == null) return;

            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                _yaw   += delta.x * rotationSensitivity;
                _pitch -= delta.y * rotationSensitivity;
                _pitch  = Mathf.Clamp(_pitch, -80f, 80f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            Vector3 right   = Vector3.ProjectOnPlane(transform.right,   Vector3.up).normalized;

            _moveDir = Vector3.zero;
            if (keyboard.wKey.isPressed) _moveDir += forward;
            if (keyboard.sKey.isPressed) _moveDir -= forward;
            if (keyboard.aKey.isPressed) _moveDir -= right;
            if (keyboard.dKey.isPressed) _moveDir += right;
        }

        void FixedUpdate()
        {
            Vector3 newPos = _rb.position + _moveDir * moveSpeed * Time.fixedDeltaTime;
            newPos.y = 1.6f;
            _rb.MovePosition(newPos);
        }
    }
}
#endif
