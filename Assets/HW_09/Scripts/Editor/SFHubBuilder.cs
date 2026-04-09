using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// EDEN — MainScene "연구소 복도"
/// 1자형 / 흰 벽 / 오른쪽 알코브 문 4개 / 양끝 완전 막힘
/// </summary>
public class SFHubBuilder : EditorWindow
{
    // ── 복도 치수 ──────────────────────────────────────────
    const float HW   = 3f;    // 반너비 (X: -HW ~ +HW)
    const float CH   = 4f;    // 높이
    const float CL   = 52f;   // 길이 (Z: 0 ~ CL)
    const float WT   = 0.15f; // 벽 두께

    // ── 문 설정 ────────────────────────────────────────────
    static readonly float[] DZ  = { 10f, 22f, 34f, 46f }; // 문 중심 Z
    const float DW   = 3f;    // 문 폭
    const float DH   = 3.5f;  // 문 높이
    const float AD   = 3.5f;  // 알코브 깊이

    static readonly string[] SCENES = { "Scene2_A","Scene2_B","Scene2_A","Scene2_B" };
    static readonly string[] LABELS = {
        "TEST  01\n- SCENE 2A -",
        "TEST  02\n- SCENE 2B -",
        "TEST  03\n- SCENE 2A -",
        "TEST  04\n- SCENE 2B -"
    };

    static Material s_white, s_floor, s_ceil, s_door, s_sign;

    [MenuItem("EDEN/Build MainScene Hub")]
    public static void Build()
    {
        var old = GameObject.Find("=== SF HUB ===");
        if (old != null) Undo.DestroyObjectImmediate(old);

        MakeMaterials();

        var root = new GameObject("=== SF HUB ===");
        Undo.RegisterCreatedObjectUndo(root, "SF Hub");

        BuildStructure(root);          // 바닥·천장·좌벽·우벽·양끝 벽
        for (int i = 0; i < 4; i++)
            BuildAlcoveDoor(root, i);  // 오른쪽 알코브 문
        BuildLights(root);             // 분위기 조명
        BuildWallStrips(root);         // 벽 발광 스트립
        BuildFloorGrid(root);          // 바닥 격자선
        BuildDeterioration(root);      // 열화 오브젝트
        SetupPlayer();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[EDEN] MainScene Hub 빌드 완료!");
    }

