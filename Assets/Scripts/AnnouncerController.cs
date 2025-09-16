using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using DG.Tweening;

using Unity.Netcode;

public class AnnouncerController : MonoBehaviour
{
    [SerializeField] RTLTextMeshPro NameOfTheRound;
    [SerializeField] RTLTextMeshPro TruthScore;
    [SerializeField] RTLTextMeshPro LieScore;

    [SerializeField] RectTransform NameOfTheRoundRECT;
    [SerializeField] RectTransform LieScoreRECT;
    [SerializeField] RectTransform TruthScoreRECT;

    [Header("Elements Positions")]
    Vector2 NameOfTheRound_OriginalPos;
    Vector2 TruthScore_OriginalPos;
    Vector2 LieScore_OriginalPos;

    [SerializeField] Button StartButton;

    void Awake() 
    {
        NameOfTheRound_OriginalPos = NameOfTheRoundRECT.localPosition;
        TruthScore_OriginalPos = LieScoreRECT.localPosition;
        LieScore_OriginalPos = TruthScoreRECT.localPosition;
    }

    void Start() 
    {
        
    }

    void OnEnable() 
    {
        Debug.Log("AnnouncerPanel Init()");
        Init();
    }

    void OnDisable() 
    {
        ResetElementsPos();
        
        // StartButton.onClick.RemoveListener(GamePlayManager.Instance.HideAnnouncerPanel);
    }

    void Init()
    {
        NameOfTheRound.text = GamePlayManager.Instance.roundManager.GetRoundName();
        TruthScore.text = GamePlayManager.Instance.roundManager.GetCorrectScore().ToString();
        LieScore.text = GamePlayManager.Instance.roundManager.GetLieScore().ToString();

        StartCoroutine(ShowPanelElements());

        if(GamePlayManager.Instance.roundManager.canplayaduio)
        {
            GamePlayManager.Instance.roundManager.canplayaduio = false;
        }
            
    }

    public IEnumerator ShowPanelElements()
    {
        switch (RoundManager.Instance.GetRoundCount())
        {
            case 1:
            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.RoundNames1);

            NameOfTheRoundRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.RoundNames1.length);

            //----------------------------------------------------------------------------//

            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.LieScore1);

            LieScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.LieScore1.length);

            //----------------------------------------------------------------------------//

            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.TruthScore1);

            TruthScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.TruthScore1.length + 0.5f);

            //----------------------------------------------------------------------------//

            GamePlayManager.Instance.state.Value = GamePlayManager.State.AddingAnswers;

            break;

            case 2:
            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.RoundNames2);

            NameOfTheRoundRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.RoundNames2.length);

            //----------------------------------------------------------------------------//

            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.LieScore2);

            LieScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.LieScore2.length);

            //----------------------------------------------------------------------------//

            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.TruthScore2);

            TruthScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.TruthScore2.length + 0.5f);

            //----------------------------------------------------------------------------//

            GamePlayManager.Instance.state.Value = GamePlayManager.State.AddingAnswers;

            break;

            case 3:
            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.RoundNames3);

            NameOfTheRoundRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.RoundNames3.length);

            //----------------------------------------------------------------------------//

            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.LieScore3);

            LieScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.LieScore3.length);

            //----------------------------------------------------------------------------//

            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.TruthScore3);

            TruthScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);

            yield return new WaitForSeconds(AudioManager.Instance.TruthScore3.length + 0.5f);

            //----------------------------------------------------------------------------//

            GamePlayManager.Instance.state.Value = GamePlayManager.State.AddingAnswers;

            break;
        }


        // if(GamePlayManager.Instance.roundManager.GetRoundCount() == 1)
        //     AudioManager.Instance.PlayAudioClip(AudioManager.Instance.RoundNames1);

        // NameOfTheRoundRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);


        // yield return new WaitForSeconds(AudioManager.Instance.RoundNames1.length);
        

        // if(GamePlayManager.Instance.roundManager.GetRoundCount() == 1)
        //     AudioManager.Instance.PlayAudioClip(AudioManager.Instance.LieScore1);

        // LieScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);


        // yield return new WaitForSeconds(AudioManager.Instance.LieScore1.length);

        // if(GamePlayManager.Instance.roundManager.GetRoundCount() == 1)
        //     AudioManager.Instance.PlayAudioClip(AudioManager.Instance.TruthScore1);

        // TruthScoreRECT.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic);


        // yield return new WaitForSeconds(AudioManager.Instance.TruthScore1.length + 0.5f);


        // // StartButton.onClick.AddListener(GamePlayManager.Instance.AnnounserOnClick);

        // // Debug.Log($"IsHost : {IsHost}");

        // // if (IsHost)
        //     GamePlayManager.Instance.state.Value = GamePlayManager.State.AddingAnswers;
        
        // // AudioManager.Instance.PlayAudioClip(BackEndManager.Instance.CurrentQuestionAudioClip);
    }

    public void ResetElementsPos()
    {
        NameOfTheRoundRECT.localPosition = NameOfTheRound_OriginalPos;
        LieScoreRECT.localPosition = TruthScore_OriginalPos;
        TruthScoreRECT.localPosition = LieScore_OriginalPos;
    }
}
