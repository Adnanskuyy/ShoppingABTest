using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6.0f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 45.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    // The critical variable that controls movement.
    private bool canMoveAndLook = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // --- DEBUGGING STEP 2: Confirm the subscription ---
        Debug.Log("<color=green>PlayerController:</color> Subscribing to SetPlayerMovement event.");
        GameEvents.onSetPlayerMovement += SetMovementAndLook;
    }

    private void OnDestroy()
    {
        GameEvents.onSetPlayerMovement -= SetMovementAndLook;
    }

    void Update()
    {
        // The Update loop now only checks the boolean.
        if (canMoveAndLook)
        {
            HandleMovement();
            HandleLook();
        }
    }

    // This method is called by the event from the UIManager.
    private void SetMovementAndLook(bool state)
    {
        // --- DEBUGGING STEP 3: Confirm the event was received ---
        Debug.Log($"<color=green>PlayerController:</color> Received SetPlayerMovement event! Setting canMoveAndLook to <color=yellow>{state}</color>.");
        canMoveAndLook = state;
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        float curSpeedX = moveSpeed * Input.GetAxis("Vertical");
        float curSpeedY = moveSpeed * Input.GetAxis("Horizontal");
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (!characterController.isGrounded)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
