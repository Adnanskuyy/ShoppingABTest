using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    private Camera playerCamera;
    private IInteractable currentInteractable;

    // --- THIS IS THE FIX ---
    // You were using this variable, but it was never declared.
    // Declaring it here makes it available to the whole class.
    private GameObject currentInteractableGameObject;
    // --- END FIX ---

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        InputManager.Instance.onInteractPressed += TryInteract;
    }

    private void OnDestroy()
    {
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
            if (hitInfo.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                if (interactable != currentInteractable)
                {
                    currentInteractable = interactable;
                    currentInteractableGameObject = hitInfo.collider.gameObject; // This line will now work
                    GameEvents.TriggerShowInteractionPrompt(hitInfo.collider.gameObject, currentInteractable.InteractionPrompt);
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable = null;
            currentInteractableGameObject = null; // It's also good practice to clear it here
            GameEvents.TriggerHideInteractionPrompt();
        }
    }

    private void TryInteract()
    {
        if (currentInteractable != null)
        {
            bool interacted = currentInteractable.Interact(this);

            if (interacted && currentInteractable is Product)
            {
                // This line will now also work
                GameEvents.TriggerShowProductPanel((Product)currentInteractable, currentInteractableGameObject);
            }
        }
    }
}