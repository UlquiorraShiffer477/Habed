using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

using System.Threading.Tasks;

using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using Unity.Netcode.Transports.UTP;
using Unity.Netcode.Transports;
using Unity.Networking.Transport.Relay;
using Spine.Unity.Examples;
using WebSocketSharp;

public class LobbyNetworkManager : MonoBehaviour
{

#region Instance
	// ----------------------Instance Section---------------------- //
    public static LobbyNetworkManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<LobbyNetworkManager>();

			return _instance;
		}
	}
    private static LobbyNetworkManager _instance;
	// ----------------------Instance Section---------------------- //
#endregion

	Lobby JoinedLobby;
	float HeartbeatTimer;

 	public	const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

	public string LOCALRELAYJOINCODE = "";



void Awake() 
{
    DontDestroyOnLoad(this.gameObject);
}

void Start() 
{
	// await SubscribeToLobbyEventsAsync();
}


void Update() 
{
	HandleHeartbeat();
}

#region  Relay Main Functions
async Task<Allocation> AllocateRelay()
{
	try
	{
 		Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MainNetworkManager.MAX_PLAYER_AMOUNT -1);

		return allocation;
	}
	catch(RelayServiceException e)
	{
		Debug.Log(e);

		return default;
	}
}

async Task<string> GetRelayJoinCode(Allocation _allocation)
{
	try
	{
		string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(_allocation.AllocationId);

		return relayJoinCode;
	}
	catch (RelayServiceException e)
	{
		Debug.Log(e);

		return default;
	}
}

async Task<JoinAllocation> JoinRelay(string _joinCode)
{
	try
	{	
		JoinAllocation joinAllocation;
		if (!_joinCode.IsNullOrEmpty())
			joinAllocation = await RelayService.Instance.JoinAllocationAsync(_joinCode);
		else
		{
			Debug.LogError("Join code is null or empty!");
			MainNetworkManager.Instance.BufferingScreen.SetActive(false);
			return default;
		}

		return joinAllocation;
	}
	catch (RelayServiceException e)
	{
		Debug.Log(e);

		MainNetworkManager.Instance.BufferingScreen.SetActive(false);

		return default;
	}
}
#endregion

