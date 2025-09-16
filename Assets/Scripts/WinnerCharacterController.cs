using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using Unity.Services.Lobbies.Models;
using DG.Tweening;
using System;

using UnityEngine.SceneManagement;

using Spine.Unity.AttachmentTools;

using Unity.Netcode;

using Spine.Unity.Examples;
using Spine.Unity;

public class WinnerCharacterController : MonoBehaviour
{
    // [SerializeField] int PlayerIndex;
    // [SerializeField] RTLTextMeshPro PlayerName;
    [SerializeField] CustomizationContoller SpineUI;
    [SerializeField] bool IsMan;
    // [SerializeField] [SpineAnimation] public string PrimaryAnim;
    // [SerializeField] [SpineAnimation] public string SecondaryAnim;


    [SerializeField] Gender gender = Gender.NotSelected;


    void Start() 
    {
        InitPlayer();
    }

    // public IEnumerator Init()
    // {
    //     PlayerInfo playerInfo = RoomSessionManager.Instance.Results[0].fullPlayerAnswer.playerInfo;

    //     if(IsMan == playerInfo.IsMan)
    //     {
    //         SpineUI.SetAllCharacterSkins(playerInfo);
    //     }
    //     else
    //     {
    //         Hide();
    //     }
        

    // }

    public void InitPlayer()
    {
        PlayerInfo playerInfo = RoomSessionManager.Instance.Results[0].fullPlayerAnswer.playerInfo;

        if(gender == playerInfo.gender)
        {
            // Debug.Log("Show");
            // PlayerName.text = playerInfo.PlayerName.ToString();
            SpineUI.SetAllCharacterSkins(playerInfo);
            
            // Debug.Log($"IsMan = {playerInfo.IsMan}");
            Show();

            // if (MainNetworkManager.Instance.GetPlayerInfoIndexFromClientID(playerInfo.ClientId) == PlayerIndex)   
            //     SpineUI.UpdateSelectedAnimation(PrimaryAnim, SecondaryAnim, .1f);
        }
        else
        {
            Hide();
        }
    }

    void Show()
    {
        gameObject.SetActive(true);
        
        // gameObject.transform.localScale = Vector3.one;
    }
        
    void Hide()
    {
        gameObject.SetActive(false);
    }
}
