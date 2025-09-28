using System;
using UnityEngine;

// A static class to hold all game-wide events.
// This allows for a decoupled event-driven architecture.
public static class GameEvents
{
    // --- UI Events ---
    public static event Action<GameObject, string> onShowInteractionPrompt;
    // Public method to safely trigger the event from any script.
    public static void TriggerShowInteractionPrompt(GameObject target, string promptText) => onShowInteractionPrompt?.Invoke(target, promptText);

    public static event Action onHideInteractionPrompt;
    // Public method to safely trigger the event.
    public static void TriggerHideInteractionPrompt() => onHideInteractionPrompt?.Invoke();

    public static event Action<Product> onShowProductPanel;
    // Public method to safely trigger the event.
    public static void TriggerShowProductPanel(Product product) => onShowProductPanel?.Invoke(product);

    // --- Player Events ---
    public static event Action<bool> onSetPlayerMovement;
    // Public method to safely trigger the event.
    public static void TriggerSetPlayerMovement(bool canMove) => onSetPlayerMovement?.Invoke(canMove);
}

