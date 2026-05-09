using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09.heejo
{
    public class SceneManager3A : MonoBehaviour
    {
        [Header("── 오브젝트 참조 ──────────────────────")]
        public GameObject door;
        public GameObject candle;
        public GameObject candle1;

        [Header("── 조명 ────────────────────────────────")]
        public Light candleLight;
        public Light candleLight1;
        public float normalIntensity = 1f;
        public float brightIntensity = 3f;

        [Header("── 하이라이트 색상 (Emission) ──────────")]
        public Color highlightColor = new Color(1f, 0.85f, 0.2f);
        [Range(0.5f, 5f)] public float highlightIntensity = 1.5f;

        [Header("── 오디오 ──────────────────────────────")]
        public AudioSource audioSource;
        public AudioClip clip1;
        public AudioClip clip2;
        public AudioClip clip3;
        public AudioClip clip4;
        public AudioClip clip5;

        [Header("── Player_OVRInput_Parts 참조 ─────────")]
        public Transform ovrPlayerRoot;
        public Transform ovrCenterEye;
        public Transform ovrLeftHand;
        public Transform ovrRightHand;

        [Header("── 거리 설정 ──────────────────────────")]
        public float candleProximityDistance = 2.5f;
        public float doorProximityDistance = 2.5f;
        public float handNearDistance = 0.6f;

        [Header("── 씬 전환 ─────────────────────────────")]
        public string nextSceneName = "MainScene";
        public float sceneLoadDelay = 0.5f;

        [Header("── 디버그 ─────────────────────────────")]
        public bool showDebugGUI = true;
        public bool verboseLog = true;

        [Header("── 상태 (읽기 전용) ────────────────────")]
        public Step currentStep = Step.None;

        public enum Step
        {
            None,
            IntroPlaying,
            WaitingForCandleNear,
            CandleNearSequence,
            WaitingForGrab,
            GrabSuccessSequence,
            WaitingForDoor,
            Done
        }

        private bool _candleNearTriggered = false;
        private bool _candleGrabbed = false;
        private bool _doorTriggered = false;
        private Coroutine _activeFlow;

        void Start()
        {
            AutoBindOVRReferences();
            SetupAudio();
            ValidateReferences();

            EnsureCollider(candle);
            EnsureCollider(candle1);
            EnsureCollider(door);

            EnsureRigidbody(candle);
            EnsureRigidbody(candle1);

            SetHighlight(candle, false);
            SetHighlight(candle1, false);
            SetHighlight(door, false);
            SetLightIntensity(normalIntensity);

            LogRefs();
            RunFlow(IntroFlow());
        }

        void Update()
        {
            Transform playerPos = GetPlayerPosition();

            if (currentStep == Step.WaitingForCandleNear && !_candleNearTriggered)
            {
                bool playerNear =
                    playerPos != null &&
                    (IsNear(playerPos.position, candle, candleProximityDistance) ||
                     IsNear(playerPos.position, candle1, candleProximityDistance));

                bool handNear = IsHandNearCandle();

                if (playerNear || handNear)
                    OnCandleProximityEntered();
            }

            if (currentStep == Step.WaitingForGrab)
            {
                if (Input.GetMouseButtonDown(0))
                    TryClickCandle();
            }

            if (currentStep == Step.WaitingForDoor && !_doorTriggered && playerPos != null)
            {
                if (IsNear(playerPos.position, door, doorProximityDistance))
                    OnPlayerEnterDoor();
            }

            HandleDebugKeys();
        }

        public void OnCandleProximityEntered()
        {
            if (currentStep != Step.WaitingForCandleNear) return;
            if (_candleNearTriggered) return;
            _candleNearTriggered = true;
            Log("캔들 근접 감지");
            RunFlow(CandleNearFlow());
        }

        public void OnCandleGrabbed()
        {
            if (currentStep != Step.WaitingForGrab) return;
            if (_candleGrabbed) return;
            _candleGrabbed = true;
            Log("캔들 집기 성공");
            RunFlow(GrabSuccessFlow());
        }

        public void OnPlayerEnterDoor()
        {
            if (currentStep != Step.WaitingForDoor) return;
            if (_doorTriggered) return;
            _doorTriggered = true;
            Log("문 도달 → 씬 로드");
            RunFlow(LoadNextSceneFlow());
        }

        IEnumerator IntroFlow()
        {
            currentStep = Step.IntroPlaying;
            yield return PlayAndWait(clip1, 3f);
            SetHighlight(candle, true);
            SetHighlight(candle1, true);
            currentStep = Step.WaitingForCandleNear;
        }

        IEnumerator CandleNearFlow()
        {
            currentStep = Step.CandleNearSequence;
            SetLightIntensity(brightIntensity);
            SetHighlight(candle, false);
            SetHighlight(candle1, false);
            yield return PlayAndWait(clip2, 3f);
            SetHighlight(candle, true);
            SetHighlight(candle1, true);
            yield return PlayAndWait(clip3, 3f);
            currentStep = Step.WaitingForGrab;
        }

        IEnumerator GrabSuccessFlow()
        {
            currentStep = Step.GrabSuccessSequence;
            SetHighlight(candle, false);
            SetHighlight(candle1, false);
            yield return PlayAndWait(clip4, 2f);
            yield return new WaitForSeconds(2f);
            SetHighlight(door, true);
            yield return PlayAndWait(clip5, 3f);
            currentStep = Step.WaitingForDoor;
        }

        IEnumerator LoadNextSceneFlow()
        {
            currentStep = Step.Done;
            yield return new WaitForSeconds(sceneLoadDelay);
            SceneManager.LoadScene(nextSceneName);
        }

        void RunFlow(IEnumerator flow)
        {
            if (_activeFlow != null) StopCoroutine(_activeFlow);
            _activeFlow = StartCoroutine(flow);
        }

        IEnumerator PlayAndWait(AudioClip clip, float fallback)
        {
            if (audioSource == null || clip == null)
            {
                yield return new WaitForSeconds(fallback);
                yield break;
            }
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length > 0.05f ? clip.length : fallback);
        }

        void TryClickCandle()
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 25f)) return;
            GameObject clicked = hit.collider.gameObject;
            if (clicked == candle || clicked == candle1 ||
                IsChildOf(clicked, candle) || IsChildOf(clicked, candle1))
                OnCandleGrabbed();
        }

        Transform GetPlayerPosition()
        {
            if (ovrCenterEye != null) return ovrCenterEye;
            if (ovrPlayerRoot != null) return ovrPlayerRoot;
            if (Camera.main != null) return Camera.main.transform;
            return null;
        }

        bool IsHandNearCandle()
        {
            var hands = GetHandPositions();
            foreach (Vector3 handPos in hands)
            {
                if ((candle  != null && Vector3.Distance(handPos, candle.transform.position)  <= handNearDistance) ||
                    (candle1 != null && Vector3.Distance(handPos, candle1.transform.position) <= handNearDistance))
                    return true;
            }
            return false;
        }

        List<Vector3> GetHandPositions()
        {
            var list = new List<Vector3>();
            if (ovrLeftHand  != null) list.Add(ovrLeftHand.position);
            if (ovrRightHand != null) list.Add(ovrRightHand.position);
            return list;
        }

        bool IsNear(Vector3 pos, GameObject target, float dist)
            => target != null && Vector3.Distance(pos, target.transform.position) <= dist;

        bool IsChildOf(GameObject obj, GameObject parent)
        {
            if (obj == null || parent == null) return false;
            Transform t = obj.transform;
            while (t != null) { if (t.gameObject == parent) return true; t = t.parent; }
            return false;
        }

        void SetHighlight(GameObject obj, bool active)
        {
            if (obj == null) return;
            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material mat in r.materials)
                {
                    if (mat == null) continue;
                    if (active) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor", highlightColor * highlightIntensity); }
                    else        { mat.SetColor("_EmissionColor", Color.black); mat.DisableKeyword("_EMISSION"); }
                }
            }
        }

        void SetLightIntensity(float intensity)
        {
            if (candleLight  != null) candleLight.intensity  = intensity;
            if (candleLight1 != null) candleLight1.intensity = intensity;
        }

        void EnsureCollider(GameObject obj)
        {
            if (obj == null) return;
            if (obj.GetComponentInChildren<Collider>() != null) return;
            MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();
            foreach (MeshFilter mf in filters)
            {
                if (mf.sharedMesh != null)
                {
                    var mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                    return;
                }
            }
            obj.AddComponent<SphereCollider>().radius = 0.2f;
        }

        void EnsureRigidbody(GameObject obj)
        {
            if (obj == null || obj.GetComponent<Rigidbody>() != null) return;
            var rb = obj.AddComponent<Rigidbody>();
            rb.useGravity = false; rb.isKinematic = false;
            rb.drag = 5f; rb.angularDrag = 5f;
        }

        void SetupAudio()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;
        }

        void AutoBindOVRReferences()
        {
            if (ovrPlayerRoot == null)
            {
                GameObject rootObj = GameObject.Find("Player_OVRInput_Parts");
                if (rootObj != null) ovrPlayerRoot = rootObj.transform;
            }
            if (ovrPlayerRoot == null) return;

            ovrCenterEye  = ovrCenterEye  ?? FindChildDeep(ovrPlayerRoot, "CenterEyeAnchor") ?? FindChildDeep(ovrPlayerRoot, "Main Camera");
            ovrLeftHand   = ovrLeftHand   ?? FindChildDeep(ovrPlayerRoot, "LeftControllerAnchor")  ?? FindChildDeep(ovrPlayerRoot, "LeftHandAnchor");
            ovrRightHand  = ovrRightHand  ?? FindChildDeep(ovrPlayerRoot, "RightControllerAnchor") ?? FindChildDeep(ovrPlayerRoot, "RightHandAnchor");
        }

        Transform FindChildDeep(Transform parent, string targetName)
        {
            if (parent == null) return null;
            foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
                if (child.name == targetName) return child;
            return null;
        }

        void ValidateReferences()
        {
            if (door   == null) Debug.LogWarning("[Scene3A] door 미연결");
            if (candle == null) Debug.LogWarning("[Scene3A] candle 미연결");
        }

        void LogRefs()
        {
            if (verboseLog)
                Debug.Log($"[Scene3A] Root={ovrPlayerRoot?.name ?? "null"}, Eye={ovrCenterEye?.name ?? "null"}");
        }

        void HandleDebugKeys()
        {
            if (Input.GetKeyDown(KeyCode.F1)) { StopAllCoroutines(); _candleNearTriggered = false; _candleGrabbed = false; _doorTriggered = false; RunFlow(IntroFlow()); }
            else if (Input.GetKeyDown(KeyCode.F2)) { _candleNearTriggered = false; currentStep = Step.WaitingForCandleNear; OnCandleProximityEntered(); }
            else if (Input.GetKeyDown(KeyCode.F3)) { currentStep = Step.WaitingForGrab; OnCandleGrabbed(); }
            else if (Input.GetKeyDown(KeyCode.F4)) { currentStep = Step.WaitingForDoor; OnPlayerEnterDoor(); }
        }

        void Log(string msg) { if (verboseLog) Debug.Log($"[Scene3A] {msg}"); }

        void OnGUI()
        {
            if (!showDebugGUI) return;
            GUIStyle style = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.MiddleLeft };
            style.normal.textColor = Color.white;
            string text = currentStep switch
            {
                Step.IntroPlaying         => "▶ 1단계: 음성 재생 중",
                Step.WaitingForCandleNear => "▶ 2단계: 캔들 가까이 다가가세요",
                Step.WaitingForGrab       => "▶ 4단계: 캔들을 집으세요",
                Step.WaitingForDoor       => "▶ 6단계: 문 앞으로 이동하세요",
                Step.Done                 => "✓ 완료! Scene3_B 로딩 중",
                _                         => $"▶ {currentStep}"
            };
            GUI.Box(new Rect(10, 10, 460, 36), text, style);
            GUI.Box(new Rect(10, 50, 460, 28), "F1=재시작  F2=접근  F3=집기  F4=문", style);
        }
    }
}
