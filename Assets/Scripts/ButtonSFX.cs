using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSFX : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        //Play Button Audios..
        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        //------------
    }
}
