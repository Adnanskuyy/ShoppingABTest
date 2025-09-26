using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This ensures the CharacterController component is automatically added to any object this script is attached to.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float lookSpeed = 2.0f;
    [SerializeField] private float lookXLimit = 45.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        // We are grounded, so recalculate move direction based on axes
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Get input from keyboard
        float curSpeedX = moveSpeed * Input.GetAxis("Vertical"); // Forward/Backward
        float curSpeedY = moveSpeed * Input.GetAxis("Horizontal"); // Left/Right

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Apply gravity.
        moveDirection.y = gravity;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (playerCamera == null) return;

        // Player rotation (left/right)
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        // Camera rotation (up/down)
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }
}
