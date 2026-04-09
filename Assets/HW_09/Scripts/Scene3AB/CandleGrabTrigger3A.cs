using UnityEngine;

/// <summary>
/// Meta XR SDK (OVRInput) 기반 캔들 집기 감지기.
/// 왼손 LeftControllerAnchor 기준 레이 + OVRInput 트리거 버튼으로 판정합니다.
///
/// Inspector 설정:
///   - candle / candle(1) 오브젝트에 이 컴포넌트 추가
///   - manager 필드에 SceneManager3A 연결 (자동 탐색됨)
/// </summary>
public class CandleGrabTrigger3A : MonoBehaviour
{
    [Header("── 연결 필요 ──────────────────────────")]
    public SceneManager3A manager;

    [Header("── 레이 설정 ────────────────────────")]
    public float rayDistance  = 8f;
    public float sphereRadius = 0.08f;

    [Header("── 트리거 존 크기 ────────────────────")]
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
        Debug.Log($"[CandleGrabTrigger3A:{name}] 왼손 Anchor = {(_leftAnchor ? _leftAnchor.name : "null")}");

        CreateProximityZone();
    }

    void Update()
    {
        if (_grabFired) return;
        if (manager == null) return;
        if (manager.currentStep != SceneManager3A.Step.WaitingForGrab) return;

        // ── 왼손 트리거 버튼 판정 ──────────────────────────────
        bool pressed = OVRInput.Get(OVRInput.RawButton.LIndexTrigger)
                    || OVRInput.Get(OVRInput.RawButton.LHandTrigger);
        if (!pressed) return;

        // ── 레이가 이 캔들에 닿는지 판정 ──────────────────────
        if (_leftAnchor != null && RayHitsThisObject(_leftAnchor))
            FireGrab("왼손 트리거 + 레이 hit");
    }

    // ─── 레이캐스트 ───────────────────────────────────────────
    bool RayHitsThisObject(Transform anchor)
    {
        Ray ray = new Ray(anchor.position, anchor.forward);
        if (!Physics.SphereCast(ray, sphereRadius, out RaycastHit hit,
                                rayDistance, ~0, QueryTriggerInteraction.Collide))
            return false;

        Transform t = hit.collider.transform;
        while (t != null)
        {
            if (t == transform) return true;
            t = t.parent;
        }
        return false;
    }

    // ─── 왼손 Anchor 탐색 (우선순위 순) ─────────────────────
    Transform ResolveLeftAnchor()
    {
        // 1) SceneManager3A.ovrLeftHand가 ControllerAnchor면 바로 사용
        if (manager.ovrLeftHand != null)
        {
            string n = manager.ovrLeftHand.name;
            if (n.Contains("ControllerAnchor") || n.Contains("ControllerInHand"))
                return manager.ovrLeftHand;
        }

        // 2) 씬에서 이름 직접 검색
        string[] names = {
            "LeftControllerAnchor",
            "LeftControllerInHandAnchor",
            "LeftHandAnchor"
        };
        foreach (string n in names)
        {
            var go = GameObject.Find(n);
            if (go != null) return go.transform;
        }

        // 3) Fallback: manager.ovrLeftHand 그대로
        return manager.ovrLeftHand;
    }

    // ─── Proximity ────────────────────────────────────────────
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
        Debug.Log($"[CandleGrabTrigger3A:{name}] 근접 → OnCandleProximityEntered");
        manager.OnCandleProximityEntered();
    }

    void FireGrab(string reason)
    {
        if (_grabFired) return;
        if (manager.currentStep != SceneManager3A.Step.WaitingForGrab) return;
        _grabFired = true;
        Debug.Log($"[CandleGrabTrigger3A:{name}] 집기({reason}) → OnCandleGrabbed");
        manager.OnCandleGrabbed();
    }

    void OnTriggerEnter(Collider other) => OnProximityEnter(other);

    // ─── ProximityZone ────────────────────────────────────────
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
