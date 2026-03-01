using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Bounds (optional fallback)")]
    [SerializeField] private Vector3 boundsMin = new Vector3(-20f, -1f, -20f);
    [SerializeField] private Vector3 boundsMax = new Vector3(20f, 5f, 20f);

    private CharacterController _cc;
    private Vector3 _velocity;
    private bool _isMoving;

    public bool IsMoving => _isMoving;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        _isMoving = move.sqrMagnitude > 0.01f;

        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;

        _cc.Move((move * walkSpeed + _velocity) * Time.deltaTime);

        ClampToBounds();
    }

    private void ClampToBounds()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, boundsMin.x, boundsMax.x);
        pos.y = Mathf.Clamp(pos.y, boundsMin.y, boundsMax.y);
        pos.z = Mathf.Clamp(pos.z, boundsMin.z, boundsMax.z);

        if (pos != transform.position)
        {
            _cc.enabled = false;
            transform.position = pos;
            _cc.enabled = true;
        }
    }
}
