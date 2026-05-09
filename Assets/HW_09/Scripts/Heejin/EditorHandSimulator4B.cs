#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace HW09.Heejin
{
    [RequireComponent(typeof(Camera))]
    public class EditorHandSimulator4B : MonoBehaviour
    {
        public float interactRange = 5f;

        private Camera _cam;
        private GUIStyle _dotStyle;

        void Awake() { _cam = GetComponent<Camera>(); }

        void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.fKey.wasPressedThisFrame) return;

            Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out RaycastHit hit, interactRange)) return;

            GameObject go = hit.collider.gameObject;

            if (go.CompareTag("HandshakeTrigger"))
            {
                HandshakeDetector4B detector = go.GetComponent<HandshakeDetector4B>()
                                            ?? go.GetComponentInParent<HandshakeDetector4B>();
                if (detector != null) detector.SimulateTrigger();
                return;
            }

            if (go.name == "BusinessCard")
            {
                // ISampleInteractable은 HW09 네임스페이스에 있으므로 사용 가능
                var card = go.GetComponent<HW09.ISampleInteractable>();
                if (card != null) card.OnGrab();
            }
        }

        void OnGUI()
        {
            if (_dotStyle == null)
            {
                _dotStyle = new GUIStyle();
                _dotStyle.normal.textColor = Color.white;
                _dotStyle.fontSize = 18;
                _dotStyle.alignment = TextAnchor.MiddleCenter;
            }
            float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f, size = 12f;
            GUI.color = Color.black;
            GUI.Label(new Rect(cx - size * 0.5f + 1, cy - size * 0.5f + 1, size, size), "•", _dotStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(cx - size * 0.5f, cy - size * 0.5f, size, size), "•", _dotStyle);
        }
    }
}
#endif
