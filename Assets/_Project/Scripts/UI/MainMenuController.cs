using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "Main";
    [SerializeField] private string loadingSceneName = "Loading";

    public void StartGame()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is empty.");
            return;
        }

        SceneLoadContext.SetTargetScene(gameplaySceneName);

        if (string.IsNullOrWhiteSpace(loadingSceneName))
        {
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        SceneManager.LoadScene(loadingSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit requested.");
        Application.Quit();
    }
}
