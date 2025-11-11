using System;
using UnityEngine;

// A static class to hold all game-wide events.
// This allows for a decoupled event-driven architecture.
public static class GameEvents
{
    // --- UI Events ---
    public static event Action<GameObject, string> onShowInteractionPrompt;
    public static void TriggerShowInteractionPrompt(GameObject target, string promptText) => onShowInteractionPrompt?.Invoke(target, promptText);

    public static event Action onHideInteractionPrompt;
    public static void TriggerHideInteractionPrompt() => onHideInteractionPrompt?.Invoke();

    public static event Action<Product> onShowProductPanel;
    public static void TriggerShowProductPanel(Product product) => onShowProductPanel?.Invoke(product);

    // --- Timer Events ---
    public static event Action<float> onUpdateTimer;
    public static void TriggerUpdateTimer(float timeLeft) => onUpdateTimer?.Invoke(timeLeft);

    // --- Player Events ---
    public static event Action<bool> onSetPlayerMovement;
    public static void TriggerSetPlayerMovement(bool canMove) => onSetPlayerMovement?.Invoke(canMove);
}

