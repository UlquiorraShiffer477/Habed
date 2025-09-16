using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

using System;

public class PlayerContainer : MonoBehaviour
{
    [SerializeField] int PlayerIndex;
    [SerializeField] RTLTextMeshPro PlayerName;

    void Start() 
    {
        MainNetworkManager.Instance.OnPlayersInfoNetworkListChanged += HomeMenuManager_OnPlayersInfoNetworkListChanged;

        UpdatePlayer();
    }

    private void HomeMenuManager_OnPlayersInfoNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    public void UpdatePlayer()
    {
        if (MainNetworkManager.Instance.IsPlayerIndexConnected(PlayerIndex))
        {
            Show();

            PlayerInfo playerInfo = MainNetworkManager.Instance.GetPlayerInfoFromPlayerIndex(PlayerIndex);

            PlayerName.text = playerInfo.PlayerName.ToString();
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
}
