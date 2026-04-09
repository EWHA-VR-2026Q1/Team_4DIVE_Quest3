using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SFCorridorBuilder : EditorWindow
{
    [MenuItem("EDEN/Build SF Corridor V3")]
    public static void BuildCorridor()
    {
        // ── 머티리얼 ─────────────────────────────────────────────
        Material whiteMat = new Material(Shader.Find("Standard"));
        whiteMat.color = new Color(0.95f, 0.96f, 0.98f);
        whiteMat.SetFloat("_Metallic", 0.1f);
        whiteMat.SetFloat("_Glossiness", 0.96f);
        AssetDatabase.CreateAsset(whiteMat, "Assets/HW_09/Materials/SF_White.mat");

        Material darkMat = new Material(Shader.Find("Standard"));
        darkMat.color = new Color(0.06f, 0.06f, 0.08f);
        darkMat.SetFloat("_Metallic", 0.9f);
        darkMat.SetFloat("_Glossiness", 0.8f);
        AssetDatabase.CreateAsset(darkMat, "Assets/HW_09/Materials/SF_Dark.mat");

        Material glowMat = new Material(Shader.Find("Standard"));
        glowMat.color = new Color(1f, 1f, 1f);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", new Color(2.2f, 2.4f, 2.8f));
        AssetDatabase.CreateAsset(glowMat, "Assets/HW_09/Materials/SF_Glow.mat");

        AssetDatabase.SaveAssets();

        // ── 루트 오브젝트 ─────────────────────────────────────────
        GameObject root = new GameObject("--- SF CORRIDOR ---");
        Undo.RegisterCreatedObjectUndo(root, "SF Corridor");

        float rx      = 2.4f;   // 타원 반지름 X
        float ry      = 3.2f;   // 타원 반지름 Y (세로로 긴 타원)
        float cy      = 3.2f;   // 타원 중심 Y
        int   segs    = 18;     // 벽 분할 수
        float corrLen = 40f;    // 복도 길이
        float panelH  = 0.03f;  // 패널 두께

        // ── 벽 패널 (복도 길이 방향) ─────────────────────────────
        GameObject wallParent = new GameObject("WallPanels");
        wallParent.transform.SetParent(root.transform);

        // 바닥
        CreateCube(wallParent, "Floor",
            new Vector3(0, -0.05f, 0),
            new Vector3(rx * 1.6f, 0.08f, corrLen), whiteMat);

        // 타원 상반부 패널
        for (int i = 0; i < segs; i++)
        {
            float a0 = Mathf.PI * i / segs;
            float a1 = Mathf.PI * (i + 1) / segs;
            Vector2 p0  = new Vector2(rx * Mathf.Sin(a0), cy + ry * Mathf.Cos(a0));
            Vector2 p1  = new Vector2(rx * Mathf.Sin(a1), cy + ry * Mathf.Cos(a1));
            Vector2 mid = (p0 + p1) * 0.5f;
            float   len = Vector2.Distance(p0, p1);
            float   ang = Mathf.Atan2(p1.x - p0.x, p1.y - p0.y) * Mathf.Rad2Deg;
            bool isTop  = (p0.y > cy + ry * 0.35f && p1.y > cy + ry * 0.35f);
            Material mat = isTop ? glowMat : whiteMat;

            GameObject panel = CreateCube(wallParent, "WallSeg_R" + i,
                new Vector3(mid.x, mid.y, 0),
                new Vector3(panelH, len + 0.02f, corrLen), mat);
            panel.transform.eulerAngles = new Vector3(0, 0, ang);

            if (Mathf.Abs(p0.x) > 0.05f)
            {
                GameObject panelL = CreateCube(wallParent, "WallSeg_L" + i,
                    new Vector3(-mid.x, mid.y, 0),
                    new Vector3(panelH, len + 0.02f, corrLen), mat);
                panelL.transform.eulerAngles = new Vector3(0, 0, -ang);
            }
        }

        // ── 링 아치 ───────────────────────────────────────────────
        int   ringCount = 11;
        float ringStep  = corrLen / (ringCount + 1);
        float ringThick = 0.16f;
        float ringDepth = 0.28f;

        GameObject ringParent = new GameObject("ArchRings");
        ringParent.transform.SetParent(root.transform);

        for (int r = 0; r < ringCount; r++)
        {
            float zPos = -corrLen / 2f + ringStep * (r + 1);
            GameObject ring = new GameObject("Ring_" + r);
            ring.transform.SetParent(ringParent.transform);

            // 바닥 수평 바
            CreateCube(ring, "Bot",
                new Vector3(0, 0f, zPos),
                new Vector3(rx * 2f + ringThick * 2f, ringThick, ringDepth), darkMat);

            int archSegs = 28;
            for (int i = 0; i < archSegs; i++)
            {
                float a0  = Mathf.PI * i / archSegs;
                float a1  = Mathf.PI * (i + 1) / archSegs;
                Vector2 p0  = new Vector2(rx * Mathf.Sin(a0), cy + ry * Mathf.Cos(a0));
                Vector2 p1  = new Vector2(rx * Mathf.Sin(a1), cy + ry * Mathf.Cos(a1));
                Vector2 mid = (p0 + p1) * 0.5f;
                float   len = Vector2.Distance(p0, p1);
                float   ang = Mathf.Atan2(p1.x - p0.x, p1.y - p0.y) * Mathf.Rad2Deg;

                // 어두운 링 세그먼트
                GameObject segR = CreateCube(ring, "SegR_" + i,
                    new Vector3(mid.x, mid.y, zPos),
                    new Vector3(ringThick, len + 0.01f, ringDepth), darkMat);
                segR.transform.eulerAngles = new Vector3(0, 0, ang);

                if (Mathf.Abs(p0.x) > 0.05f)
                {
                    GameObject segL = CreateCube(ring, "SegL_" + i,
                        new Vector3(-mid.x, mid.y, zPos),
                        new Vector3(ringThick, len + 0.01f, ringDepth), darkMat);
                    segL.transform.eulerAngles = new Vector3(0, 0, -ang);
                }

                // 흰 내부 트림 라인
                float rxT = rx - 0.1f, ryT = ry - 0.1f;
                Vector2 t0  = new Vector2(rxT * Mathf.Sin(a0), cy + ryT * Mathf.Cos(a0));
                Vector2 t1  = new Vector2(rxT * Mathf.Sin(a1), cy + ryT * Mathf.Cos(a1));
                Vector2 tm  = (t0 + t1) * 0.5f;
                float   tl  = Vector2.Distance(t0, t1);
                float   ta  = Mathf.Atan2(t1.x - t0.x, t1.y - t0.y) * Mathf.Rad2Deg;

                GameObject trimR = CreateCube(ring, "TrimR_" + i,
                    new Vector3(tm.x, tm.y, zPos + ringDepth * 0.5f + 0.02f),
                    new Vector3(0.03f, tl, 0.04f), whiteMat);
                trimR.transform.eulerAngles = new Vector3(0, 0, ta);

                if (Mathf.Abs(t0.x) > 0.05f)
                {
                    GameObject trimL = CreateCube(ring, "TrimL_" + i,
                        new Vector3(-tm.x, tm.y, zPos + ringDepth * 0.5f + 0.02f),
                        new Vector3(0.03f, tl, 0.04f), whiteMat);
                    trimL.transform.eulerAngles = new Vector3(0, 0, -ta);
                }
            }
        }

        // ── 사이드 레일 ───────────────────────────────────────────
        GameObject railParent = new GameObject("SideRails");
        railParent.transform.SetParent(root.transform);

        float[] railYs = { 0.06f, 0.22f, 0.40f };
        foreach (float rh in railYs)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                CreateCube(railParent, "Rail",
                    new Vector3(s * (rx - 0.1f), rh, 0),
                    new Vector3(0.06f, 0.04f, corrLen), darkMat);
            }
        }

        // ── 조명 ──────────────────────────────────────────────────
        GameObject lightParent = new GameObject("--- LIGHTS ---");
        lightParent.transform.SetParent(root.transform);

        for (int i = 0; i < 9; i++)
        {
            float lz = -corrLen / 2f + corrLen / 10f * (i + 1);
            GameObject lo = new GameObject("Light_" + i);
            lo.transform.SetParent(lightParent.transform);
            lo.transform.position = new Vector3(0, cy + ry - 0.3f, lz);
            Light lt = lo.AddComponent<Light>();
            lt.type      = LightType.Point;
            lt.range     = 18f;
            lt.intensity = 3.5f;
            lt.color     = new Color(0.90f, 0.94f, 1.0f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[EDEN] SF Corridor V3 생성 완료!");
    }

    static GameObject CreateCube(GameObject parent, string name, Vector3 pos, Vector3 scale, Material mat = null)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(g, name);
        g.name = name;
        g.transform.SetParent(parent.transform);
        g.transform.position = pos;
        g.transform.localScale = scale;
        if (mat != null) g.GetComponent<MeshRenderer>().material = mat;
        Object.DestroyImmediate(g.GetComponent<BoxCollider>());
        return g;
    }
}
