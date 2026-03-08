using UnityEngine;

/// <summary>
/// Paranormal iz orbu. Arda'nın kamerayla yaklaştıkça görünür hale gelir.
/// 1 birim içinde otomatik toplanır ve GameManager'a bildirir.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class ParanormalOrb : MonoBehaviour
{
    [Header("Proximity")]
    [SerializeField] private float collectRadius = 1f;
    [SerializeField] private float visibilityStartRadius = 8f;

    [Header("Glow")]
    [SerializeField] private float minEmission = 0f;
    [SerializeField] private float maxEmission = 2.5f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Sanity Cost")]
    [SerializeField] private float sanityCostPerSecondNear = 2f;

    private Renderer _renderer;
    private Transform _player;
    private SanitySystem _sanity;
    private MaterialPropertyBlock _mpb;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();

        // Başlangıçta görünmez
        SetAlpha(0f);
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _sanity = playerObj.GetComponentInChildren<SanitySystem>();
        }
    }

    void Update()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);

        UpdateVisibility(dist);
        UpdateGlow(dist);

        if (dist <= collectRadius)
            Collect();
    }

    private void UpdateVisibility(float dist)
    {
        float alpha = 1f - Mathf.Clamp01((dist - collectRadius) / (visibilityStartRadius - collectRadius));
        SetAlpha(alpha);

        // Sanity'e yakınlık etkisi
        if (_sanity != null)
        {
            bool near = dist <= visibilityStartRadius;
            _sanity.SetParanormalNearby(near);
            if (near)
                _sanity.ModifySanity(-sanityCostPerSecondNear * Time.deltaTime);
        }
    }

    private void UpdateGlow(float dist)
    {
        float t = 1f - Mathf.Clamp01(dist / visibilityStartRadius);
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float emission = Mathf.Lerp(minEmission, maxEmission, t * pulse);

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissionColor, Color.cyan * emission);
        _renderer.SetPropertyBlock(_mpb);
    }

    private void SetAlpha(float alpha)
    {
        _renderer.GetPropertyBlock(_mpb);
        Color c = _mpb.GetColor("_Color");
        c.a = alpha;
        _mpb.SetColor("_Color", c);
        _renderer.SetPropertyBlock(_mpb);
    }

    private void Collect()
    {
        if (_sanity != null)
            _sanity.SetParanormalNearby(false);

        GameManager.Instance?.RegisterTrace();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visibilityStartRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}
