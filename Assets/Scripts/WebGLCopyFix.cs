using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WebGLCopyFix : MonoBehaviour, ISelectHandler, IPointerClickHandler
{
    // Triggered when the user clicks the box
    public void OnPointerClick(PointerEventData eventData)
    {
        ReleaseKeyboard();
    }

    // Triggered when the box gains focus (e.g. via Tab key)
    public void OnSelect(BaseEventData eventData)
    {
        ReleaseKeyboard();
    }

    private void ReleaseKeyboard()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        // Force Unity to release the keyboard so the Browser can hear 'Ctrl+C'
        WebGLInput.captureAllKeyboardInput = false;
        Debug.Log("Keyboard Capture Released for Copying");
#endif
    }
}
