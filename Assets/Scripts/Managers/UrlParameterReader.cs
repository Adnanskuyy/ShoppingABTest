using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Web; // Used for URL decoding

/// <summary>
/// This singleton script reads URL parameters on game start (in WebGL).
/// It provides the rest of the game with the Participant ID and the A/B test variant.
/// </summary>
public class UrlParameterReader : MonoBehaviour
{
    public static UrlParameterReader Instance { get; private set; }

    [Header("Parsed URL Data")]
    [SerializeField] private string participantID;
    [SerializeField] private string variant;
    public string ParticipantID { get => participantID; private set => participantID = value; }
    public string Variant { get => variant; private set => variant = value; }

    [Header("Editor Debug Overrides")]
    [Tooltip("This ID will be used when running in the Unity Editor.")]
    [SerializeField] private string debugParticipantID = "EDITOR_TEST_ID_001";
    [Tooltip("This variant will be used when running in the Unity Editor.")]
    [SerializeField] private string debugVariant = "A"; // "A" for trolley, "B" for no-trolley

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- PARSE THE URL ---
        ParseUrlParameters();
    }

    private void ParseUrlParameters()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        // --- This code only runs in a real WebGL build ---
        try
        {
            string url = Application.absoluteURL;
            if (url.Contains("?"))
            {
                // Get the parameter part of the URL (everything after "?")
                string parameters = url.Split('?')[1];
                string[] paramArray = parameters.Split('&');

                foreach (string param in paramArray)
                {
                    if (param.StartsWith("uid="))
                    {
                        // Get the value after "uid="
                        ParticipantID = param.Substring(4);
                        ParticipantID = HttpUtility.UrlDecode(ParticipantID); // Decode special characters
                    }
                    else if (param.StartsWith("variant="))
                    {
                        // Get the value after "variant="
                        Variant = param.Substring(8);
                        Variant = HttpUtility.UrlDecode(Variant);
                    }
                }
            }

            // --- Set defaults if parameters were not found ---
            if (string.IsNullOrEmpty(ParticipantID))
            {
                ParticipantID = "NO_UID_FOUND";
                Debug.LogWarning("UrlParameterReader: No 'uid' parameter found in URL.");
            }
            if (string.IsNullOrEmpty(Variant))
            {
                Variant = "B"; // Default to 'B' (no trolley) if variant isn't specified
                Debug.LogWarning("UrlParameterReader: No 'variant' parameter found in URL. Defaulting to 'B'.");
            }
            
            Debug.Log($"UrlParameterReader: Parsed ID '{ParticipantID}' and Variant '{Variant}' from URL.");

        }
        catch (System.Exception e)
        {
            Debug.LogError($"UrlParameterReader: Error parsing URL. {e.Message}");
            ParticipantID = "URL_PARSE_ERROR";
            Variant = "B";
        }
#else
        // --- This code only runs in the Unity Editor ---
        // We use our debug override values
        ParticipantID = debugParticipantID;
        Variant = debugVariant;
        Debug.LogWarning($"UrlParameterReader: Running in Editor. Using debug ID '{ParticipantID}' and Variant '{Variant}'.");
#endif
    }
}