using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System;

using UnityEngine.UI;
using Spine.Unity.Examples;

using Krivodeling.UI.Effects;
using Krivodeling.UI.Effects.Examples;

using DG.Tweening;


public class LobbyManager : NetworkBehaviour
{
#region Instance
	// ----------------------Instance Section---------------------- //
    public static LobbyManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<LobbyManager>();

			return _instance;
		}
	}
    private static LobbyManager _instance;
    // ----------------------Instance Section---------------------- //
#endregion

    public event EventHandler OnReadyChange;

    Dictionary<ulong, bool> PlayerReadyDictionary;

    bool AllClientsReady = false;

    public GameObject GoBackToMainMenuAfterDisconnectingPanel;

    // public UIBlur uiBlur;

    public bool IsLocalPlayerReady;

    void Awake() 
    {
        PlayerReadyDictionary = new Dictionary<ulong, bool>();
    }

    void Start()
    {
        MainNetworkManager.Instance.BufferingScreen.SetActive(false);
    }

#region Player Ready Logic...

    public void SetPlayerReadyState()
    {
        if (IsLocalReadyButtonPressed())
            SetPlayerReady();
        else
            SetPlayerNotReady();
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }

    public void SetPlayerNotReady()
    {
        SetPlayerNotReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerReadyServerRpc(ServerRpcParams _serverRpcParams = default)
    {
        SetPlayerReadyClientRpc(_serverRpcParams.Receive.SenderClientId);

        PlayerReadyDictionary[_serverRpcParams.Receive.SenderClientId] = true;
        
        AllClientsReady = true;

        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!PlayerReadyDictionary.ContainsKey(clientID) || !PlayerReadyDictionary[clientID])
            {
                AllClientsReady = false;
                break;
            }
        }
    }

    [ClientRpc]
    void SetPlayerReadyClientRpc(ulong _clientId)
    {
        PlayerReadyDictionary[_clientId] = true;

        OnReadyChange?.Invoke(this, EventArgs.Empty);
    }



    [ServerRpc(RequireOwnership = false)]
    void SetPlayerNotReadyServerRpc(ServerRpcParams _serverRpcParams = default)
    {
        SetPlayerNotReadyClientRpc(_serverRpcParams.Receive.SenderClientId);

        PlayerReadyDictionary[_serverRpcParams.Receive.SenderClientId] = false;
        
        AllClientsReady = false;

        // foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        // {
        //     if (!PlayerReadyDictionary.ContainsKey(clientID) || !PlayerReadyDictionary[clientID])
        //     {
        //         AllClientsReady = false;
        //         break;
        //     }
        // }
    }

    [ClientRpc]
    void SetPlayerNotReadyClientRpc(ulong _clientId)
    {
        PlayerReadyDictionary[_clientId] = false;

        OnReadyChange?.Invoke(this, EventArgs.Empty);
    }



    public bool IsPlayerReady(ulong _clientId)
    {
        return PlayerReadyDictionary.ContainsKey(_clientId) && PlayerReadyDictionary[_clientId];
    }

    public bool IsLocalReadyButtonPressed()
    {
        if (IsLocalPlayerReady)
        {
            IsLocalPlayerReady = false;
            return false;
        }
        else
        {
            IsLocalPlayerReady = true;
            return true;
        }
    }

#endregion

    public void StartGame()
    {
        // StartCoroutine(StartGameAfterRetrivingData((returnValue)=>
        // {
            if (AllClientsReady)
            {
                // LobbyNetworkManager.Instance.DeleteLobby();

                Loader.LoadNetwork(Loader.Scene.GamePlayScreen);
            }
        // }));       
    }

    // IEnumerator StartGameAfterRetrivingData(Action<bool> returnValue)
    // {
    //     yield return null;

    //     StartCoroutine(BackEndManager.Instance.RetriveDataFromJson(BackEndManager.Instance.jsonURL));

    //     returnValue();
    // }

    public void CleanUp()
    {
        if (MainNetworkManager.Instance != null)
            Destroy(MainNetworkManager.Instance.gameObject);

        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);
        
        if (HomeMenuManager.Instance != null)
            Destroy(HomeMenuManager.Instance.gameObject);
        
        if (LobbyNetworkManager.Instance != null)
            Destroy(LobbyNetworkManager.Instance.gameObject);
    }

    // -------------------Lobby Interaction With Players Functions------------------- //

    public void CloseBlurPanelPressed(float _blurSpeed = 1, float _animationSpeed = 1)
    {
        //Reseting Parent Before Starting The Animation...
        PlayerUI playerUI = LobbyManagerUI.Instance.blurTest.gameObject.GetComponentInChildren<PlayerUI>();

        LobbyManagerUI.Instance.blurTest.OnBlurEnds(_blurSpeed);

        //Starting The Animation To Get Player Back To Stage...
        playerUI.gameObject.transform.DOScale(new Vector3(1f, 1f, 1f), _animationSpeed).SetEase(Ease.InOutCubic);

        playerUI.gameObject.transform.SetParent(playerUI.OriginalParentTransfrom);

        playerUI.gameObject.GetComponent<RectTransform>().DOAnchorPos(Vector3.zero, _animationSpeed).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            // LobbyManagerUI.Instance.blurTest.OnBlurEnds(_blurSpeed);

            playerUI.IsCharacterZoomedIn = false;
        });
    }
}
