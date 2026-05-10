using UnityEngine;

namespace HW09.Heejin
{
    /// <summary>
    /// Scene3_B 아이스 블럭 픽업 스크립트.
    /// 컨트롤러(HandTrigger 버튼) + 핸드트래킹(검지 핀치) 양쪽 모두 지원.
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
        public bool allowLeftHand  = true;
        public bool allowRightHand = true;

        [Header("── 입력 모드 ──────────────────────────")]
        [Tooltip("컨트롤러 HandTrigger 버튼으로 잡기")]
        public bool useController = true;
        [Tooltip("핸드트래킹 검지 핀치로 잡기")]
        public bool useHandTracking = true;

        [Header("── Scene Manager 연동 ──────────────")]
        public SceneManager3B manager;

        [Header("── 디버그 ────────────────────────────")]
        public bool drawGizmo = true;

        // ── 내부 상태 ──
        private Rigidbody _rb;
        private Transform _leftAnchor;
        private Transform _rightAnchor;
        private OVRHand _leftHand;
        private OVRHand _rightHand;

        private Transform _grabbedBy;
        private Vector3 _previousHandPosition;
        private Vector3 _handVelocity;
        private Transform _originalParent;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _leftAnchor  = ResolveAnchor(true);
            _rightAnchor = ResolveAnchor(false);
            _originalParent = transform.parent;

            // OVRHand 검색 (핸드트래킹용)
            ResolveOVRHands();

            if (manager == null) manager = FindObjectOfType<SceneManager3B>();

            if (_leftAnchor == null && _rightAnchor == null)
            {
                Debug.LogWarning($"[IceBlockGrabber3B:{name}] 손 앵커를 찾을 수 없음. " +
                                 $"OVRCameraRig가 씬에 있는지 확인.");
            }

            if (useHandTracking && _leftHand == null && _rightHand == null)
            {
                Debug.Log($"[IceBlockGrabber3B:{name}] 핸드트래킹 모드 사용 가능하지만 OVRHand 컴포넌트 없음. " +
                          $"컨트롤러로만 작동. 핸드트래킹 원하면 OVRHandPrefab을 LeftHandAnchor/RightHandAnchor에 추가.");
            }
        }

        void Update()
        {
            // 잡고 있는 동안 손 속도 추적 (던지기용)
            if (_grabbedBy != null)
            {
                _handVelocity = (_grabbedBy.position - _previousHandPosition) / Time.deltaTime;
                _previousHandPosition = _grabbedBy.position;

                if (!IsGrabPressed(_grabbedBy == _leftAnchor))
                {
                    Release();
                }
                return;
            }

            // 안 잡고 있을 때: 양손 중 그랩 입력 + 가까이 있으면 잡기
            if (allowRightHand && _rightAnchor != null
                && IsGrabPressed(false)
                && IsHandClose(_rightAnchor))
            {
                Grab(_rightAnchor);
                return;
            }
            if (allowLeftHand && _leftAnchor != null
                && IsGrabPressed(true)
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

            _rb.isKinematic = true;
            _rb.useGravity = false;
            transform.SetParent(handAnchor);

            if (manager != null) manager.OnIceGrabbed();
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

        // ── 그랩 입력 통합 (컨트롤러 OR 핸드트래킹) ──
        bool IsGrabPressed(bool isLeft)
        {
            // 1) 컨트롤러: HandTrigger 버튼 (중지)
            if (useController)
            {
                var btn = isLeft ? OVRInput.RawButton.LHandTrigger
                                 : OVRInput.RawButton.RHandTrigger;
                if (OVRInput.Get(btn)) return true;
            }

            // 2) 핸드트래킹: 검지 핀치 (엄지+검지 모음)
            if (useHandTracking)
            {
                OVRHand h = isLeft ? _leftHand : _rightHand;
                if (h != null && h.IsTracked && h.HandConfidence == OVRHand.TrackingConfidence.High)
                {
                    if (h.GetFingerIsPinching(OVRHand.HandFinger.Index))
                        return true;
                }
            }

            return false;
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

        // ── OVRHand 자동 검색 (핸드트래킹용) ──────────
        void ResolveOVRHands()
        {
            OVRHand[] all = FindObjectsOfType<OVRHand>();
            foreach (var h in all)
            {
                // 1) 부모 이름으로 좌/우 판별
                Transform t = h.transform;
                while (t != null)
                {
                    string n = t.name.ToLower();
                    if (n.Contains("left"))  { if (_leftHand == null)  _leftHand = h;  break; }
                    if (n.Contains("right")) { if (_rightHand == null) _rightHand = h; break; }
                    t = t.parent;
                }

                // 2) OVRSkeleton 타입으로 판별 (더 정확)
                var skel = h.GetComponent<OVRSkeleton>();
                if (skel != null)
                {
                    if (skel.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft && _leftHand == null)
                        _leftHand = h;
                    else if (skel.GetSkeletonType() == OVRSkeleton.SkeletonType.HandRight && _rightHand == null)
                        _rightHand = h;
                }
            }
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
