using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System;

public class CheckMarkController : MonoBehaviour
{
    [SerializeField] int Index;

    [SerializeField] Image CheckMark;

    [SerializeField] Sprite CheckMarkInActiveSprite;
    [SerializeField] Sprite CheckMarkActiveSprite;
    

    // void Awake() 
    // {
    //     CheckMark = GetComponent<Image>();
    // }

    void Start() 
    {
        GamePlayManager.Instance.OnPlayerAnswerReady += GamePlayManager_OnPlayerAnswerReady;

        MainNetworkManager.Instance.OnPlayersInfoNetworkListChanged += GamePlayManager_OnPlayerAnswerReady;

        GamePlayManager.Instance.OnNextInstanceStarted += GamePlayManager_OnNextInstanceStarted;

        UpdateCheckMarks();
    }

    private void GamePlayManager_OnPlayerAnswerReady(object sender, EventArgs e)
    {
        UpdateCheckMarks();
    }
    private void GamePlayManager_OnNextInstanceStarted(object sender, EventArgs e)
    {
        ResetCheckMark();
        // Hide();
    }

    void UpdateCheckMarks()
    {
        if (MainNetworkManager.Instance.IsPlayerIndexConnected(Index))
        {
            Show();

            PlayerInfo playerInfo = MainNetworkManager.Instance.GetPlayerInfoFromPlayerIndex(Index);

            if (GamePlayManager.Instance.IsPlayerAnswerReady(playerInfo.ClientId))
            {
                CheckMark.sprite = CheckMarkActiveSprite;

                Debug.Log("CheckMark Is Green");
            }
        }
        else
        {
            Hide();
        }
    }


    void Show()
    {
        gameObject.SetActive(true);
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }

    void ResetCheckMark()
    {
        CheckMark.sprite = CheckMarkInActiveSprite;
    }

    void OnDisable() 
    {
        
    }
}
