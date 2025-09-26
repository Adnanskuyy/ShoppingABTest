using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using System.Collections.Generic; // Required for lists
using TMPro; // Required for TextMeshPro text elements

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionDistance = 5f;
    [SerializeField] private LayerMask interactableLayer; // Set this to the layer your products are on

    [Header("UI References")]
    [SerializeField] private GameObject interactionPanel; // A panel that holds the button and text
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private Button addToCartButton;

    [Header("Cart Display UI")]
    [SerializeField] private TextMeshProUGUI cartContentsText;

    private Camera playerCamera;
    private Product currentProduct;

    // A simple in-memory cart
    private Dictionary<string, int> shoppingCart = new Dictionary<string, int>();

    void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        interactionPanel.SetActive(false); // Hide UI at start
        addToCartButton.onClick.AddListener(OnAddToCartClicked); // Wire up the button click
        UpdateCartDisplay();
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, interactionDistance, interactableLayer))
        {
            // We hit a product
            Product product = hitInfo.collider.GetComponent<Product>();
            if (product != null)
            {
                currentProduct = product;
                ShowInteractionUI(product.productName);
                return; // Exit early since we found something
            }
        }

        // If we reach here, we are not looking at a product
        currentProduct = null;
        HideInteractionUI();
    }

    private void ShowInteractionUI(string name)
    {
        interactionPanel.SetActive(true);
        productNameText.text = "Product: " + name;
    }

    private void HideInteractionUI()
    {
        interactionPanel.SetActive(false);
    }

    private void OnAddToCartClicked()
    {
        if (currentProduct == null) return;

        // Add to our dictionary-based cart
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

        // --- SEND BYTEBREW EVENT ---
        // This is the critical data tracking step.
        var eventParams = new Dictionary<string, string>
        {
            { "ProductName", productName },
            { "Variant", GameManager.Instance.currentVariant.ToString() }
        };
        ByteBrewSDK.ByteBrew.NewCustomEvent("Product_Added_To_Cart", eventParams);
        Debug.Log("ANALYTICS: Sent Product_Added_To_Cart event.");

        UpdateCartDisplay();
    }

    private void UpdateCartDisplay()
    {
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
