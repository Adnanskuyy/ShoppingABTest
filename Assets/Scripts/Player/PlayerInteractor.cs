using UnityEngine;

// This script's ONLY job is to raycast and find interactable objects.
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    private Camera playerCamera;
    private IInteractable currentInteractable;

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        // Subscribe to the OnInteractPressed event from the InputManager.
        InputManager.Instance.onInteractPressed += TryInteract;
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when the object is destroyed.
        if (InputManager.Instance != null)
        {
            InputManager.Instance.onInteractPressed -= TryInteract;
        }
    }

    void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer))
        {
            // Check if the object we hit has a component that implements IInteractable.
            if (hitInfo.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                // If we are looking at a new interactable, update it.
                if (interactable != currentInteractable)
                {
                    currentInteractable = interactable;
                    // OLD (Error): GameEvents.onShowInteractionPrompt?.Invoke(...);
                    // NEW (Correct): Call the public trigger method.
                    GameEvents.TriggerShowInteractionPrompt(hitInfo.collider.gameObject, currentInteractable.InteractionPrompt);
                }
                return;
            }
        }

        // If we are not looking at anything, clear the current interactable and hide the prompt.
        if (currentInteractable != null)
        {
            currentInteractable = null;
            // OLD (Error): GameEvents.onHideInteractionPrompt?.Invoke();
            // NEW (Correct): Call the public trigger method.
            GameEvents.TriggerHideInteractionPrompt();
        }
    }

    private void TryInteract()
    {
        // When the interact button is pressed, if we are looking at something, call its Interact method.
        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }
}

