using UnityEngine;

namespace HW09.heejo
{
    [RequireComponent(typeof(Collider))]
    public class DoorTrigger3A : MonoBehaviour
    {
        [Header("── 연결 필요 ──────────────────────────────")]
        public SceneManager3A manager;

        [Header("── 감지 태그 ──────────────────────────────")]
        public string playerTag  = "Player";
        public string ovrHandTag = "OVRHand";

        void Start()
        {
            if (manager == null)
                manager = FindObjectOfType<SceneManager3A>();

            if (manager == null)
                Debug.LogError("[DoorTrigger3A] SceneManager3A를 찾을 수 없습니다.");

            Collider col = GetComponent<Collider>();
            if (!col.isTrigger) col.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (manager == null) return;
            if (IsPlayer(other))
                manager.OnPlayerEnterDoor();
        }

        bool IsPlayer(Collider other)
        {
            if (other.CompareTag(playerTag))  return true;
            if (other.CompareTag(ovrHandTag)) return true;

            string n = other.gameObject.name;
            if (n.Contains("OVRCameraRig") || n.Contains("CenterEyeAnchor") ||
                n.Contains("Player_OVRInput") || n.Contains("XR Origin")) return true;

            Transform t = other.transform.parent;
            int depth = 0;
            while (t != null && depth < 5)
            {
                if (t.CompareTag(playerTag)) return true;
                if (t.gameObject.name.Contains("Player_OVRInput")) return true;
                t = t.parent; depth++;
            }
            return false;
        }
    }
}
