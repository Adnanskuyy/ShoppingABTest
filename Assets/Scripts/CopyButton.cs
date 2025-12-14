using UnityEngine;
using TMPro;
using System.Runtime.InteropServices;

public class CopyButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_InputField codeInputField; // The box with the ID
    [SerializeField] private TextMeshProUGUI buttonLabel;   // The text on this button

    // Link to the .jslib file
    [DllImport("__Internal")]
    private static extern void CopyToClipboard(string str);

    public void OnClick_Copy()
    {
        if (codeInputField == null) return;

        string code = codeInputField.text;

        // Platform check
#if UNITY_WEBGL && !UNITY_EDITOR
            CopyToClipboard(code); // Call Javascript
#else
        GUIUtility.systemCopyBuffer = code; // Editor fallback
#endif

        // Visual Feedback
        if (buttonLabel != null) buttonLabel.text = "COPIED!";
        Debug.Log("Copied code: " + code);
    }
}