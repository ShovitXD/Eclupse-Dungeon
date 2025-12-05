using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;   // exposed in Inspector
    public Transform cameraTransform;     // assign your child camera here
    public float minPitch = -80f;         // clamp up/down
    public float maxPitch = 80f;

    private float pitch = 0f;             // current camera x-rotation
    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // recommended: freeze rotation so physics doesn't tilt the capsule
        rb.freezeRotation = true;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ReadMovementInput();
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void ReadMovementInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal"); // A/D
        float inputZ = Input.GetAxisRaw("Vertical");   // W/S

        moveInput = (transform.right * inputX + transform.forward * inputZ).normalized;
    }

    void HandleMovement()
    {
        Vector3 move = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Left/right – rotate the PLAYER (yaw)
        Quaternion yawRotation = Quaternion.Euler(0f, mouseX, 0f);
        rb.MoveRotation(rb.rotation * yawRotation);

        // Up/down – rotate the CAMERA (pitch)
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
