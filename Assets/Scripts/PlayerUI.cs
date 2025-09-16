using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using Unity.Services.Lobbies.Models;
using DG.Tweening;
using System;

using UnityEngine.SceneManagement;

using Spine.Unity.AttachmentTools;

using Unity.Netcode;

namespace Spine.Unity.Examples {
    public class PlayerUI : NetworkBehaviour
    {
        [SerializeField] int PlayerIndex;
        [SerializeField] RTLTextMeshPro PlayerName;
        [SerializeField] CustomizationContoller SpineUI;

        [SerializeField] GameObject CheckMark;

        // [SerializeField] bool IsMan;

        public bool IsCharacterZoomedIn;

        public Transform OriginalParentTransfrom;
        public Vector3 OriginalPlayerPosition;

        [SerializeField] float BlurSpeed = 1;
        [SerializeField] float AnimationSpeed = 1;

        [SerializeField] [SpineAnimation] public string PrimaryAnim;
        [SerializeField] [SpineAnimation] public string SecondaryAnim;


        [SerializeField] Gender gender = Gender.NotSelected;

        // void Awake() 
        // {
        //     Hide();
        // }

        void Start() 
        {
            OriginalParentTransfrom = this.transform.parent;
            OriginalPlayerPosition = Vector3.zero;

            MainNetworkManager.Instance.OnPlayersInfoNetworkListChanged += HomeMenuManager_OnPlayersInfoNetworkListChanged;
            LobbyManager.Instance.OnReadyChange += LobbyManager_OnReadyChange;

            GetComponent<Button>().onClick.AddListener(() =>
            {
                // BlurTest();
                StartZoomInCharacter(BlurSpeed, AnimationSpeed);
            });

            InitPlayer();
        }

        private void LobbyManager_OnReadyChange(object sender, EventArgs e)
        {
            UpdatePlayer();
        }

        private void HomeMenuManager_OnPlayersInfoNetworkListChanged(object sender, EventArgs e)
        {
            // if (SceneManager.GetActiveScene().ToString() == Loader.Scene.LobbyScene.ToString())
            // {
                if (this == null)
                    return;

                InitPlayer();
                // Debug.Log("Player Init");
            // }      
        }

        public void InitPlayer()
        {
            if (MainNetworkManager.Instance.IsPlayerIndexConnected(PlayerIndex))
            {
                PlayerInfo playerInfo = MainNetworkManager.Instance.GetPlayerInfoFromPlayerIndex(PlayerIndex);

                if(gender == playerInfo.gender)
                {
                    Show();

                    PlayerName.text = playerInfo.PlayerName.ToString();

                    Debug.Log("Client ID = " + playerInfo.ClientId, this.gameObject);
                    // Debug.Log("Player Index = " + PlayerIndex, this.gameObject);
                    // Debug.Log("Player Name = " + playerInfo.PlayerName, this.gameObject);
                    // Debug.Log("Player ID = " + playerInfo.PlayerID, this.gameObject);

                    SpineUI.SetAllCharacterSkins(playerInfo);
                    SpineUI.UpdateSelectedAnimation(PrimaryAnim, SecondaryAnim, .1f);
                }
                else
                {
                    if (this.gameObject != null)
                        Hide();
                }
            }
            else
            {
                if (this != null)
                    Hide();
            }
        }

        public void UpdatePlayer()
        {
            if (MainNetworkManager.Instance.IsPlayerIndexConnected(PlayerIndex))
            {
                PlayerInfo playerInfo = MainNetworkManager.Instance.GetPlayerInfoFromPlayerIndex(PlayerIndex);

                if(gender == playerInfo.gender)
                {
                    // Debug.Log("Show");

                    CheckMark.SetActive(LobbyManager.Instance.IsPlayerReady(playerInfo.ClientId));

                    // Debug.Log($"IsMan = {playerInfo.IsMan}");
                }
            }
        } 


        void Show()
        {
            // if (IsMan == PlayerDataManager.Instance.playerInfo.IsMan)
            
            // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.);

            gameObject.SetActive(true);

            // gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            gameObject.transform.localScale = Vector3.one;
            // gameObject.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
            // {
            //     gameObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutCubic);
            // });
            // gameObject.SetActive(true);
        }

        void Hide()
        {
            if (gameObject != null)
            {
                // Debug.Log("",this.gameObject);
                gameObject.SetActive(false);
            }
        }

        void OnDestroy() 
        {
            // MainNetworkManager.Instance.OnPlayersInfoNetworkListChanged -= HomeMenuManager_OnPlayersInfoNetworkListChanged;
            // LobbyManager.Instance.OnReadyChange -= LobbyManager_OnReadyChange;

            // Debug.Log("PlayerUI: Lobby Scene Destoryed! Unsubbing!");
        }

        // -------------------Lobby Interaction With Players Functions------------------- //

        public void BlurTest()
        {
            LobbyManagerUI.Instance.blurTest.OnBlurStart();

            // gameObject.transform.SetParent(LobbyManagerUI.Instance.uiBlur.gameObject.transform);
        }

        public void StartZoomInCharacter(float _blurSpeed, float _animationsSpeed)
        {
            if (IsCharacterZoomedIn)
                return;

            IsCharacterZoomedIn = true; 

            StartCoroutine(ZoomInToTheCharacter(_blurSpeed, _animationsSpeed));
        }

        public IEnumerator ZoomInToTheCharacter(float _blurSpeed, float _animationsSpeed)
        {
            gameObject.transform.SetParent(LobbyManagerUI.Instance.uiBlur.gameObject.transform);

            LobbyManagerUI.Instance.blurTest.OnBlurStart(_blurSpeed);

            yield return new WaitForSeconds(0.1f);

            gameObject.transform.SetParent(LobbyManagerUI.Instance.blurTest.gameObject.transform);

            // yield return new WaitForSeconds(_blurSpeed);

            

            gameObject.transform.DOScale(new Vector3(2.5f, 2.5f, 2.5f), _animationsSpeed).SetEase(Ease.InOutCubic);

            gameObject.transform.DOLocalMove(new Vector3(0f, 0f, 0f), _animationsSpeed).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                
            });

            yield return null;
        }
    }
}   
