using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

public class AnswerShowingPanelController : MonoBehaviour
{
    public bool IsCorrectAnswer;
    public string FixedText;

    public Sprite CorrectAnswerImage;
    public GameObject Thumb;

    public RTLTextMeshPro AnswerField;
    public RTLTextMeshPro CorrectAnswerText;

    public List<RTLTextMeshPro> PlayersWhoMadeThis;


    public List<RectTransform> PlayersWhoChoseThis;
    public List<RTLTextMeshPro> PlayersWhoChoseThisText;


    public PlayerAnswerGroup FullPlayerAnswerGroup = new PlayerAnswerGroup();


    public List<RectTransform> NamesPostitions_Lies;
    public List<RectTransform> NamesPostitions_Truth;

    public List<RTLTextMeshPro> ScoreText;

    public RectTransform AnswerPosition;
    
    void OnEnable() 
    {
        Init();
    }

    public void Init()
    {
        IsCorrectAnswer = FullPlayerAnswerGroup.IsCorrect;

        if(IsCorrectAnswer)
        {
            for (int i = 0 ; i < ScoreText.Count ; i++)
            {
                ScoreText[i].text = GamePlayManager.Instance.roundManager.GetCorrectScore().ToString();
            }

            for (int i = 0; i < FullPlayerAnswerGroup.Players.Count; i++)
            {
                if(FullPlayerAnswerGroup.Players[i].ClientId != 100)
                    PlayersWhoMadeThis[i].text = FullPlayerAnswerGroup.Players[i].PlayerName;
                else 
                    PlayersWhoMadeThis[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
            {
                PlayersWhoChoseThisText[i].text = FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer[i].ToString();
            }

        }

        else
        {
            for (int i = 0 ; i < ScoreText.Count ; i++)
            {
                ScoreText[i].text = GamePlayManager.Instance.roundManager.GetLieScore().ToString();
            }

            string s;

            for (int i = 0; i < FullPlayerAnswerGroup.Players.Count; i++)
            {
                if (FullPlayerAnswerGroup.Players[i].IsDefaultAnswer)
                {
                    s = FixedText + " " + "Computer";
                    PlayersWhoMadeThis[i].text = s;
                }
                else
                {
                    PlayersWhoMadeThis[i].text = FixedText + " " + FullPlayerAnswerGroup.Players[i].PlayerName;
                }
            }
            
            for (int i = 0; i < FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
            {
                Debug.Log("index = " + i);
                PlayersWhoChoseThisText[i].text = FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer[i].ToString();

                PlayersWhoChoseThis[i].transform.localPosition = NamesPostitions_Lies[i].localPosition;
            }
        }

        AnswerField.text = FullPlayerAnswerGroup.PlayerAnswer;
    }
}
