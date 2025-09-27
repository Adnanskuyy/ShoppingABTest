using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt; // A simple UI text like "[E] Interact"
    [SerializeField] private GameObject interactionPanel; // The main panel with the button
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private Button addToCartButton;
    [SerializeField] private TextMeshProUGUI cartContentsText;

    private Camera playerCamera;
    private Product currentProduct;
    private PlayerController playerController; // Reference to the player controller

    // --- NEW: State Management ---
    private bool isUIVisible = false;

    private Dictionary<string, int> shoppingCart = new Dictionary<string, int>();

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        playerController = GetComponent<PlayerController>(); // Get the controller component

        interactionPanel.SetActive(false);
        interactionPrompt.SetActive(false);

        addToCartButton.onClick.AddListener(OnAddToCartClicked);
        UpdateCartDisplay();
    }

    void Update()
    {
        // If the main UI panel is visible, the only thing we check for is the 'Cancel' button.
        if (isUIVisible)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                CloseInteractionPanel();
            }
            return; // Don't do anything else
        }

        // --- Raycast Logic (happens only when not in UI mode) ---
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        Product detectedProduct = null;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer))
        {
            if (hitInfo.collider.TryGetComponent<Product>(out detectedProduct))
            {
                currentProduct = detectedProduct;
                interactionPrompt.SetActive(true); // Show the small "[E] Interact" prompt
            }
            else
            {
                currentProduct = null;
                interactionPrompt.SetActive(false);
            }
        }
        else
        {
            currentProduct = null;
            interactionPrompt.SetActive(false);
        }

        // Check for the interaction input ONLY if we are looking at a product.
        if (currentProduct != null && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            OpenInteractionPanel();
        }
    }

    private void OpenInteractionPanel()
    {
        isUIVisible = true;
        interactionPrompt.SetActive(false); // Hide the small prompt

        // Show and populate the main panel
        interactionPanel.SetActive(true);
        productNameText.text = "Product: " + currentProduct.productName;

        // --- CRITICAL: Freeze player and show cursor ---
        playerController.SetMovementAndLook(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseInteractionPanel()
    {
        isUIVisible = false;
        interactionPanel.SetActive(false);

        // --- CRITICAL: Unfreeze player and hide cursor ---
        playerController.SetMovementAndLook(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnAddToCartClicked()
    {
        if (currentProduct == null) return;

        string productName = currentProduct.productName;
        if (shoppingCart.ContainsKey(productName))
        {
            shoppingCart[productName]++;
        }
        else
        {
            shoppingCart.Add(productName, 1);
        }

        Debug.Log($"Added {productName} to cart. Total: {shoppingCart[productName]}");

        var eventParams = new Dictionary<string, string>
        {
            { "ProductName", productName },
            { "Variant", GameManager.Instance.currentVariant.ToString() }
        };
        ByteBrewSDK.ByteBrew.NewCustomEvent("Product_Added_To_Cart", eventParams);
        Debug.Log("ANALYTICS: Sent Product_Added_To_Cart event.");

        UpdateCartDisplay();

        // After adding to cart, automatically close the panel
        CloseInteractionPanel();
    }

    private void UpdateCartDisplay()
    {
        // ... existing code, no changes needed here ...
        string cartText = "Cart:\n";
        if (shoppingCart.Count == 0)
        {
            cartText += "Empty";
        }
        else
        {
            foreach (var item in shoppingCart)
            {
                cartText += $"{item.Key}: {item.Value}\n";
            }
        }
        cartContentsText.text = cartText;
    }
}