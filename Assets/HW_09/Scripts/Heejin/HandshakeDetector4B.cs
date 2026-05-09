using System.Collections;
using UnityEngine;

namespace HW09.Heejin
{
    public class HandshakeDetector4B : MonoBehaviour, IRayInteractable
    {
        [Header("Detection")]
        public string playerHandTag = "PlayerHand";

        [Header("Glitch")]
        public Material glitchMaterial;
        public float glitchDuration = 0.5f;

        private MeshRenderer _renderer;
        private Material _originalMaterial;
        private bool _isGlitching;
        private bool _hasTriggered = false;

        public void OnRayEnter() { }
        public void OnRayExit()  { }
        public void OnRayStay()  { }
        public void OnRayClick() { SimulateTrigger(); }

        void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            if (_renderer != null) _originalMaterial = _renderer.sharedMaterial;

#if UNITY_EDITOR
            if (glitchMaterial == null)
                glitchMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/HW_09/Materials/GlitchMaterial.mat");
#endif
        }

        void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered || _isGlitching) return;
            if (!other.CompareTag(playerHandTag)) return;
            StartCoroutine(GlitchRoutine());
            PlayAudio("AudioHacker_Phase2");
        }

        IEnumerator GlitchRoutine()
        {
            _hasTriggered = true;
            _isGlitching  = true;

            if (_renderer != null && glitchMaterial != null)
                _renderer.sharedMaterial = glitchMaterial;

            yield return new WaitForSeconds(glitchDuration);

            if (_renderer != null) _renderer.sharedMaterial = _originalMaterial;
            _isGlitching = false;

            ActivateExitDoor();
        }

        void ActivateExitDoor()
        {
            GameObject exitDoor = GameObject.Find("ExitDoor");
            if (exitDoor == null) { Debug.LogWarning("[HandshakeDetector4B] ExitDoor not found."); return; }

            Renderer r = exitDoor.GetComponent<Renderer>();
            if (r != null)
            {
                Material mat      = r.material;
                Color baseColor   = new Color(0f, 1f, 0.667f);
                Color emissionHDR = baseColor * Mathf.Pow(2f, 2f);
                mat.SetColor("_EmissionColor", emissionHDR);
                mat.EnableKeyword("_EMISSION");
                DynamicGI.SetEmissive(r, emissionHDR);
            }

            Transform existing = exitDoor.transform.Find("ExitDoorLight");
            if (existing != null) { existing.gameObject.SetActive(true); return; }

            GameObject lightGO = new GameObject("ExitDoorLight");
            lightGO.transform.SetParent(exitDoor.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0f, 0.2f);

            Light pointLight = lightGO.AddComponent<Light>();
            pointLight.type      = LightType.Point;
            pointLight.color     = new Color(0f, 1f, 0.667f);
            pointLight.intensity = 1.5f;
            pointLight.range     = 3f;
        }

        void PlayAudio(string goName)
        {
            GameObject go = GameObject.Find(goName);
            if (go == null) return;
            AudioSource src = go.GetComponent<AudioSource>();
            if (src != null) src.Play();
        }

        public void SimulateTrigger()
        {
            if (_hasTriggered || _isGlitching) return;
            StartCoroutine(GlitchRoutine());
            PlayAudio("AudioHacker_Phase2");
        }
    }
}
