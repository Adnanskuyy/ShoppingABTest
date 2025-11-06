using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This is the ONLY script that talks to ByteBrew.
/// It has been refactored to be a simple, "dumb" logger.
/// It doesn't know *why* events are sent, it just sends them.
/// </summary>
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

    private void Start()
    {
        // Initialize ByteBrew SDK
        // We only do this in a real build.
#if !UNITY_EDITOR && UNITY_WEBGL
        ByteBrewSDK.ByteBrew.InitializeByteBrew();
#endif
    }

    // --- This script no longer needs OnDestroy or Initialize methods ---
    // --- It no longer subscribes to any events ---

    /// <summary>
    /// Sends a "Product Added" event to ByteBrew, tagged with the participant's ID.
    /// </summary>
    public void SendProductAddedEvent(Product product, string participantID)
    {
        if (product == null) return;

        var eventParams = new Dictionary<string, string>
        {
            { "ProductName", product.productName },
            { "qualtrics_id", participantID } // Add the Qualtrics ID
        };

        // Note: We no longer need to send the "Variant" in this event,
        // because the Session_Start event has already tagged this entire session
        // with that user's variant. It's redundant data.

        ByteBrewSDK.ByteBrew.NewCustomEvent("Product_Added_To_Cart", eventParams);
        Debug.Log($"ANALYTICS: Sent Product_Added_To_Cart for {product.productName} (ID: {participantID})");
    }

    /// <summary>
    /// Sends a "Session Start" event, tagging the session with the participant's ID and variant.
    /// </summary>
    public void SendSessionStartEvent(string variant, string participantID)
    {
        var eventParams = new Dictionary<string, string>
        {
            { "Variant", variant },
            { "qualtrics_id", participantID } // Add the Qualtrics ID
        };

        ByteBrewSDK.ByteBrew.NewCustomEvent("Session_Start", eventParams);
        Debug.Log($"ANALYTICS: Sent Session_Start for Variant {variant} (ID: {participantID})");
    }
}