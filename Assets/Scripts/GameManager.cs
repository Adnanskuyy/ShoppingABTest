using System;
using System.Text;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("BUILD CONFIGURATION")]
    [Tooltip("Set this to 'A' for the Trolley/Arm build, or 'B' for the No Trolley/Arm build. THIS MUST BE SET BEFORE BUILDING.")]
    [SerializeField] private Variant thisBuildsVariant;

    [Header("Variant Setup")]
    [SerializeField] private GameObject trolleyObject; // The arm/basket ViewModel rig

    [Header("Experiment Settings")]
    [Tooltip("The maximum time for the experiment in seconds. (e.g., 300 = 5 minutes)")]
    [SerializeField] private float maxExperimentDuration = 300f;

    [Header("Animation Settings")]
    [Tooltip("Drag the 'Hand' or 'Basket' bone/transform from your ViewModel_Rig here. This is where the item will fly to.")]
    [SerializeField] private Transform basketTarget;
    [Tooltip("How long the 'fly to cart' animation should take in seconds.")]
    [SerializeField] private float flyAnimationDuration = 0.75f;

    // This event fires when the experiment ends, passing the final code.
    public event Action<string> onExperimentEnded;

    // --- Public Properties ---
    public ShoppingCart cart { get; private set; }
    public Product lastAddedProduct { get; private set; }
    public Variant currentVariant { get; private set; }
    public enum Variant { A_Trolley, B_NoTrolley }
    public string ParticipantID { get; private set; }

    // --- Private State ---
    private bool isExperimentOver = false;
    private float timeElapsed = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- REFACTORED ---
        // 1. Create the cart
        cart = new ShoppingCart();
        // 2. Generate the random ID for this session
        ParticipantID = GenerateRandomID(6);
        // 3. Set the variant based on the Inspector setting
        SetupABTest();
    }

    private void Start()
    {
        // Subscribe to events
        cart.onCartUpdated += UIManager.Instance.UpdateCartDisplay;
        InputManager.Instance.onFinishPressed += RequestEndExperiment;

        // Initialize the first UI state
        UIManager.Instance.UpdateCartDisplay(cart.GetItems());

        // 5. Start the master timer
        StartCoroutine(ExperimentTimer());
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.onFinishPressed -= RequestEndExperiment;
        }
        if (cart != null)
        {
            cart.onCartUpdated -= UIManager.Instance.UpdateCartDisplay;
        }
    }

    /// <summary>
    /// This now correctly uses the 'thisBuildsVariant' to set the game state.
    /// </summary>
    private void SetupABTest()
    {
        // --- REFACTORED ---
        // This was missing from your script.
        currentVariant = thisBuildsVariant; // Set the variant for the whole game

        if (trolleyObject != null)
        {
            // Show/hide the trolley object based on the variant
            trolleyObject.SetActive(currentVariant == Variant.A_Trolley);
        }
        Debug.Log($"GameManager: Set up game for Variant {currentVariant}");
    }

    /// <summary>
    /// Called by the UIManager when "Add to Cart" is clicked.
    /// This starts the whole animation sequence.
    /// </summary>
    public void StartFlyingItemAnimation(Product product, GameObject originalObject)
    {
        if (isExperimentOver) return;

        // Freeze the player (this is your brilliant idea)
        GameEvents.TriggerSetPlayerMovement(false);

        // Find start and end points
        Vector3 startPos = originalObject.transform.position;
        Vector3 endPos = basketTarget.position;

        // Create the duplicate
        GameObject duplicate = Instantiate(originalObject, startPos, originalObject.transform.rotation);
        // Make sure the duplicate doesn't have the Product script or it will be interactable
        if (duplicate.TryGetComponent<Product>(out var p)) { Destroy(p); }
        if (duplicate.TryGetComponent<Collider>(out var c)) { c.enabled = false; }


        // Hide the original shelf item
        originalObject.SetActive(false);

        // Start the animation coroutine
        StartCoroutine(FlyItemCoroutine(duplicate, endPos, product, originalObject));
    }

    /// <summary>
    // Animates the item from the shelf to the basket.
    /// </summary>
    private IEnumerator FlyItemCoroutine(GameObject itemToFly, Vector3 targetPosition, Product productToAdd, GameObject originalObject)
    {
        // --- THE "CHEAT" ---
        // Temporarily tell the Main Camera to *also* see the ViewModel layer,
        // so it can see both the item and the basket.
        Camera.main.cullingMask |= (1 << LayerMask.NameToLayer("ViewModel"));

        float
        elapsed = 0f;
        Vector3 startPos = itemToFly.transform.position;
        Vector3 startScale = itemToFly.transform.localScale;

        while (elapsed < flyAnimationDuration)
        {
            float t = elapsed / flyAnimationDuration;
            // Ease-in-out curve for smooth motion
            t = t * t * (3f - 2f * t);

            // Animate position
            itemToFly.transform.position = Vector3.Lerp(startPos, targetPosition, t);

            // Animate scale (shrink to 0)
            itemToFly.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // --- Animation Finished ---
        Destroy(itemToFly); // Clean up the duplicate

        // --- This is the logic from the old AddItemToCart() method ---
        lastAddedProduct = productToAdd;
        cart.AddItem(productToAdd.productName);
        UIManager.Instance.ShowNotification(productToAdd.productName);

        // Tell the Main Camera to STOP seeing the ViewModel layer.
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("ViewModel"));

        // Unfreeze the player
        GameEvents.TriggerSetPlayerMovement(true);
    }

    // --- All Experiment End Logic (Unchanged) ---
    // This part of your code is perfect.

    public void RequestEndExperiment()
    {
        if (isExperimentOver) return;
        UIManager.Instance.ShowConfirmationPanel(true);
    }

    public void CancelEndExperiment()
    {
        if (isExperimentOver) return;
        UIManager.Instance.ShowConfirmationPanel(false);
    }

    public void ConfirmEndExperiment()
    {
        if (isExperimentOver) return;
        TriggerExperimentEnd("User Finished");
    }

    private IEnumerator ExperimentTimer()
    {
        timeElapsed = 0f;
        while (timeElapsed < maxExperimentDuration)
        {
            if (!isExperimentOver)
            {
                timeElapsed += Time.deltaTime;
                float timeLeft = maxExperimentDuration - timeElapsed;
                GameEvents.TriggerUpdateTimer(timeLeft);
            }
            yield return null;
        }
        TriggerExperimentEnd("Timer Expired");
    }

    private void TriggerExperimentEnd(string reason)
    {
        if (isExperimentOver) return;
        isExperimentOver = true;

        StopAllCoroutines();
        GameEvents.TriggerSetPlayerMovement(false);

        string finalCode = GenerateFinalCode();
        onExperimentEnded?.Invoke(finalCode);
        Debug.Log($"Experiment ENDED. Reason: {reason}. Final Code: {finalCode}");
    }

    private string GenerateFinalCode()
    {
        string uid = this.ParticipantID;
        int items = cart.GetTotalItemCount();
        int time = Mathf.RoundToInt(timeElapsed);

        return $"{uid}-{time}-{items}";
    }

    private string GenerateRandomID(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        StringBuilder result = new StringBuilder(length);
        System.Random rand = new System.Random();
        for (int i = 0; i < length; i++)
        {
            result.Append(chars[rand.Next(chars.Length)]);
        }
        return result.ToString();
    }
}