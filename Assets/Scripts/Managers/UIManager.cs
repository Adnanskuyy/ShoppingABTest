using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro; // Make sure TextMeshPro namespace is included

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPrompt; // Parent GameObject with Panel bg
    [SerializeField] private TextMeshProUGUI interactionPromptText; // Child TextMeshPro
    [SerializeField] private GameObject productPanel; // Parent GameObject with Panel bg
    [SerializeField] private TextMeshProUGUI productNameText; // Child TextMeshPro
    [SerializeField] private TextMeshProUGUI productPriceText; // Child TextMeshPro (NEW)
    [SerializeField] private Button addToCartButton; // Child Button
    [SerializeField] private Button closeButton; // Child Button (NEW)

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI cubeCounterText;
    [SerializeField] private TextMeshProUGUI sphereCounterText;
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2.5f;
    [SerializeField] private GameObject cartHintPanel; // Parent GameObject (NEW)
    [SerializeField] private GameObject gameGuidePanel; // Parent GameObject (NEW)

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
        // Subscribe to events
        GameEvents.onShowInteractionPrompt += ShowInteractionPrompt;
        GameEvents.onHideInteractionPrompt += HideInteractionPrompt;
        GameEvents.onShowProductPanel += ShowProductPanel;
        InputManager.Instance.onCancelPressed += HideProductPanel; // 'C' key closes panel

        // Set up button listeners
        addToCartButton.onClick.AddListener(OnAddToCartClicked);
        closeButton.onClick.AddListener(HideProductPanel); // 'X' button closes panel

        // Initial UI State
        interactionPrompt.SetActive(false);
        productPanel.SetActive(false);
        notificationPanel.SetActive(false);
        UpdateCartDisplay(new Dictionary<string, int>()); // Initialize counters

        // Activate persistent HUD elements as per client request
        if (cartHintPanel != null) cartHintPanel.SetActive(true);
        if (gameGuidePanel != null) gameGuidePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameEvents.onShowInteractionPrompt -= ShowInteractionPrompt;
        GameEvents.onHideInteractionPrompt -= HideInteractionPrompt;
        GameEvents.onShowProductPanel -= ShowProductPanel;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.onCancelPressed -= HideProductPanel;
        }
        // Button listeners are automatically cleaned up
    }

    private void ShowInteractionPrompt(GameObject target, string promptText)
    {
        if (!productPanel.activeSelf)
        {
            interactionPrompt.SetActive(true);
            interactionPromptText.text = promptText; // Example: "[E] Inspect Cereal"
        }
    }

    private void HideInteractionPrompt()
    {
        interactionPrompt.SetActive(false);
    }

    private void ShowProductPanel(Product product)
    {
        HideInteractionPrompt(); // Ensure prompt is hidden

        currentProductForPanel = product;
        productPanel.SetActive(true);

        // Update panel content
        productNameText.text = product.productName;
        productPriceText.text = $"Price: ${product.price:F2}"; // Format price nicely

        // Freeze player and show cursor
        GameEvents.TriggerSetPlayerMovement(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HideProductPanel()
    {
        if (!productPanel.activeSelf) return; // Only hide if it's currently showing

        productPanel.SetActive(false);

        // Unfreeze player and hide cursor
        GameEvents.TriggerSetPlayerMovement(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnAddToCartClicked()
    {
        if (currentProductForPanel != null)
        {
            GameManager.Instance.AddItemToCart(currentProductForPanel);
            // Hiding the panel is now handled by the event system if needed,
            // or simply close it directly after adding. Let's close it.
            HideProductPanel();
        }
    }

    public void ShowNotification(string productName)
    {
        if (notificationCoroutine != null) StopCoroutine(notificationCoroutine);
        notificationCoroutine = StartCoroutine(NotificationCoroutine(productName));
    }

    private IEnumerator NotificationCoroutine(string productName)
    {
        notificationPanel.SetActive(true);
        notificationText.text = $"{productName} has been added to cart.";
        yield return new WaitForSeconds(notificationDuration);
        notificationPanel.SetActive(false);
        notificationCoroutine = null;
    }

    public void UpdateCartDisplay(Dictionary<string, int> cartItems)
    {
        cartItems.TryGetValue("Cube", out int cubeCount); // Use actual product names if they differ
        cubeCounterText.text = $"Cube : {cubeCount}";

        cartItems.TryGetValue("Sphere", out int sphereCount);
        sphereCounterText.text = $"Sphere : {sphereCount}";

        // If you add more product types, update their counters here
    }
}

