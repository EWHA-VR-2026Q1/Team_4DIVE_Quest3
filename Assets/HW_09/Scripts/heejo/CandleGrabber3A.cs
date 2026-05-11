using UnityEngine;

namespace HW09.heejo
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class CandleGrabber3A : MonoBehaviour
    {
        [Header("Grab Settings")]
        public float grabRadius = 0.4f;
        public float throwVelocityScale = 1.5f;

        [Header("Hand Selection")]
        public bool allowLeftHand = true;
        public bool allowRightHand = true;

        [Header("Input Mode")]
        public bool useController = true;
        public bool useHandTracking = true;
        public bool enableLaserGrab = true;
        public float rayDistance = 8f;
        public float sphereRadius = 0.08f;

        [Header("Scene Manager")]
        public SceneManager3A manager;

        [Header("Debug")]
        public bool drawGizmo = true;

        private Rigidbody _rb;
        private Transform _leftAnchor;
        private Transform _rightAnchor;
        private OVRHand _leftHand;
        private OVRHand _rightHand;

        private Transform _grabbedBy;
        private bool _grabbedWithLaser;
        private bool _grabbedByLeftHand;
        private Vector3 _previousHandPosition;
        private Vector3 _handVelocity;
        private Transform _originalParent;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _leftAnchor = ResolveAnchor(true);
            _rightAnchor = ResolveAnchor(false);
            _originalParent = transform.parent;

            ResolveOVRHands();

            if (manager == null) manager = FindObjectOfType<SceneManager3A>();

            if (_leftAnchor == null && _rightAnchor == null)
                Debug.LogWarning($"[CandleGrabber3A:{name}] Hand anchors were not found.");

            if (useHandTracking && _leftHand == null && _rightHand == null)
                Debug.Log($"[CandleGrabber3A:{name}] OVRHand components were not found. Controller grab still works.");
        }

        void Update()
        {
            if (_grabbedBy != null)
            {
                _handVelocity = (_grabbedBy.position - _previousHandPosition) / Time.deltaTime;
                _previousHandPosition = _grabbedBy.position;

                bool stillPressed = _grabbedWithLaser
                    ? IsLaserPressed(_grabbedByLeftHand)
                    : IsGrabPressed(_grabbedByLeftHand);

                if (!stillPressed)
                    Release();

                return;
            }

            if (manager != null && manager.currentStep != SceneManager3A.Step.WaitingForGrab)
                return;

            if (allowRightHand && _rightAnchor != null
                && IsGrabPressed(false)
                && IsHandClose(_rightAnchor))
            {
                Grab(_rightAnchor, false, false);
                return;
            }

            if (allowLeftHand && _leftAnchor != null
                && IsGrabPressed(true)
                && IsHandClose(_leftAnchor))
            {
                Grab(_leftAnchor, true, false);
                return;
            }

            if (!enableLaserGrab) return;

            if (allowRightHand && _rightAnchor != null
                && IsLaserPressed(false)
                && RayHitsThisObject(_rightAnchor))
            {
                Grab(_rightAnchor, false, true);
                return;
            }

            if (allowLeftHand && _leftAnchor != null
                && IsLaserPressed(true)
                && RayHitsThisObject(_leftAnchor))
            {
                Grab(_leftAnchor, true, true);
            }
        }

        void Grab(Transform handAnchor, bool isLeftHand, bool grabbedWithLaser)
        {
            _grabbedBy = handAnchor;
            _grabbedByLeftHand = isLeftHand;
            _grabbedWithLaser = grabbedWithLaser;
            _previousHandPosition = handAnchor.position;
            _handVelocity = Vector3.zero;

            _rb.isKinematic = true;
            _rb.useGravity = false;
            transform.SetParent(handAnchor);

            if (manager != null) manager.OnCandleGrabbed();
        }

        void Release()
        {
            transform.SetParent(_originalParent);
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = _handVelocity * throwVelocityScale;
            _grabbedBy = null;
            _grabbedWithLaser = false;
        }

        bool IsHandClose(Transform hand)
        {
            return Vector3.Distance(hand.position, transform.position) <= grabRadius;
        }

        bool IsGrabPressed(bool isLeft)
        {
            if (useController)
            {
                var btn = isLeft ? OVRInput.RawButton.LHandTrigger
                                 : OVRInput.RawButton.RHandTrigger;
                if (OVRInput.Get(btn)) return true;
            }

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

        bool IsLaserPressed(bool isLeft)
        {
            if (useController)
            {
                var btn = isLeft ? OVRInput.RawButton.LIndexTrigger
                                 : OVRInput.RawButton.RIndexTrigger;
                if (OVRInput.Get(btn)) return true;
            }

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

        bool RayHitsThisObject(Transform anchor)
        {
            Ray ray = new Ray(anchor.position, anchor.forward);
            if (!Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, rayDistance, ~0, QueryTriggerInteraction.Collide))
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
                    if (n.Contains("left")) { if (_leftHand == null) _leftHand = h; break; }
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

        void OnDrawGizmosSelected()
        {
            if (!drawGizmo) return;
            Gizmos.color = new Color(0.5f, 0.9f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, grabRadius);
        }
    }
}
