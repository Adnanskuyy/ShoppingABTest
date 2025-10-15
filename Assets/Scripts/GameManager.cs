using UnityEngine;
using ByteBrewSDK;

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

    [Header("EDITOR ONLY: A/B Test Override")]
    [Tooltip("In the editor, ByteBrew always returns the default value. Use this to force a variant for testing.")]
    [SerializeField] private EditorTestVariant editorVariantOverride = EditorTestVariant.UseByteBrewDefault;
    public enum EditorTestVariant { UseByteBrewDefault, ForceVariant_A_Trolley, ForceVariant_B_NoTrolley }

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
        // SetupABTest();
    }

    void Start()
    {
        cart.onCartUpdated += UIManager.Instance.UpdateCartDisplay;
        UIManager.Instance.UpdateCartDisplay(cart.items);
        UIManager.Instance.UpdateCartDisplay(cart.GetItems());

        // AnalyticsManager.Instance.Initialize();

        ByteBrewSDK.ByteBrew.RemoteConfigsUpdated(() =>
        {
            Debug.Log("ByteBrew Remote Configs have been updated. Setting up A/B Test.");
            // SetupABTest();
        });
#if !UNITY_EDITOR
            ByteBrewSDK.ByteBrew.InitializeByteBrew();
#endif
    }

    //     private void SetupABTest()
    //     {
    //         string variantValue = ByteBrewSDK.ByteBrew.GetRemoteConfigForKey("TrolleyTest", "B");
    //         currentVariant = (variantValue == "A") ? Variant.A_Trolley : Variant.B_NoTrolley;

    // #if UNITY_EDITOR
    //         // In the Editor, we use our override dropdown.
    //         if (editorVariantOverride == EditorTestVariant.ForceVariant_A_Trolley)
    //         {
    //             variantValue = "A";
    //         }
    //         else if (editorVariantOverride == EditorTestVariant.ForceVariant_B_NoTrolley)
    //         {
    //             variantValue = "B";
    //         }
    //         else // UseByteBrewDefault
    //         {
    //             // This simulates the real build behavior of getting the default value.
    //             variantValue = ByteBrew.GetRemoteConfigForKey("TrolleyTest", "B");
    //         }
    // #else
    //             // In a real build, this is the only code that runs.
    //             variantValue = ByteBrewSDK.ByteBrew.GetRemoteConfigForKey("TrolleyTest", "B");
    // #endif
    //         if (variantValue == "A")
    //         {
    //             currentVariant = Variant.A_Trolley;
    //             if (shoppingTrolleyObject != null) shoppingTrolleyObject.SetActive(true);
    //             Debug.Log("GameManager Setup: Variant A (With Trolley)");
    //         }
    //         else
    //         {
    //             currentVariant = Variant.B_NoTrolley;
    //             if (shoppingTrolleyObject != null) shoppingTrolleyObject.SetActive(false);
    //             Debug.Log("GameManager Setup: Variant B (No Trolley)");
    //         }

    //         if (shoppingTrolleyObject != null)
    //         {
    //             shoppingTrolleyObject.SetActive(currentVariant == Variant.A_Trolley);
    //         }

    //         AnalyticsManager.Instance.SendSessionStartEvent(currentVariant);
    //     }

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
