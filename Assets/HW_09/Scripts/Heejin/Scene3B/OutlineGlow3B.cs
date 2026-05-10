using UnityEngine;

namespace HW09.Heejin
{
    /// <summary>
    /// 머티리얼의 Emission을 켜고 끄는 글로우 헬퍼.
    /// 자식 렌더러까지 적용하므로 복잡한 모델에도 작동.
    /// (Standard Shader 기준 Built-in RP. 다른 셰이더는 _EmissionColor 프로퍼티 필요)
    /// </summary>
    public class OutlineGlow3B : MonoBehaviour
    {
        [Header("── 글로우 색상 ──")]
        public Color glowColor = Color.yellow;
        [Range(0f, 5f)] public float glowIntensity = 1.5f;

        [Header("── 옵션 ──")]
        [Tooltip("자식 오브젝트의 렌더러도 같이 적용")]
        public bool useChildRenderers = true;

        private Renderer[] _renderers;
        private Material[] _matInstances;
        private Color[] _originalEmissions;
        private bool[] _hadEmission;

        void Awake()
        {
            _renderers = useChildRenderers
                ? GetComponentsInChildren<Renderer>(true)
                : new Renderer[] { GetComponent<Renderer>() };

            int n = _renderers.Length;
            _matInstances = new Material[n];
            _originalEmissions = new Color[n];
            _hadEmission = new bool[n];

            for (int i = 0; i < n; i++)
            {
                if (_renderers[i] == null) continue;
                _matInstances[i] = _renderers[i].material; // 인스턴스화
                if (_matInstances[i].HasProperty("_EmissionColor"))
                {
                    _originalEmissions[i] = _matInstances[i].GetColor("_EmissionColor");
                    _hadEmission[i] = _matInstances[i].IsKeywordEnabled("_EMISSION");
                }
            }
        }

        public void SetGlow(bool on)
        {
            for (int i = 0; i < _matInstances.Length; i++)
            {
                if (_matInstances[i] == null) continue;
                if (!_matInstances[i].HasProperty("_EmissionColor")) continue;

                if (on)
                {
                    _matInstances[i].EnableKeyword("_EMISSION");
                    _matInstances[i].SetColor("_EmissionColor", glowColor * glowIntensity);
                    DynamicGI.SetEmissive(_renderers[i], glowColor * glowIntensity);
                }
                else
                {
                    _matInstances[i].SetColor("_EmissionColor", _originalEmissions[i]);
                    if (!_hadEmission[i]) _matInstances[i].DisableKeyword("_EMISSION");
                }
            }
        }
    }
}
