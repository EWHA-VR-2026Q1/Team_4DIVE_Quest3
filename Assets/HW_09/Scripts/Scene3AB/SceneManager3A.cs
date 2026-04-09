using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public AudioClip clip1;   // Scene3_A_1
    public AudioClip clip2;   // Scene3_A_2
    public AudioClip clip3;   // Scene3_A_3
    public AudioClip clip4;   // Scene3_A_4
    public AudioClip clip5;   // Scene3_A_5

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
        Log("문 도달 → Scene3_B 로드 시작");
        RunFlow(LoadNextSceneFlow());
    }

    IEnumerator IntroFlow()
    {
        currentStep = Step.IntroPlaying;

        yield return PlayAndWait(clip1, 3f);

        SetHighlight(candle, true);
        SetHighlight(candle1, true);

        currentStep = Step.WaitingForCandleNear;
        Log("clip1 종료 → candle, candle1 Outline ON → 접근 대기");
    }

    IEnumerator CandleNearFlow()
    {
        currentStep = Step.CandleNearSequence;

        // 접근 트리거 → 빛 밝아짐 + Outline 잠시 OFF → clip2 재생
        SetLightIntensity(brightIntensity);
        SetHighlight(candle, false);
        SetHighlight(candle1, false);

        yield return PlayAndWait(clip2, 3f);

        // clip2 끝 → Outline 다시 ON → clip3 재생
        SetHighlight(candle, true);
        SetHighlight(candle1, true);

        yield return PlayAndWait(clip3, 3f);

        currentStep = Step.WaitingForGrab;
        Log("clip3 종료 → 캔들 집기 대기");
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
        Log("clip5 종료 → door Outline ON → 문 접근 대기");
    }

    IEnumerator LoadNextSceneFlow()
    {
        currentStep = Step.Done;
        yield return new WaitForSeconds(sceneLoadDelay);
        SceneManager.LoadScene(nextSceneName);
    }

    void RunFlow(IEnumerator flow)
    {
        if (_activeFlow != null)
            StopCoroutine(_activeFlow);

        _activeFlow = StartCoroutine(flow);
    }

    IEnumerator PlayAndWait(AudioClip clip, float fallback)
    {
        if (audioSource == null || clip == null)
        {
            if (clip == null)
                Debug.LogWarning($"[Scene3A] AudioClip이 비어 있습니다. fallback {fallback}초 대기");

            yield return new WaitForSeconds(fallback);
            yield break;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        Log($"재생: {clip.name} ({clip.length:F2}s)");
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
        {
            OnCandleGrabbed();
        }
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
        if (hands.Count == 0) return false;

        foreach (Vector3 handPos in hands)
        {
            if ((candle != null && Vector3.Distance(handPos, candle.transform.position) <= handNearDistance) ||
                (candle1 != null && Vector3.Distance(handPos, candle1.transform.position) <= handNearDistance))
            {
                return true;
            }
        }

        return false;
    }

    List<Vector3> GetHandPositions()
    {
        List<Vector3> list = new List<Vector3>();

        if (ovrLeftHand != null) list.Add(ovrLeftHand.position);
        if (ovrRightHand != null) list.Add(ovrRightHand.position);

        return list;
    }

    bool IsNear(Vector3 pos, GameObject target, float dist)
    {
        return target != null && Vector3.Distance(pos, target.transform.position) <= dist;
    }

    bool IsChildOf(GameObject obj, GameObject parent)
    {
        if (obj == null || parent == null) return false;

        Transform t = obj.transform;
        while (t != null)
        {
            if (t.gameObject == parent) return true;
            t = t.parent;
        }
        return false;
    }

    void SetHighlight(GameObject obj, bool active)
    {
        if (obj == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat == null) continue;

                if (active)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
                else
                {
                    mat.SetColor("_EmissionColor", Color.black);
                    mat.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    void SetLightIntensity(float intensity)
    {
        if (candleLight != null) candleLight.intensity = intensity;
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
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                Log($"MeshCollider 자동 추가: {mf.gameObject.name}");
                return;
            }
        }

        SphereCollider sc = obj.AddComponent<SphereCollider>();
        sc.radius = 0.2f;
        Log($"SphereCollider 자동 추가: {obj.name}");
    }

    void EnsureRigidbody(GameObject obj)
    {
        if (obj == null) return;
        if (obj.GetComponent<Rigidbody>() != null) return;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.drag = 5f;
        rb.angularDrag = 5f;
        Log($"Rigidbody 자동 추가: {obj.name} (useGravity=false)");
    }

    void SetupAudio()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.loop = false;
        audioSource.mute = false;

        if (FindObjectOfType<AudioListener>() == null)
            Debug.LogError("[Scene3A] AudioListener가 없습니다. HMD 카메라 또는 메인 카메라에 AudioListener를 추가하세요.");
    }

    void AutoBindOVRReferences()
    {
        if (ovrPlayerRoot == null)
        {
            GameObject rootObj = GameObject.Find("Player_OVRInput_Parts");
            if (rootObj != null)
                ovrPlayerRoot = rootObj.transform;
        }

        if (ovrPlayerRoot == null) return;

        if (ovrCenterEye == null)
        {
            ovrCenterEye =
                FindChildDeep(ovrPlayerRoot, "CenterEyeAnchor") ??
                FindChildDeep(ovrPlayerRoot, "OVRCenterEye") ??
                FindChildDeep(ovrPlayerRoot, "OVRHmd") ??
                FindChildDeep(ovrPlayerRoot, "Main Camera");
        }

        if (ovrLeftHand == null)
        {
            ovrLeftHand =
                FindChildDeep(ovrPlayerRoot, "LeftControllerAnchor") ??
                FindChildDeep(ovrPlayerRoot, "LeftHandAnchor") ??
                FindChildDeep(ovrPlayerRoot, "LeftControllerInHandAnchor") ??
                FindChildDeep(ovrPlayerRoot, "LeftController") ??
                FindChildDeep(ovrPlayerRoot, "OVRLeftController");
        }

        if (ovrRightHand == null)
        {
            ovrRightHand =
                FindChildDeep(ovrPlayerRoot, "RightHandAnchor") ??
                FindChildDeep(ovrPlayerRoot, "RightControllerAnchor") ??
                FindChildDeep(ovrPlayerRoot, "RightController") ??
                FindChildDeep(ovrPlayerRoot, "OVRRightController");
        }
    }

    Transform FindChildDeep(Transform parent, string targetName)
    {
        if (parent == null) return null;

        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
                return child;
        }

        return null;
    }

    void ValidateReferences()
    {
        if (door == null) Debug.LogWarning("[Scene3A] door 미연결");
        if (candle == null) Debug.LogWarning("[Scene3A] candle 미연결");
        if (candle1 == null) Debug.LogWarning("[Scene3A] candle1 미연결");

        if (candleLight == null) Debug.LogWarning("[Scene3A] candleLight 미연결");
        if (candleLight1 == null) Debug.LogWarning("[Scene3A] candleLight1 미연결");

        if (clip1 == null) Debug.LogWarning("[Scene3A] clip1(Scene3_A_1) 미연결");
        if (clip2 == null) Debug.LogWarning("[Scene3A] clip2(Scene3_A_2) 미연결");
        if (clip3 == null) Debug.LogWarning("[Scene3A] clip3(Scene3_A_3) 미연결");
        if (clip4 == null) Debug.LogWarning("[Scene3A] clip4(Scene3_A_4) 미연결");
        if (clip5 == null) Debug.LogWarning("[Scene3A] clip5(Scene3_A_5) 미연결");

        if (ovrPlayerRoot == null) Debug.LogWarning("[Scene3A] ovrPlayerRoot(Player_OVRInput_Parts) 미연결");
        if (ovrCenterEye == null) Debug.LogWarning("[Scene3A] ovrCenterEye 미연결");
        if (ovrLeftHand == null) Debug.LogWarning("[Scene3A] ovrLeftHand 미연결");
        if (ovrRightHand == null) Debug.LogWarning("[Scene3A] ovrRightHand 미연결");
    }

    void LogRefs()
    {
        if (!verboseLog) return;

        Debug.Log(
            $"[Scene3A] Refs | Root={GetNameOrNull(ovrPlayerRoot)}, Eye={GetNameOrNull(ovrCenterEye)}, Left={GetNameOrNull(ovrLeftHand)}, Right={GetNameOrNull(ovrRightHand)}"
        );
    }

    string GetNameOrNull(Transform t)
    {
        return t == null ? "null" : t.name;
    }

    void HandleDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StopAllCoroutines();
            _activeFlow = null;

            _candleNearTriggered = false;
            _candleGrabbed = false;
            _doorTriggered = false;

            SetHighlight(candle, false);
            SetHighlight(candle1, false);
            SetHighlight(door, false);
            SetLightIntensity(normalIntensity);

            RunFlow(IntroFlow());
            Debug.Log("[Scene3A][F1] 전체 재시작");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            _candleNearTriggered = false;
            currentStep = Step.WaitingForCandleNear;
            OnCandleProximityEntered();
            Debug.Log("[Scene3A][F2] 캔들 접근 강제");
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            currentStep = Step.WaitingForGrab;
            OnCandleGrabbed();
            Debug.Log("[Scene3A][F3] 캔들 집기 강제");
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            currentStep = Step.WaitingForDoor;
            OnPlayerEnterDoor();
            Debug.Log("[Scene3A][F4] 문 도달 강제");
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log($"[Scene3A][F5] 현재 단계: {currentStep}");
        }
    }

    void Log(string msg)
    {
        if (verboseLog)
            Debug.Log($"[Scene3A] {msg}");
    }

    void OnGUI()
    {
        if (!showDebugGUI) return;

        GUIStyle style = new GUIStyle(GUI.skin.box)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleLeft
        };
        style.normal.textColor = Color.white;

        string text = currentStep switch
        {
            Step.IntroPlaying         => "▶ 1단계: Scene3_A_1 음성 재생 중",
            Step.WaitingForCandleNear => "▶ 2단계: 캔들 가까이 다가가세요",
            Step.CandleNearSequence   => "▶ 3단계: 밝아짐 + Scene3_A_2/3 재생 중",
            Step.WaitingForGrab       => "▶ 4단계: 캔들을 집으세요",
            Step.GrabSuccessSequence  => "▶ 5단계: Scene3_A_4/5 진행 중",
            Step.WaitingForDoor       => "▶ 6단계: door 앞으로 이동하세요",
            Step.Done                 => "✓ 완료! Scene3_B 로딩 중",
            _                         => $"▶ {currentStep}"
        };

        GUI.Box(new Rect(10, 10, 460, 36), text, style);
        GUI.Box(new Rect(10, 50, 460, 28), "F1=재시작  F2=접근  F3=집기  F4=문  F5=상태", style);
    }
}