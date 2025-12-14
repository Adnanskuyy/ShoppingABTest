using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class CodeEnforcer : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    private TMP_InputField inputField;
    private string correctCode;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        // If user types/deletes anything, we force the code back instantly
        inputField.onValueChanged.AddListener((val) => ReforceCode());
    }

    // Call this from GameManager when game ends
    public void SetFinalCode(string code)
    {
        correctCode = code;
        inputField.text = code;
    }

    // If they try to change it, change it back
    private void ReforceCode()
    {
        if (inputField.text != correctCode)
        {
            inputField.text = correctCode;
        }
    }

    // --- THE KEYBOARD FIX ---
    // This runs when they click the box to select it.
    public void OnPointerClick(PointerEventData eventData)
    {
        DisableUnityKeyboard();
    }

    public void OnSelect(BaseEventData eventData)
    {
        DisableUnityKeyboard();
    }

    private void DisableUnityKeyboard()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        // This tells Unity: "Stop listening to keys, let the Browser handle Ctrl+C"
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }
}