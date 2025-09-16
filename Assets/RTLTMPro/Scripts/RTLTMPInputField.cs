using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System;
using RTLTMPro;

namespace RTLTMPro 
{
    [AddComponentMenu("UI/RTLTMPro - Input Field")]
    public class RTLTMPInputField : TMP_InputField 
    {
        private RTLTextMeshPro m_RTLTextComponent;
        private bool m_IsCompositionActive = false;
        protected Event m_ProcessingEvent = new Event();

        [SerializeField] protected bool preserveNumbers;
        [SerializeField] protected bool farsi = true;
        [SerializeField] protected bool fixTags = true;
        [SerializeField] protected bool forceFix;

        public bool PreserveNumbers
        {
            get { return preserveNumbers; }
            set
            {
                if (preserveNumbers != value)
                {
                    preserveNumbers = value;
                    UpdateLabel();
                }
            }
        }

        public bool Farsi
        {
            get { return farsi; }
            set
            {
                if (farsi != value)
                {
                    farsi = value;
                    UpdateLabel();
                }
            }
        }

        public bool FixTags
        {
            get { return fixTags; }
            set
            {
                if (fixTags != value)
                {
                    fixTags = value;
                    UpdateLabel();
                }
            }
        }

        public bool ForceFix
        {
            get { return forceFix; }
            set
            {
                if (forceFix != value)
                {
                    forceFix = value;
                    UpdateLabel();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            // Convert TextMeshProUGUI to RTLTextMeshPro if needed
            if (textComponent != null && !(textComponent is RTLTextMeshPro))
            {
                var rtlText = textComponent.gameObject.AddComponent<RTLTextMeshPro>();
                rtlText.text = textComponent.text;
                rtlText.fontSize = textComponent.fontSize;
                rtlText.fontStyle = textComponent.fontStyle;
                rtlText.alignment = textComponent.alignment;
                rtlText.color = textComponent.color;
                rtlText.font = textComponent.font;
                
                DestroyImmediate(textComponent);
                textComponent = rtlText;
            }

            m_RTLTextComponent = textComponent as RTLTextMeshPro;
            if (m_RTLTextComponent != null)
            {
                m_RTLTextComponent.PreserveNumbers = preserveNumbers;
                m_RTLTextComponent.Farsi = farsi;
                m_RTLTextComponent.FixTags = fixTags;
                m_RTLTextComponent.ForceFix = forceFix;
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            if (m_RTLTextComponent != null)
            {
                caretPosition = m_RTLTextComponent.text.Length;
            }
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (!isFocused)
                return;

            if (m_IsCompositionActive)
            {
                HandleComposition();
                return;
            }

            while (Event.PopEvent(m_ProcessingEvent))
            {
                if (m_ProcessingEvent.rawType == EventType.KeyDown)
                {
                    HandleKeyInput(m_ProcessingEvent);
                }
            }

            base.OnUpdateSelected(eventData);
        }

        private void HandleKeyInput(Event evt)
        {
            char c = evt.character;

            if (c == '\0')
                return;

            if (c == '\b')
            {
                if (text.Length > 0)
                {
                    text = text.Substring(0, text.Length - 1);
                }
                return;
            }

            if (characterLimit > 0 && text.Length >= characterLimit)
                return;

            if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c))
            {
                string newText = text + c;
                text = newText;
            }
        }

        private void HandleComposition()
        {
            string composition = Input.compositionString;
            if (!string.IsNullOrEmpty(composition))
            {
                string newText = text + composition;
                if (m_RTLTextComponent != null)
                {
                    m_RTLTextComponent.text = newText;
                }
            }
            m_IsCompositionActive = Input.imeCompositionMode != IMECompositionMode.Off;
        }

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (m_RTLTextComponent != null)
            {
                m_RTLTextComponent.PreserveNumbers = preserveNumbers;
                m_RTLTextComponent.Farsi = farsi;
                m_RTLTextComponent.FixTags = fixTags;
                m_RTLTextComponent.ForceFix = forceFix;
                m_RTLTextComponent.UpdateText();
            }
        }
        #endif
    }
}