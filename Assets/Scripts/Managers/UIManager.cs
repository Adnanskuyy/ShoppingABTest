using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactionPrompt; // Parent GameObject with Panel bg
    [SerializeField] private TextMeshProUGUI interactionPromptText; // Child TextMeshPro
    [SerializeField] private GameObject productPanel; // Parent GameObject with Panel bg
    [SerializeField] private TextMeshProUGUI productNameText; // Child TextMeshPro
    [SerializeField] private Button addToCartButton; // Child Button
    [SerializeField] private Button closeButton; // Child Button (NEW)

    [Header("HUD Elements")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private float notificationDuration = 2.5f;
    [SerializeField] private GameObject cartHintPanel; // Parent GameObject (NEW)
    [SerializeField] private GameObject gameGuidePanel; // Parent GameObject (NEW)

    [Header("Cart Panel UI")]
    [SerializeField] private GameObject cartPanel; // The main parent panel GameObject
    [SerializeField] private Button closeCartButton; // The 'X' button on the cart panel
    [SerializeField] private Transform cartItemContainer; // The parent object with VerticalLayoutGroup
    [SerializeField] private GameObject cartItemPrefab; // The TextMeshPro prefab for list items
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
        InputManager.Instance.onToggleCartPressed += ToggleCartPanel;

        // Set up button listeners
        addToCartButton.onClick.AddListener(OnAddToCartClicked);
        closeButton.onClick.AddListener(HideProductPanel); // 'X' button closes panel
        if (closeCartButton != null) closeCartButton.onClick.AddListener(HideCartPanel);

        // Initial UI State
        interactionPrompt.SetActive(false);
        productPanel.SetActive(false);
        notificationPanel.SetActive(false);
        if (cartPanel != null) cartPanel.SetActive(false);
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
            InputManager.Instance.onToggleCartPressed -= ToggleCartPanel;
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
        // 2. Update Cart Panel List (Only if the panel and container exist)
        if (cartPanel != null && cartItemContainer != null && cartItemPrefab != null)
        {
            // Clear previous items
            foreach (Transform child in cartItemContainer)
            {
                Destroy(child.gameObject);
            }

            // Instantiate new items if the cart panel is currently active
            if (cartPanel.activeSelf)
            {
                if (cartItems.Count == 0)
                {
                    // Optional: Display a "Cart is empty" message
                    GameObject emptyMsg = Instantiate(cartItemPrefab, cartItemContainer);
                    if (emptyMsg.TryGetComponent<TextMeshProUGUI>(out var textComponent))
                    {
                        textComponent.text = "Cart is empty";
                        textComponent.fontStyle = FontStyles.Italic; // Make it look different
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, int> pair in cartItems)
                    {
                        GameObject newItem = Instantiate(cartItemPrefab, cartItemContainer);
                        if (newItem.TryGetComponent<TextMeshProUGUI>(out var textComponent))
                        {
                            // Display Name : Quantity
                            textComponent.text = $"{pair.Key} : {pair.Value}";
                        }
                    }
                }
            }
        }
    }

    private void ToggleCartPanel()
    {
        if (cartPanel == null) return; // Safety check

        bool isOpening = !cartPanel.activeSelf;
        cartPanel.SetActive(isOpening);

        if (isOpening)
        {
            // Update the list content WHEN opening
            UpdateCartDisplay(GameManager.Instance.cart.GetItems());

            // Freeze player and show cursor
            GameEvents.TriggerSetPlayerMovement(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Unfreeze player and hide cursor
            GameEvents.TriggerSetPlayerMovement(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HideCartPanel() // Called by the 'X' button
    {
        if (cartPanel != null && cartPanel.activeSelf)
        {
            ToggleCartPanel(); // Just call the toggle function to close it
        }
    }
}

