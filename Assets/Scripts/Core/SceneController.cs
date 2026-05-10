using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//[RequireComponent(typeof(CanvasGroup))]
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (fadeCanvas == null) fadeCanvas = GetComponent<CanvasGroup>();
        }
        else Destroy(gameObject);
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(DoLoad(sceneIndex));
    }

    IEnumerator DoLoad(int sceneIndex)
    {
        yield return StartCoroutine(Fade(1f));
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
        while (!op.isDone) yield return null;
        yield return StartCoroutine(Fade(0f));
    }

    IEnumerator Fade(float target)
    {
        float start = fadeCanvas.alpha;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = target;
    }
}
