using System;
using UnityEngine;

// A static class to hold all game-wide events.
public static class GameEvents
{
    // --- UI Events ---
    public static event Action<GameObject, string> onShowInteractionPrompt;
    public static void TriggerShowInteractionPrompt(GameObject target, string promptText) => onShowInteractionPrompt?.Invoke(target, promptText);

    public static event Action onHideInteractionPrompt;
    public static void TriggerHideInteractionPrompt() => onHideInteractionPrompt?.Invoke();

    // --- UPDATED ---
    // This event now passes the Product *and* the GameObject.
    public static event Action<Product, GameObject> onShowProductPanel;
    public static void TriggerShowProductPanel(Product product, GameObject productGO) => onShowProductPanel?.Invoke(product, productGO);
    // --- END UPDATED ---

    // --- Timer Events ---
    public static event Action<float> onUpdateTimer;
    public static void TriggerUpdateTimer(float timeLeft) => onUpdateTimer?.Invoke(timeLeft);

    // --- Player Events ---
    public static event Action<bool> onSetPlayerMovement;
    public static void TriggerSetPlayerMovement(bool canMove) => onSetPlayerMovement?.Invoke(canMove);
}