using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class JoinButtonController : MonoBehaviour
{
    private Lobby lobby;
    // [SerializeField] bool IsEmpty = true;
    // [SerializeField] string JoinCode;

    // void Update() 
    // {
    //     if(IsEmpty)
    //     {
    //         if(MainNetworkManager.Instance.JoinCode != "")
    //         {
    //             JoinCode = MainNetworkManager.Instance.JoinCode;
    //             IsEmpty = false;
    //         }
    //     }
    // }

    private void Awake() 
    {
        // IsEmpty = true;

        // GetComponent<Button>().onClick.AddListener(() => {
        //     // MainNetworkManager.Instance.JoinLobby_New(lobby);

        //     MainNetworkManager.Instance.StartClient(MainNetworkManager.Instance.JoinCode);

        //     // LobbyCreateManager.Instance.HideScreen();
        // });
    }

    public void UpdateLobby(Lobby lobby)
    {
        this.lobby = lobby;
    }
}
