using System.Collections;
using UnityEngine;

namespace HW09.Heejin
{
    public class OutroManager : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip audio1;
        public AudioClip audio2;
        public AudioClip audio3;
        public AudioClip audio4;
        public float audio1MaxDuration = 9.7f;

        [Header("Button")]
        public GameObject buttonObject;
        public Vector3 textOffset = new Vector3(0f, 0.35f, 0f);
        public Vector3 finalTextOffset = new Vector3(0f, 1.5f, 0f);
        public float buttonDownDistance = 0.4f;
        public float touchRadius = 0.35f;
        public float rayDistance = 8f;
        public float sphereRadius = 0.06f;

        [Header("Ending")]
        public float quitDelay = 7f;

        private readonly Color _softRed = new Color(1f, 0.18f, 0.12f, 1f);
        private readonly Color _strongRed = new Color(1f, 0.02f, 0.01f, 1f);

        private Outline _outline;
        private Light _buttonGlow;
        private TextMesh _buttonText;
        private Transform _leftAnchor;
        private Transform _rightAnchor;
        private Transform _centerEye;
        private Vector3 _buttonStartPosition;
        private Vector3 _textWorldPosition;
        private bool _buttonReady;
        private bool _pressed;

        void Start()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            if (buttonObject == null) buttonObject = GameObject.Find("Button");
            if (buttonObject != null)
            {
                _buttonStartPosition = buttonObject.transform.position;
                SetupButtonCollider();
                SetupOutline();
                SetupButtonText();
                SetButtonVisual(false, false);
            }

            _leftAnchor = ResolveAnchor(true);
            _rightAnchor = ResolveAnchor(false);
            _centerEye = ResolveCenterEye();

