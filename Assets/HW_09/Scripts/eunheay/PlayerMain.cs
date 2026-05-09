using UnityEngine;

namespace HW09
{
    /// <summary>
    /// MainScene 플레이어 이동 스크립트
    /// OVRCameraRig(헤드 트래킹) + OVRInput(조이스틱) 방식
    /// Player_OVRInput_Parts 프리팹에 부착
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMain : MonoBehaviour
    {
        [Header("이동 속도")]
        public float moveSpeed   = 3f;
        public float sprintSpeed = 6f;

        [Header("VR 스냅 턴")]
        public float snapAngle    = 45f;
        public float snapDeadzone = 0.6f;

        // OVRCameraRig의 centerEyeAnchor (이동 방향 기준)
        protected Transform _centerEye;
        protected CharacterController _cc;

        private float _verticalVelocity;
        private bool  _snapReady = true;

        protected virtual void Awake()
        {
            _cc = GetComponent<CharacterController>();

            // OVRCameraRig에서 centerEyeAnchor 자동 탐색
            var rig = GetComponentInChildren<OVRCameraRig>();
            if (rig != null)
                _centerEye = rig.centerEyeAnchor;
        }

        protected virtual void Update()
        {
            HandleMove();
            HandleSnapTurn();
        }

        protected void HandleMove()
        {
            // 중력
            if (_cc.isGrounded)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += Physics.gravity.y * Time.deltaTime;

            // 왼손 조이스틱
            Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

            // PC 에디터 WASD 폴백
            if (input.magnitude < 0.1f)
            {
#if UNITY_EDITOR
                if (Input.GetKey(KeyCode.W)) input.y += 1f;
                if (Input.GetKey(KeyCode.S)) input.y -= 1f;
                if (Input.GetKey(KeyCode.D)) input.x += 1f;
                if (Input.GetKey(KeyCode.A)) input.x -= 1f;
#endif
            }

            // 카메라 방향 기준 이동
            Transform pivot = _centerEye != null ? _centerEye : transform;
            Vector3 fwd   = Vector3.ProjectOnPlane(pivot.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(pivot.right,   Vector3.up).normalized;

            // 달리기: 왼손 썸스틱 누르기
            bool sprint = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);
            float speed = sprint ? sprintSpeed : moveSpeed;

            Vector3 move = (fwd * input.y + right * input.x) * speed;
            _cc.Move((move + Vector3.up * _verticalVelocity) * Time.deltaTime);
        }

        protected void HandleSnapTurn()
        {
            float x = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

            if (Mathf.Abs(x) >= snapDeadzone)
            {
                if (_snapReady)
                {
                    transform.Rotate(Vector3.up, snapAngle * Mathf.Sign(x));
                    _snapReady = false;
                }
            }
            else
            {
                _snapReady = true;
            }
        }
    }
}
