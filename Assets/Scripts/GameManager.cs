using UnityEngine;

// GameManager is now correctly named and placed in the Managers folder.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // --- Public Properties ---
    // These are now correctly exposed for other scripts to access.
    public ShoppingCart cart { get; private set; }
    public Product lastAddedProduct { get; private set; }
    public enum Variant { A_Trolley, B_NoTrolley }
    public Variant currentVariant;

    [SerializeField] private GameObject shoppingTrolleyObject;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        cart = new ShoppingCart();
    }

    void Start()
    {
        cart.onCartUpdated += UIManager.Instance.UpdateCartDisplay;
        UIManager.Instance.UpdateCartDisplay(cart.items);
        UIManager.Instance.UpdateCartDisplay(cart.GetItems());

        AnalyticsManager.Instance.Initialize();

        ByteBrewSDK.ByteBrew.RemoteConfigsUpdated(() =>
        {
            Debug.Log("ByteBrew Remote Configs have been updated. Setting up A/B Test.");
            SetupABTest();
        });
    }

    private void SetupABTest()
    {
        string variantValue = ByteBrewSDK.ByteBrew.GetRemoteConfigForKey("TrolleyTest", "B");
        currentVariant = (variantValue == "A") ? Variant.A_Trolley : Variant.B_NoTrolley;

        if (shoppingTrolleyObject != null)
        {
            shoppingTrolleyObject.SetActive(currentVariant == Variant.A_Trolley);
        }

        AnalyticsManager.Instance.SendSessionStartEvent(currentVariant);
    }

    // This method is now public, so UIManager can call it.
    public void AddItemToCart(Product product)
    {
        if (product == null) return;
        lastAddedProduct = product;
        cart.AddItem(product.productName);
        UIManager.Instance.ShowNotification(product.productName);
        UIManager.Instance.UpdateCartDisplay(cart.GetItems());
    }
}
