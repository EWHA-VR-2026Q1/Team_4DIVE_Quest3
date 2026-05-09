using UnityEngine;

namespace HW09.heejo
{
    public class CandleGrabTrigger3A : MonoBehaviour
    {
        [Header("── 연결 필요 ──────────────────────────────")]
        public SceneManager3A manager;

        [Header("── 레이 설정 ──────────────────────────────")]
        public float rayDistance  = 8f;
        public float sphereRadius = 0.08f;

        [Header("── 트리거 존 크기 ────────────────────────")]
        public float proximityRadius = 0.5f;

        private Transform _leftAnchor;
        private bool _proximityFired = false;
        private bool _grabFired      = false;

        void Start()
        {
            if (manager == null)
                manager = FindObjectOfType<SceneManager3A>();

            if (manager == null)
            {
                Debug.LogError($"[CandleGrabTrigger3A:{name}] SceneManager3A 없음.");
                enabled = false;
                return;
            }

            _leftAnchor = ResolveLeftAnchor();
            CreateProximityZone();
        }

        void Update()
        {
            if (_grabFired) return;
            if (manager == null) return;
            if (manager.currentStep != SceneManager3A.Step.WaitingForGrab) return;

            bool pressed = OVRInput.Get(OVRInput.RawButton.LIndexTrigger)
                        || OVRInput.Get(OVRInput.RawButton.LHandTrigger);
            if (!pressed) return;

            if (_leftAnchor != null && RayHitsThisObject(_leftAnchor))
                FireGrab("왼손 트리거 + 레이 hit");
        }

        bool RayHitsThisObject(Transform anchor)
        {
            Ray ray = new Ray(anchor.position, anchor.forward);
            if (!Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, rayDistance, ~0, QueryTriggerInteraction.Collide))
                return false;
            Transform t = hit.collider.transform;
            while (t != null) { if (t == transform) return true; t = t.parent; }
            return false;
        }

        Transform ResolveLeftAnchor()
        {
            if (manager.ovrLeftHand != null)
            {
                string n = manager.ovrLeftHand.name;
                if (n.Contains("ControllerAnchor") || n.Contains("ControllerInHand"))
                    return manager.ovrLeftHand;
            }
            foreach (string n in new[] { "LeftControllerAnchor", "LeftControllerInHandAnchor", "LeftHandAnchor" })
            {
                var go = GameObject.Find(n);
                if (go != null) return go.transform;
            }
            return manager.ovrLeftHand;
        }

        public void TriggerProximity() => FireProximity();
        public void TriggerGrab()      => FireGrab("외부 호출");

        public void OnProximityEnter(Collider other)
        {
            string n = other.name.ToLower();
            if (n.Contains("hand") || n.Contains("controller") || n.Contains("anchor"))
                FireProximity();
        }

        void FireProximity()
        {
            if (_proximityFired) return;
            _proximityFired = true;
            manager.OnCandleProximityEntered();
        }

        void FireGrab(string reason)
        {
            if (_grabFired) return;
            if (manager.currentStep != SceneManager3A.Step.WaitingForGrab) return;
            _grabFired = true;
            manager.OnCandleGrabbed();
        }

        void OnTriggerEnter(Collider other) => OnProximityEnter(other);

        void CreateProximityZone()
        {
            if (transform.Find("ProximityZone") != null) return;

            GameObject zone = new GameObject("ProximityZone");
            zone.transform.SetParent(transform);
            zone.transform.localPosition = Vector3.zero;
            zone.transform.localRotation = Quaternion.identity;
            zone.transform.localScale    = Vector3.one;

            var sc      = zone.AddComponent<SphereCollider>();
            sc.radius    = proximityRadius;
            sc.isTrigger = true;

            var relay   = zone.AddComponent<ProximityZoneRelay>();
            relay.owner = this;
        }
    }

    public class ProximityZoneRelay : MonoBehaviour
    {
        public CandleGrabTrigger3A owner;
        void OnTriggerEnter(Collider other)
        {
            if (owner != null) owner.OnProximityEnter(other);
        }
    }
}
