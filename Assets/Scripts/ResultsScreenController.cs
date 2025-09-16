using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RTLTMPro;

public class ResultsScreenController : MonoBehaviour
{
    [SerializeField] RTLTextMeshPro Title;

    void OnEnable() 
    {   
        Title.text = "نتائج " + GamePlayManager.Instance.roundManager.GetRoundName();

        if (RoundManager.Instance.GetRoundCount() == 3)
        {
            GamePlayManager.Instance.NextQuestionButton.SetActive(true);
        }
            

        AudioManager.Instance.StopBackGroundMusice();

        AudioManager.Instance.PlayAudioClip(AudioManager.Instance.ResultsShowing);

        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.NarratorOnFinishARoundAudioClips);
    }
}
