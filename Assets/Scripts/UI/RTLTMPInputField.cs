using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[AddComponentMenu("UI/TMP Input Field RTL aaa")]
public class TMPInputFieldRTL : TMP_InputField
{
    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(OnTextChanged);
    }

    private void OnDestroy()
    {
        onValueChanged.RemoveListener(OnTextChanged);
    }

    private void OnTextChanged(string newText)
    {
        UpdateRTLSettings();
    }

    private void UpdateRTLSettings()
    {
        if (textComponent is TMP_Text tmpText)
        {
            tmpText.isRightToLeftText = IsRightToLeft(text);
            tmpText.alignment = IsRightToLeft(text) ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
        }
    }

    private bool IsRightToLeft(string text)
    {
        // Add your custom logic to determine if the text is right-to-left
        // For example, you can check if the text contains any Arabic characters
        foreach (char c in text)
        {
            if (char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.LetterNumber)
            {
                return true;
            }
        }
        return false;
    }

    // This ensures updates in edit mode
    private void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateRTLSettings();
        }
    }

    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateRTLSettings();
    }
    #endif
}