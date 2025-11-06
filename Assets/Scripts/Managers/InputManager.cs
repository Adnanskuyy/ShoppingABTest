using UnityEngine;
using System;

// Singleton to handle all player input.
// Its only job is to detect key presses and fire events.
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // Events that other systems can subscribe to.
    public event Action onInteractPressed;
    public event Action onCancelPressed;
    public event Action onToggleCartPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Check for the interact key (E or Left Mouse)
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Fire the event if there are any listeners.
            onInteractPressed?.Invoke();
        }

        // Check for the cancel key (C)
        if (Input.GetKeyDown(KeyCode.C))
        {
            onCancelPressed?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            onToggleCartPressed?.Invoke();
        }
    }
}
