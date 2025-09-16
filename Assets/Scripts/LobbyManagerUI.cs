using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using Unity.Netcode;
using Unity.Services.Lobbies.Models;

using Krivodeling.UI.Effects;
using Krivodeling.UI.Effects.Examples;

using System;
using Unity.Services.Authentication;

using DG.Tweening;
using Unity.Networking.Transport;
using System.Threading.Tasks;


public class LobbyManagerUI : NetworkBehaviour
{
#region Instance
	// ----------------------Instance Section---------------------- //
    public static LobbyManagerUI Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<LobbyManagerUI>();

			return _instance;
		}
	}
    private static LobbyManagerUI _instance;
    // ----------------------Instance Section---------------------- //
#endregion
    
    public event EventHandler OnReadyCloseBlurPanelEvent;

    [SerializeField] Button BackToHomeButon;

    [SerializeField] Button ReadyButton;
    // [SerializeField] GameObject ReadyButtonHolder;

    public Button PlayerNowButton;
    [SerializeField] GameObject PlayButtonHolder;

    [SerializeField] GameObject ReadyButtonCheckMark;

    [SerializeField] TextMeshProUGUI LobbyCodeText;

    public UIBlur uiBlur;
    public BlurTest blurTest;
    public Button CloseBlurPanel;

    [SerializeField] float BlurSpeed = 1;
    [SerializeField] float AnimationSpeed = 1;

    [Header("Popup Properties"), Space]
    public GameObject ExitGamePanel;
    public GameObject PopUpBox_ExitGame;
    public float Duration = 0.5f;
    public List<AudioClip> ShowPopupAudioClips;
    public AudioClip HidePopupAudioClip;

    [Header("Loading Related"), Space]
    public GameObject LoadingScreen;


    void Awake() 
    {
        CloseBlurPanel.onClick.AddListener(() =>
        {
            LobbyManager.Instance.CloseBlurPanelPressed(BlurSpeed, AnimationSpeed);
        });

        BackToHomeButon.onClick.AddListener(() =>
        {
            ShowExitGamePopUp();
        });

        ReadyButton.onClick.AddListener(() =>
        {
            ReadyButtonCheckMark.SetActive(LobbyManager.Instance.IsLocalReadyButtonPressed());

            LobbyManager.Instance.IsLocalReadyButtonPressed();
    
            LobbyManager.Instance.SetPlayerReadyState();
        });

        PlayerNowButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.StartGame();
        });

        
    }

    public override void OnNetworkSpawn()
    {
        
        Debug.Log("LobbyManagerUI OnNetworkSpawn");
        NetworkManager.Singleton.OnTransportFailure += NetworkManager_OnTransportFailure;
    }

    private void NetworkManager_OnTransportFailure()
    {
        ShowExitGamePopUp();
    }

    void Start() 
    {
        Invoke(nameof (LobbyChecker), 1);
            
        
        MainNetworkManager.Instance.OnPlayersInfoNetworkListChanged += MainNetworkManager_OnPlayersInfoNetworkListChanged;

        PlayerNowButton.interactable = MainNetworkManager.Instance.CheckIfCanStartGame();

        StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.Lobby));

        Lobby lobby = LobbyNetworkManager.Instance.GetLobby();

        LobbyCodeText.text = lobby.LobbyCode; 

        if (!IsServer)
            PlayButtonHolder.SetActive(false);
    }

    void LobbyChecker()
    {
        if (LobbyNetworkManager.Instance.GetLobby() == null)
        {
            Debug.Log("Lobby was not found!");
            ShowExitGamePopUp();
        }
    }

    private void MainNetworkManager_OnPlayersInfoNetworkListChanged(object sender, EventArgs e)
    {
        PlayerNowButton.interactable = MainNetworkManager.Instance.CheckIfCanStartGame();
    }

    public void LeaveLobbyOnButtonClick()
    {
        StartCoroutine(LeaveLobbyCoroutine());
    }

    public IEnumerator LeaveLobbyCoroutine()
    {
        // Prevent mulitple clicks...
        if (NetworkManager.Singleton.ShutdownInProgress)
            yield break;
        
        //LoadingScreen.SetActive(true);

        Task task = LobbyNetworkManager.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);
        yield return new WaitUntil(() => task.IsCompleted);

        NetworkManager.Singleton.Shutdown();
        yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);

        LobbyManager.Instance.CleanUp();
        Loader.Load(Loader.Scene.HomeScreen);
    }


    public void ShowExitGamePopUp()
    {
        ExitGamePanel.SetActive(true);
        AudioManager.Instance.PlayRandomAudioFromPool(ShowPopupAudioClips);
        PopUpBox_ExitGame.transform.DOScale(Vector3.one, Duration).SetEase(Ease.OutBounce, 2).SetUpdate(true);
    }
    public void CloseExitGamePopUp()
    {
        AudioManager.Instance.PlayAudioClip(HidePopupAudioClip);
        PopUpBox_ExitGame.transform.DOScale(Vector2.zero, Duration).SetEase(Ease.InBounce, 2).SetUpdate(true).OnComplete(() =>
        {
            ExitGamePanel.gameObject.SetActive(false);
        });
    }

    // void Update() 
    // {
    //     if (!IsServer)
    //         return;

    //     if (MainNetworkManager.Instance.PlayerInfoNetworkList.Count < 2)
    //     {
    //         PlayerNowButton.interactable = false;
    //         ReadyButton.interactable = false;

    //     }
    //     else
    //     {
    //         PlayerNowButton.interactable = true;
    //         ReadyButton.interactable = true;
    //     }
    // }

    public void CopyText()
    {
        GUIUtility.systemCopyBuffer = LobbyCodeText.text;
        AnimateCopiedText();
        Debug.Log("Text copied to clipboard: " + LobbyCodeText.text);
    }
     [SerializeField] private TextMeshProUGUI notificationText;
     [SerializeField] private RectTransform notificationRectTransform;
     [SerializeField] private Vector2 endPosition;
     private Vector2 startPosition;
     private Tween positionTween;
     private Tween fadeTween;
    
    private void AnimateCopiedText()
    {
        // Kill any ongoing tweens
        positionTween?.Kill();
        fadeTween?.Kill();
        // Activate notification text and reset its alpha
        notificationText.gameObject.SetActive(true);
        notificationText.alpha = 1f;
        // Reset position to start
        notificationRectTransform.anchoredPosition = startPosition;
        // Animate notification text position
        positionTween = notificationRectTransform.DOAnchorPos(endPosition, 1f)
            .OnComplete(() => {
                // Fade out the text
                fadeTween = notificationText.DOFade(0f, 0.5f)
                    .OnComplete(() => {
                        // Deactivate and reset the notification after fade-out
                        notificationText.gameObject.SetActive(false);
                        notificationRectTransform.anchoredPosition = startPosition;
                        notificationText.alpha = 1f;  // Reset alpha for next use
                    });
            });
    }
    private void OnDestroy() 
    {
        MainNetworkManager.Instance.OnPlayersInfoNetworkListChanged -= MainNetworkManager_OnPlayersInfoNetworkListChanged;
    }
}

