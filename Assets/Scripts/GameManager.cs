using UnityEngine;
using ByteBrewSDK;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Variant Setup")]
    // Drag your "ViewModel_Rig" or "Arm_Basket" object here
    [SerializeField] private GameObject trolleyObject;

    public ShoppingCart cart { get; private set; }
    public Product lastAddedProduct { get; private set; }
    public Variant currentVariant { get; private set; }
    public enum Variant { A_Trolley, B_NoTrolley }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        cart = new ShoppingCart();
    }

    private void Start()
    {
        // Subscribe to cart events
        cart.onCartUpdated += UIManager.Instance.UpdateCartDisplay;

        // --- NEW A/B TEST LOGIC ---
        // We no longer wait for ByteBrew. We ask our URL reader immediately.
        SetupABTest();
        // --- END NEW ---

        // Initialize the first UI state
        UIManager.Instance.UpdateCartDisplay(cart.GetItems());
    }

    private void SetupABTest()
    {
        // Ask the UrlParameterReader what the variant is.
        string variantValue = UrlParameterReader.Instance.Variant;

        if (variantValue.ToUpper() == "A")
        {
            currentVariant = Variant.A_Trolley;
            if (trolleyObject != null) trolleyObject.SetActive(true);
        }
        else
        {
            currentVariant = Variant.B_NoTrolley;
            if (trolleyObject != null) trolleyObject.SetActive(false);
        }

        Debug.Log($"GameManager: Set up game for Variant {currentVariant}");

        // --- NEW ANALYTICS CALL ---
        // Tell the AnalyticsManager to send the session start event,
        // passing it the data it needs.
        AnalyticsManager.Instance.SendSessionStartEvent(
            currentVariant.ToString(),
            UrlParameterReader.Instance.ParticipantID
        );
    }

    public void AddItemToCart(Product product)
    {
        if (product == null) return;

        lastAddedProduct = product;
        cart.AddItem(product.productName);
        UIManager.Instance.ShowNotification(product.productName);

        // --- NEW ANALYTICS CALL ---
        // Tell the AnalyticsManager to send the product event,
        // passing it the data it needs.
        AnalyticsManager.Instance.SendProductAddedEvent(
            product,
            UrlParameterReader.Instance.ParticipantID
        );
    }
}