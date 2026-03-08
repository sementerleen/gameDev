using UnityEngine;

/// <summary>
/// Mouse look + head bob için Arda'nın kamera kontrolü.
/// Bu component kamera GameObject'ine eklenir; PlayerController ise parent'ta durur.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float verticalClamp = 80f;

    [Header("Head Bob")]
    [SerializeField] private float bobFrequency = 1.8f;
    [SerializeField] private float bobAmplitudeY = 0.06f;
    [SerializeField] private float bobAmplitudeX = 0.03f;
    [SerializeField] private float bobSmoothing = 8f;

    private PlayerController _player;
    private float _xRotation;
    private float _bobTimer;
    private Vector3 _bobOffset;
    private Vector3 _defaultLocalPos;

    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        _defaultLocalPos = transform.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
        HandleHeadBob();

        // ESC ile imleç serbest bırakma (editor/test için)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -verticalClamp, verticalClamp);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX);
    }

    private void HandleHeadBob()
    {
        if (_player != null && _player.IsMoving)
        {
            _bobTimer += Time.deltaTime * bobFrequency;
            _bobOffset = new Vector3(
                Mathf.Sin(_bobTimer * 2f) * bobAmplitudeX,
                Mathf.Sin(_bobTimer) * bobAmplitudeY,
                0f
            );
        }
        else
        {
            _bobTimer = 0f;
            _bobOffset = Vector3.Lerp(_bobOffset, Vector3.zero, Time.deltaTime * bobSmoothing);
        }

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _defaultLocalPos + _bobOffset,
            Time.deltaTime * bobSmoothing
        );
    }
}
