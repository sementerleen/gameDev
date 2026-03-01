using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Oyun durumunu yöneten Singleton.
/// Paranormal iz sayacı, kazanma/kaybetme koşulları burada kontrol edilir.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    [SerializeField] private int requiredTraces = 3;

    private int _collectedTraces;
    private GameState _state = GameState.Playing;

    public int CollectedTraces => _collectedTraces;
    public int RequiredTraces => requiredTraces;
    public GameState State => _state;

    public event System.Action<int> OnTraceCollected;   // toplam sayı
    public event System.Action OnGameWon;
    public event System.Action OnGameLost;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterTrace()
    {
        if (_state != GameState.Playing) return;

        _collectedTraces++;
        OnTraceCollected?.Invoke(_collectedTraces);

        if (_collectedTraces >= requiredTraces)
            TriggerWin();
    }

    public void TriggerLoss()
    {
        if (_state != GameState.Playing) return;
        _state = GameState.Lost;
        OnGameLost?.Invoke();
        Debug.Log("[GameManager] Game Over — Ela yakalandı veya akıl sağlığı tükendi.");
    }

    private void TriggerWin()
    {
        _state = GameState.Won;
        OnGameWon?.Invoke();
        Debug.Log("[GameManager] Kazandınız — Tüm izler toplandı!");
    }

    public void RestartGame()
    {
        _collectedTraces = 0;
        _state = GameState.Playing;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
}

public enum GameState { Playing, Won, Lost, Paused }
