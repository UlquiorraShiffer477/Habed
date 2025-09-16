using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTLTMPro;
using UnityEngine.UI;

using DG.Tweening;

using Unity.Netcode;

using System.Linq;

public class ResultsController : MonoBehaviour, IComparer<ResultsController>
{
    public int Compare(ResultsController x, ResultsController y)
    {
        // Compare the scores in descending order
        return y.score_Int.CompareTo(x.score_Int);
    }

    public PlayerSessionInfo fullPlayerAnswer = new PlayerSessionInfo();

    [Header("Main Properties")]
    public RTLTextMeshPro PlayerName;
    public RTLTextMeshPro Score;

    public int score_Int;
    int currentScore;

    public Image PlayerProfilePic;

    public int PlayerRank;
    public RTLTextMeshPro RankText;

    //Colors For Each Rank.

    void Start() 
    {
        //playerInfo = PlayerDataManager.Instance.GetPlayerInfo();

        Init();
    }

    //----------------------Main Functions----------------------//

    public void Init()
    {
        score_Int = fullPlayerAnswer.PlayerScore;

        PlayerName.text = fullPlayerAnswer.PlayerName.ToString();
        // Score.text = score_Int.ToString();
        AnimateScoreCounter();

        // Here we assign the index of the pic to the player info object then retrive the pic.
        // PlayerProfilePic.sprite = playerInfo.PlayerProfilePic;

        // SetColors();
    }

    void AnimateScoreCounter()
    {
        DOTween.To(() => currentScore, x => currentScore = x, score_Int, 0.5f)
            .SetEase(Ease.Linear)
            .OnUpdate(UpdateScoreText);
    }

    void UpdateScoreText()
    {
        Score.text = currentScore.ToString();
        // You can perform any additional logic or updates here based on the current score
    }

    void SetColors()
    {
        // if(PlayerRank == 1)
        //     Score.color = GamePlayManager.Instance.RankingColors[0];
        // else if(PlayerRank == 2)
        //     Score.color = GamePlayManager.Instance.RankingColors[1];
        // else if(PlayerRank == 3)
        //     Score.color = GamePlayManager.Instance.RankingColors[2];
        // else
        //     Score.color = GamePlayManager.Instance.RankingColors[3];
    }
        
}