#region  Main Lobby Functions
	void HandleHeartbeat()
	{
		if (IsLobbyHost())
		{
			HeartbeatTimer -= Time.deltaTime;
			if (HeartbeatTimer <= 0)
			{
				float HeartbeatTimerMax = 10f;
				HeartbeatTimer = HeartbeatTimerMax;

				LobbyService.Instance.SendHeartbeatPingAsync(JoinedLobby.Id);
			}
		}
	}

	bool IsLobbyHost()
	{
		// Debug.Log($"Is Lobby Host = {JoinedLobby != null  && JoinedLobby.HostId == AuthenticationService.Instance.PlayerId}");

		return JoinedLobby != null  && JoinedLobby.HostId == AuthenticationService.Instance.PlayerId;
	}

	public async void CreateLobby(string _lobbyName, bool _isPrivate = false)
	{
		try
		{
			MainNetworkManager.Instance.BufferingScreen.SetActive(true);

			JoinedLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, MainNetworkManager.MAX_PLAYER_AMOUNT, new CreateLobbyOptions{
				IsPrivate = _isPrivate,
			});

			// Allocation allocation = await AllocateRelay();
			// string relayJoinCode = await GetRelayJoinCode(allocation);

			Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MainNetworkManager.MAX_PLAYER_AMOUNT -1);

			string relayJoinCode = "";
			try
			{
				relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			}
			catch
			{
				Debug.Log("Error occured during connection please exitr and try again!");

				if (GetLobby() != null)
				{
					await LeaveLobby(AuthenticationService.Instance.PlayerId);
				}
				HomeMenuManager.Instance.Connection_Message.text = "حدث خطأ أثناء الاتصال!";
				HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();
				MainNetworkManager.Instance.BufferingScreen.SetActive(false);
				return;
			}

			await LobbyService.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions{
				Data = new Dictionary<string, DataObject>{
					{ KEY_RELAY_JOIN_CODE , new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
				}
			});

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "udp"));

			MainNetworkManager.Instance.StartHost(allocation);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);

			if (e.Reason == LobbyExceptionReason.LobbyAlreadyExists)
				HomeMenuManager.Instance.Connection_Message.text = "هذا اللاعب متواجد في الغرفة!";
			else if (e.Reason == LobbyExceptionReason.IncorrectPassword)
				HomeMenuManager.Instance.Connection_Message.text = "كود الغرفة خاطئ!";
			else if (e.Reason == LobbyExceptionReason.InvalidJoinCode)
				HomeMenuManager.Instance.Connection_Message.text = "كود الغرفة غير صالح!";
			else if (e.Reason == LobbyExceptionReason.LobbyNotFound)
				HomeMenuManager.Instance.Connection_Message.text = "الغرفة غير موجودة!";
			else if (e.Reason == LobbyExceptionReason.LobbyFull)
				HomeMenuManager.Instance.Connection_Message.text = " الغرفة ممتلئة!";
			else	
				HomeMenuManager.Instance.Connection_Message.text = e.Reason.ToString();
				
			HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();

			MainNetworkManager.Instance.BufferingScreen.SetActive(false);
		}
	}

	public async void QuickLobbyJoin()
	{
		try
		{
			MainNetworkManager.Instance.BufferingScreen.SetActive(true);

			JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

			string relayJoinCode = JoinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

			JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);


			LOCALRELAYJOINCODE = relayJoinCode;


			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "udp"));

			// await SubscribeToLobbyEventsAsync();

			MainNetworkManager.Instance.StartClient(joinAllocation);
		}
		catch (LobbyServiceException e)
		{
			Debug.Log(e);

			MainNetworkManager.Instance.BufferingScreen.SetActive(false);
		}
	}

	public async void JoinLobbyWithCode(string _lobbyCode, bool _showBufferingScreen = true)
	{
		try
		{
			if (_showBufferingScreen)
				MainNetworkManager.Instance.BufferingScreen.SetActive(true);

			if (!_lobbyCode.IsNullOrEmpty())
				JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_lobbyCode);
			else
			{
				HomeMenuManager.Instance.Connection_Message.text = "كود الغرفة غير صالح!";
				HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();
				MainNetworkManager.Instance.BufferingScreen.SetActive(false);
			}

			string relayJoinCode = JoinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

			JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);


			LOCALRELAYJOINCODE = relayJoinCode;


			NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "udp"));

			// await SubscribeToLobbyEventsAsync();

			MainNetworkManager.Instance.StartClient(joinAllocation);
		}
		catch(LobbyServiceException e)
		{
			Debug.Log(e);

			if (e.Reason == LobbyExceptionReason.LobbyAlreadyExists)
				HomeMenuManager.Instance.Connection_Message.text = "هذا اللاعب متواجد في الغرفة!";
			else if (e.Reason == LobbyExceptionReason.IncorrectPassword)
				HomeMenuManager.Instance.Connection_Message.text = "كود الغرفة خاطئ!";
			else if (e.Reason == LobbyExceptionReason.InvalidJoinCode)
				HomeMenuManager.Instance.Connection_Message.text = "كود الغرفة غير صالح!";
			else if (e.Reason == LobbyExceptionReason.LobbyNotFound)
				HomeMenuManager.Instance.Connection_Message.text = "الغرفة غير موجودة!";
			else if (e.Reason == LobbyExceptionReason.LobbyFull)
				HomeMenuManager.Instance.Connection_Message.text = " الغرفة ممتلئة!";
			else	
				HomeMenuManager.Instance.Connection_Message.text = e.Reason.ToString();
			
			if (GetLobby() != null)
			{
				await LeaveLobby(AuthenticationService.Instance.PlayerId);
			}

			HomeMenuManager.Instance.ShowFaildToJoinRoom_Popup();

			MainNetworkManager.Instance.BufferingScreen.SetActive(false);
		}
	}

	public async Task<Lobby> ReconnectToLobbyAsync()
        {
            try
            {
                return await LobbyService.Instance.ReconnectToLobbyAsync(JoinedLobby.Id);
            }
            catch (LobbyServiceException e)
            {
                // If Lobby is not found and if we are not the host, it has already been deleted. No need to publish the error here.
				Debug.Log("Error: " + e);
                // if (e.Reason != LobbyExceptionReason.LobbyNotFound && !m_LocalUser.IsHost)
                // {
                //     PublishError(e);
                // }
            }

            return null;
        }

	public Lobby GetLobby()
	{
		if(JoinedLobby != null)
		{
			Debug.Log("Get Lobby: Lobby Exists!");
			return JoinedLobby;
		}
			
		else
		{
			Debug.Log("Get Lobby: Lobby Is Null!");
			return null;
		}
	}

	public async void DeleteLobby()
	{
		if (JoinedLobby != null)
		{
			try
			{
				await LobbyService.Instance.DeleteLobbyAsync(JoinedLobby.Id);

				JoinedLobby = null;
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}
	}

	public async Task LeaveLobby(string _playerID)
	{
		if (JoinedLobby != null)
		{
			try
			{
				await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, _playerID);

				JoinedLobby = null;
			}
			catch (LobbyServiceException e)
			{
				Debug.Log(e);
			}
		}
	}
