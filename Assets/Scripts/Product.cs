using UnityEngine;

// Product now implements the IInteractable interface.
public class Product : MonoBehaviour, IInteractable
{
    [field: SerializeField] public string productName { get; private set; } = "Default Product";
    [field: SerializeField] public float price { get; private set; } = 10.0f;

    // This is the implementation of the property from the IInteractable interface.
    public string InteractionPrompt => $"Press 'E' to Interact with {productName}";

    // This is the implementation of the method from the IInteractable interface.
    public bool Interact(PlayerInteractor interactor)
    {
        // OLD (Error): GameEvents.onShowProductPanel?.Invoke(this);
        // NEW (Correct): Call the public trigger method instead.
        Debug.Log($"Interaction triggered for {productName}. Firing event.");
        return true;
    }
}

