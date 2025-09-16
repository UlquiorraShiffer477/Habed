using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using RTLTMPro;
using TMPro;

public class InputFieldForScreenKeyboardPanelAdjuster : MonoBehaviour {

     // Assign panel here in order to adjust its height when TouchScreenKeyboard is shown
    public RectTransform panel;

    private TMP_InputField inputField;
    private Vector2 panelOriginalPosition;
    private float currentKeyboardHeightRatio;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        panelOriginalPosition = panel.anchoredPosition;
    }

    private void LateUpdate()
    {
        if (inputField.isFocused)
        {
            float newKeyboardHeightRatio = GetKeyboardHeightRatio();
            if (currentKeyboardHeightRatio != newKeyboardHeightRatio)
            {
                Debug.Log("InputFieldForScreenKeyboardPanelAdjuster: Adjust to keyboard height ratio: " + newKeyboardHeightRatio);
                currentKeyboardHeightRatio = newKeyboardHeightRatio;
                StartCoroutine(AdjustPanelPosition());
            }
        }
        else if (currentKeyboardHeightRatio != 0f)
        {
            StartCoroutine(ResetPanelPosition());
            currentKeyboardHeightRatio = 0f;
        }
    }

    private IEnumerator AdjustPanelPosition()
    {
        yield return new WaitForSeconds(0.1f); // Adjust this delay as needed

        float yOffset = currentKeyboardHeightRatio * Screen.height;
        panel.anchoredPosition = new Vector2(panelOriginalPosition.x, yOffset);
    }

    private IEnumerator ResetPanelPosition()
    {
        yield return new WaitForSeconds(0.1f); // Adjust this delay as needed

        Debug.Log("InputFieldForScreenKeyboardPanelAdjuster: Revert to original");
        panel.anchoredPosition = panelOriginalPosition;
    }

    private float GetKeyboardHeightRatio()
    {
        if (Application.isEditor)
        {
            return 0.4f; // fake TouchScreenKeyboard height ratio for debug in the editor
        }

#if UNITY_ANDROID
        using (AndroidJavaClass UnityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject View = UnityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
            using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                View.Call("getWindowVisibleDisplayFrame", rect);
                return (float)(Screen.height - rect.Call<int>("height")) / Screen.height;
            }
        }
#else
        return (float)TouchScreenKeyboard.area.height / Screen.height;
#endif
    }
}