    // ════════════════════════════════════════════════════════
    // 머티리얼
    // ════════════════════════════════════════════════════════
    static void MakeMaterials()
    {
        // 공포+SF: 어두운 금속 벽, 극도로 광택 있는 바닥, 차가운 청록 발광
        s_white = Mat(new Color(0.28f,0.30f,0.34f), 0.65f, 0.88f);  // 어두운 금속 벽
        s_floor = Mat(new Color(0.12f,0.13f,0.16f), 0.90f, 1.00f);  // 검은 거울 바닥
        s_ceil  = MatEmit(new Color(0.1f,0.12f,0.15f),               // 어두운 천장
                          new Color(0.0f,0.35f,0.55f));               // 차가운 파란 발광
        s_door  = MatEmit(new Color(0.5f,0.02f,0.02f),               // 붉은 문 패널
                          new Color(2.5f,0.05f,0.05f));               // 강렬한 붉은 발광
        s_sign  = MatEmit(new Color(0.04f,0.1f,0.12f),               // 어두운 사인 배경
                          new Color(0.0f,0.5f,0.6f));                 // 청록 발광

        // 씬 전역 분위기
        RenderSettings.fog      = true;
        RenderSettings.fogColor = new Color(0.02f, 0.03f, 0.06f);
        RenderSettings.fogMode  = FogMode.Linear;
        RenderSettings.fogStartDistance = 8f;
        RenderSettings.fogEndDistance   = 45f;
        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.04f, 0.07f);
    }

    static Material Mat(Color c, float metal, float gloss)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c; m.SetFloat("_Metallic", metal); m.SetFloat("_Glossiness", gloss);
        return m;
    }
    static Material MatEmit(Color c, Color emit)
    {
        var m = Mat(c, 0.05f, 0.7f);
        m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emit);
        return m;
    }

    // ════════════════════════════════════════════════════════
    // 복도 기본 구조
    // ════════════════════════════════════════════════════════
    static void BuildStructure(GameObject root)
    {
        var s = Child(root, "Structure");

        // 바닥
        Box(s,"Floor",    new Vector3(0, -WT*.5f, CL*.5f),   new Vector3(HW*2,WT,CL),         s_floor);
        // 천장
        Box(s,"Ceiling",  new Vector3(0, CH+WT*.5f, CL*.5f), new Vector3(HW*2,WT,CL),         s_ceil);
        // 왼쪽 벽 (전체)
        Box(s,"WallLeft", new Vector3(-HW-WT*.5f, CH*.5f, CL*.5f), new Vector3(WT,CH,CL),     s_white);
        // 남쪽 끝 벽
        Box(s,"WallSouth",new Vector3(0, CH*.5f, -WT*.5f),   new Vector3(HW*2+WT*2,CH,WT),    s_white);
        // 북쪽 끝 벽
        Box(s,"WallNorth",new Vector3(0, CH*.5f, CL+WT*.5f), new Vector3(HW*2+WT*2,CH,WT),    s_white);

        // 오른쪽 벽 — 문 사이 세그먼트 (하단부: 0 ~ DH)
        float[] sZ = BuildRightWallSegments(s);
    }

    static float[] BuildRightWallSegments(GameObject parent)
    {
        // 문 구간: [DZ[i]-DW/2 ~ DZ[i]+DW/2]
        // 세그먼트 경계
        float rx = HW + WT * .5f;
        float linH = CH - DH; // 문 위 상단 높이

        // 벽 구간 5개 (문 4개 사이)
        float[] starts = { 0,
            DZ[0]+DW*.5f, DZ[1]+DW*.5f, DZ[2]+DW*.5f, DZ[3]+DW*.5f };
        float[] ends = {
            DZ[0]-DW*.5f, DZ[1]-DW*.5f, DZ[2]-DW*.5f, DZ[3]-DW*.5f, CL };

        for (int i = 0; i < 5; i++)
        {
            float len = ends[i] - starts[i];
            if (len < 0.01f) continue;
            float zc = (starts[i] + ends[i]) * .5f;
            // 전체 높이 벽
            Box(parent, $"RWall_{i}", new Vector3(rx, CH*.5f, zc),
                new Vector3(WT, CH, len), s_white);
        }

        // 문 위 lintel (4개)
        if (linH > 0.01f)
        {
            for (int i = 0; i < 4; i++)
                Box(parent, $"Lintel_{i}",
                    new Vector3(rx, DH + linH*.5f, DZ[i]),
                    new Vector3(WT, linH, DW), s_white);
        }

        return starts;
    }

    // ════════════════════════════════════════════════════════
    // 알코브 문
    // ════════════════════════════════════════════════════════
    static void BuildAlcoveDoor(GameObject root, int idx)
    {
        float dz  = DZ[idx];
        float rx  = HW + WT;          // 알코브 시작 X (벽 바깥)
        float rEnd = rx + AD;          // 알코브 끝 X

        var door = Child(root, $"Door_{idx+1:D2}");

        // 알코브: 남벽·북벽·천장
        Box(door,"AlcS", new Vector3((rx+rEnd)*.5f, DH*.5f,    dz-DW*.5f-WT*.5f),
            new Vector3(AD, DH, WT), s_white);
        Box(door,"AlcN", new Vector3((rx+rEnd)*.5f, DH*.5f,    dz+DW*.5f+WT*.5f),
            new Vector3(AD, DH, WT), s_white);
        Box(door,"AlcT", new Vector3((rx+rEnd)*.5f, DH+WT*.5f, dz),
            new Vector3(AD, WT, DW+WT*2), s_white);

        // 발광 문 패널
        Box(door,"Panel", new Vector3(rEnd+WT*.5f, DH*.5f, dz),
            new Vector3(WT, DH, DW), s_door);

        // DoorInteraction 트리거
        var trig = Child(door, "Trigger");
        trig.transform.position = new Vector3(rx + AD*.55f, DH*.5f, dz);
        var col = trig.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(AD*.7f, DH, DW*.85f);
        var di = trig.AddComponent<DoorMain>();
        var so = new SerializedObject(di);
        so.FindProperty("targetScene").stringValue = SCENES[idx];
        so.ApplyModifiedProperties();

        // 표지판
        BuildSign(door, idx, dz, HW - 0.01f);

        Debug.Log($"[EDEN] Door {idx+1}: {LABELS[idx].Split('\n')[0]} → {SCENES[idx]}");
    }

    static void BuildSign(GameObject parent, int idx, float dz, float x)
    {
        float linH = CH - DH;
        float sy   = DH + linH * .5f;

        var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(bg, "SignBG");
        bg.name = "SignBG";
        bg.transform.SetParent(parent.transform);
        bg.transform.position    = new Vector3(x, sy, dz);
        bg.transform.eulerAngles = new Vector3(0, 90, 0);
        bg.transform.localScale  = new Vector3(DW*.88f, linH*.78f, 0.06f);
        Object.DestroyImmediate(bg.GetComponent<BoxCollider>());
        bg.GetComponent<MeshRenderer>().material = s_sign;

        var tGo = new GameObject("SignText");
        Undo.RegisterCreatedObjectUndo(tGo, "SignText");
        tGo.transform.SetParent(bg.transform);
        tGo.transform.localPosition    = new Vector3(0, 0, -0.6f);
        tGo.transform.localEulerAngles = Vector3.zero;
        tGo.transform.localScale       = new Vector3(0.35f, 0.35f, 0.35f);

        var tmp = tGo.AddComponent<TextMeshPro>();
        tmp.text = LABELS[idx]; tmp.fontSize = 4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.5f, 0.95f, 1f);
        var rt = tGo.GetComponent<RectTransform>();
        if (rt) rt.sizeDelta = new Vector2(7, 4);
    }

    // ════════════════════════════════════════════════════════
    // 분위기 조명 — 차갑고 희미한 파란 점 조명
    // ════════════════════════════════════════════════════════
    static void BuildLights(GameObject root)
    {
        var lg = Child(root, "Lights");
        for (int i = 0; i < 9; i++)
        {
            float z = CL / 10f * (i + 1);
            var go  = new GameObject($"PL_{i}");
            Undo.RegisterCreatedObjectUndo(go, "PL");
            go.transform.SetParent(lg.transform);
            go.transform.position = new Vector3(0, CH - 0.3f, z);
            var lt = go.AddComponent<Light>();
            lt.type      = LightType.Point;
            lt.color     = new Color(0.4f, 0.65f, 1.0f);   // 차가운 파란빛
            lt.intensity = 1.4f;
            lt.range     = 13f;
        }

        // 문 앞 붉은 경고등 (각 알코브)
        foreach (float dz in DZ)
        {
            var go = new GameObject("DoorWarnLight");
            Undo.RegisterCreatedObjectUndo(go, "DoorWarnLight");
            go.transform.SetParent(lg.transform);
            go.transform.position = new Vector3(HW + 0.5f, DH + 0.3f, dz);
            var lt = go.AddComponent<Light>();
            lt.type      = LightType.Point;
            lt.color     = new Color(1f, 0.05f, 0.02f);
            lt.intensity = 1.8f;
            lt.range     = 5f;
        }
    }

    // ════════════════════════════════════════════════════════
    // 벽 하단 발광 스트립 (SF 느낌)
    // ════════════════════════════════════════════════════════
    static void BuildWallStrips(GameObject root)
    {
        var sg = Child(root, "WallStrips");
        var stripMat = MatEmit(new Color(0.02f,0.05f,0.08f),
                               new Color(0.0f, 0.6f, 0.9f));  // 밝은 청록

        // 왼쪽 벽 하단 스트립
        Box(sg,"StripL", new Vector3(-HW+WT*0.6f, 0.12f, CL*.5f),
            new Vector3(0.04f, 0.08f, CL), stripMat);

        // 오른쪽 벽 세그먼트마다 스트립 (문 구간 제외)
        float[] sStarts = { 0,
            DZ[0]+DW*.5f, DZ[1]+DW*.5f, DZ[2]+DW*.5f, DZ[3]+DW*.5f };
        float[] sEnds   = {
            DZ[0]-DW*.5f, DZ[1]-DW*.5f, DZ[2]-DW*.5f, DZ[3]-DW*.5f, CL };
        for (int i = 0; i < 5; i++)
        {
            float len = sEnds[i] - sStarts[i];
            if (len < 0.1f) continue;
            Box(sg,$"StripR_{i}",
                new Vector3(HW-WT*0.6f, 0.12f, (sStarts[i]+sEnds[i])*.5f),
                new Vector3(0.04f, 0.08f, len), stripMat);
        }

        // 천장 중앙 스트립 (이중선)
        var ceilStripMat = MatEmit(new Color(0.02f,0.03f,0.04f),
                                   new Color(0.0f, 0.25f, 0.4f));
        Box(sg,"StripCeil1", new Vector3(-0.6f, CH-WT*0.6f, CL*.5f),
            new Vector3(0.06f, 0.04f, CL), ceilStripMat);
        Box(sg,"StripCeil2", new Vector3( 0.6f, CH-WT*0.6f, CL*.5f),
            new Vector3(0.06f, 0.04f, CL), ceilStripMat);
    }

    // ════════════════════════════════════════════════════════
    // 바닥 중앙 격자선 (SF 느낌)
    // ════════════════════════════════════════════════════════
    static void BuildFloorGrid(GameObject root)
    {
        var gg = Child(root, "FloorGrid");
        var lineMat = MatEmit(new Color(0.02f,0.05f,0.06f),
                              new Color(0.0f,0.2f,0.35f));

        // 중앙 세로선
        Box(gg,"GridCenter", new Vector3(0, 0.005f, CL*.5f),
            new Vector3(0.06f, 0.01f, CL), lineMat);

        // 가로 구분선 (8m 간격)
        for (float z = 8f; z < CL; z += 8f)
        {
            Box(gg,$"GridH_{z:F0}", new Vector3(0, 0.005f, z),
                new Vector3(HW*2, 0.01f, 0.05f), lineMat);
        }
    }

    // ════════════════════════════════════════════════════════
    // 열화 오브젝트
    // ════════════════════════════════════════════════════════
    static void BuildDeterioration(GameObject root)
    {
        var det = Child(root, "Deterioration");

        // ① lightOff — 북쪽 끝 붉은 경보등
        var lo = new GameObject("LightOff");
        Undo.RegisterCreatedObjectUndo(lo, "LightOff");
        lo.transform.SetParent(det.transform);
        lo.transform.position = new Vector3(0, CH-0.3f, CL-4f);
        var rl = lo.AddComponent<Light>();
        rl.type = LightType.Point; rl.color = new Color(1f,0.07f,0.03f);
        rl.intensity = 5f; rl.range = 16f;
        lo.SetActive(false);

        // ② wallCrack — 왼쪽 벽 균열
        var wc = Quad(det, "WallCrack",
            new Vector3(-HW+0.02f, 2.4f, 26f), new Vector3(0,90,9f), new Vector3(3.5f,4.5f,1f),
            Mat(new Color(0.03f,0.03f,0.03f),0,0));
        wc.SetActive(false);

        // ③ waterPuddle — 바닥 물
        var wm = Mat(new Color(0.18f,0.28f,0.42f,0.8f), 0.95f, 1f);
        wm.SetInt("_Mode",3);
        wm.SetInt("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        wm.SetInt("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        wm.SetInt("_ZWrite",0); wm.renderQueue=3000;
        var wp = Quad(det, "WaterPuddle",
            new Vector3(-0.3f,0.02f,34f), new Vector3(90,0,0), new Vector3(4f,7f,1f), wm);
        wp.SetActive(false);

        // ④ structureTwist — 파편
        var st = Child(det, "StructureTwist");
        st.transform.position = new Vector3(0,0,44f);
        var fm = Mat(new Color(0.78f,0.78f,0.82f), 0.1f, 0.6f);
        (Vector3 p, Vector3 r, Vector3 sc)[] frags = {
            (new Vector3(-1f,3.3f, 0),new Vector3(22, 5, 0),new Vector3(5f,0.18f,2.8f)),
            (new Vector3( 1f,2.0f, 1),new Vector3(-14,0,28),new Vector3(0.18f,3.5f,2.2f)),
            (new Vector3( 0f,1.0f,-1),new Vector3(0,  0,38),new Vector3(4f,0.18f,1.8f)),
        };
        foreach (var (p,r,sc) in frags)
        {
            var f = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(f,"Frag"); f.name="Frag";
            f.transform.SetParent(st.transform);
            f.transform.localPosition=p; f.transform.localEulerAngles=r; f.transform.localScale=sc;
            Object.DestroyImmediate(f.GetComponent<BoxCollider>());
            f.GetComponent<MeshRenderer>().material = fm;
        }
        st.SetActive(false);

        // GameManager 연결
        var gm = Object.FindObjectOfType<ManagerMain>();
        if (gm != null)
        {
            var so = new SerializedObject(gm);
            so.FindProperty("lightOff").objectReferenceValue       = lo;
            so.FindProperty("wallCrack").objectReferenceValue      = wc;
            so.FindProperty("waterPuddle").objectReferenceValue    = wp;
            so.FindProperty("structureTwist").objectReferenceValue = st;
            so.ApplyModifiedProperties();
            Debug.Log("[EDEN] GameManager 연결 완료");
        }
    }

    // ════════════════════════════════════════════════════════
    // 플레이어
    // ════════════════════════════════════════════════════════
    static void SetupPlayer()
    {
        var p = GameObject.FindWithTag("Player");
        if (p == null) { p = new GameObject("Player"); Undo.RegisterCreatedObjectUndo(p,"Player"); p.tag="Player"; }
        p.transform.position = new Vector3(0, 1.7f, 4f);

        var rb = p.GetComponent<Rigidbody>() ?? p.AddComponent<Rigidbody>();
        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        if (p.GetComponentInChildren<Camera>() == null)
        {
            // 씬에 이미 있는 AudioListener 제거 (중복 방지)
            foreach (var al in Object.FindObjectsOfType<AudioListener>())
                Object.DestroyImmediate(al);

            var cam = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(cam,"Cam");
            cam.transform.SetParent(p.transform);
            cam.transform.localPosition = Vector3.zero;
            cam.AddComponent<Camera>(); cam.AddComponent<AudioListener>();
        }
        if (p.GetComponent<HW09.PlayerMain>() == null)
            p.AddComponent<HW09.PlayerMain>();
    }

    // ════════════════════════════════════════════════════════
    // 헬퍼
    // ════════════════════════════════════════════════════════
    static GameObject Child(GameObject parent, string name)
    {
        var g = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(g, name);
        g.transform.SetParent(parent.transform);
        return g;
    }

    static GameObject Box(GameObject parent, string name, Vector3 pos, Vector3 size, Material mat)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(g, name); g.name = name;
        g.transform.SetParent(parent.transform);
        g.transform.position = pos; g.transform.localScale = size;
        g.GetComponent<MeshRenderer>().material = mat;
        return g;
    }

    static GameObject Quad(GameObject parent, string name, Vector3 pos, Vector3 rot, Vector3 size, Material mat)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Undo.RegisterCreatedObjectUndo(g, name); g.name = name;
        g.transform.SetParent(parent.transform);
        g.transform.position = pos; g.transform.eulerAngles = rot; g.transform.localScale = size;
        Object.DestroyImmediate(g.GetComponent<MeshCollider>());
        g.GetComponent<MeshRenderer>().material = mat;
        return g;
    }
}
