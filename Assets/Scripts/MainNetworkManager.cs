using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using Unity.Services.Authentication;
using Unity.Services.Core;

using UnityEngine;

using UnityEngine.Networking;
using Unity.Networking;
using Unity.Networking.Transport;
using Unity.Netcode.Transports;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using Unity.Services.CloudSave;

using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

using UnityEngine.SocialPlatforms;

using Spine.Unity.Examples;
using DG.Tweening;
using WebSocketSharp;
// using UnityEditorInternal;

[Serializable]
public class SampleObject
{
    public string SophisticatedString;
    public int SparklingInt;
    public float AmazingFloat;
}

public class MainNetworkManager : NetworkBehaviour {

#region Instance
    // ----------------------Instance Section---------------------- //
    public static MainNetworkManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<MainNetworkManager>();

			return _instance;
		}
	}
    private static MainNetworkManager _instance;
    // ----------------------Instance Section---------------------- //
#endregion


    public const int MAX_PLAYER_AMOUNT = 8;


    public enum PlayerAvatar 
    {
        Male,
        Female
    }

    // --------------Network Events-------------- //
    public event EventHandler OnPlayersInfoNetworkListChanged;



    // --------------Network Variables-------------- //
    [Header("Network Varriables")]
    public NetworkList<PlayerInfo> PlayerInfoNetworkList;


    [Header("Characters")]
    public Transform MalePlayerPrefab;
    public Transform FemalePlayerPrefab;

    [Header("Buffering Screen")]
    public GameObject BufferingScreen;

    [Header("Network Releated")]
    public Allocation RelayAllocation;

    

    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);

        PlayerInfoNetworkList = new NetworkList<PlayerInfo>();
        PlayerInfoNetworkList.OnListChanged += PlayersIndoNetworkList_OnListChanged;
    }

    private void PlayersIndoNetworkList_OnListChanged(NetworkListEvent<PlayerInfo> changeEvent)
    {
        OnPlayersInfoNetworkListChanged?.Invoke(this, EventArgs.Empty);

        Debug.Log("PlayerInfoNetworkList.Count is: " + PlayerInfoNetworkList.Count);
    }

    public override void OnNetworkSpawn()
    {
        // NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnect;
    }
    // -----------------------------------------------End----------------------------------------------- //

#region Cloud Save Functions
    public async Task SaveDataObject(string _key, PlayerData _object)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                {_key, _object}
            };

            await CloudSaveService.Instance.Data.ForceSaveAsync(data);

            Debug.Log($"Successfully saved {_key}:{_object}");
        }

        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }

        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }

        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<T> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> {key});
            if (results.TryGetValue(key, out string value))
            {
                return JsonUtility.FromJson<T>(value);
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
        return default;
    }
