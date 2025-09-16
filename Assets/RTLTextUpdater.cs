using UnityEngine;
using TMPro;
using UnityEngine.Events;
using RTLTMPro;

public class RTLTextSynchronizer : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RTLTextMeshPro textComponent;

    private void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();

        if (textComponent == null)
            textComponent = GetComponent<RTLTextMeshPro>();

        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(OnInputFieldTextChanged);
            inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        }
    }

    private void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveListener(OnInputFieldTextChanged);
            inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
        }
    }

    private void OnInputFieldTextChanged(string newText)
    {
        UpdateRTLText(newText);
    }

    private void OnInputFieldEndEdit(string newText)
    {
        UpdateRTLText(newText);
    }

    private void UpdateRTLText(string text)
    {
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }

}