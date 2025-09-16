using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RTLTMPro;
using UnityEngine.UI;
using DG.Tweening;

using Unity.Netcode;

public class FinalResultsController : MonoBehaviour
{
    public PlayerSessionInfo WinnersPlayersInfo = new PlayerSessionInfo();

    [Header("Main Properties")]
    public RTLTextMeshPro PlayerName;
    public RTLTextMeshPro Score;
    public RTLTextMeshPro Rank_Text;

    public int score_Int;
    

    void Start() 
    {
        Init();
    }

    public void Init()
    {
        if (WinnersPlayersInfo == null)
        {
            this.gameObject.SetActive(false);
            return;
        }

        score_Int = WinnersPlayersInfo.PlayerScore;

        PlayerName.text = WinnersPlayersInfo.PlayerName.ToString();

        Score.text = score_Int.ToString();

        Rank_Text.text = WinnersPlayersInfo.PlayerRank.ToString();
        
        // Here we assign the index of the pic to the player info object then retrive the pic.
        // PlayerProfilePic.sprite = playerInfo.PlayerProfilePic;

        // SetColors();
    }
}
