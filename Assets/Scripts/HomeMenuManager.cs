using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using RTLTMPro;


using System.IO;

using Unity.Netcode;
using System;
using System.Text;
using Newtonsoft.Json;

using System.Linq;
using System.ComponentModel.Design;

namespace Spine.Unity.Examples
{
    public class HomeMenuManager : NetworkBehaviour
    {
        public static HomeMenuManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = GameObject.FindObjectOfType<HomeMenuManager>();

                return _instance;
            }
        }

        private static HomeMenuManager _instance;

        // [Header("Network Reference")]
        // private NetworkList<CharacterSelectState> players;

        [Header("Choosing Chatacrer Info")]
        public GameObject ChoosingChatacrerScreen;
        public bool IsMale;
        public bool IsCharacterSelected;
        public RectTransform Male;
        public RectTransform Female;
        public Vector3 OriginalMalePos;
        public Vector3 OriginalFemalePos;

        public CustomizationContoller MaleCustomizationContoller;
        public CustomizationContoller FemaleCustomizationContoller;

        public CustomizationContoller ActiveCustomizationController;

        public RectTransform MaleItemsHolder;
        public RectTransform FemaleItemsHolder;

        public RectTransform Title;

        public Image BackButton;
        public Image FinishButton;


        [Header("Character Cutomization Info")]
        public GameObject CharacterCutomizationScreen_Male;
        public GameObject CharacterCutomizationScreen_Female;

        public RectTransform CharacterCutomizationPanel_Male;
        public RectTransform CharacterCutomizationPanel_Female;

        public float Items_Y_Offset = 150.25f;

        public Button NextButton;


        [Header("Lobby Screen Info")]
        public GameObject LobbyScreen;
        public GameObject MaleDanceScreen;
        public GameObject FemaleDanceScreen;


        [Header("Settings Info")]
        public RectTransform AccountSettings_Panel;
        public RectTransform MainSettings_Panel;
        public Toggle VFX_Toggle;
        public Toggle Music_Toggle;
        public Image VFXToggle_Image;
        public Image MusicToggle_Image;
        public Sprite ActiveToggle_Sprite;
        public Sprite InactiveToggle_Sprite;

        [Header("Popup Properties")]
        public GameObject SignOutPanel;
        public GameObject PopUpBox_SignOut;
        public GameObject PopUpBox_DeleteAccount;
        public GameObject PopUpBox_ConfirmDeleteAccount;
        // public Button ContinueButton;
        public float Duration = 0.5f;
        public List<AudioClip> ShowPopupAudioClips;
        public AudioClip HidePopupAudioClip;

        public bool StartAtCenter = true;

        [Header("Coins Properites")]
        public int FirstPlaceReword = 500;
        public int SecondPlaceReword = 250;
        public int ThirdPlaceReword = 100;
        public int OtherPlaceReword = 10;

        public PlayerData HomeMenuPlayerData;
        public GameObject CoinPlusIcon;
        public GameObject CoinButtonBlocker;

        public ImageSwipe imageSwipe;

        public bool IsDoubleRewards;

        [Header("Connection Popup Properties")]
        public GameObject Connection_PopupTent;
        public GameObject Connection_PopupContainer;
        public Button Connection_ContinueButton;
        public float Connection_Duration = 0.5f;
        public List<AudioClip> Connection_ShowPopupAudioClips;
        public AudioClip Connection_HidePopupAudioClip;
        public RTLTextMeshPro Connection_Message;


        [Header("Testing")]
        public Button TestAds_Btn;


        // oiR3XKOMuwB8vEIbyAtzko7TJNOe

        void Awake()
        {
            // Init();
        }


        void Start()
        {
            // Initialize Settings.....
            Init();
        }

        void Init()
        {
            Connection_ContinueButton.onClick.AddListener(() =>
            {
                CloseFaildToJoinRoom_Popup();
            });

            TestAds_Btn.onClick.AddListener(() =>
            {
                if (MediationAdvertismentsBase.Instance.IsRewardedAdAvailable())
                {
                    MediationAdvertismentsBase.Instance.ShowRewardBasedVideo((shouldReward) =>
                    {
                        if (!shouldReward)
                        {
                            Debug.Log("Rewarded Ad Not Complete");
                            return;
                        }

                        Debug.Log("Rewarded Ad => Completed Successfully!");

                    });
                }
                else
                {
                    Debug.Log("Rewarded Ad Not Availabe");
                }
            });

            Debug.Log("Init HomeManager");

            OriginalFemalePos = Female.localPosition;
            OriginalMalePos = Male.localPosition;

            // if (GlobalManager.Instance.CanPlayMusic)
            // {
            //     Music_Toggle.isOn = true;
            //     StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));
            // }

            // if (GlobalManager.Instance.CanPlaySFX)
            //     VFX_Toggle.isOn = true;

            // StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));

            if (PlayerPrefManager.PlayerInternalSettings.GetMusicOn())
            {
                Music_Toggle.isOn = true;
                // StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.CharacterCreation));
            }
            else
            {
                Music_Toggle.isOn = false;
            }

            if (PlayerPrefManager.PlayerInternalSettings.GetVFXOn())
            {
                VFX_Toggle.isOn = true;
            }
            else
            {
                VFX_Toggle.isOn = false;
            }

            HomeMenuPlayerData = PlayerDataManager.Instance.GetClonedPlayerData();

            ItemsManager.Instance.OwnedItems = HomeMenuPlayerData.OwnedItems;
            Debug.Log("ItemsManager.Instance.OwnedItems.Count = " + ItemsManager.Instance.OwnedItems.Count);
            // foreach (var v in ItemsManager.Instance.OwnedItems)
            // {
            //     Debug.Log("ItemsManager - " + v.ItemID + " / " + "IsPurchased = " + v.IsPurchased + " / " + "IsEquipped = " + v.IsEquipped);
            // }
            ItemsManager.Instance.SetUpItems();

            if (!GlobalManager.Instance.IsNewPlayer)
            {
                SkipPlayerCustomization();
            }
            else
            {
                StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.CharacterCreation));
            }

            AccountSettings_Panel.localPosition = new Vector2(UIManagerTheUltimate.Instance.RightStartPosition, 0);

            NextButton.gameObject.SetActive(false);

            LobbyScreen.SetActive(GlobalManager.Instance.isLobby);

            // MusicoggleFunction();
        }

        void SetUpDancesScreens()
        {
            if (HomeMenuPlayerData.playerInfo.gender == Gender.Male)
            {
                MaleDanceScreen.SetActive(true);
                FemaleDanceScreen.SetActive(false);
            }

            else if (HomeMenuPlayerData.playerInfo.gender == Gender.Female)
            {
                FemaleDanceScreen.SetActive(true);
                MaleDanceScreen.SetActive(false);
            }
        }

        // ------------------------Choose Character Section------------------------ //
        public void OnMaleSelected()
        {
            //Play Button Audios..
            AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            //------------


            //This is for testing "Saving Data" only..
            // PlayerInfo playerInfo = await MainNetworkManager.Instance.RetrieveSpecificData<PlayerInfo>("PLAYER_INFO");
            // Debug.Log("playerInfo data is: " + playerInfo.PlayerName);
            //-------------------------------


            NextButton.gameObject.SetActive(true);

            IsCharacterSelected = true;
            IsMale = true;

            // old...
            PlayerDataManager.Instance.playerData.IsMan = true;

            // new...
            // PlayerDataManager.Instance.playerData.playerInfo.gender =  global::Gender.Male;

            HomeMenuPlayerData.playerInfo.gender = global::Gender.Male;

            GlobalManager.Instance.IsMale = true;

            // Male.GetComponent<Image>().DOFade(1 , .3f);

            // Female.GetComponent<Image>().DOFade(0.5f , .3f);

            ActiveCustomizationController = MaleCustomizationContoller;

            Female.DOScale(Vector3.one, 0.4f).SetEase(Ease.InOutCubic);
            Male.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.4f).SetEase(Ease.InOutCubic);
        }

        public void OnFemaleSelected()
        {
            //Play Button Audios..
            AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            //------------

            NextButton.gameObject.SetActive(true);

            IsCharacterSelected = true;
            IsMale = false;

            // old...
            PlayerDataManager.Instance.playerData.IsMan = false;

            // new...
            // PlayerDataManager.Instance.playerData.playerInfo.gender =  global::Gender.Female;

            HomeMenuPlayerData.playerInfo.gender = global::Gender.Female;

            GlobalManager.Instance.IsMale = false;

            // Female.GetComponent<Image>().DOFade(1 , .3f);
            // Male.GetComponent<Image>().DOFade(0.5f , .3f);

            ActiveCustomizationController = FemaleCustomizationContoller;

            Male.DOScale(Vector3.one, 0.4f).SetEase(Ease.InOutCubic);
            Female.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.4f).SetEase(Ease.InOutCubic);
        }

        public void OnContinueClicked()
        {
            // //Play Button Audios..
            // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            // //------------

            // if(IsCharacterSelected && IsMale)
            // {
            //     AnimatePanelIn(CharacterCutomizationPanel_Male);
            // }
            // else if(IsCharacterSelected & !IsMale)
            // {
            //     AnimatePanelIn(CharacterCutomizationPanel_Female);
            // }

            AnimateCharacterIn(Items_Y_Offset);
        }

        public void OnCharCustomizationBack()
        {
            // //Play Button Audios..
            // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            // //------------

            if (IsMale)
            {
                AnimatePanelOut(CharacterCutomizationPanel_Male);
            }
            else
            {
                AnimatePanelOut(CharacterCutomizationPanel_Female);
            }
        }



        public void SkipPlayerCustomization()
        {
            Debug.Log("SkipPlayerCustomization!");

            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));

            CoinButtonBlocker.SetActive(false);
            CoinPlusIcon.SetActive(true);

            ChoosingChatacrerScreen.SetActive(false);
            // CharacterCutomizationScreen_Female.SetActive(false);
            // CharacterCutomizationScreen_Male.SetActive(false);

            // StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));

            // if(HomeMenuPlayerData.playerInfo.gender == global::Gender.Male)
            // {
            //     MaleDanceScreen.SetActive(true);
            //     FemaleDanceScreen.SetActive(false);
            // }

            // else if (HomeMenuPlayerData.playerInfo.gender == global::Gender.Female)
            // {
            //     FemaleDanceScreen.SetActive(true);
            //     MaleDanceScreen.SetActive(false);
            // }

            SetUpDancesScreens();

            LobbyScreen.SetActive(true);

            GlobalManager.Instance.isLobby = true;

            GoToHomeScreen();
        }

        public async void OnCharCustomiztationFinishClick()
        {
            CoinButtonBlocker.SetActive(false);
            CoinPlusIcon.SetActive(true);

            ChoosingChatacrerScreen.SetActive(false);
            // CharacterCutomizationScreen_Female.SetActive(false);
            // CharacterCutomizationScreen_Male.SetActive(false);

            UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));

            /*// Create a temp PlayerData to assign and save later...
            // PlayerData playerData = PlayerDataManager.Instance.playerData;

            // if(HomeMenuPlayerData.playerInfo.gender == Gender.Male)
            // {
            //     // Old...
            //     PlayerDataManager.Instance.playerInfo.IsMan = true;

            //     // old...
            //     PlayerDataManager.Instance.playerData.IsMan = true;

            //     // // new...
            //     // PlayerDataManager.Instance.playerData.playerInfo.gender =  global::Gender.Male;

            //     // test...
            //     HomeMenuPlayerData.playerInfo.gender = global::Gender.Male;
    
            //     // MaleDanceScreen.SetActive(true);
            //     // FemaleDanceScreen.SetActive(false);
            // }
                
            // else
            // {
            //     // Old...
            //     PlayerDataManager.Instance.playerInfo.IsMan = false;

            //     // old...
            //     PlayerDataManager.Instance.playerData.IsMan = false;

            //     // // new...
            //     // PlayerDataManager.Instance.playerData.playerInfo.gender =  global::Gender.Female;

            //     // test...
            //     HomeMenuPlayerData.playerInfo.gender = global::Gender.Female;
    
            //     // FemaleDanceScreen.SetActive(true);
            //     // MaleDanceScreen.SetActive(false);
            // }*/

            // Check if the two lists are equal
            bool listsAreEqual = AreItemListsEqual(HomeMenuPlayerData.OwnedItems, PlayerDataManager.Instance.playerData.OwnedItems) &&
                                                    HomeMenuPlayerData.playerInfo.gender == PlayerDataManager.Instance.playerData.playerInfo.gender;

            Debug.Log(listsAreEqual);

            if (!listsAreEqual)
            {
                Debug.Log(listsAreEqual);
                PlayerDataManager.Instance.SetPlayerData(HomeMenuPlayerData);
                await PlayerDataManager.Instance.SaveDataObject(PlayerDataManager.Instance.PLAYERDATA, PlayerDataManager.Instance.playerData);

                Debug.Log("New PlayerData Has Been Saved!");
            }
            else
            {
                Debug.Log("Same PlayerData Was Found! Nothing To Save.");
            }

            SetUpDancesScreens();

            // ActiveCustomizationController

            LobbyScreen.SetActive(true);

            GlobalManager.Instance.isLobby = true;

            GoToHomeScreen();
        }


        static bool AreItemListsEqual(List<ItemData> list1, List<ItemData> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                {
                    Debug.Log(JSONUtilityManager.ToJson(list1[i]) + " -> is not equal to -> " + JSONUtilityManager.ToJson(list2[i]));

                    return false;
                }
            }

            return true;
        }

        public void OnCharCustomiztationFinishClick_New()
        {
            // //Play Button Audios..
            // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            // //------------

            ChoosingChatacrerScreen.SetActive(false);
            // CharacterCutomizationScreen_Female.SetActive(false);
            // CharacterCutomizationScreen_Male.SetActive(false);

            UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));


            if (PlayerDataManager.Instance.playerData.IsMan)
            {
                // Old...
                // PlayerDataManager.Instance.playerInfo.IsMan = true;

                // New...
                // PlayerDataManager.Instance.playerData.IsMan = true;

                MaleDanceScreen.SetActive(true);
                FemaleDanceScreen.SetActive(false);
            }

            else
            {
                // Old...
                // PlayerDataManager.Instance.playerInfo.IsMan = false;

                // New...
                // PlayerDataManager.Instance.playerData.IsMan = false;

                FemaleDanceScreen.SetActive(true);
                MaleDanceScreen.SetActive(false);
            }

            LobbyScreen.SetActive(true);

            GlobalManager.Instance.isLobby = true;
        }

        public void GoToCharacterCustomizationScreen()
        {
            CoinButtonBlocker.SetActive(true);
            CoinPlusIcon.SetActive(false);

            LobbyScreen.SetActive(false);
            GlobalManager.Instance.isLobby = false;

            ChoosingChatacrerScreen.SetActive(true);
            // CharacterCutomizationScreen_Female.SetActive(true);
            // CharacterCutomizationScreen_Male.SetActive(true);

            UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.CharacterCreation));
        }


        public void GoToHomeScreen()
        {
            imageSwipe.GoToHomeScreen();
        }


        public void AnimatePanelIn(RectTransform _rect)
        {
            _rect.DOAnchorPosX(0, 0.6f).SetEase(Ease.InOutCubic);
        }
        public void AnimatePanelOut(RectTransform _rect)
        {
            _rect.DOAnchorPosX(UIManagerTheUltimate.Instance.RightStartPosition, 0.6f).SetEase(Ease.InOutCubic);
        }

        // ----------------------------New Character Customization Animations---------------------------- //
        public void AnimateCharacterIn(float _anchorPosY = 150.25f)
        {
            AudioManager.Instance.PlayAudioClip(AudioManager.Instance.ChoosingCharacterSFX);

            if (IsMale)
            {
                MaleCustomizationContoller.UpdateSelectedAnimation(MaleCustomizationContoller.IdleAnim, MaleCustomizationContoller.SelectedAnim);

                Male.DOAnchorPosX(UIManagerTheUltimate.Instance.CenterStartPosition, 0.6f).SetEase(Ease.InOutCubic);
                Female.DOAnchorPosX(UIManagerTheUltimate.Instance.RightStartPosition, 0.6f).SetEase(Ease.InOutCubic);

                Male.DOAnchorPosY(80, 0.6f).SetEase(Ease.InOutCubic);
                Male.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.6f).SetEase(Ease.InOutCubic);

                Title.DOAnchorPosY(100, 0.6f).SetEase(Ease.InOutCubic);

                NextButton.GetComponent<Button>().interactable = false;

                NextButton.gameObject.SetActive(false);

                MaleItemsHolder.DOAnchorPosY(_anchorPosY, 0.6f).SetEase(Ease.InOutCubic);

                FinishButton.DOFade(1, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    FinishButton.GetComponent<Button>().interactable = true;
                });

                BackButton.DOFade(1, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    BackButton.GetComponent<Button>().interactable = true;
                });
            }

            else
            {
                FemaleCustomizationContoller.UpdateSelectedAnimation(FemaleCustomizationContoller.IdleAnim, FemaleCustomizationContoller.SelectedAnim);

                Male.DOAnchorPosX(UIManagerTheUltimate.Instance.LeftStartPosition, 0.6f).SetEase(Ease.InOutCubic);
                Female.DOAnchorPosX(UIManagerTheUltimate.Instance.CenterStartPosition, 0.6f).SetEase(Ease.InOutCubic);

                Female.DOAnchorPosY(80, 0.6f).SetEase(Ease.InOutCubic);
                Female.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.6f).SetEase(Ease.InOutCubic);

                Title.DOAnchorPosY(100, 0.6f).SetEase(Ease.InOutCubic);

                NextButton.GetComponent<Button>().interactable = false;

                NextButton.gameObject.SetActive(false);

                FemaleItemsHolder.DOAnchorPosY(_anchorPosY, 0.6f).SetEase(Ease.InOutCubic);

                FinishButton.DOFade(1, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    FinishButton.GetComponent<Button>().interactable = true;
                });

                BackButton.DOFade(1, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    BackButton.GetComponent<Button>().interactable = true;
                });
            }
        }

        public void AnimateCharacterOut()
        {
            Male.DOAnchorPos(OriginalMalePos, 0.6f).SetEase(Ease.InOutCubic);
            Female.DOAnchorPos(OriginalFemalePos, 0.6f).SetEase(Ease.InOutCubic);
            Male.DOScale(Vector3.one, 0.6f).SetEase(Ease.InOutCubic);
            Female.DOScale(Vector3.one, 0.6f).SetEase(Ease.InOutCubic);

            Title.DOAnchorPosY(-80, 0.6f).SetEase(Ease.InOutCubic);

            MaleItemsHolder.DOAnchorPosY(-150.25f, 0.6f).SetEase(Ease.InOutCubic);
            FemaleItemsHolder.DOAnchorPosY(-150.25f, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                NextButton.GetComponent<Button>().interactable = true;

                NextButton.gameObject.SetActive(true);
            });

            FinishButton.DOFade(0, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                FinishButton.GetComponent<Button>().interactable = false;
            });
            BackButton.DOFade(0, 0.6f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                BackButton.GetComponent<Button>().interactable = false;
            });
        }

        //-----------------------------GamePlay Info-----------------------------//


        //-----------------------------Settings Info-----------------------------//
        public void OnAccountSettingsClicked()
        {
            UIAnimator.Instance.Move_X_Animation(MainSettings_Panel, UIManagerTheUltimate.Instance.LeftStartPosition, 0.5f);
            UIAnimator.Instance.Move_X_Animation(AccountSettings_Panel, UIManagerTheUltimate.Instance.CenterStartPosition, 0.5f);
        }

        public void OnAccountSettingsBackClicked()
        {
            UIAnimator.Instance.Move_X_Animation(MainSettings_Panel, UIManagerTheUltimate.Instance.CenterStartPosition, 0.5f);
            UIAnimator.Instance.Move_X_Animation(AccountSettings_Panel, UIManagerTheUltimate.Instance.RightStartPosition, 0.5f);
        }

        public void VFXToggleFunction()
        {
            //Play Button Audios..
            AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            //------------

            if (VFX_Toggle.isOn)
            {
                VFXToggle_Image.transform.DOLocalMoveX(-23, 0.4f).SetEase(Ease.OutCubic);
                VFXToggle_Image.sprite = ActiveToggle_Sprite;

                //Add aditional logic here for the SFX toggle ON.
                AudioManager.Instance.AS.volume = 0.6f;
                PlayerPrefManager.PlayerInternalSettings.SetVFXOn(0);

                Debug.Log("PlayerPrefManager.PlayerInternalSettings.GetVFXOn(): " + PlayerPrefManager.PlayerInternalSettings.GetVFXOn());
            }
            else
            {
                VFXToggle_Image.transform.DOLocalMoveX(23, 0.4f).SetEase(Ease.OutCubic);
                VFXToggle_Image.sprite = InactiveToggle_Sprite;

                //Add aditional logic here for the SFX toggle OFF.
                AudioManager.Instance.AS.volume = 0;
                PlayerPrefManager.PlayerInternalSettings.SetVFXOn();

                Debug.Log("PlayerPrefManager.PlayerInternalSettings.GetVFXOn(): " + PlayerPrefManager.PlayerInternalSettings.GetVFXOn());
            }
        }

        public void MusicoggleFunction()
        {
            //Play Button Audios..
            AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
            //------------

            if (Music_Toggle.isOn)
            {
                MusicToggle_Image.transform.DOLocalMoveX(-23, 0.4f).SetEase(Ease.OutCubic);
                MusicToggle_Image.sprite = ActiveToggle_Sprite;

                //Add aditional logic here for the Music toggle ON.
                AudioManager.Instance.Background_AS.volume = 0.2f;
                if (!AudioManager.Instance.Background_AS.isPlaying)
                    StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));
                // GlobalManager.Instance.CanPlayMusic = true;
                PlayerPrefManager.PlayerInternalSettings.SetMusicOn(0);

                Debug.Log("PlayerPrefManager.PlayerInternalSettings.GetMusicOn(): " + PlayerPrefManager.PlayerInternalSettings.GetMusicOn());
            }
            else
            {
                MusicToggle_Image.transform.DOLocalMoveX(23, 0.4f).SetEase(Ease.OutCubic);
                MusicToggle_Image.sprite = InactiveToggle_Sprite;

                //Add aditional logic here for the Music toggle OFF.
                AudioManager.Instance.Background_AS.volume = 0f;
                // GlobalManager.Instance.CanPlaySFX = false;
                PlayerPrefManager.PlayerInternalSettings.SetMusicOn();

                Debug.Log("PlayerPrefManager.PlayerInternalSettings.GetMusicOn(): " + PlayerPrefManager.PlayerInternalSettings.GetMusicOn());
            }
        }

        public void CleanUp()
        {
            if (MainNetworkManager.Instance != null)
                Destroy(MainNetworkManager.Instance.gameObject);

            if (NetworkManager.Singleton != null)
                Destroy(NetworkManager.Singleton.gameObject);

            if (LobbyNetworkManager.Instance != null)
                Destroy(LobbyNetworkManager.Instance.gameObject);
        }

        public void OnSignOutClick() // Assigned in the inspector.
        {
            UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

            CleanUp();

            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.GameStart));

            switch (AuthenticationManager.Instance.userLoggedInState)
            {
                case UserLoggedInState.Anonymously:
                    AuthenticationManager.Instance.GuestSignOut();
                    PlayerDataManager.Instance.playerData = new PlayerData();
                    // Loader.Load(Loader.Scene.RegistrationScreen);
                    break;


                case UserLoggedInState.Google:
                    PlayerDataManager.Instance.playerData = new PlayerData();
                    AuthenticationManager.Instance.GoogleSignOut();
                    // Loader.Load(Loader.Scene.RegistrationScreen);
                    break;


                case UserLoggedInState.Apple:
                    PlayerDataManager.Instance.playerData = new PlayerData();
                    AuthenticationManager.Instance.AppleSignOut();
                    // Loader.Load(Loader.Scene.RegistrationScreen);
                    break;
            }
        }


        public void ShowSignOutPopup() // Assigned in the inspector.
        {
            SignOutPanel.SetActive(true);
            AudioManager.Instance.PlayRandomAudioFromPool(ShowPopupAudioClips);
            PopUpBox_SignOut.transform.DOScale(Vector3.one, Duration).SetEase(Ease.OutBounce, 2);
        }
        public void CloseSignOutPopup() // Assigned in the inspector.
        {
            AudioManager.Instance.PlayAudioClip(HidePopupAudioClip);
            PopUpBox_SignOut.transform.DOScale(Vector2.zero, Duration).SetEase(Ease.InBounce, 2).OnComplete(() =>
            {
                SignOutPanel.gameObject.SetActive(false);
            });
        }

        public void ShowDeleteAccountPopUp()
        {
            SignOutPanel.SetActive(true);
            AudioManager.Instance.PlayRandomAudioFromPool(ShowPopupAudioClips);
            PopUpBox_DeleteAccount.transform.DOScale(Vector3.one, Duration).SetEase(Ease.OutBounce, 2);
        }
        public void CloseDeleteAccountPopUp()
        {
            AudioManager.Instance.PlayAudioClip(HidePopupAudioClip);

            PopUpBox_DeleteAccount.transform.DOScale(Vector2.zero, Duration).SetEase(Ease.InBounce, 2).OnComplete(() =>
            {
                SignOutPanel.gameObject.SetActive(false);
            });
        }

        public void CloseDeleteAccountPopUp_Internally(bool _playSFXOnClose = true, bool _closeSignOutPanel = true)
        {
            if (_playSFXOnClose)
                AudioManager.Instance.PlayAudioClip(HidePopupAudioClip);

            PopUpBox_DeleteAccount.transform.DOScale(Vector2.zero, Duration).SetEase(Ease.InBounce, 2).OnComplete(() =>
            {
                if (_closeSignOutPanel)
                    SignOutPanel.gameObject.SetActive(false);
            });
        }

        public void ShowConfirmDeletionPopUp()
        {
            CloseDeleteAccountPopUp_Internally(false, false);

            AudioManager.Instance.PlayRandomAudioFromPool(ShowPopupAudioClips);
            PopUpBox_ConfirmDeleteAccount.transform.DOScale(Vector3.one, Duration).SetEase(Ease.OutBounce, 2);
        }

        public void CloseConfirmDeletionPopUp()
        {
            AudioManager.Instance.PlayAudioClip(HidePopupAudioClip);

            PopUpBox_ConfirmDeleteAccount.transform.DOScale(Vector2.zero, Duration).SetEase(Ease.InBounce, 2).OnComplete(() =>
            {
                SignOutPanel.gameObject.SetActive(false);
            });
        }

        public void ChangeUserName()
        {
            GlobalManager.Instance.IsChangeUserNameSection = true;
            GlobalManager.Instance.isLobby = false;
            UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
            MainNetworkManager.Instance.CleanUp();

            Loader.Load(Loader.Scene.RegistrationScreen);
        }

        public void DeleteAccount()
        {
            // OnSignOutClick();
            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.GameStart));

            GlobalManager.Instance.IsNewPlayer = true;
            GlobalManager.Instance.isLobby = false;
            UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
            AuthenticationManager.Instance.DeletePlayaerAccount();
        }

        // ------------------------------- Pop-up ------------------------------- //

        public void ShowFaildToJoinRoom_Popup()
        {
            Connection_PopupTent.SetActive(true);
            AudioManager.Instance.PlayRandomAudioFromPool(Connection_ShowPopupAudioClips);
            Connection_PopupContainer.transform.DOScale(Vector3.one, Connection_Duration).SetEase(Ease.OutBounce, 2);
        }
        public void CloseFaildToJoinRoom_Popup()
        {
            AudioManager.Instance.PlayAudioClip(Connection_HidePopupAudioClip);
            Connection_PopupContainer.transform.DOScale(Vector2.zero, Connection_Duration).SetEase(Ease.InBounce, 2).OnComplete(() =>
            {
                Connection_PopupTent.gameObject.SetActive(false);
            });
        }
    }
}
