using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Unity.Netcode;

public class ServerManager : MonoBehaviour
{
    // public static ServerManager Instance
	// {
	// 	get
	// 	{
	// 		if (!_instance)
	// 			_instance = GameObject.FindObjectOfType<ServerManager>();
	// 		return _instance;
	// 	}
	// }

    // private static ServerManager _instance;

    // // public Dictionary<ulong, PlayerInfo> PlayerInfo { get; private set; }

    // void Awake() 
    // {
    //     DontDestroyOnLoad(_instance);
    // }

    // public void StartServer()
    // {
    //     NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

    //     NetworkManager.Singleton.StartServer();
    // }

    // public void StartHost()
    // {
    //     NetworkManager.Singleton.StartHost();
    // }

    // public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest _request, NetworkManager.ConnectionApprovalResponse _response)
    // {

    // }
}
