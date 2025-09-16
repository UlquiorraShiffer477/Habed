using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RTLTMPro;
using TMPro;

using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class HomeLobbyUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] Button CreateLobbyButton;
    [SerializeField] Button JoinLobbyButton;

    [SerializeField] Button PublicLobbyButton;
    [SerializeField] Button PrivateLobbyButton;

    [SerializeField] Button BackButton;

    [Header("Panels")]
    [SerializeField] RectTransform LobbyOptionsPanel1;
    [SerializeField] RectTransform LobbyOptionsPanel2;
    bool CanAnimate = true;

    [SerializeField] string Title1;
    [SerializeField] string Title2;
    [SerializeField] RTLTextMeshPro Title;

    int LobbyPanelsSlidingSwitch = 1;

    Vector2 OffScreenRight;
    Vector2 OffScreenLeft;
    Vector2 InsideScreen;

    [Header("RoomCode Section")]
    [SerializeField] RectTransform RoomCodeSection;
    [SerializeField] Button HideRoomCodeSectionButton;
    [SerializeField] Button ConfirmJoinPrivateLobbyButton;
    [SerializeField] TMP_InputField RoomCodeInputField;

    bool IsRoomCodeSectionVisibale;



    void Awake() 
    {
        BackButton.gameObject.SetActive(false);

        CreateLobbyButton.onClick.AddListener(() => 
        {
            LobbyPanelsSlidingAnimation();
            OnCreateButtonPressed();
        });
        JoinLobbyButton.onClick.AddListener(() => 
        {
            LobbyPanelsSlidingAnimation();
            OnJoinButtonPressed();
        });
        BackButton.onClick.AddListener(() => 
        {
            OnBackClicked();
        });
    }

    void Start() 
    {
        OffScreenRight = new Vector2(UIManagerTheUltimate.Instance.RightStartPosition,0);
        OffScreenLeft = new Vector2(UIManagerTheUltimate.Instance.LeftStartPosition,0);
        InsideScreen = Vector2.zero;

        LobbyPanelsInitSetUp();
    }

    

    void LobbyPanelsInitSetUp()
    {
        LobbyOptionsPanel1.anchoredPosition = Vector2.zero;
        LobbyOptionsPanel2.anchoredPosition = OffScreenRight;
    }

    void LobbyPanelsSlidingAnimation()
    {
        if(!CanAnimate)
            return;
        
        switch (LobbyPanelsSlidingSwitch)
        {
            case 1:
            {
                StartCoroutine(LobbySlidingToPanel2());
                Title.text = Title2;
                BackButton.gameObject.SetActive(true);
                LobbyPanelsSlidingSwitch++;
                break;
            }
            case 2:
            {
                StartCoroutine(LobbySlidingToPanel1());
                Title.text = Title1;
                BackButton.gameObject.SetActive(false);
                LobbyPanelsSlidingSwitch--;
                break;
            }
        }
    }

    void OnCreateButtonPressed()
    {
        DisableAllButtons();

        CleanAndRemoveListeners();

        AssignButtonsOnCreatePressed();
    }
    void OnJoinButtonPressed()
    {
        DisableAllButtons();

        CleanAndRemoveListeners();

        AssignButtonsOnJoinPressed();
    }
    void OnBackClicked()
    {
        if(IsRoomCodeSectionVisibale)
            StartCoroutine(HideRoomCodeSection());

        LobbyPanelsSlidingAnimation();
    }

    void CleanAndRemoveListeners()
    {
        Debug.Log("Reseted All Listeners!");

        PublicLobbyButton.onClick.RemoveAllListeners();
        PrivateLobbyButton.onClick.RemoveAllListeners();
    }

    void DisableAllButtons()
    {
        CreateLobbyButton.interactable = false;
        JoinLobbyButton.interactable = false;
        PublicLobbyButton.interactable = false;
        PrivateLobbyButton.interactable = false;
        BackButton.interactable = false;

        ConfirmJoinPrivateLobbyButton.interactable = false;
        HideRoomCodeSectionButton.interactable = false;
    }
    void EnableAllButtons()
    {
        CreateLobbyButton.interactable = true;
        JoinLobbyButton.interactable = true;

        // This is for the first launch...
        PublicLobbyButton.interactable = false;

        //Enable this when clinet wants to add the public feature...
        // PublicLobbyButton.interactable = true;
        PrivateLobbyButton.interactable = true;
        BackButton.interactable = true;

        ConfirmJoinPrivateLobbyButton.interactable = true;
        HideRoomCodeSectionButton.interactable = true;
    }

    void AssignButtonsOnCreatePressed()
    {
        Debug.Log("Create Lobby Functions Assigned!");

        PublicLobbyButton.onClick.AddListener(() => 
        {
            LobbyNetworkManager.Instance.CreateLobby("Lobby Name");
        });
        PrivateLobbyButton.onClick.AddListener(() => 
        {
            LobbyNetworkManager.Instance.CreateLobby("Lobby Name", true);
        });
    }
    void AssignButtonsOnJoinPressed()
    {
        Debug.Log("Join Lobby Functions Assigned!");

        PublicLobbyButton.onClick.AddListener(() => 
        {
            LobbyNetworkManager.Instance.QuickLobbyJoin();
        });
        PrivateLobbyButton.onClick.AddListener(() => 
        {
            StartCoroutine(ShowRoomCodeInputSection());
        });
        ConfirmJoinPrivateLobbyButton.onClick.AddListener(() => 
        {
            // Join Lobby through Code...
            OnConfirmButtonPressed();
        });
        HideRoomCodeSectionButton.onClick.AddListener(() => 
        {
            StartCoroutine(HideRoomCodeSection());
        });
    }

