using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using DG.Tweening;
using RTLTMPro;

using Unity.Netcode;
public class LobbyCreateManager : MonoBehaviour
{
    #region Instance
	// ----------------------Instance Section---------------------- //
    private static LobbyCreateManager _instance;
    
    public static LobbyCreateManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<LobbyCreateManager>();
			return _instance;
		}
	}
    // ----------------------Instance Section---------------------- //
    #endregion

    public Button JoinButton;
    public Button CreateLobbyButton;

    public Button OnCustomizationFinished_Button_Male;
    public Button OnCustomizationFinished_Button_Female;

    // public List<Transform> 

    void Awake() 
    {
        CreateLobbyButton.onClick.AddListener(() => {
            // MainNetworkManager.Instance.StartHost();
        });

        JoinButton.onClick.AddListener(() => {
            // Debug.Log($"Join Code Is : {MainNetworkManager.Instance.JoinCode}");
            // MainNetworkManager.Instance.StartClient();
        });

        OnCustomizationFinished_Button_Male.onClick.AddListener(() => {
            
        });

        OnCustomizationFinished_Button_Female.onClick.AddListener(() => {
            
        });
    }
}
