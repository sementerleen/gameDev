using UnityEngine;

/// <summary>
/// Simple FPS player controller for horror game.
/// Requires: CharacterController component on same GameObject.
/// Camera should be a child named "PlayerCamera".
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FPSPlayer : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed  = 7f;
    public float gravity   = -15f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle     = 85f;

    [Header("Footstep Sounds")]
    public AudioClip[] footstepClips;
    public float footstepInterval = 0.5f;

    // Components
    CharacterController _cc;
    Camera              _cam;
    AudioSource         _audio;

    // State
    float   _verticalVelocity;
    float   _cameraPitch;
    float   _footstepTimer;
    bool    _isMoving;

    void Awake()
    {
        _cc    = GetComponent<CharacterController>();
        _cam   = GetComponentInChildren<Camera>();
        _audio = GetComponent<AudioSource>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        HandleFootsteps();

        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate body horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate camera vertically (clamped)
        _cameraPitch -= mouseY;
        _cameraPitch  = Mathf.Clamp(_cameraPitch, -maxLookAngle, maxLookAngle);
        if (_cam != null)
            _cam.transform.localEulerAngles = new Vector3(_cameraPitch, 0f, 0f);
    }

    void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool running = Input.GetKey(KeyCode.LeftShift);
        float speed  = running ? runSpeed : walkSpeed;

        Vector3 move = transform.right * h + transform.forward * v;
        _isMoving = move.magnitude > 0.1f;

        // Gravity
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += gravity * Time.deltaTime;
        move.y = _verticalVelocity;

        _cc.Move(move * speed * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        if (!_isMoving || !_cc.isGrounded || _audio == null || footstepClips == null || footstepClips.Length == 0)
        {
            _footstepTimer = 0f;
            return;
        }

        _footstepTimer += Time.deltaTime;
        if (_footstepTimer >= footstepInterval)
        {
            _footstepTimer = 0f;
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            _audio.PlayOneShot(clip, 0.6f);
        }
    }
}
