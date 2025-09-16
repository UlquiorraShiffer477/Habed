using UnityEngine;
using TMPro;
using RTLTMPro;

[RequireComponent(typeof(TMP_InputField))]
public class TMPRTLInputLink : MonoBehaviour
{
    [SerializeField]
    private RTLTextMeshPro targetRTLText;

    private TMP_InputField inputField;
    private bool isInitialized = false;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!isInitialized)
        {
            inputField = GetComponent<TMP_InputField>();
            
            // Configure the input field
            inputField.textComponent.isRightToLeftText = true;
            inputField.textComponent.alignment = TextAlignmentOptions.Right;
            
            // Add listener for text changes
            inputField.onValueChanged.AddListener(OnTextChanged);
            
            isInitialized = true;
        }
    }

    private void OnDisable()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnTextChanged);
        }
    }

    private void OnTextChanged(string newValue)
    {
        if (targetRTLText != null)
        {
            targetRTLText.text = newValue;
        }
    }

    // This makes it work in edit mode
    void Update()
    {
        if (!Application.isPlaying)
        {
            Initialize();
            if (inputField != null && targetRTLText != null && inputField.text != targetRTLText.text)
            {
                targetRTLText.text = inputField.text;
            }
        }
    }

    // Auto-setup when component is added
    void Reset()
    {
        // Try to find the RTLTextMeshPro component above this one in hierarchy
        targetRTLText = GetComponentInParent<RTLTextMeshPro>();
    }

    #if UNITY_EDITOR
    // Ensure settings are correct in editor
    void OnValidate()
    {
        Initialize();
        if (inputField != null)
        {
            inputField.textComponent.isRightToLeftText = true;
            inputField.textComponent.alignment = TextAlignmentOptions.Right;
        }
    }
    #endif
}