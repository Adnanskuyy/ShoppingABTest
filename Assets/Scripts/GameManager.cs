using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Make sure to include the ByteBrew SDK namespace
using ByteBrewSDK;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // This will hold the result from the A/B test
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

        // This makes sure the GameManager persists if you ever add new scenes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // --- STEP 1: INITIALIZE BYTEBREW ---
        // As per the documentation, initialize the SDK first.
        ByteBrew.InitializeByteBrew();

        // --- STEP 2: REGISTER FOR REMOTE CONFIGS CALLBACK ---
        // The documentation states we need to wait for configs to be updated.
        // This registers a function (SetupABTest) to be called ONLY when
        // ByteBrew confirms it has fetched the latest settings from the server.
        ByteBrew.RemoteConfigsUpdated(() =>
        {
            Debug.Log("ByteBrew Remote Configs have been updated. Setting up A/B Test.");
            SetupABTest();
        });
    }

    private void SetupABTest()
    {
        // --- STEP 3: GET THE A/B TEST VALUE ---
        // On your ByteBrew dashboard, you will create a Remote Config key named "TrolleyTest".
        // You will set it up as an A/B test.
        // - Variant A will have the value "A"
        // - Variant B (Control) will have the value "B"
        string variantValue = ByteBrew.GetRemoteConfigForKey("TrolleyTest", "B"); // "B" is the default if anything fails

        Debug.Log($"Received variant value from ByteBrew: {variantValue}");

        // --- STEP 4: APPLY THE VARIANT LOGIC ---
        if (variantValue == "A")
        {
            currentVariant = Variant.A_Trolley;
        }
        else
        {
            currentVariant = Variant.B_NoTrolley;
        }

        // Apply the visual change
        if (shoppingTrolleyObject != null)
        {
            shoppingTrolleyObject.SetActive(currentVariant == Variant.A_Trolley);
        }

        // --- STEP 5: SEND THE SESSION START EVENT ---
        // Now that we know our variant, we can start the session and tag it correctly.
        // We will send a custom event with a parameter.
        ByteBrew.NewCustomEvent("Session_Start", "Variant:" + currentVariant.ToString());
        Debug.Log($"ANALYTICS: Sending Session_Start event for {currentVariant.ToString()}");
    }
}

