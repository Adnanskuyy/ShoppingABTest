using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 100.0f;
    private float xRotation = 0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    private bool canMoveAndLook = true;
    private Vector3 playerVelocity;
    private bool isGrounded;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Subscribe to the event. This is the core of our fix.
        GameEvents.onSetPlayerMovement += SetPlayerMovement;

        // Initial state
        SetPlayerMovement(true);
    }

    private void OnDestroy()
    {
        // Always unsubscribe
        GameEvents.onSetPlayerMovement -= SetPlayerMovement;
    }

    private void SetPlayerMovement(bool canMove)
    {
        canMoveAndLook = canMove;

        if (canMoveAndLook)
        {
            // --- RETURN TO GAME ---
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // --- NEW FIX (for WebGL) ---
            // Tell Unity to RE-CAPTURE all keyboard input for gameplay
#if !UNITY_EDITOR && UNITY_WEBGL
                WebGLInput.captureAllKeyboardInput = true;
#endif
        }
        else
        {
            // --- FREEZE FOR UI ---
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // --- NEW FIX (for WebGL) ---
            // Tell Unity to RELEASE keyboard input so the browser can handle
            // keys like Ctrl+C for copy/paste.
#if !UNITY_EDITOR && UNITY_WEBGL
                WebGLInput.captureAllKeyboardInput = false;
#endif
        }
    }

    void Update()
    {
        // Only run movement/look logic if we are allowed to
        if (!canMoveAndLook)
        {
            return;
        }

        // --- Mouse Look ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- Movement ---
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f; // A small negative value helps stick to ground
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // --- Gravity ---
        playerVelocity.y += gravityValue * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }
}
