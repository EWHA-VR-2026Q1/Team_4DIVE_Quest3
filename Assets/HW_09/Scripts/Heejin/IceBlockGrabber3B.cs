using UnityEngine;

namespace HW09.Heejin
{
    /// <summary>
    /// Scene3_B 아이스 블럭 픽업 스크립트.
    /// 플레이어가 손 트리거(중지)를 눌러 가까이 있는 얼음을 잡고,
    /// 손을 떼면 손의 속도로 던진다.
    /// 사용법: IceBlock 오브젝트에 Rigidbody + Collider와 함께 부착.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class IceBlockGrabber3B : MonoBehaviour
    {
        [Header("── 그랩 설정 ──────────────────────────")]
        [Tooltip("이 거리 안에 손이 있어야 잡을 수 있음 (m)")]
        public float grabRadius = 0.4f;

        [Tooltip("던질 때 속도 배율 (1.0 = 손 속도 그대로)")]
        public float throwVelocityScale = 1.5f;

        [Header("── 손 선택 ────────────────────────────")]
        [Tooltip("어느 손으로 잡을 수 있는지: 양손 모두 가능")]
        public bool allowLeftHand  = true;
        public bool allowRightHand = true;

        [Header("── 디버그 ────────────────────────────")]
        [Tooltip("Scene 뷰에서 그랩 영역 표시")]
        public bool drawGizmo = true;

        // ── 내부 상태 ──
        private Rigidbody _rb;
        private Transform _leftAnchor;
        private Transform _rightAnchor;
        private Transform _grabbedBy;          // 현재 잡고 있는 손 (없으면 null)
        private Vector3 _previousHandPosition;
        private Vector3 _handVelocity;
        private Transform _originalParent;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _leftAnchor  = ResolveAnchor(true);
            _rightAnchor = ResolveAnchor(false);
            _originalParent = transform.parent;

            if (_leftAnchor == null && _rightAnchor == null)
            {
                Debug.LogWarning($"[IceBlockGrabber3B:{name}] 손 앵커를 찾을 수 없음. " +
                                 $"OVRCameraRig가 씬에 있는지 확인하세요.");
            }
        }

        void Update()
        {
            // 잡고 있는 동안 손 속도 추적 (던지기용)
            if (_grabbedBy != null)
            {
                _handVelocity = (_grabbedBy.position - _previousHandPosition) / Time.deltaTime;
                _previousHandPosition = _grabbedBy.position;

                // 손 트리거를 떼면 놓기
                if (!IsGrabButtonPressed(_grabbedBy == _leftAnchor))
                {
                    Release();
                }
                return;
            }

            // 안 잡고 있을 때: 양손 중 그랩 버튼 누르고 가까이 있으면 잡기
            if (allowRightHand && _rightAnchor != null
                && IsGrabButtonPressed(false)
                && IsHandClose(_rightAnchor))
            {
                Grab(_rightAnchor);
                return;
            }

            if (allowLeftHand && _leftAnchor != null
                && IsGrabButtonPressed(true)
                && IsHandClose(_leftAnchor))
            {
                Grab(_leftAnchor);
                return;
            }
        }

        // ── 잡기 ──────────────────────────────────────
        void Grab(Transform handAnchor)
        {
            _grabbedBy = handAnchor;
            _previousHandPosition = handAnchor.position;
            _handVelocity = Vector3.zero;

            _rb.isKinematic = true;            // 잡고 있는 동안 물리 끄기
            _rb.useGravity = false;
            transform.SetParent(handAnchor);   // 손에 부착 (손 따라 움직임)
        }

        // ── 놓기 (던지기) ──────────────────────────────
        void Release()
        {
            transform.SetParent(_originalParent);
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = _handVelocity * throwVelocityScale;
            _grabbedBy = null;
        }

        // ── 손이 가까이 있는지 ─────────────────────────
        bool IsHandClose(Transform hand)
        {
            return Vector3.Distance(hand.position, transform.position) <= grabRadius;
        }

        // ── 그랩 버튼 입력 (중지 트리거) ────────────────
        bool IsGrabButtonPressed(bool isLeft)
        {
            return isLeft
                ? OVRInput.Get(OVRInput.RawButton.LHandTrigger)
                : OVRInput.Get(OVRInput.RawButton.RHandTrigger);
        }

        // ── 손 앵커 자동 찾기 ─────────────────────────
        Transform ResolveAnchor(bool isLeft)
        {
            string[] names = isLeft
                ? new[] { "LeftControllerAnchor", "LeftControllerInHandAnchor", "LeftHandAnchor" }
                : new[] { "RightControllerAnchor", "RightControllerInHandAnchor", "RightHandAnchor" };

            foreach (string n in names)
            {
                var go = GameObject.Find(n);
                if (go != null) return go.transform;
            }
            return null;
        }

        // ── Scene 뷰 시각화 ──────────────────────────
        void OnDrawGizmosSelected()
        {
            if (!drawGizmo) return;
            Gizmos.color = new Color(0.5f, 0.9f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, grabRadius);
        }
    }
}