#endregion

    // ----------------------------------------------Relay Functions---------------------------------------------- //
    public async void StartHost(Allocation _allocation)
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_OnConnectionApproval;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnect;
        NetworkManager.Singleton.OnTransportFailure += NetworkManager_OnTransportFailure;

        if (NetworkManager.Singleton.StartHost())
        {
            await LobbyNetworkManager.Instance.SubscribeToLobbyEventsAsync();
            Debug.Log("Host Started");

            string relayJoinCode = "";
            try
            {
                // Set this for later uses...
                RelayAllocation = _allocation;

                relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);

                Debug.Log("relayJoinCode is valid!");
                Loader.LoadNetwork(Loader.Scene.LobbyScene);
            }
            catch
            {
                if (relayJoinCode.IsNullOrEmpty())
                {
                   Disconnect_Host();
                   return;
                }  
            }
        }


    }

    private void NetworkManager_OnTransportFailure()
    {
        Debug.Log("Host OnTransportFailure");
        if (HomeMenuManager.Instance)
            HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();
        else if (LobbyManager.Instance)
        {
            LobbyNetworkManager.Instance.DeleteLobby();
            LobbyManager.Instance.GoBackToMainMenuAfterDisconnectingPanel.SetActive(true);
        }
    }

    private void NetworkManager_OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name == Loader.Scene.GamePlayScreen.ToString())
        {
            Debug.Log("Player already exists in the game! Reconnecting...");
        }
        // else if (SceneManager.GetActiveScene().name != Loader.Scene.LobbyScene.ToString())
        // {
        //     response.Approved = false;
        //     response.Reason = "Game Already Started!";

        //     Debug.Log("Game Already Started!");
        //     return;
        // }

        if(NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        {
            response.Approved = false;
            response.Reason = "The Lobby Is Full!";

            Debug.Log("The Lobby Is Full!");
            return;
        }

        // BufferingScreen.SetActive(false);

        response.Approved = true;
    }

    private void NetworkManager_OnClientConnected(ulong _clientId)
    {
        // if (GamePlayManager.Instance != null)
        //     if (IsServer)
        //         GamePlayManager.Instance.state.Value = GamePlayManager.Instance.CurrentState;
        
        Debug.Log("NetworkManager_OnClientConnected IsServer: " + IsServer);

        PlayerInfoNetworkList.Add(new PlayerInfo {

            ClientId = _clientId,

            PlayerID = PlayerDataManager.Instance.playerData.PlayerID,

            IsMan = PlayerDataManager.Instance.playerData.playerInfo.IsMan,
            gender = PlayerDataManager.Instance.playerData.playerInfo.gender,

            HairIndex = PlayerDataManager.Instance.playerData.playerInfo.HairIndex,
            LowerFaceIndex = PlayerDataManager.Instance.playerData.playerInfo.LowerFaceIndex,
            ShirtIndex = PlayerDataManager.Instance.playerData.playerInfo.ShirtIndex,
            PantsIndex = PlayerDataManager.Instance.playerData.playerInfo.PantsIndex,
            HeadAccessoriesIndex = PlayerDataManager.Instance.playerData.playerInfo.HeadAccessoriesIndex,
            EyeAccessoriesIndex = PlayerDataManager.Instance.playerData.playerInfo.EyeAccessoriesIndex,
            ShoulderAccessoriesIndex = PlayerDataManager.Instance.playerData.playerInfo.ShoulderAccessoriesIndex,
            BodyAccessoriesIndex = PlayerDataManager.Instance.playerData.playerInfo.BodyAccessoriesIndex,
            ShoesIndex = PlayerDataManager.Instance.playerData.playerInfo.ShoesIndex,
            NoseIndex = PlayerDataManager.Instance.playerData.playerInfo.NoseIndex,

            MaleItems = PlayerDataManager.Instance.playerData.playerInfo.MaleItems,
            FemaleItems = PlayerDataManager.Instance.playerData.playerInfo.FemaleItems,
            
            MaleSpecialShirtEquipped = PlayerDataManager.Instance.playerData.playerInfo.MaleSpecialShirtEquipped,
            FemaleSpecialShirtEquipped = PlayerDataManager.Instance.playerData.playerInfo.FemaleSpecialShirtEquipped,
        });

        Debug.Log("ClientId =" + _clientId);

        for (int i = 0 ; i < PlayerInfoNetworkList.Count ; i++)
        {
            Debug.Log($"Client {i} ID is: {PlayerInfoNetworkList[i].ClientId}");
            Debug.Log($"PlayerID {i} ID is: {PlayerInfoNetworkList[i].PlayerID}");
        }

        Debug.Log($"PlayerInfoNetworkList Count Is: {PlayerInfoNetworkList.Count}");

        SetPlayerNameServerRpc(PlayerDataManager.Instance.GetPlayerNamePlayerData());
        SetPlayerIsMaleGenderServerRpc(PlayerDataManager.Instance.GetPlayerGender());
        SetPlayerSkinsServerRpc(PlayerDataManager.Instance.GetPlayerInfo());
    }
    
    private void NetworkManager_OnClientDisconnect(ulong _clientId)
    {
        DisconnectPlayer(_clientId);


        // if (IsServer)
        // {
        //     GamePlayManager.Instance.CurrentState = GamePlayManager.Instance.state.Value;

        //     GamePlayManager.Instance.state.Value = GamePlayManager.State.GamePaused;
        // }
    }

    public async void DisconnectPlayer(ulong _clientId)
    {
        // if (SceneManager.GetActiveScene().name != "GamePlayScreen")
        //     return;

        var clientInfo = GetPlayerInfoFromClientID(_clientId);
        
        Debug.Log("Client: " + _clientId + " - " + clientInfo.PlayerName + " Has Disconnected!");

        for (int i = 0; i < PlayerInfoNetworkList.Count; i ++)
        {
            PlayerInfo playerInfo = PlayerInfoNetworkList[i];
            if(playerInfo.ClientId == _clientId)
            {
                //Disconnect!
                PlayerInfoNetworkList.RemoveAt(i);

                await LobbyNetworkManager.Instance.LeaveLobby(playerInfo.PlayerID.ToString());

                // LobbyNetworkManager.Instance.LeaveLobby();
                // NetworkManager.Singleton.Shutdown();
                // CleanUp();

                Debug.Log($"PlayerInfoNetworkList Count Is: {PlayerInfoNetworkList.Count}");
                break;
            }
        }

        if (GamePlayManager.Instance)
        {
            if (GamePlayManager.Instance.OriginalPlayersInSession != null && GamePlayManager.Instance.OriginalPlayersInSession.Count != 0)
            {
                for (int i = 0; i < GamePlayManager.Instance.OriginalPlayersInSession.Count; i ++)
                {
                    PlayerSessionInfo playerSessionInfo = GamePlayManager.Instance.OriginalPlayersInSession[i]; 
                    if(playerSessionInfo.ClientId == _clientId)
                    {
                        //Disconnect!
                        GamePlayManager.Instance.OriginalPlayersInSession.RemoveAt(i);

                        Debug.Log($"OriginalPlayersInSession Count Is: {GamePlayManager.Instance.OriginalPlayersInSession.Count}");
                        break;
                    }
                }
            }

            if (GamePlayManager.Instance.OutOfFocusPlayers != null && GamePlayManager.Instance.OutOfFocusPlayers.Count != 0)
            {
                for (int i = 0; i < GamePlayManager.Instance.OutOfFocusPlayers.Count; i ++)
                {
                    PlayerSessionInfo playerSessionInfo = GamePlayManager.Instance.OutOfFocusPlayers[i]; 
                    if(playerSessionInfo.ClientId == _clientId)
                    {
                        //Disconnect!
                        GamePlayManager.Instance.OutOfFocusPlayers.RemoveAt(i);

                        Debug.Log($"OutOfFocusPlayers Count Is: {GamePlayManager.Instance.OutOfFocusPlayers.Count}");
                        break;
                    }
                }
            }

        } 
        
        Debug.Log("Client With ID: " + _clientId + " Has Disconnected!");

        if (GamePlayManager.Instance != null)
        {
            if (GamePlayManager.Instance.state.Value == GamePlayManager.State.GameEnd)
                return;

            StartCoroutine(GamePlayManager.Instance.ShowPlayerDisconnectedMessage());

            if(!IsOwner)
                return;

            // GamePlayManager.Instance.RPCsChecker();
        }
    }

    

    //-----------------------------------Client Functions-----------------------------------//

    public async void StartClient(JoinAllocation _joinAllocation)
    {
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnect;
        NetworkManager.Singleton.OnTransportFailure += NetworkManager_Client_OnTransportFailure;


        if(NetworkManager.Singleton.StartClient())
        {
            await LobbyNetworkManager.Instance.SubscribeToLobbyEventsAsync();
            Debug.Log("Client Started!");
        }
    }

    private void NetworkManager_Client_OnTransportFailure()
    {
        Debug.Log("Client OnTransportFailure");

        Disconnect_Client();
    }

    private void NetworkManager_Client_OnClientConnected(ulong _clientId)
    {
        Debug.Log("Client Connected!");

        Debug.Log("NetworkManager_OnClientConnected IsServer: " + IsServer);

        // if (GamePlayManager.Instance != null)
        //     GamePlayManager.Instance.DisconnectionPanel.SetActive(false);

        SetPlayerNameServerRpc(PlayerDataManager.Instance.GetPlayerNamePlayerData());
        SetPlayerIDServerRpc(AuthenticationService.Instance.PlayerId);
        SetPlayerIsMaleGenderServerRpc(PlayerDataManager.Instance.GetPlayerGender());
        SetPlayerSkinsServerRpc(PlayerDataManager.Instance.GetPlayerInfo());
    }
    private async void NetworkManager_Client_OnClientDisconnect(ulong _clientId)
    {
        Debug.Log("Client With ID: " + _clientId + " Has Disconnected!");

        if (LobbyNetworkManager.Instance.GetLobby() != null)
            await LobbyNetworkManager.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);
        // GamePlayManager.Instance.SetPauseReadyServerRpc(false, default);

        // StartCoroutine(GamePlayManager.Instance.ReconnectCoroutine(_clientId));

        if (GamePlayManager.Instance)
        {
            GamePlayManager.Instance.GoBackToMainMenuAfterDisconnectingPanel.SetActive(true);  
            
            if (GamePlayManager.Instance.OriginalPlayersInSession != null && GamePlayManager.Instance.OriginalPlayersInSession.Count != 0)
            {  
                for (int i = 0; i < GamePlayManager.Instance.OriginalPlayersInSession.Count; i ++)
                {
                    PlayerSessionInfo playerSessionInfo = GamePlayManager.Instance.OriginalPlayersInSession[i]; 
                    if(playerSessionInfo.ClientId == _clientId)
                    {
                        //Disconnect!
                        GamePlayManager.Instance.OriginalPlayersInSession.RemoveAt(i);

                        // LobbyNetworkManager.Instance.LeaveLobby();
                        // NetworkManager.Singleton.Shutdown();
                        // CleanUp();

                        Debug.Log($"OriginalPlayersInSession Count Is: {GamePlayManager.Instance.OriginalPlayersInSession.Count}");
                        break;
                    }
                }
            } 
        }

        if (HomeMenuManager.Instance)
        {
            HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();
            BufferingScreen.SetActive(false);
        }

        if (LobbyManager.Instance)
        {
            LobbyManager.Instance.GoBackToMainMenuAfterDisconnectingPanel.SetActive(true);
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Client_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Client_OnClientDisconnect;

    }

    public bool CheckIfCanStartGame()
    {
        if (PlayerInfoNetworkList.Count > 1)
            return true;
        else
            return false;
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerNameServerRpc(string _playerName, ServerRpcParams _serverRpcParams = default)
    {
        int playerInfoIndex = GetPlayerInfoIndexFromClientID(_serverRpcParams.Receive.SenderClientId);

        PlayerInfo playerInfo = PlayerInfoNetworkList[playerInfoIndex];

        //Assign the name...
        playerInfo.PlayerName = _playerName;

        PlayerInfoNetworkList[playerInfoIndex] = playerInfo;
    }
    [ServerRpc(RequireOwnership = false)]
    void SetPlayerIDServerRpc(string _playerID, ServerRpcParams _serverRpcParams = default)
    {
        int playerInfoIndex = GetPlayerInfoIndexFromClientID(_serverRpcParams.Receive.SenderClientId);

        PlayerInfo playerInfo = PlayerInfoNetworkList[playerInfoIndex];

        //Assign the PlayerID...
        playerInfo.PlayerID = _playerID;

        PlayerInfoNetworkList[playerInfoIndex] = playerInfo;
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerIsMaleGenderServerRpc(global::Gender _gender, ServerRpcParams _serverRpcParams = default)
    {
        int playerInfoIndex = GetPlayerInfoIndexFromClientID(_serverRpcParams.Receive.SenderClientId);

        PlayerInfo playerInfo = PlayerInfoNetworkList[playerInfoIndex];

        //Assign the gender...
        playerInfo.gender = _gender;

        PlayerInfoNetworkList[playerInfoIndex] = playerInfo;
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerSkinsServerRpc(PlayerInfo _playerInfo, ServerRpcParams _serverRpcParams = default)
    {
        int playerInfoIndex = GetPlayerInfoIndexFromClientID(_serverRpcParams.Receive.SenderClientId);

        PlayerInfo playerInfo = PlayerInfoNetworkList[playerInfoIndex];

        Debug.Log("SetPlayerSkinsServerRpc");

        //Set Skins...
        playerInfo.HairIndex =                      _playerInfo.HairIndex;
        playerInfo.LowerFaceIndex =                 _playerInfo.LowerFaceIndex;
        playerInfo.ShirtIndex =                     _playerInfo.ShirtIndex;
        playerInfo.PantsIndex =                     _playerInfo.PantsIndex;
        playerInfo.HeadAccessoriesIndex =           _playerInfo.HeadAccessoriesIndex;
        playerInfo.EyeAccessoriesIndex =            _playerInfo.EyeAccessoriesIndex;
        playerInfo.ShoulderAccessoriesIndex =       _playerInfo.ShoulderAccessoriesIndex;
        playerInfo.BodyAccessoriesIndex =           _playerInfo.BodyAccessoriesIndex;
        playerInfo.ShoesIndex =                     _playerInfo.ShoesIndex;
        playerInfo.NoseIndex =                      _playerInfo.NoseIndex;

        playerInfo.MaleItems =                      _playerInfo.MaleItems;
        playerInfo.FemaleItems =                    _playerInfo.FemaleItems;
        //----------------------------------------------------------------------------
        playerInfo.MaleSpecialShirtEquipped =       _playerInfo.MaleSpecialShirtEquipped;
        playerInfo.FemaleSpecialShirtEquipped =       _playerInfo.FemaleSpecialShirtEquipped;
        //------------------------------------alaa is retarted------------------------------------
        //Assign the new playerInfo Object...
        PlayerInfoNetworkList[playerInfoIndex] = playerInfo;
    }

    // ------------------------General Network Functions------------------------ //

    public bool DoesPlayerExist(string _playerID)
    {
        foreach (var player in PlayerInfoNetworkList)
        {
            if (player.PlayerID == _playerID) 
                return true;
        }

        return false;
    }   

    public bool IsPlayerIndexConnected(int _playerIndex)
    {
        return _playerIndex < PlayerInfoNetworkList.Count;
    }

    public PlayerInfo GetPlayerInfoFromPlayerIndex(int _playerIndex)
    {
        return PlayerInfoNetworkList[_playerIndex];
    }

    public int GetPlayerInfoIndexFromClientID(ulong _clientId)
    {
        for (int i = 0; i < PlayerInfoNetworkList.Count; i++)
        {
            if (PlayerInfoNetworkList[i].ClientId == _clientId)
            {
                return i;
            }
        }

        return -1;
    }

    public PlayerInfo GetPlayerInfoFromClientID(ulong _clientId)
    {
        foreach (PlayerInfo playerInfo in PlayerInfoNetworkList)
        {
            if(playerInfo.ClientId == _clientId)
            {
                return playerInfo;
            }
        }

        return default;
    }

    public bool GetIsPlayerGenderIsMale(int _playerIndex)
    {
        return PlayerInfoNetworkList[_playerIndex].IsMan;
    }



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

    public async void Disconnect_Host()
    {
        
        NetworkManager.Singleton.ConnectionApprovalCallback -= NetworkManager_OnConnectionApproval;
        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnect;
        NetworkManager.Singleton.OnTransportFailure -= NetworkManager_OnTransportFailure;
        

        await LobbyNetworkManager.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);

        StartCoroutine(ShutDownNetworkManager_Host());

        // yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);
    }

    public IEnumerator ShutDownNetworkManager_Host()
    {
        PlayerInfoNetworkList.Clear();

        NetworkManager.Singleton.Shutdown();
        yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);

        Debug.Log("relayJoinCode is null or empty...cant continue!");
        HomeMenuManager.Instance.Connection_Message.text = "حدث خطأ أثناء الاتصال!";
        BufferingScreen.SetActive(false);
        HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();
    }

    public async void Disconnect_Client()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManager_Client_OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_Client_OnClientDisconnect;
        NetworkManager.Singleton.OnTransportFailure -= NetworkManager_Client_OnTransportFailure;

        if (LobbyNetworkManager.Instance.GetLobby() != null)
            await LobbyNetworkManager.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);
        
        Debug.Log("relayJoinCode is null or empty...cant continue!");
        HomeMenuManager.Instance.Connection_Message.text = "حدث خطأ أثناء الاتصال!";
        BufferingScreen.SetActive(false);
        HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();

        // StartCoroutine(ShutDownNetworkManager());

        // yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);
    }
}