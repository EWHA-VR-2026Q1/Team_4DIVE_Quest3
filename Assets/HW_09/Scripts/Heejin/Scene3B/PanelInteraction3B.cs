using UnityEngine;

namespace HW09.Heejin
{
    /// <summary>
    /// Panel과의 상호작용:
    ///  A) 손/컨트롤러가 직접 닿으면 트리거 (콜라이더의 isTrigger=ON)
    ///  B) 컨트롤러 레이저 + 검지 트리거 (RIndexTrigger / LIndexTrigger)
    ///  C) 핸드트래킹 레이저 + 검지 핀치
    /// 셋 중 하나로 활성화. WaitingPanel 단계에서만 작동.
    /// </summary>
    public class PanelInteraction3B : MonoBehaviour
    {
        [Header("── 연결 ──")]
        public SceneManager3B manager;

        [Header("── 레이캐스트 설정 ──")]
        public float rayDistance = 8f;
        public float sphereRadius = 0.05f;

        [Header("── 옵션 ──")]
        public bool enableTouch = true;
        public bool enableLaserClick = true;
        public bool useController = true;
        public bool useHandTracking = true;

        private Transform _leftAnchor;
        private Transform _rightAnchor;
        private OVRHand _leftHand;
        private OVRHand _rightHand;
        private bool _fired = false;

        void Start()
        {
            if (manager == null) manager = FindObjectOfType<SceneManager3B>();
            _leftAnchor = ResolveAnchor(true);
            _rightAnchor = ResolveAnchor(false);
            ResolveOVRHands();

            if (enableTouch)
            {
                var col = GetComponent<Collider>();
                if (col != null) col.isTrigger = true;
            }
        }

        void Update()
        {
            if (_fired) return;
            if (manager == null) return;
            if (manager.currentStep != SceneManager3B.Step.WaitingPanel) return;
            if (!enableLaserClick) return;

            // 오른손
            if (_rightAnchor != null && IsClickPressed(false) && RayHitsThis(_rightAnchor))
            {
                Fire();
                return;
            }
            // 왼손
            if (_leftAnchor != null && IsClickPressed(true) && RayHitsThis(_leftAnchor))
            {
                Fire();
                return;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (_fired) return;
            if (manager == null) return;
            if (manager.currentStep != SceneManager3B.Step.WaitingPanel) return;
            if (!enableTouch) return;

            string n = other.name.ToLower();
            if (n.Contains("hand") || n.Contains("controller") || n.Contains("anchor")
                || other.CompareTag("Player"))
            {
                Fire();
            }
        }

        void Fire()
        {
            _fired = true;
            manager.OnPanelInteracted();
        }

        bool IsClickPressed(bool isLeft)
        {
            // 컨트롤러 IndexTrigger
            if (useController)
            {
                var btn = isLeft ? OVRInput.RawButton.LIndexTrigger
                                 : OVRInput.RawButton.RIndexTrigger;
                if (OVRInput.Get(btn)) return true;
            }

            // 핸드트래킹: 검지 핀치
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

        bool RayHitsThis(Transform anchor)
        {
            Ray ray = new Ray(anchor.position, anchor.forward);
            if (!Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, rayDistance))
                return false;

            Transform t = hit.collider.transform;
            while (t != null)
            {
                if (t == transform) return true;
                t = t.parent;
            }
            return false;
        }

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

        void ResolveOVRHands()
        {
            OVRHand[] all = FindObjectsOfType<OVRHand>();
            foreach (var h in all)
            {
                Transform t = h.transform;
                while (t != null)
                {
                    string n = t.name.ToLower();
                    if (n.Contains("left"))  { if (_leftHand == null)  _leftHand = h;  break; }
                    if (n.Contains("right")) { if (_rightHand == null) _rightHand = h; break; }
                    t = t.parent;
                }
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
    }
}