#region RoomCode Related Functions
    IEnumerator HideRoomCodeSection()
    {
        if(IsRoomCodeSectionVisibale)
        {
            DisableAllButtons();

            RoomCodeSection.DOAnchorPosY(-34, 0.35f).SetEase(Ease.InOutCubic);

            yield return new WaitForSeconds(0.35f);

            IsRoomCodeSectionVisibale = false;

            RoomCodeSection.gameObject.SetActive(false);

            EnableAllButtons();
        }
        else
        {
            yield return null;
        }
    }
    
    IEnumerator ShowRoomCodeInputSection()
    {
        if(IsRoomCodeSectionVisibale)
        {
            StartCoroutine(HideRoomCodeSection());
            yield return null;;
        }
        else
        {
            DisableAllButtons();

            RoomCodeSection.gameObject.SetActive(true);

            RoomCodeSection.DOAnchorPosY(-125, 0.35f).SetEase(Ease.InOutCubic);

            yield return new WaitForSeconds(0.35f);

            IsRoomCodeSectionVisibale = true;

            EnableAllButtons();
        }  
    }
    
    void OnConfirmButtonPressed()
    {
        LobbyNetworkManager.Instance.JoinLobbyWithCode(RoomCodeInputField.text);
    }
#endregion

    IEnumerator LobbySlidingToPanel1()
    {
        CanAnimate = false;

        LobbyOptionsPanel1.DOAnchorPos(InsideScreen, .5f).SetEase(Ease.InOutCubic);
        LobbyOptionsPanel2.DOAnchorPos(OffScreenRight, .5f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.5f);

        EnableAllButtons();

        CanAnimate = true;
    }
    IEnumerator LobbySlidingToPanel2()
    {
        CanAnimate = false;

        LobbyOptionsPanel2.DOAnchorPos(InsideScreen, .5f).SetEase(Ease.InOutCubic);
        LobbyOptionsPanel1.DOAnchorPos(OffScreenLeft, .5f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.5f);

        EnableAllButtons();

        CanAnimate = true;
    }
}
