using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [Header("Fallback")]
    [SerializeField] private string fallbackSceneName = "Main";

    [Header("Optional UI")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Text progressText;

    private void Start()
    {
        string target = string.IsNullOrWhiteSpace(SceneLoadContext.TargetSceneName)
            ? fallbackSceneName
            : SceneLoadContext.TargetSceneName;

        StartCoroutine(LoadAsync(target));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"LoadSceneAsync failed for scene: {sceneName}");
            yield break;
        }

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            if (progressText != null)
            {
                int percent = Mathf.RoundToInt(progress * 100f);
                progressText.text = $"Loading... {percent}%";
            }

            yield return null;
        }

        SceneLoadContext.Clear();
    }
}
