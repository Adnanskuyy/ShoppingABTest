using System;
using System.Collections;
using UnityEngine;
using ByteBrewSDK;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Variant Setup")]
    [SerializeField] private GameObject trolleyObject;

    public ShoppingCart cart { get; private set; }
    public Product lastAddedProduct { get; private set; }
    public Variant currentVariant { get; private set; }
    public enum Variant { A_Trolley, B_NoTrolley }

    [Header("Experiment Settings")]
    [Tooltip("The maximum time for the experiment in seconds. (e.G., 300 = 5 minutes)")]
    [SerializeField] private float maxExperimentDuration = 300f;
    public event Action<string> onExperimentEnded;

    private bool isExperimentOver = false;
    private float timeElapsed = 0f;

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

        InputManager.Instance.onFinishPressed += RequestEndExperiment;

        SetupABTest();

        // Initialize the first UI state
        UIManager.Instance.UpdateCartDisplay(cart.GetItems());

        StartCoroutine(ExperimentTimer());
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.onFinishPressed -= RequestEndExperiment;
        }
    }

    private void SetupABTest()
    {
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
    }

    public void AddItemToCart(Product product)
    {
        if (isExperimentOver) return;
        lastAddedProduct = product;
        cart.AddItem(product.productName);
        UIManager.Instance.ShowNotification(product.productName);
    }

    public void RequestEndExperiment()
    {
        if (isExperimentOver) return;
        UIManager.Instance.ShowConfirmationPanel(true);
    }

    // This is called by the UIManager's "Cancel" button
    public void CancelEndExperiment()
    {
        if (isExperimentOver) return;
        UIManager.Instance.ShowConfirmationPanel(false);
    }

    // This is called by the UIManager's "Confirm" button
    public void ConfirmEndExperiment()
    {
        if (isExperimentOver) return;
        TriggerExperimentEnd("User Finished");
    }

    private IEnumerator ExperimentTimer()
    {
        timeElapsed = 0f; // Reset timer

        while (timeElapsed < maxExperimentDuration)
        {
            // Only tick the timer if the game is not paused by a UI
            if (!isExperimentOver)
            {
                timeElapsed += Time.deltaTime;
                float timeLeft = maxExperimentDuration - timeElapsed;

                // Fire the event to update the UI
                GameEvents.TriggerUpdateTimer(timeLeft);
            }
            yield return null; // Wait for the next frame
        }

        // If the loop finishes, the timer has expired
        TriggerExperimentEnd("Timer Expired");
    }

    private void TriggerExperimentEnd(string reason)
    {
        if (isExperimentOver) return;
        isExperimentOver = true;

        StopAllCoroutines(); // Stops the master timer
        GameEvents.TriggerSetPlayerMovement(false);

        // --- UPDATED: Generate the new code format ---
        string finalCode = GenerateFinalCode();

        onExperimentEnded?.Invoke(finalCode);
        Debug.Log($"Experiment ENDED. Reason: {reason}. Final Code: {finalCode}");
    }

    private string GenerateFinalCode()
    {
        string uid = UrlParameterReader.Instance.ParticipantID;
        int items = cart.GetTotalItemCount();
        int time = Mathf.RoundToInt(timeElapsed); // Get the actual time spent

        return $"{uid}-{time}-{items}";
    }
}