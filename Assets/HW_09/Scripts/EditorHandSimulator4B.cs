#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Editor-only hand interaction simulator.
/// Attach to Main Camera. Press F to interact with objects in the crosshair.
///   - Object tagged "HandshakeTrigger"  → fires HandshakeDetector.SimulateTrigger()
///   - Object named   "BusinessCard"     → fires BusinessCardInteractable.OnGrab()
/// </summary>
[RequireComponent(typeof(Camera))]
public class EditorHandSimulator4B : MonoBehaviour
{
    [Tooltip("Max raycast distance for interaction.")]
    public float interactRange = 5f;

    private Camera _cam;
    private GUIStyle _dotStyle;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (!keyboard.fKey.wasPressedThisFrame) return;

        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange)) return;

        GameObject go = hit.collider.gameObject;

        // HandshakeTrigger
        if (go.CompareTag("HandshakeTrigger"))
        {
            HandshakeDetector4B detector = go.GetComponent<HandshakeDetector4B>();
            if (detector == null)
                detector = go.GetComponentInParent<HandshakeDetector4B>();

            if (detector != null)
                detector.SimulateTrigger();
            else
                Debug.LogWarning("[EditorHandSimulator] No HandshakeDetector found on/above " + go.name);

            return;
        }

        // BusinessCard
        if (go.name == "BusinessCard")
        {
            ISampleInteractable card = go.GetComponent<ISampleInteractable>();
            if (card != null)
                card.OnGrab();
            else
                Debug.LogWarning("[EditorHandSimulator] No ISampleInteractable on BusinessCard.");
        }
    }

    private void OnGUI()
    {
        if (_dotStyle == null)
        {
            _dotStyle = new GUIStyle();
            _dotStyle.normal.textColor = Color.white;
            _dotStyle.fontSize = 18;
            _dotStyle.alignment = TextAnchor.MiddleCenter;
        }

        float cx   = Screen.width  * 0.5f;
        float cy   = Screen.height * 0.5f;
        float size = 12f;

        // Outline
        GUI.color = Color.black;
        GUI.Label(new Rect(cx - size * 0.5f + 1, cy - size * 0.5f + 1, size, size), "•", _dotStyle);
        // Dot
        GUI.color = Color.white;
        GUI.Label(new Rect(cx - size * 0.5f, cy - size * 0.5f, size, size), "•", _dotStyle);

        GUI.color = Color.white;
    }
}
#endif
