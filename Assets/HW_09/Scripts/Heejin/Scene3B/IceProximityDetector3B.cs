using UnityEngine;

namespace HW09.Heejin
{
    /// <summary>
    /// IceBlock 근처에 손/컨트롤러가 들어왔는지 감지해서 Manager에 알림.
    /// 거리 기반(매 프레임 Vector3.Distance)으로 콜라이더 의존 없이 작동.
    /// IceBlock 오브젝트에 부착.
    /// </summary>
    public class IceProximityDetector3B : MonoBehaviour
    {
        [Header("── 연결 ──")]
        public SceneManager3B manager;

        [Header("── 감지 거리 ──")]
        [Tooltip("이 거리 안에 손이 들어오면 트리거 (m)")]
        public float radius = 0.5f;

        [Header("── 디버그 ──")]
        public bool drawGizmo = true;

        private Transform _leftHand;
        private Transform _rightHand;
        private bool _fired = false;

        void Start()
        {
            if (manager == null) manager = FindObjectOfType<SceneManager3B>();
            _leftHand = ResolveAnchor(true);
            _rightHand = ResolveAnchor(false);
        }

        void Update()
        {
            if (_fired) return;
            if (manager == null) return;
            if (manager.currentStep != SceneManager3B.Step.WaitingApproach) return;

            if (_leftHand != null &&
                Vector3.Distance(_leftHand.position, transform.position) <= radius)
            {
                Fire();
                return;
            }
            if (_rightHand != null &&
                Vector3.Distance(_rightHand.position, transform.position) <= radius)
            {
                Fire();
                return;
            }
        }

        void Fire()
        {
            _fired = true;
            manager.OnIceApproached();
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

        void OnDrawGizmosSelected()
        {
            if (!drawGizmo) return;
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
