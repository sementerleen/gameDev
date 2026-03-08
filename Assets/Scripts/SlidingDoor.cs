using UnityEngine;

/// <summary>
/// Proximity-based sliding door.
/// Two child panels slide apart when player enters trigger zone.
/// </summary>
public class SlidingDoor : MonoBehaviour
{
    [Header("Panels")]
    public Transform panelLeft;
    public Transform panelRight;

    [Header("Settings")]
    public float openDistance  = 0.65f;   // how far each panel slides
    public float slideSpeed    = 2.5f;
    public float triggerRadius = 2.5f;

    bool  _isOpen;
    float _currentOffset;
    Vector3 _leftClosed, _rightClosed;

    void Start()
    {
        if (panelLeft  != null) _leftClosed  = panelLeft.localPosition;
        if (panelRight != null) _rightClosed = panelRight.localPosition;
    }

    void Update()
    {
        // Check for player proximity
        GameObject player = GameObject.FindWithTag("Player");
        bool playerNear = player != null &&
            Vector3.Distance(transform.position, player.transform.position) < triggerRadius;

        _isOpen = playerNear;
        float target = _isOpen ? openDistance : 0f;
        _currentOffset = Mathf.MoveTowards(_currentOffset, target, slideSpeed * Time.deltaTime);

        if (panelLeft  != null) panelLeft.localPosition  = _leftClosed  + Vector3.left  * _currentOffset;
        if (panelRight != null) panelRight.localPosition = _rightClosed + Vector3.right * _currentOffset;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
