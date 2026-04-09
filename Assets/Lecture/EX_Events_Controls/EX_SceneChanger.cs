using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EX_SceneChanger : MonoBehaviour
{
    public static EX_SceneChanger Instance;

    [Header("VR Fade Settings")]
    [SerializeField] private MeshRenderer fadeSphereRenderer;
    [SerializeField] private float fadeDuration = 1.0f;

    private Transform cameraTransform;
    private bool isTransitioning = false;
    private Material fadeMaterial;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (fadeSphereRenderer != null)
            {
                fadeMaterial = fadeSphereRenderer.material;
                fadeSphereRenderer.transform.SetParent(null);
                DontDestroyOnLoad(fadeSphereRenderer.gameObject);
                SetAlpha(0f);
            }
        }
        else { Destroy(gameObject); } // 이미 있으면 중복 생성 방지
    }

    private void LateUpdate()
    {
        if (fadeSphereRenderer == null || !fadeSphereRenderer.gameObject.activeSelf) return;

        if (cameraTransform == null) FindNewCamera();

        if (cameraTransform != null)
        {
            fadeSphereRenderer.transform.position = cameraTransform.position;
        }
    }

    private void FindNewCamera()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;
        else
        {
            GameObject eye = GameObject.Find("CenterEyeAnchor");
            if (eye != null) cameraTransform = eye.transform;
        }
    }

    public void ChangeScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeRoutine(1f));

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        cameraTransform = null;
        FindNewCamera();
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(FadeRoutine(0f));
        isTransitioning = false;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        if (fadeMaterial == null) yield break;
        fadeSphereRenderer.gameObject.SetActive(true);

        Color color = fadeMaterial.color;
        float startAlpha = color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeMaterial.SetColor("_Color", color);
            yield return null;
        }
        SetAlpha(targetAlpha);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeMaterial != null)
        {
            Color c = fadeMaterial.color;
            c.a = alpha;
            fadeMaterial.SetColor("_Color", c);
            if (alpha <= 0.01f) fadeSphereRenderer.gameObject.SetActive(false);
        }
    }
}