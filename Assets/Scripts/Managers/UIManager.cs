using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionPromptText;
    [SerializeField] private GameObject productPanel;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private Button addToCartButton;

    // [Header("Cart Display UI")]
    // [SerializeField] private TextMeshProUGUI cartContentsText;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI cubeCounterText;
    [SerializeField] private TextMeshProUGUI sphereCounterText;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2.5f;

    private Coroutine notificationCoroutine;
    private Product currentProductForPanel;

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

    void Start()
    {
        GameEvents.onShowInteractionPrompt += ShowInteractionPrompt;
        GameEvents.onHideInteractionPrompt += HideInteractionPrompt;
        GameEvents.onShowProductPanel += ShowProductPanel;
        InputManager.Instance.onCancelPressed += HideProductPanel;
        addToCartButton.onClick.AddListener(OnAddToCartClicked);

        interactionPrompt.SetActive(false);
        productPanel.SetActive(false);
        notificationPanel.SetActive(false); // Hide notification panel at start
        UpdateCartDisplay(new Dictionary<string, int>()); // Initialize counters to 0
    }

    private void OnDestroy()
    {
        GameEvents.onShowInteractionPrompt -= ShowInteractionPrompt;
        GameEvents.onHideInteractionPrompt -= HideInteractionPrompt;
        GameEvents.onShowProductPanel -= ShowProductPanel;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.onCancelPressed -= HideProductPanel;
        }
    }

    private void ShowInteractionPrompt(GameObject target, string promptText)
    {
        // Only show the prompt if the main panel isn't already open.
        if (!productPanel.activeSelf)
        {
            interactionPrompt.SetActive(true);
            interactionPromptText.text = promptText;
        }
    }

    private void HideInteractionPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    private void ShowProductPanel(Product product)
    {
        // --- BUG FIX ---
        // Explicitly hide the interaction prompt before showing the main panel.
        HideInteractionPrompt();

        currentProductForPanel = product;
        productPanel.SetActive(true);
        productNameText.text = product.productName;

        GameEvents.TriggerSetPlayerMovement(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideProductPanel()
    {
        if (!productPanel.activeSelf) return;

        productPanel.SetActive(false);

        GameEvents.TriggerSetPlayerMovement(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnAddToCartClicked()
    {
        if (currentProductForPanel != null)
        {
            GameManager.Instance.AddItemToCart(currentProductForPanel);
            HideProductPanel();
        }
    }

    public void ShowNotification(string productName)
    {
        // If a notification is already showing, stop it first.
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }
        // Start the new notification coroutine.
        notificationCoroutine = StartCoroutine(NotificationCoroutine(productName));
    }

    private IEnumerator NotificationCoroutine(string productName)
    {
        notificationPanel.SetActive(true);
        notificationText.text = $"{productName} has been added to cart.";

        // Wait for the specified duration.
        yield return new WaitForSeconds(notificationDuration);

        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }

    public void UpdateCartDisplay(Dictionary<string, int> cartItems)
    {
        // Get the count for "Cube" from the dictionary. If it doesn't exist, default to 0.
        cartItems.TryGetValue("Cube", out int cubeCount);
        cubeCounterText.text = $"Cube : {cubeCount}";

        // Get the count for "Sphere" from the dictionary. If it doesn't exist, default to 0.
        cartItems.TryGetValue("Sphere", out int sphereCount);
        sphereCounterText.text = $"Sphere : {sphereCount}";
    }
}

