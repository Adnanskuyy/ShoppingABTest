using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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

    [Header("Cart Display UI")]
    [SerializeField] private TextMeshProUGUI cartContentsText;

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

    public void UpdateCartDisplay(Dictionary<string, int> cartItems)
    {
        string cartText = "Cart:\n";
        if (cartItems.Count == 0)
        {
            cartText += "Empty";
        }
        else
        {
            foreach (var item in cartItems)
            {
                cartText += $"{item.Key}: {item.Value}\n";
            }
        }
        cartContentsText.text = cartText;
    }
}

