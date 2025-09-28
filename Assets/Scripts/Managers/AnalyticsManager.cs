using UnityEngine;
using System.Collections.Generic;

// Singleton to handle all analytics calls.
// This is the ONLY script in the entire project that should know about ByteBrew.
public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }

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

    public void Initialize()
    {
        // Subscribe to the OnCartUpdated event from the ShoppingCart.
        GameManager.Instance.cart.onCartUpdated += OnCartUpdated;

        // Initialize ByteBrew SDK.
        ByteBrewSDK.ByteBrew.InitializeByteBrew();
    }

    private void OnDestroy()
    {
        // Unsubscribe
        if (GameManager.Instance != null)
        {
            GameManager.Instance.cart.onCartUpdated -= OnCartUpdated;
        }
    }

    // This method is called whenever the shopping cart changes.
    private void OnCartUpdated(Dictionary<string, int> items)
    {
        // For simplicity, we'll send an event for the last added item.
        // A more complex implementation could send the entire cart state.
        SendProductAddedEvent(GameManager.Instance.lastAddedProduct);
    }

    private void SendProductAddedEvent(Product product)
    {
        if (product == null) return;

        var eventParams = new Dictionary<string, string>
        {
            { "ProductName", product.productName },
            { "Variant", GameManager.Instance.currentVariant.ToString() }
        };
        ByteBrewSDK.ByteBrew.NewCustomEvent("Product_Added_To_Cart", eventParams);
        Debug.Log($"ANALYTICS: Sent Product_Added_To_Cart event for {product.productName}.");
    }

    public void SendSessionStartEvent(GameManager.Variant variant)
    {
        ByteBrewSDK.ByteBrew.NewCustomEvent("Session_Start", "Variant:" + variant.ToString());
        Debug.Log($"ANALYTICS: Sending Session_Start event for {variant.ToString()}");
    }
}
