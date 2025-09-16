using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using RTLTMPro;
using System;

using Unity.Netcode;
using Unity.Collections;


// [Serializable]
// public class AnswerDetails
// {
//     public string AnswerText;
//     public string PlayerMadeIt;
// }
public class AnswerController : MonoBehaviour
{
    // [Header("Main Data")]
    // public AnswersShowing answerDetails;
    public bool IsCorrect;
    public bool IsDefaultAnswer;

    [Header("Main Properties")]
    public RTLTextMeshPro AnswerField;
    public Sprite PickedSprite;
    public Button AnswerButton;
    public ulong[] OwnerClientID;

    

    public PlayerSessionInfo playerSessionInfo = new PlayerSessionInfo();
    public PlayerAnswerGroup playerAnswerGroup = new PlayerAnswerGroup();

    

    void Start() 
    {
        // playerSessionInfo = new PlayerSessionInfo();

        AnswerField = transform.GetChild(0).GetComponent<RTLTextMeshPro>();
        AnswerButton = GetComponent<Button>();
        AnswerButton.onClick.AddListener(() =>
        {
            SavePlayerAnswer();
        });

        // GamePlayManager.Instance.OnPlayerAnswerReady += GamePlayManager_OnPlayerAnswerReady;

        Init();
    }

    private void GamePlayManager_OnPlayerAnswerReady(object sender, EventArgs e)
    {
        // Init();
    }

    // void OnEnable() 
    // {
    //     Init();
    // }

    public void Init()
    {
        // OwnerClientID = playerSessionInfo.ClientId;
        // AnswerField.text = playerSessionInfo.PlayerAnswer;
        // IsCorrect = playerSessionInfo.IsCorrect;
        // IsDefaultAnswer = playerSessionInfo.IsDefaultAnswer;

        List<ulong> tempIDList = new List<ulong>();

        foreach (var v in playerAnswerGroup.Players)
        {
            tempIDList.Add(v.ClientId);
            // OwnerClientID.Add(v.ClientId);
            AnswerField.text = playerAnswerGroup.PlayerAnswer;
            IsDefaultAnswer = v.IsDefaultAnswer;
        }

        OwnerClientID = tempIDList.ToArray();

        IsCorrect = playerAnswerGroup.IsCorrect;

        if (IsCorrect)
        {
            AnswerField.text = RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount() - 1].questions[RoundManager.Instance.GetSubRoundCount() - 1].answer;
        }

        // if (IsDefaultAnswer)
        //     AnswerField.text = RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount()-1].questions[RoundManager.Instance.GetSubRoundCount()-1].
        //                         defaultAnswer[Convert.ToInt32(NetworkManager.Singleton.LocalClientId.ToString())];
        foreach (var v in OwnerClientID)
        {
            if (v == NetworkManager.Singleton.LocalClientId)
            {
                // this.gameObject.SetActive(false);
                AnswerButton.interactable = false;
            }
        }
    }

    public void CorrectAnswerInit()
    {
        
    }

    public void SavePlayerAnswer()
    {
        this.GetComponent<Image>().sprite = PickedSprite;

        GamePlayManager.Instance.RayCastPanel.SetActive(true);

        Debug.Log("Test Called");
        try
        {
            Debug.Log("Player Answer Saved Successfully!");
            
            GamePlayManager.Instance.SendAnswerDetailsServerRpc(OwnerClientID);
            // GamePlayManager.Instance.SetPlayerChooseReadyClientRpc(OwnerClientID);
        }
        catch
        {
            Debug.Log("Error");
        }
         
    }

    // [ServerRpc(RequireOwnership = false)]
    // void SendAnswerDetailsServerRpc(ServerRpcParams serverRpcParams = default)
    // {
    //     Debug.Log("SendAnswerDetailsServerRpc Called");
    //     SendAnswerDetailsClientRpc(serverRpcParams.Receive.SenderClientId);
    // }

    // [ClientRpc]
    // void SendAnswerDetailsClientRpc(ulong _clientID)
    // {
    //     Debug.Log("SendAnswerDetailsClientRpc Called");
    //     SendAnswerDetails(GamePlayManager.Instance.PlayersInSessionList ,_clientID);
    // }

    // public void SendAnswerDetails(List<PlayerSessionInfo> _playerSessionInfo, ulong _clientID)
    // {
    //     // // //Play Button Audios..
    //     // // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
    //     // // //------------

    //     Debug.Log("SendAnswerDetails Called");

    //     foreach (var v in _playerSessionInfo)
    //     {
    //         if (v.ClientId == _clientID)
    //         {
    //             Debug.Log("Client ID Found And Added To The List!");
    //             v.PlayersWhoChooseOwnerAnswer.Add(GamePlayManager.Instance.GetPlayersInSessionFromClientID(_clientID).PlayerName);
    //         }   
    //         else
    //         {
    //             Debug.Log("Client ID Not Found!");
    //         }
    //     }

    //     GetComponent<Image>().sprite = PickedSprite;

    //     // GamePlayManager.Instance.roomSessionManager.AddToFinalPlayerAnswersList(playerAnswer);
        

    //     // if(IsCorrect)
    //     // {
    //     //     if(GamePlayManager.Instance.roundManager.GetRoundCount() == 1)
    //     //         GamePlayManager.Instance.roomSessionManager.CQA[0].playersWhoClicked = playerAnswer.playersWhoClicked;
    //     //     else if(GamePlayManager.Instance.roundManager.GetRoundCount() == 2)
    //     //         GamePlayManager.Instance.roomSessionManager.CQA[1].playersWhoClicked = playerAnswer.playersWhoClicked;
    //     //     else if(GamePlayManager.Instance.roundManager.GetRoundCount() == 3)
    //     //         GamePlayManager.Instance.roomSessionManager.CQA[2].playersWhoClicked = playerAnswer.playersWhoClicked;
    //     // }
        
        
    //     //GamePlayManager.Instance.roomSessionManager.AddToFinalPlayerAnswersList(playerAnswer);

    //     //GamePlayManager.Instance.roomSessionManager.AddCorrectAnswerToList();
    // }


    public void SendCorrectAnswerInfo()
    {

    }
}