using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Arda'nın akıl sağlığı sistemi.
/// Zamanla azalır; kritik eşiklerde event'ler tetiklenir (UI bozulması, ses distorsiyonu vb.).
/// </summary>
public class SanitySystem : MonoBehaviour
{
    [Header("Sanity")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float drainRate = 1.5f;      // saniye başına düşen miktar (normal)
    [SerializeField] private float paranormalDrainMultiplier = 3f;  // orb/varlık yakınındayken çarpan

    [Header("Thresholds")]
    [SerializeField] private float lowThreshold = 40f;
    [SerializeField] private float criticalThreshold = 20f;

    [Header("Events")]
    public UnityEvent onLowSanity;
    public UnityEvent onCriticalSanity;
    public UnityEvent onSanityRestored;   // eşiklerin üstüne çıkınca
    public UnityEvent<float> onSanityChanged; // 0–1 normalize değeri

    private float _current;
    private bool _inLow;
    private bool _inCritical;
    private bool _paranormalNearby;

    public float Normalized => _current / maxSanity;
    public float Current => _current;

    void Start()
    {
        _current = maxSanity;
    }

    void Update()
    {
        float rate = drainRate * (_paranormalNearby ? paranormalDrainMultiplier : 1f);
        ModifySanity(-rate * Time.deltaTime);
    }

    public void SetParanormalNearby(bool value) => _paranormalNearby = value;

    public void ModifySanity(float amount)
    {
        _current = Mathf.Clamp(_current + amount, 0f, maxSanity);
        onSanityChanged?.Invoke(Normalized);

        CheckThresholds();
    }

    private void CheckThresholds()
    {
        if (_current <= criticalThreshold && !_inCritical)
        {
            _inCritical = true;
            _inLow = true;
            onCriticalSanity?.Invoke();
        }
        else if (_current <= lowThreshold && !_inLow)
        {
            _inLow = true;
            onLowSanity?.Invoke();
        }

        if (_current > lowThreshold && (_inLow || _inCritical))
        {
            _inLow = false;
            _inCritical = false;
            onSanityRestored?.Invoke();
        }
    }
}
