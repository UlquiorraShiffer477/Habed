using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddingAnswersScreenController : MonoBehaviour
{
    void OnEnable() 
    {
        StartCoroutine(AudioManager.Instance.PlayBackgroundMusic());

        StartCoroutine(GamePlayManager.Instance.PlayQuestionAudio());
        // AudioManager.Instance.PlayAudioClip(BackEndManager.Instance.CurrentQuestionAudioClip);
    }
}