#endregion

	public async Task SubscribeToLobbyEventsAsync()
    {
		Debug.Log("SubscribeToLobbyEventsAsync: Started!");

        var callbacks = new LobbyEventCallbacks();
        callbacks.PlayerLeft += OnPlayerLeft;
        callbacks.LobbyChanged += OnLobbyChanged;
        callbacks.PlayerJoined += OnPlayerJoined;
        
        try
        {
            await Lobbies.Instance.SubscribeToLobbyEventsAsync(LobbyNetworkManager.Instance.GetLobby().Id, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby: Debug.LogWarning($"Already subscribed to lobby[{LobbyNetworkManager.Instance.GetLobby().Id}]. We did not need to try and subscribe again. Exception Message: {ex.Message}"); break;
                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy: Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {ex.Message}"); throw;
                case LobbyExceptionReason.LobbyEventServiceConnectionError: Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}"); throw;
                default: throw;
            }
        }
    }

    private void OnPlayerJoined(List<LobbyPlayerJoined> list)
    {
		foreach (var v in list)
		{
			Debug.Log("Player Id That Just Joined Is: " + v.Player.Id);
			Debug.Log("Player Index That Just Joined Is: " + v.PlayerIndex);
		}
        Debug.Log("A player just joined the lobby!");
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        Debug.Log("Lobby changed!");
    }

    private void OnPlayerLeft(List<int> list)
    {
        Debug.Log("A player just left the lobby!");
    }

	public async Task SetupClientConnectionAsync()
    {
        Debug.Log("ConnectionMethodRelay: SetupClientConnectionAsync()");
        Debug.Log("Setting up Unity Relay client");
        // SetConnectionPayload(GetPlayerId(), m_PlayerName);
        // if (m_LobbyServiceFacade.CurrentUnityLobby == null)
        // {
        //     throw new Exception("Trying to start relay while Lobby isn't set");
        // }

        string relayJoinCode = LobbyNetworkManager.Instance.GetLobby().Data[LobbyNetworkManager.KEY_RELAY_JOIN_CODE].Value;

        Debug.Log($"Setting Unity Relay client with join code {relayJoinCode}");
        // Create client joining allocation from join code
        var joinedAllocation = await JoinRelay(relayJoinCode);

        Debug.Log($"client: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
            $"host: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
            $"client: {joinedAllocation.AllocationId}");

        // await m_LobbyServiceFacade.UpdatePlayerDataAsync(joinedAllocation.AllocationId.ToString(), m_LocalLobby.RelayJoinCode);

        // Configure UTP with allocation
        // var utp = (UnityTransport)m_ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
        // utp.SetRelayServerData(new RelayServerData(joinedAllocation, k_DtlsConnType));

		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinedAllocation, "dtls"));
    }

	// void Example_KeepingConnectionAlive()
	// {
	// 	// Update the NetworkDrivers regularly to ensure the host/player is kept online.
	// 	if (HostDriver.IsCreated && isRelayServerConnected)
	// 	{
	// 		HostDriver.ScheduleUpdate().Complete();

	// 		//Accept incoming client connections
	// 		while (HostDriver.Accept() != default(NetworkConnection))
	// 		{
	// 			Debug.Log("Accepted an incoming connection.");
	// 		}
	// 	}

	// 	if (PlayerDriver.IsCreated && clientConnection.IsCreated)
	// 	{
	// 		PlayerDriver.ScheduleUpdate().Complete();

	// 		//Resolve event queue
	// 		NetworkEvent.Type eventType;
	// 		while ((eventType = clientConnection.PopEvent(PlayerDriver, out _)) != NetworkEvent.Type.Empty)
	// 		{
	// 			if (eventType == NetworkEvent.Type.Connect)
	// 			{
	// 				Debug.Log("Client connected to the server");
	// 			}
	// 			else if (eventType == NetworkEvent.Type.Disconnect)
	// 			{
	// 				Debug.Log("Client got disconnected from server");
	// 				clientConnection = default(NetworkConnection);
	// 			}
	// 		}
	// 	}
	// }
}
