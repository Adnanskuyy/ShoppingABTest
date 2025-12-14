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
    // [SerializeField] private GameObject cartHintPanel; // Parent GameObject (NEW)
    [SerializeField] private GameObject gameGuidePanel; // Parent GameObject (NEW)
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Cart Panel UI")]
    [SerializeField] private GameObject cartPanel; // The main parent panel GameObject
    [SerializeField] private Button closeCartButton; // The 'X' button on the cart panel
    [SerializeField] private Transform cartItemContainer; // The parent object with VerticalLayoutGroup
    [SerializeField] private GameObject cartItemPrefab; // The TextMeshPro prefab for list items
    [SerializeField] private TextMeshProUGUI totalItemsText;

    [Header("Confirmation Panel UI")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button confirmEndButton;
    [SerializeField] private Button cancelEndButton;

    [Header("End Screen UI")]
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private TextMeshProUGUI endScreenInstructions;
    [SerializeField] private TMP_InputField finalCodeInput;

    [Header("Instruction Panel")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private Button startGameButton;
    private Coroutine notificationCoroutine;
    private Product currentProductForPanel;
    private GameObject currentProductGameObject;

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
        InputManager.Instance.onCancelPressed += HideProductPanel;
        InputManager.Instance.onToggleCartPressed += ToggleCartPanel;
        GameManager.Instance.onExperimentEnded += ShowEndScreen;
        GameEvents.onUpdateTimer += UpdateTimerText;

        // Set up button listeners
        if (addToCartButton != null) addToCartButton.onClick.AddListener(OnAddToCartClicked);
        if (closeButton != null) closeButton.onClick.AddListener(HideProductPanel);
        if (closeCartButton != null) closeCartButton.onClick.AddListener(HideCartPanel);

        // if (finishShoppingButton != null) finishShoppingButton.onClick.AddListener(OnFinishShoppingClicked);
        if (confirmEndButton != null) confirmEndButton.onClick.AddListener(OnConfirmEndClicked);
        if (cancelEndButton != null) cancelEndButton.onClick.AddListener(OnCancelEndClicked);

        // Initial UI State
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (productPanel != null) productPanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (cartPanel != null) cartPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (endScreenPanel != null) endScreenPanel.SetActive(false);

        // Activate persistent HUD elements as per client request
        // if (cartHintPanel != null) cartHintPanel.SetActive(true);
        if (gameGuidePanel != null) gameGuidePanel.SetActive(true);

        if (gameGuidePanel != null) gameGuidePanel.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(true);

        if (GameManager.Instance != null && GameManager.Instance.cart != null)
        {
            UpdateCartDisplay(GameManager.Instance.cart.GetItems());
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameEvents.onShowInteractionPrompt -= ShowInteractionPrompt;
        GameEvents.onHideInteractionPrompt -= HideInteractionPrompt;
        GameEvents.onShowProductPanel -= ShowProductPanel;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.onExperimentEnded -= ShowEndScreen;
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.onCancelPressed -= HideProductPanel;
            InputManager.Instance.onToggleCartPressed -= ToggleCartPanel;
        }
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

    private void ShowProductPanel(Product product, GameObject productGO)
    {
        HideInteractionPrompt();

        currentProductForPanel = product;
        currentProductGameObject = productGO; // Store the GameObject
        productPanel.SetActive(true);

        productNameText.text = product.productName;

        GameEvents.TriggerSetPlayerMovement(false);
    }

    private void HideProductPanel()
    {
        if (!productPanel.activeSelf) return; // Only hide if it's currently showing

        productPanel.SetActive(false);

        // Unfreeze player and hide cursor
        GameEvents.TriggerSetPlayerMovement(true);
    }

    private void OnAddToCartClicked()
    {
        if (currentProductForPanel != null && currentProductGameObject != null)
        {
            // 1. Tell GameManager to start the animation
            GameManager.Instance.StartFlyingItemAnimation(currentProductForPanel, currentProductGameObject);

            // 2. Hide this panel
            // (We no longer call AddItemToCart here, the animation will do it)
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
        // 1. Update the Visual List (This part was already working)
        if (cartPanel != null && cartItemContainer != null && cartItemPrefab != null)
        {
            // Clear previous items
            foreach (Transform child in cartItemContainer)
            {
                Destroy(child.gameObject);
            }

            // Instantiate new items
            if (cartPanel.activeSelf)
            {
                if (cartItems.Count == 0)
                {
                    GameObject emptyMsg = Instantiate(cartItemPrefab, cartItemContainer);
                    if (emptyMsg.TryGetComponent<TextMeshProUGUI>(out var textComponent))
                    {
                        textComponent.text = "Cart is empty";
                        textComponent.fontStyle = FontStyles.Italic;
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

        // --- THE FIX: Update the Total Text HERE ---
        if (totalItemsText != null)
        {
            // Recalculate the total from the dictionary directly
            int totalCount = 0;
            foreach (int qty in cartItems.Values)
            {
                totalCount += qty;
            }

            // Update the text label
            totalItemsText.text = $"Total Items: {totalCount}";
        }
    }

    private void ToggleCartPanel()
    {
        if (cartPanel == null) return;

        bool isOpening = !cartPanel.activeSelf;
        cartPanel.SetActive(isOpening);

        if (isOpening)
        {
            if (productPanel.activeSelf) HideProductPanel();
            HideInteractionPrompt();
            UpdateCartDisplay(GameManager.Instance.cart.GetItems());

            if (totalItemsText != null)
            {
                int totalCount = GameManager.Instance.cart.GetTotalItemCount();
                totalItemsText.text = $"Total Items: {totalCount}";
            }

            GameEvents.TriggerSetPlayerMovement(false);
        }
        else
        {
            GameEvents.TriggerSetPlayerMovement(true);
        }
    }

    private void HideCartPanel() // Called by the 'X' button
    {
        if (cartPanel != null && cartPanel.activeSelf)
        {
            ToggleCartPanel(); // Just call the toggle function to close it
        }
    }

    private void OnFinishShoppingClicked()
    {
        GameManager.Instance.RequestEndExperiment();
    }

    public void ShowConfirmationPanel(bool show)
    {
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(show);
            if (show)
            {
                GameEvents.TriggerSetPlayerMovement(false);
            }
            else
            {
                GameEvents.TriggerSetPlayerMovement(true);
            }
        }
    }

    private void OnConfirmEndClicked()
    {
        ShowConfirmationPanel(false); // Hide this panel
        GameManager.Instance.ConfirmEndExperiment();
    }

    private void OnCancelEndClicked()
    {
        GameManager.Instance.CancelEndExperiment();
    }

    public void ShowEndScreen(string finalCode)
    {
        // Hide ALL other UI
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (productPanel != null) productPanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);
        if (cartPanel != null) cartPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (gameGuidePanel != null) gameGuidePanel.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);

        // Show the final screen
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);

            // Set the instruction text
            if (endScreenInstructions != null)
            {
                endScreenInstructions.text = "Thank you for your participation.\n\nPlease copy this completion code and paste it into the survey:";
            }

            // Set the Read-Only Input Field's text
            if (finalCodeInput != null)
            {
                if (finalCodeInput.TryGetComponent<CodeEnforcer>(out var enforcer))
                {
                    enforcer.SetFinalCode(finalCode);
                }
                else
                {
                    // Fallback if script is missing
                    finalCodeInput.text = finalCode;
                }
            }
        }

        // This event will now also show the cursor.
        GameEvents.TriggerSetPlayerMovement(false);
    }

    private void UpdateTimerText(float timeLeft)
    {
        if (timerText == null) return;

        // Ensure time doesn't go below zero
        if (timeLeft < 0) timeLeft = 0;

        // Format the time into MM:SS
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// Called by the StartButton on the InstructionPanel.
    /// </summary>
    private void OnStartGameClicked()
    {
        // 1. Hide this instruction panel
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }

        // 2. Show the main game HUD
        if (gameGuidePanel != null) gameGuidePanel.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);

        // 3. Tell the GameManager to un-pause and start the timer
        GameManager.Instance.StartGame();
    }
}