            StartCoroutine(OutroFlow());
        }

        void Update()
        {
            if (!_buttonReady || _pressed || buttonObject == null) return;

            if (IsHandTouching(_leftAnchor) || IsHandTouching(_rightAnchor) ||
                IsRayClicking(_leftAnchor, true) || IsRayClicking(_rightAnchor, false))
            {
                PressButton();
            }
        }

        void LateUpdate()
        {
            if (_buttonText == null || _centerEye == null) return;

            Vector3 direction = _buttonText.transform.position - _centerEye.position;
            if (direction.sqrMagnitude > 0.001f)
                _buttonText.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_buttonReady || _pressed) return;
            if (IsControllerLike(other)) PressButton();
        }

        IEnumerator OutroFlow()
        {
            _buttonReady = false;
            SetButtonVisual(false, false);

            yield return PlayClip(audio1, audio1MaxDuration);
            yield return PlayClip(audio2);
            yield return PlayClip(audio3);
            yield return PlayClip(audio4);

            _buttonReady = true;
            SetButtonVisual(true, false);
            SetButtonText("STOP");
        }

        IEnumerator PlayClip(AudioClip clip, float maxDuration = -1f)
        {
            if (clip == null) yield break;

            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            float duration = maxDuration > 0f ? Mathf.Min(clip.length, maxDuration) : clip.length;
            yield return new WaitForSeconds(duration);
            audioSource.Stop();
        }

        void PressButton()
        {
            _pressed = true;
            SetButtonVisual(true, true);
            buttonObject.transform.position = _buttonStartPosition + Vector3.down * buttonDownDistance;
            SetFloatingTextPosition(finalTextOffset);
            SetButtonText("The body ultimately did not acknowledge this world as reality.", true);
            StartCoroutine(QuitFlow());
        }

        IEnumerator QuitFlow()
        {
            yield return new WaitForSeconds(quitDelay);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void SetupButtonCollider()
        {
            Collider col = buttonObject.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        void SetupOutline()
        {
            _outline = buttonObject.GetComponent<Outline>();
            if (_outline == null) _outline = buttonObject.AddComponent<Outline>();

            GameObject glow = new GameObject("Button_RedGlow");
            glow.transform.SetParent(buttonObject.transform);
            glow.transform.localPosition = Vector3.up * 0.25f;
            _buttonGlow = glow.AddComponent<Light>();
            _buttonGlow.type = LightType.Point;
            _buttonGlow.color = _softRed;
            _buttonGlow.range = 1.4f;
            _buttonGlow.intensity = 0f;
        }

        void SetupButtonText()
        {
            GameObject textObj = new GameObject("Outro_ButtonText");
            textObj.transform.SetParent(null);
            textObj.transform.localScale = Vector3.one * 0.08f;

            _buttonText = textObj.AddComponent<TextMesh>();
            _buttonText.anchor = TextAnchor.MiddleCenter;
            _buttonText.alignment = TextAlignment.Center;
            _buttonText.fontSize = 48;
            _buttonText.color = Color.white;
            _buttonText.text = "";
            SetFloatingTextPosition(textOffset);
        }

        void SetButtonVisual(bool visible, bool strong)
        {
            if (_outline != null)
            {
                _outline.enabled = visible;
                _outline.OutlineMode = Outline.Mode.OutlineAll;
                _outline.OutlineColor = strong ? _strongRed : _softRed;
                _outline.OutlineWidth = strong ? 8f : 3f;
            }

            if (_buttonGlow != null)
            {
                _buttonGlow.enabled = visible;
                _buttonGlow.color = strong ? _strongRed : _softRed;
                _buttonGlow.intensity = visible ? (strong ? 5f : 1.4f) : 0f;
                _buttonGlow.range = strong ? 2.4f : 1.4f;
            }

            if (_buttonText != null && !visible) _buttonText.text = "";
        }

        void SetButtonText(string text, bool isFinalText = false)
        {
            if (_buttonText == null) return;
            _buttonText.text = text;
            _buttonText.characterSize = isFinalText ? 0.24f : 0.35f;
            _buttonText.transform.position = _textWorldPosition;
            _buttonText.GetComponent<Renderer>().material.color = Color.white;
        }

        void SetFloatingTextPosition(Vector3 offset)
        {
            if (buttonObject == null || _buttonText == null) return;
            _textWorldPosition = buttonObject.transform.TransformPoint(offset);
            _buttonText.transform.position = _textWorldPosition;
        }

        bool IsHandTouching(Transform hand)
        {
            return hand != null && Vector3.Distance(hand.position, buttonObject.transform.position) <= touchRadius;
        }

        bool IsRayClicking(Transform anchor, bool isLeft)
        {
            if (anchor == null) return false;

            OVRInput.RawButton button = isLeft ? OVRInput.RawButton.LIndexTrigger : OVRInput.RawButton.RIndexTrigger;
            if (!OVRInput.Get(button)) return false;

            Ray ray = new Ray(anchor.position, anchor.forward);
            if (!Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, rayDistance, ~0, QueryTriggerInteraction.Collide))
                return false;

            Transform t = hit.collider.transform;
            while (t != null)
            {
                if (t.gameObject == buttonObject) return true;
                t = t.parent;
            }

            return false;
        }

        bool IsControllerLike(Collider other)
        {
            if (other == null) return false;
            if (other.CompareTag("Player")) return true;

            string n = other.name.ToLower();
            return n.Contains("hand") || n.Contains("controller") || n.Contains("anchor");
        }

        Transform ResolveAnchor(bool isLeft)
        {
            string[] names = isLeft
                ? new[] { "LeftControllerAnchor", "LeftControllerInHandAnchor", "LeftHandAnchor" }
                : new[] { "RightControllerAnchor", "RightControllerInHandAnchor", "RightHandAnchor" };

            foreach (string n in names)
            {
                GameObject go = GameObject.Find(n);
                if (go != null) return go.transform;
            }

            return null;
        }

        Transform ResolveCenterEye()
        {
            foreach (string n in new[] { "CenterEyeAnchor", "Main Camera" })
            {
                GameObject go = GameObject.Find(n);
                if (go != null) return go.transform;
            }

            return Camera.main != null ? Camera.main.transform : null;
        }
    }
}
