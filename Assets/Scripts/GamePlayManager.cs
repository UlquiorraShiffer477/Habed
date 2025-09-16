using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RTLTMPro;
using TMPro;
using Unity.Netcode;

using System.Linq;


using System;
using UnityEngine.SceneManagement;

using ScorpionSteps;
using UnityEngine.EventSystems;

using Spine.Unity.Examples;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Authentication;
using DA_Assets.Shared.CodeHelpers;
using Unity.Collections;

public class GamePlayManager : NetworkBehaviour
{
    #region Instance
    // ----------------------Instance Section---------------------- //
    private static GamePlayManager _instance;

    public static GamePlayManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<GamePlayManager>();

            return _instance;
        }
    }
    // ----------------------Instance Section---------------------- //
    #endregion


    #region Events...
    public event EventHandler OnSceneLoaded;
    public event EventHandler OnPlayerAnswerReady;
    public event EventHandler OnPlayerChoosingAnswer;

    public event EventHandler OnStateChanged;
    public event EventHandler OnPlayersInSessionListChanged;

    public event EventHandler OnNextInstanceStarted;
    #endregion

    public enum State
    {
        Pending,
        GameStart,
        AddingAnswers,
        StartQuestionCounter,
        ChoosingAnswers,
        ShowingAnswers,
        Results,
        NextQuestion,
        EndRoundResults,
        NextRound,
        FinalResults,
        GameEnd,
        GamePaused
    }

    [Header("Network Valus")]
    public NetworkVariable<State> state = new NetworkVariable<State>(State.Pending, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [HideInInspector] public NetworkVariable<bool> IsGamePaused = new NetworkVariable<bool>(false);

    [HideInInspector] public NetworkVariable<float> Test_NetworkVariable = new NetworkVariable<float>(0f);

    public List<PlayerSessionInfo> OutOfFocusPlayers = new List<PlayerSessionInfo>();


    [Header("Lists")]
    public List<PlayerSessionInfo> OriginalPlayersInSession = new List<PlayerSessionInfo>();
    public List<PlayerSessionInfo> PlayersInSessionList = new List<PlayerSessionInfo>();

    [Header("Players Answers"), Space]
    public List<PlayerAnswerGroup> GroupedPlayerAnswers = new List<PlayerAnswerGroup>();



    [Header("Main Screens")]
    public GameObject RoundAnounser;
    public GameObject AddingAnswersScreen;
    public GameObject ChoosingAnswerScreen;
    public GameObject ShowingAnswersScreen;
    public GameObject RoundResultsScreen;
    public GameObject FinalResultsScreen;
    public GameObject WinnerScreen;


    [Header("Input Fields")]
    public TMP_InputField AnswerField;


    [Header("Others")]
    public GameObject RayCastPanel;
    public RectTransform AnswerHolder;
    public RTLTextMeshPro AnswerShowing;
    public List<RectTransform> NotClicked_Positions;
    public Button BackToHomeButon;
    [SerializeField] Button FinalResultsBackButton;

    [SerializeField] NetworkVariable<int> DefaultAnswerCounter = new NetworkVariable<int>(0);

    [Header("Managers")]
    public RoundManager roundManager;
    public RoomSessionManager roomSessionManager;

    [Header("Local Player Info")]
    public Dictionary<ulong, bool> PlayerAnswerReadyDictionary;
    public Dictionary<ulong, bool> PlayerChoosenAnswerReadyDictionary;
    public Dictionary<ulong, bool> PlayerSceneReadyDictionary;
    public Dictionary<ulong, bool> PlayerQuestionAudioDonePlayingeReadyDictionary;
    public Dictionary<ulong, bool> PlayerPauseReadyDictionary;


    [Header("Buttons")]
    public GameObject NextQuestionButton;
    public RTLTextMeshPro NextQuestionButton_Text;


    [Header("Popup Properties"), Space]
    public GameObject ExitGamePanel;
    public GameObject PopUpBox_ExitGame;
    public float Duration = 0.5f;
    public List<AudioClip> ShowPopupAudioClips;
    public AudioClip HidePopupAudioClip;

    [Header("End Game Rewarded PopUp")]
    public GameObject RewardedPanel;
    public GameObject PopUpBox_Rewarded;
    public float Rewarded_Duration = 0.5f;
    public List<AudioClip> Rewarded_ShowPopupAudioClips;
    public AudioClip Rewarded_HidePopupAudioClip;

    [Header("Loading screen Related"), Space]
    public GameObject LoadingScreen;


    [Header("Disconnection Properites"), Space]
    public GameObject GoBackToMainMenuAfterDisconnectingPanel;
    public GameObject PlayerDisconnectedMessage;

    public GameObject DisconnectionPanel;
    public bool IsGameOutOfFocuse = false;


    public bool areAnswersCreated;


    void Awake()
    {
        // OutOfFocusPlayers = new NetworkList<PlayerInfo>();

        BackToHomeButon.onClick.AddListener(() =>
        {
            ShowExitGamePopUp();
        });

        FinalResultsBackButton.GetComponent<UIAnimatedObject>().advanceOptions.callBackFunction.AddListener(() =>
        {
            ShowRewardedPopUp();
        });

        // PlayerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerAnswerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerChoosenAnswerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerSceneReadyDictionary = new Dictionary<ulong, bool>();
        PlayerQuestionAudioDonePlayingeReadyDictionary = new Dictionary<ulong, bool> { { default, false } };
        PlayerPauseReadyDictionary = new Dictionary<ulong, bool>();
    }

    #region Rewarded PopUp SetUp

    public void ShowRewardedPopUp()
    {
        RewardedPanel.SetActive(true);
        AudioManager.Instance.PlayRandomAudioFromPool(Rewarded_ShowPopupAudioClips);
        PopUpBox_Rewarded.transform.DOScale(Vector3.one, Rewarded_Duration).SetEase(Ease.OutBounce, 2).SetUpdate(true);
    }
    public void DeclineRewardedAd()
    {
        // GlobalManager.Instance.LastGameRank = GetPlayerSessionInfoFromClientID(NetworkManager.Singleton.LocalClientId).PlayerRank;
        Debug.Log(GlobalManager.Instance.LastGameRank);
        GlobalManager.Instance.CanAddCoins = true;

        LeaveGame();
    }

    public void WatchRewardedAd()
    {
        if (MediationAdvertismentsBase.Instance.IsRewardedAdAvailable())
        {
            MediationAdvertismentsBase.Instance.ShowRewardBasedVideo((shouldReward) =>
            {
                if (!shouldReward)
                {
                    Debug.Log("Rewarded Ad Not Complete");

                    // GlobalManager.Instance.LastGameRank = GetPlayerSessionInfoFromClientID(NetworkManager.Singleton.LocalClientId).PlayerRank;
                    Debug.Log(GlobalManager.Instance.LastGameRank);
                    GlobalManager.Instance.CanAddCoins = true;

                    LeaveGame();
                    
                    return;
                }

                // GlobalManager.Instance.LastGameRank = GetPlayerSessionInfoFromClientID(NetworkManager.Singleton.LocalClientId).PlayerRank;
                Debug.Log(GlobalManager.Instance.LastGameRank);
                GlobalManager.Instance.CanAddCoins = true;
                GlobalManager.Instance.IsDoubleRewards = true;

                LeaveGame();
            });
        }
        else
        {
            Debug.Log("Rewarded Ad Not Availabe");
        }
    }

    #endregion

    public void InitDictionaries()
    {
        PlayerAnswerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerChoosenAnswerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerSceneReadyDictionary = new Dictionary<ulong, bool>();
        PlayerQuestionAudioDonePlayingeReadyDictionary = new Dictionary<ulong, bool>();
        PlayerPauseReadyDictionary = new Dictionary<ulong, bool>();
    }

    void Start()
    {
        MainNetworkManager.Instance.BufferingScreen.SetActive(false);

        Debug.Log(NetworkManager.Singleton.LocalClientId);

        RayCastPanel.SetActive(false);

        AnswerField.onSubmit.AddListener(AssignAnswer);
        AnswerField.onSelect.AddListener(RoundManager.Instance.AssignKeyboardType);
        AnswerField.onDeselect.AddListener(OnAnswerFieldDeselect);


        StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.EnterLies));

        if (!IsServer)
            NextQuestionButton.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // _ = SubscribeToLobbyEventsAsync();

        IsGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        state.OnValueChanged += State_OnValueChanged;
        OnStateChanged += State_OnStateChangedAction;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect_GamePlayManager;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (IsGamePaused.Value)
        {
            PauseGame();
        }
        else
        {
            UnPauseGame();
        }
    }

    public void OnClientDisconnect_GamePlayManager(ulong _clientID)
    {
        OnPlayerDisconnectClientRpc(_clientID);

        if (Instance)
        {
            if (OriginalPlayersInSession != null && OriginalPlayersInSession.Count != 0)
            {
                for (int i = 0; i < OriginalPlayersInSession.Count; i++)
                {
                    PlayerSessionInfo playerSessionInfo = OriginalPlayersInSession[i];
                    if (playerSessionInfo.ClientId == _clientID)
                    {
                        //Disconnect!
                        OriginalPlayersInSession.RemoveAt(i);

                        // LobbyNetworkManager.Instance.LeaveLobby();
                        // NetworkManager.Singleton.Shutdown();
                        // CleanUp();

                        Debug.Log($"OriginalPlayersInSession Count Is: {OriginalPlayersInSession.Count}");
                        break;
                    }
                }
            }
        }

        Debug.Log("GamePlayManager: Client diconnected Feedback Fired!");
        StartCoroutine(ShowPlayerDisconnectedMessage());
    }

    [ClientRpc]
    public void OnPlayerDisconnectClientRpc(ulong _clientID)
    {
        if (Instance)
        {
            if (OriginalPlayersInSession != null && OriginalPlayersInSession.Count != 0)
            {
                for (int i = 0; i < OriginalPlayersInSession.Count; i++)
                {
                    PlayerSessionInfo playerSessionInfo = OriginalPlayersInSession[i];
                    if (playerSessionInfo.ClientId == _clientID)
                    {
                        //Disconnect!
                        OriginalPlayersInSession.RemoveAt(i);

                        // LobbyNetworkManager.Instance.LeaveLobby();
                        // NetworkManager.Singleton.Shutdown();
                        // CleanUp();

                        Debug.Log($"OriginalPlayersInSession Count Is: {OriginalPlayersInSession.Count}");
                        break;
                    }
                }
            }
        }

        Debug.Log("GamePlayManager: Client diconnected Feedback Fired!");
        StartCoroutine(ShowPlayerDisconnectedMessage());
    }

    public void GameManager_OnLoadedEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {

    }


    void State_OnValueChanged(State previousValue, State newValue)
    {
        Debug.Log("State Got Changed");
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    void State_OnStateChangedAction(object sender, EventArgs e)
    {
        Debug.Log($"CheckingGameState = {state.Value}");

        switch (state.Value)
        {
            case State.GameStart:
                ShowRoundAnnounser();
                // SetPauseReadyServerRpc(true, default);
                // UnPauseGame();
                break;


            case State.AddingAnswers:
                StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.EnterLies));
                HideAnnouncerPanel();

                // UnPauseGame();
                break;

            case State.StartQuestionCounter:
                GamePlayManagerUI.Instance.ShowQuestionCounterAnimation();

                // UnPauseGame();
                break;

            case State.ChoosingAnswers:
                StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.ChooseLies, 0.2f));
                GamePlayProgressBarTimer.OnTimerEnded -= OnAddingAnswerTimerFinish;
                GamePlayManagerUI.Instance.HideQuestionCounterAnimation();
                StartCoroutine(OnAnswerWritten());

                // UnPauseGame();
                break;

            case State.ShowingAnswers:
                StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.ShowLies, 0.2f));
                GamePlayProgressBarTimer.OnTimerEnded -= OnAnswerChoosenTimerFinish;
                OnAnswerChoosen();

                // UnPauseGame();
                break;

            case State.Results:
                CreateResults();

                // UnPauseGame();
                break;

            case State.NextQuestion:
                StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.EnterLies, 0.2f));
                Debug.Log($"StartNextSubRound");
                StartARound(false);

                // UnPauseGame();
                break;

            case State.EndRoundResults:

                break;

            case State.NextRound:
                StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.EnterLies, 0.2f));
                Debug.Log($"StartNextRound");
                StartARound(true);

                // UnPauseGame();
                break;

            case State.FinalResults:
                RoomSessionManager.Instance.AssignFinalResultsControllerList();

                // UnPauseGame();
                break;

            case State.GamePaused:
                // PauseGame();
                break;
        }
    }

    void ShowRoundAnnounser()
    {
        RoundAnounser.SetActive(true);
    }



    public void SubscribeToLoadingBarTimer()
    {
        // ProgressBarTimer.OnTimerEnded += OnAnswerWritten;
    }


    public void HideAnnouncerPanel()
    {
        //Play Button Audios..
        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        //------------

        AddingAnswersScreen.SetActive(true);

        RoundAnounser.SetActive(false);

        Debug.Log("HideAnnouncer");

        GamePlayProgressBarTimer.OnTimerEnded += OnAddingAnswerTimerFinish;
    }

    public void AssignAnswer(string _value)
    {
        if (_value == "")
            return;

        RayCastPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);

        SetPlayerAnswerReadyServerRpc(PlayerDataManager.Instance.GetPlayerNamePlayerData(), _value);
    }


    public void OnAddingAnswerTimerFinish(object sender, EventArgs e)
    {
        if (!IsServer)
            return;

        Debug.Log("OnAddingAnswerTimerFinish PlayerInfoNetworkList.count = " + MainNetworkManager.Instance.PlayerInfoNetworkList.Count);

        foreach (var v in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            CreateDefaultAsnwerOptionServerRpc(v.PlayerName.ToString(), v.ClientId);
        }
    }

    public void OnAnswerChoosenTimerFinish(object sender, EventArgs e)
    {
        if (!IsServer)
            return;

        Debug.Log("OnAnswerChoosenTimerFinish NEW!");

        foreach (var v in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            SetPlayerChooseReadyOnTimerFinishServerRpc(v.ClientId);
        }
    }


    public void LoadLobbyScene()
    {
        UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
        Loader.Load(Loader.Scene.HomeScreen);
    }

    public void OnAnswerChoosen()
    {
        ShowingAnswersScreen.SetActive(true);
        ChoosingAnswerScreen.SetActive(false);
    }

    public IEnumerator OnAnswerWritten()
    {
        ChoosingAnswerScreen.SetActive(true);
        AddingAnswersScreen.SetActive(false);

        yield return new WaitForSeconds(1);

        Debug.Log("OnAnswerWritten NEW!");
        GamePlayProgressBarTimer.OnTimerEnded += OnAnswerChoosenTimerFinish;
    }

    void CreateResults()
    {
        roomSessionManager.CreateResults();

        // ResetRound();
        if (roundManager.GetRoundCount() == 3)
        {
            BackToHomeButon.gameObject.SetActive(false);
            NextQuestionButton_Text.text = "اظهار الفائز";
            GlobalManager.Instance.CanAddCoins = true;
            GlobalManager.Instance.LastGameRank = GetPlayerSessionInfoFromClientID(NetworkManager.Singleton.LocalClientId).PlayerRank;
        }

        RoundResultsScreen.SetActive(true);
        ShowingAnswersScreen.SetActive(false);
    }


    public IEnumerator AnimateNamesIn(List<RectTransform> _upRect, List<RectTransform> _downRect)
    {
        foreach (var v in _downRect)
        {
            v.DOAnchorPosY(-58f, 0.4f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                AnswerHolder.DOShakeRotation(0.5f, 5);
                AnswerHolder.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.3f, 10, 0.7f);
            });

            v.GetComponent<Image>().DOFade(1, 0.2f);

            yield return new WaitForSeconds(0.4f);
        }

        foreach (var v in _upRect)
        {
            v.DOAnchorPosY(107f, 0.4f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                AnswerHolder.DOShakeRotation(0.5f, 5);
                AnswerHolder.DOPunchScale(new Vector3(1.1f, 1.1f, 1.1f), 0.3f, 10, 0.7f);
            });

            v.GetComponent<Image>().DOFade(1, 0.2f);

            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(1);
    }

    // ---------------------Answers Info--------------------- //
    public void CreateAnswerObject()
    {

    }

    // ---------------------Round Info--------------------- //
    public void RetrunToMainMenu()
    {
        UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

        RoundResultsScreen.SetActive(false);
        FinalResultsScreen.SetActive(false);

        ResetGameplay();

        Loader.Load(Loader.Scene.HomeScreen);
    }

    public void SetStartNextRound()
    {
        Debug.Log($"IsHost: {IsHost}");

        // if (!IsHost)
        //     return;
        if (roundManager.GetRoundCount() == 3)
        {
            WinnerScreen.SetActive(true);
            RoundResultsScreen.SetActive(false);
            return;
        }
        else if (roundManager.GetSubRoundCount() == 3)
        {
            state.Value = State.NextRound;
            return;
        }
        else
        {
            state.Value = State.NextQuestion;
            return;
        }
    }


    public void StartARound(bool _isNewRound = false)
    {
        Debug.Log("NextQuestions");
        BackEndManager.Instance.AssignCurrentQuestionAudioClip();
        Debug.Log("Audio Assigned!");

        PlayerAnswerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerChoosenAnswerReadyDictionary = new Dictionary<ulong, bool>();
        PlayerSceneReadyDictionary = new Dictionary<ulong, bool>();

        Debug.Log("NextQuestions");

        if (RoundManager.Instance.GetRoundCount() == 3)
        {
            Debug.Log($"Round Count Is: {RoundManager.Instance.GetRoundCount()}");
            RoundAnounser.SetActive(false);
            RoundResultsScreen.SetActive(true);
        }

        if (_isNewRound)
        {
            Debug.Log("New Round Just Begun!");

            RoundManager.Instance.SetRoundCount();
            RoundManager.Instance.SetSubRoundCount(true);
        }
        else
        {
            Debug.Log("New Sub Round Just Begun!");

            RoundManager.Instance.SetSubRoundCount();
        }

        RoundManager.Instance.SetRoundName();

        RoomSessionManager.Instance.ResetDependencies();
        RoundManager.Instance.SetRoundScores();

        RoundManager.Instance.AssignQuestionTextFields();

        AnswerField.text = "";

        OnNextInstanceStarted?.Invoke(this, EventArgs.Empty);

        StartCoroutine(ResetScreens(_isNewRound));
    }

    public IEnumerator ResetScreens(bool _isNewRound = false)
    {
        AddingAnswersScreen.SetActive(false);
        ChoosingAnswerScreen.SetActive(false);
        ShowingAnswersScreen.SetActive(false);
        RoundResultsScreen.SetActive(false);
        WinnerScreen.SetActive(false);

        Debug.Log($"ResetScreens: {_isNewRound}");

        if (_isNewRound)
            ShowRoundAnnounser();
        else
        {
            yield return new WaitForSeconds(1f);
            state.Value = State.AddingAnswers;
            // AudioManager.Instance.PlayAudioClip(BackEndManager.Instance.CurrentQuestionAudioClip);
        }
    }

    public void ResetGameplay()
    {
        // roomSessionManager.ResetPlayersScores();
        roundManager.ResetRoundCount();
        roundManager.ResetScore();

        RoundManager.Instance.SetNextQuestion();
    }

    // ---------------------Adding Answers------------------- //



    [ServerRpc(RequireOwnership = false)]
    void CreateAnswerOptionsServerRpc(ServerRpcParams serverRpcParams = default)
    {
        CreateAnswerOptionsClientRpc();
    }

    [ClientRpc]
    void CreateAnswerOptionsClientRpc()
    {
        Debug.Log("Answer Created!");

        RayCastPanel.SetActive(false);

        roomSessionManager.CreateAnswerOptions();
    }

    // ---------------------Scene Ready------------------- //
    public void CheckSceneReadyServerRpcs()
    {
        bool allClientsReady = true;

        foreach (PlayerInfo playerInfo in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            if (!PlayerSceneReadyDictionary.ContainsKey(playerInfo.ClientId) || !PlayerSceneReadyDictionary[playerInfo.ClientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            Debug.Log($"CheckSceneReadyServerRpcs: allClientsReady = {allClientsReady}");
            state.Value = State.GameStart;
        }
    }

    #region Test New RPC
    // ------------------------------------------------------------------------------------------------------------------- //

    [ClientRpc]
    public void Test_SetPlayerSceneReadyClientRpc()
    {
        // Debug.Log("Test_SetPlayerSceneReadyClientRpc");
        Test_SetPlayerSceneReadyServerRpc(PlayerDataManager.Instance.GetPlayerNamePlayerData());
    }

    [ServerRpc(RequireOwnership = false)]
    public void Test_SetPlayerSceneReadyServerRpc(string _playerName, ServerRpcParams serverRpcParams = default)
    {
        PlayerSceneReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        // Debug.Log($"SendTo.Everyone - This ClientId = {serverRpcParams.Receive.SenderClientId}");
        SetPlayerSceneReadyClientRpc(_playerName, serverRpcParams.Receive.SenderClientId);

        if (IsServer)
        {
            // Stop existing coroutine if it's running
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(CheckPlayersReadyAndStartGame());
        }
    }

    private Coroutine coroutine;
    private IEnumerator CheckPlayersReadyAndStartGame()
    {
        while (true)  // Main loop for checking player readiness
        {
            bool allClientsReady = true;

            foreach (var clientId in PlayerSceneReadyDictionary)
            {
                // Debug.Log($"key: {clientId.Key} - clientId: {clientId.Value}");
            }

            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!PlayerSceneReadyDictionary.ContainsKey(clientId) || !PlayerSceneReadyDictionary[clientId])
                {
                    // Debug.Log($"allClientsReady - false: {clientId}");
                    allClientsReady = false;
                    break;
                }
            }

            Debug.Log("allClientsReady: " + allClientsReady);

            if (allClientsReady)
            {
                if (IsServer)
                {
                    state.Value = State.GameStart;
                }
                PlayerSceneReadyDictionary.Clear();  // Use Clear() instead of creating new instance
                SceneLoadedClientRpc();
                yield break;  // Exit the coroutine
            }

            yield return new WaitForSeconds(2);
        }
    }

    // ------------------------------------------------------------------------------------------------------------------- //
    #endregion Test New RPC

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerSceneReadyServerRpc(string _playerName, ServerRpcParams serverRpcParams = default)
    {
        PlayerSceneReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;
        SetPlayerSceneReadyClientRpc(_playerName, serverRpcParams.Receive.SenderClientId);
        bool allClientsReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!PlayerSceneReadyDictionary.ContainsKey(clientId) || !PlayerSceneReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            state.Value = State.GameStart;
            PlayerSceneReadyDictionary = new Dictionary<ulong, bool>();
            SceneLoadedClientRpc();
        }

        Debug.Log("allClientsReady: " + allClientsReady);
    }

    [ClientRpc]
    void SetPlayerSceneReadyClientRpc(string _playerName, ulong _clientId)
    {
        OriginalPlayersInSession.Add(new PlayerSessionInfo
        {
            ClientId = _clientId,
            PlayerName = _playerName,
            playerInfo = MainNetworkManager.Instance.GetPlayerInfoFromClientID(_clientId)
        });

        PlayerSceneReadyDictionary[_clientId] = true;

        Debug.Log($"ClientRpc PlayerSceneReadyDictionary = {PlayerSceneReadyDictionary[_clientId]}");


    }

    [ClientRpc]
    void SceneLoadedClientRpc()
    {
        OnSceneLoaded?.Invoke(this, EventArgs.Empty);
    }


    public void CheckQuestionAudioDonePlayingReadyServerRpcs()
    {
        bool allClientsReady = true;

        foreach (PlayerInfo playerInfo in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            Debug.Log("clientId in NetworkManager.Singleton.ConnectedClientsIds is: " + playerInfo.ClientId);

            if (!PlayerQuestionAudioDonePlayingeReadyDictionary.ContainsKey(playerInfo.ClientId) || !PlayerQuestionAudioDonePlayingeReadyDictionary[playerInfo.ClientId])
            {
                allClientsReady = false;
            }
        }

        if (allClientsReady)
        {
            state.Value = State.StartQuestionCounter;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetQuestionAudioDonePlayingReadyServerRpc(ulong _clientID = 1000, ServerRpcParams serverRpcParams = default)
    {
        SetQuestionAudioDonePlayingReadyClientRpc(_clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId);

        PlayerQuestionAudioDonePlayingeReadyDictionary[_clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId] = true;

        Debug.Log("SetQuestionAudioDonePlayingReadyServerRpc Client ID is: " + (_clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId));

        bool allClientsReady = true;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log("clientId in NetworkManager.Singleton.ConnectedClientsIds is: " + clientId);

            if (!PlayerQuestionAudioDonePlayingeReadyDictionary.ContainsKey(clientId) || !PlayerQuestionAudioDonePlayingeReadyDictionary[clientId])
            {
                allClientsReady = false;
            }
        }

        if (allClientsReady)
        {
            state.Value = State.StartQuestionCounter;
            PlayerQuestionAudioDonePlayingeReadyDictionary = new Dictionary<ulong, bool>();
        }

        Debug.Log("allClientsReady: " + allClientsReady);
    }

    [ClientRpc]
    void SetQuestionAudioDonePlayingReadyClientRpc(ulong _clientId)
    {
        PlayerQuestionAudioDonePlayingeReadyDictionary[_clientId] = true;

        Debug.Log("SetQuestionAudioDonePlayingReadyClientRpc Client ID is: " + _clientId);
    }



    public IEnumerator PlayQuestionAudio()
    {
        // Old...
        // AudioManager.Instance.PlayAudioClip(BackEndManager.Instance.CurrentQuestionAudioClip);

        // New...
        AudioManager.Instance.PlayQuestionAudioClip(BackEndManager.Instance.CurrentQuestionAudioClip);

        yield return new WaitForSeconds(BackEndManager.Instance.CurrentQuestionAudioClip.length);

        if (!IsServer)
            yield break;

        foreach (var v in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            SetQuestionAudioDonePlayingReadyServerRpc(v.ClientId);
        }
    }





    // ---------------------Check Marks------------------- //
    public void CheckDefaultAsnwerOptionServerRpcs()
    {
        bool allClientsReady = true;
        foreach (PlayerInfo playerInfo in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            if (!PlayerAnswerReadyDictionary.ContainsKey(playerInfo.ClientId) || !PlayerAnswerReadyDictionary[playerInfo.ClientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerOptionsServerRpc();
            state.Value = State.ChoosingAnswers;
        }
    }


    // [ServerRpc(RequireOwnership = false)]
    // public void CreateDefaultAsnwerOptionServerRpc(string _playerName, ulong _clientID = 1000, ServerRpcParams serverRpcParams = default)
    // {
    //     Debug.Log("CreateDefaultAsnwerOptionServerRpc(): " + " _playerName = " + _playerName + " _clientID = " + _clientID + "  SenderClientId = " + serverRpcParams.Receive.SenderClientId);

    //     if (_clientID != 1000)
    //         if (IsPlayerAnswerReady(_clientID))
    //             return;

    //     CreateDefaultAsnwerOptionClientRpc(_playerName, _clientID != 1000? _clientID :serverRpcParams.Receive.SenderClientId);

    //     PlayerAnswerReadyDictionary[_clientID != 1000? _clientID :serverRpcParams.Receive.SenderClientId] = true;

    //     bool allClientsReady = true;
    //     foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
    //     {
    //         if (!PlayerAnswerReadyDictionary.ContainsKey(clientId) || !PlayerAnswerReadyDictionary[clientId])
    //         {
    //             allClientsReady = false;
    //             break;
    //         }
    //     }

    //     if (allClientsReady)
    //     {
    //         CreateAnswerOptionsServerRpc();
    //         state.Value = State.ChoosingAnswers;
    //         PlayerAnswerReadyDictionary = new Dictionary<ulong, bool>();
    //     }

    //     Debug.Log("allClientsReady: " + allClientsReady);
    // }
    //     [ServerRpc(RequireOwnership = false)]
    // public void CreateDefaultAsnwerOptionServerRpc(string _playerName, ulong _clientID = 1000, ServerRpcParams serverRpcParams = default)
    // {
    //     ulong clientId = _clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId;

    //     Debug.Log($"CreateDefaultAsnwerOptionServerRpc(): _playerName = {_playerName}, clientId = {clientId}");

    //     if (IsPlayerAnswerReady(clientId))
    //         return;

    //     CreateDefaultAsnwerOptionClientRpc(_playerName, clientId);

    //     PlayerAnswerReadyDictionary[clientId] = true;

    //     bool allClientsReady = NetworkManager.Singleton.ConnectedClientsIds.All(id => 
    //         PlayerAnswerReadyDictionary.ContainsKey(id) && PlayerAnswerReadyDictionary[id]);

    //     if (allClientsReady)
    //     {
    //         CreateAnswerOptionsServerRpc();
    //         state.Value = State.ChoosingAnswers;
    //         PlayerAnswerReadyDictionary.Clear();
    //     }

    //     Debug.Log($"allClientsReady: {allClientsReady}");
    // }

    //     [ClientRpc]
    //     public void CreateDefaultAsnwerOptionClientRpc(string _playerName, ulong _clientID)
    //     {   
    //         PlayersInSessionList.Add(new PlayerSessionInfo{
    //             ClientId = _clientID,
    //             PlayerName = _playerName,
    //             PlayerAnswer = RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount()-1].questions[RoundManager.Instance.GetSubRoundCount()-1].
    //                             defaultAnswer[Convert.ToInt32(_clientID.ToString())],
    //             DidPlayerAnswer = false,
    //             IsDefaultAnswer = true
    //         });

    //         Debug.Log($"RoundManager.Instance.rounds[{RoundManager.Instance.GetRoundCount()-1}].questions[{RoundManager.Instance.GetSubRoundCount()-1}].defaultAnswer[{Convert.ToInt32(_clientID.ToString())}] = " + RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount()-1].questions[RoundManager.Instance.GetSubRoundCount()-1].defaultAnswer[Convert.ToInt32(_clientID.ToString())]);

    //       //  PlayerAnswerReadyDictionary[_clientID] = true;
    //     }
    [ServerRpc(RequireOwnership = false)]
    public void CreateDefaultAsnwerOptionServerRpc(string _playerName, ulong _clientID = 1000, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = _clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId;

        Debug.Log($"CreateDefaultAsnwerOptionServerRpc(): _playerName = {_playerName}, clientId = {clientId}");

        if (PlayerAnswerReadyDictionary.ContainsKey(clientId) && PlayerAnswerReadyDictionary[clientId])
        {
            Debug.Log($"Player {_playerName} (ID: {clientId}) already processed. Skipping.");
            return;
        }

        CreateDefaultAsnwerOptionClientRpc(_playerName, clientId);

        PlayerAnswerReadyDictionary[clientId] = true;

        bool allClientsReady = NetworkManager.Singleton.ConnectedClientsIds.All(id =>
            PlayerAnswerReadyDictionary.ContainsKey(id) && PlayerAnswerReadyDictionary[id]);

        Debug.Log($"Clients ready: {string.Join(", ", PlayerAnswerReadyDictionary.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");

        if (allClientsReady)
        {
            CreateAnswerOptionsServerRpc();
            state.Value = State.ChoosingAnswers;
            PlayerAnswerReadyDictionary.Clear();
        }

        Debug.Log($"allClientsReady: {allClientsReady}");
    }

    [ClientRpc]
    public void CreateDefaultAsnwerOptionClientRpc(string _playerName, ulong _clientID)
    {
        if (PlayersInSessionList.Any(p => p.ClientId == _clientID))
        {
            Debug.Log($"Player {_playerName} (ID: {_clientID}) already in PlayersInSessionList. Skipping.");
            return;
        }


        PlayersInSessionList.Add(new PlayerSessionInfo
        {
            ClientId = _clientID,
            PlayerName = _playerName,
            DidPlayerAnswer = false,
            IsDefaultAnswer = true
        });

        int currentPlayerIndex = PlayersInSessionList.FindIndex(p => p.ClientId == _clientID);

        PlayersInSessionList[currentPlayerIndex].PlayerAnswer = RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount() - 1].questions[RoundManager.Instance.GetSubRoundCount() - 1].
                            defaultAnswer[currentPlayerIndex];

        Debug.Log($"Added player {_playerName} (ID: {_clientID}) to PlayersInSessionList. Current count: {PlayersInSessionList.Count}");
        Debug.Log($"RoundManager.Instance.rounds[{RoundManager.Instance.GetRoundCount() - 1}].questions[{RoundManager.Instance.GetSubRoundCount() - 1}].defaultAnswer[{currentPlayerIndex}]");
        Debug.Log($"RoundManager.Instance.rounds[{RoundManager.Instance.GetRoundCount() - 1}].questions[{RoundManager.Instance.GetSubRoundCount() - 1}].defaultAnswer[{currentPlayerIndex}] = " + RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount() - 1].questions[RoundManager.Instance.GetSubRoundCount() - 1].defaultAnswer[currentPlayerIndex]);
    }


    public void CheckPlayerAnswerReadyServerRpcs()
    {
        bool allClientsReady = true;
        foreach (PlayerInfo playerInfo in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            if (!PlayerAnswerReadyDictionary.ContainsKey(playerInfo.ClientId) || !PlayerAnswerReadyDictionary[playerInfo.ClientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerOptionsServerRpc();
            state.Value = State.ChoosingAnswers;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SetPlayerAnswerReadyServerRpc(string _playerName, string _playerAnswer, ServerRpcParams serverRpcParams = default)
    {
        // For Debbuging...
        foreach (var v in PlayersInSessionList)
        {
            Debug.Log("ServerRpc Debugg: " + v.PlayerName);
        }

        Debug.Log("ServerRpc Debugg: " + PlayersInSessionList.Count);

        SetPlayerAnswerReadyClientRpc(_playerName, _playerAnswer, serverRpcParams.Receive.SenderClientId);

        PlayerAnswerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!PlayerAnswerReadyDictionary.ContainsKey(clientId) || !PlayerAnswerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerOptionsServerRpc();
            state.Value = State.ChoosingAnswers;
            PlayerAnswerReadyDictionary = new Dictionary<ulong, bool>();
        }

        Debug.Log("allClientsReady: " + allClientsReady);
    }

    [ClientRpc]
    void SetPlayerAnswerReadyClientRpc(string _playerName, string _playerAnswer, ulong _clientId)
    {
        PlayersInSessionList.Add(new PlayerSessionInfo
        {
            ClientId = _clientId,
            PlayerName = _playerName,
            PlayerAnswer = _playerAnswer,
            DidPlayerAnswer = true
        });

        foreach (var v in PlayersInSessionList)
        {
            Debug.Log($"PlayersInSessionList.ClientId: {v.ClientId} \n PlayersInSessionList.PlayerName: {v.PlayerName} \n PlayersInSessionList.PlayerAnswer: {v.PlayerAnswer} \n ");
        }

        foreach (var v in PlayersInSessionList)
        {
            Debug.Log("ClientRpc Debugg: " + v.PlayerName);
        }

        Debug.Log("ClientRpc Debugg: " + PlayersInSessionList.Count);

        PlayerAnswerReadyDictionary[_clientId] = true;

        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.FunnySFXList);

        OnPlayerAnswerReady?.Invoke(this, EventArgs.Empty);
    }

    public void OnAnswerFieldDeselect(string _temp)
    {
        AnswerField.contentType = TMPro.TMP_InputField.ContentType.Standard;

        AnswerField.SetDirty();
    }

    //------------------------Network Helper Functions------------------------//
    public bool IsPlayerAnswerReady(ulong _clientId)
    {
        return PlayerAnswerReadyDictionary.ContainsKey(_clientId) && PlayerAnswerReadyDictionary[_clientId];
    }

    public bool IsPlayerChoosenAnswerReady(ulong _clientId)
    {
        return PlayerChoosenAnswerReadyDictionary.ContainsKey(_clientId) && PlayerChoosenAnswerReadyDictionary[_clientId];
    }

    // -----------------------------------------Helper Functions----------------------------------------- //
    public PlayerSessionInfo GetOriginalPlayersInSessionFromClientID(ulong _clientId)
    {
        foreach (PlayerSessionInfo playerSessionInfo in OriginalPlayersInSession)
        {
            Debug.Log(playerSessionInfo.ClientId + " =? " + _clientId);
            if (playerSessionInfo.ClientId == _clientId)
            {
                Debug.Log("Object is not Null");
                return playerSessionInfo;
            }
        }

        Debug.Log("Object is Null");
        return default;
    }
    public PlayerSessionInfo GetPlayersInSessionFromClientID(ulong _clientId)
    {
        foreach (PlayerSessionInfo playerSessionInfo in PlayersInSessionList)
        {
            if (playerSessionInfo.ClientId == _clientId)
            {
                Debug.Log("Object is not Null");
                return playerSessionInfo;
            }
        }

        Debug.Log("Object is Null");
        return default;
    }

    public PlayerSessionInfo GetPlayersInSessionFromClientIDInGroupedPlayerAnswers(ulong _clientId)
    {
        foreach (var v in GroupedPlayerAnswers)
        {
            foreach (PlayerSessionInfo playerSessionInfo in v.Players)
            {
                if (playerSessionInfo.ClientId == _clientId)
                {
                    Debug.Log("Object is not Null");
                    return playerSessionInfo;
                }
            }
        }


        Debug.Log("Object is Null");
        return default;
    }

    public int GetPlayerInfoIndexFromClientID(ulong _clientId)
    {
        for (int i = 0; i < PlayersInSessionList.Count; i++)
        {
            if (PlayersInSessionList[i].ClientId == _clientId)
            {
                return i;
            }
        }

        return -1;
    }

    // ---------------------------------------Choosing Answers Functions------------------------------------- //


    [ServerRpc(RequireOwnership = false)]
    public void SendAnswerDetailsServerRpc(ulong[] _ownerClientID, ServerRpcParams serverRpcParams = default)
    {
        // Debug.Log("SendAnswerDetailsServerRpc Called");
        SendAnswerDetailsClientRpc(_ownerClientID, serverRpcParams.Receive.SenderClientId);
        // SetPlayerChooseReadyServerRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    void SendAnswerDetailsClientRpc(ulong[] _ownerClientID, ulong _senderClientID)
    {
        // Debug.Log("SendAnswerDetailsClientRpc Called");
        SendAnswerDetails(_ownerClientID, _senderClientID);
    }

    public void SendAnswerDetails(ulong[] _ownerClientID, ulong _senderClientID)
    {
        // // //Play Button Audios..
        // // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        // // //------------

        Debug.Log("SendAnswerDetails Called");

        foreach (var v in roomSessionManager.roundAnswersGOs)
        {
            // Old...
            /*
            // if (v.playerSessionInfo.ClientId == _ownerClientID)
            // {         
            //     v.playerSessionInfo.PlayersWhoChooseOwnerAnswer.Add(GamePlayManager.Instance.GetPlayersInSessionFromClientID(_senderClientID).PlayerName);
            //     v.playerSessionInfo.PlayersIDWhoChooseThis.Add(GamePlayManager.Instance.GetPlayersInSessionFromClientID(_senderClientID).ClientId);
                
            //     // Key Line!!! xD
            //     if(!IsOwner)
            //         return;

            //     SetPlayerChooseReadyServerRpc(_senderClientID);
            // }   
            // else
            // {
   
            // }
            */


            // New...
            foreach (var id in _ownerClientID)
            {
                foreach (var player in v.playerAnswerGroup.Players)
                {
                    if (v.playerAnswerGroup.PlayersID_WhoChooseThis.Contains(GetPlayersInSessionFromClientIDInGroupedPlayerAnswers(_senderClientID).ClientId))
                        return;

                    if (player.ClientId == id)
                    {
                        v.playerAnswerGroup.PlayersID_WhoChooseThis.Add(GetPlayersInSessionFromClientIDInGroupedPlayerAnswers(_senderClientID).ClientId);
                        v.playerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Add(GetPlayersInSessionFromClientIDInGroupedPlayerAnswers(_senderClientID).PlayerName);

                        // Key Line!!! xD
                        if (!IsOwner)
                            return;

                        SetPlayerChooseReadyServerRpc(_senderClientID);
                    }
                    else
                    {

                    }
                }
            }
        }
    }

    // -------------------------------------Showing Answers Functions------------------------------------- //
    public void CheckPlayerChooseReadyOnTimerFinishServerRpcs()
    {
        Debug.Log("CheckPlayerChooseReadyOnTimerFinishServerRpcs");

        bool allClientsReady = true;
        foreach (PlayerInfo playerInfo in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            if (!PlayerChoosenAnswerReadyDictionary.ContainsKey(playerInfo.ClientId) || !PlayerChoosenAnswerReadyDictionary[playerInfo.ClientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerPanelServerRpc();
            state.Value = State.ShowingAnswers;
            // CreateAnswerPanelClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerChooseReadyOnTimerFinishServerRpc(ulong _clientID = 1000, ServerRpcParams serverRpcParams = default)
    {
        if (_clientID != 1000)
            if (IsPlayerChoosenAnswerReady(_clientID))
                return;

        Debug.Log("SetPlayerChooseReadyOnTimerFinishServerRpc");

        SetPlayerChooseReadyOnTimerFinishClientRpc(_clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId);

        PlayerChoosenAnswerReadyDictionary[_clientID != 1000 ? _clientID : serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!PlayerChoosenAnswerReadyDictionary.ContainsKey(clientId) || !PlayerChoosenAnswerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerPanelServerRpc(/*_clientID != 1000? _clientID :serverRpcParams.Receive.SenderClientId*/);
            state.Value = State.ShowingAnswers;
            PlayerChoosenAnswerReadyDictionary = new Dictionary<ulong, bool>();
        }

        Debug.Log("Choosen Answers OnTimerFinish - allClientsReady: " + allClientsReady);
    }

    [ClientRpc]
    public void SetPlayerChooseReadyOnTimerFinishClientRpc(ulong _clientID)
    {
        PlayerChoosenAnswerReadyDictionary[_clientID] = true;
        Debug.Log("PlayerChoosenAnswerClientRpc Ready");
    }


    public void CheckPlayerChooseReadyServerRpcs()
    {
        Debug.Log("CheckPlayerChooseReadyServerRpcs");

        bool allClientsReady = true;
        foreach (PlayerInfo playerInfo in MainNetworkManager.Instance.PlayerInfoNetworkList)
        {
            if (!PlayerChoosenAnswerReadyDictionary.ContainsKey(playerInfo.ClientId) || !PlayerChoosenAnswerReadyDictionary[playerInfo.ClientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerPanelServerRpc();
            state.Value = State.ShowingAnswers;
            // CreateAnswerPanelClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerChooseReadyServerRpc(ulong _clientID, ServerRpcParams serverRpcParams = default)
    {
        Debug.Log("SetPlayerChooseReadyServerRpc");

        SetPlayerChooseReadyClientRpc(_clientID);

        PlayerChoosenAnswerReadyDictionary[_clientID] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!PlayerChoosenAnswerReadyDictionary.ContainsKey(clientId) || !PlayerChoosenAnswerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            CreateAnswerPanelServerRpc();
            state.Value = State.ShowingAnswers;
            PlayerChoosenAnswerReadyDictionary = new Dictionary<ulong, bool>();
            // CreateAnswerPanelClientRpc();
        }

        Debug.Log("Choosen Answers - allClientsReady: " + allClientsReady);
    }

    [ClientRpc]
    void SetPlayerChooseReadyClientRpc(ulong _clientId)
    {
        Debug.Log("SetPlayerChooseReadyClientRpc");
        PlayerChoosenAnswerReadyDictionary[_clientId] = true;
    }

    [ServerRpc(RequireOwnership = false)]
    void CreateAnswerPanelServerRpc(/*ulong _clientID = 1000, ServerRpcParams serverRpcParams = default*/)
    {
        if (areAnswersCreated)
            return;

        areAnswersCreated = true;

        Debug.Log("CreateAnswerPanelServerRpc");

        // CreateAnswerPanelClientRpc(_clientID != 1000? _clientID :serverRpcParams.Receive.SenderClientId);
        CreateAnswerPanelClientRpc();
    }

    [ClientRpc]
    void CreateAnswerPanelClientRpc()
    {
        Debug.Log("CreateAnswerPanelClientRpc");

        RayCastPanel.SetActive(false);

        Debug.Log("Panels Created");

        // Old...
        // foreach (AnswerController ac in roomSessionManager.roundAnswersGOs)
        // {
        //     foreach (var v in ac.playerAnswerGroup.Players)
        //     {
        //         roomSessionManager.finalPlayersAnswer.Add(v);
        //     }
        // }


        // New...
        foreach (AnswerController ac in roomSessionManager.roundAnswersGOs)
        {
            roomSessionManager.FinalPlayerAnswerGroup.Add(ac.playerAnswerGroup);
        }

        StartCoroutine(roomSessionManager.CreateAnswerPanels());
    }

    //--------------------------------------------------------------------------------------------//

    public PlayerSessionInfo GetPlayerSessionInfoFromPlayerRank(int _rank)
    {
        foreach (PlayerSessionInfo playerSessionInfo in OriginalPlayersInSession)
        {
            if (playerSessionInfo.PlayerRank == _rank)
            {
                return playerSessionInfo;
            }
        }

        return default;
    }

    public PlayerSessionInfo GetPlayerSessionInfoFromClientID(ulong _clientID)
    {
        foreach (PlayerSessionInfo playerSessionInfo in OriginalPlayersInSession)
        {
            if (playerSessionInfo.ClientId == _clientID)
            {
                return playerSessionInfo;
            }
        }

        return default;
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

    public void LeaveGame()
    {
        StartCoroutine(LeaveGame_Coroutine());
    }
    public IEnumerator LeaveGame_Coroutine()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        LoadingScreen.SetActive(true);
        Debug.Log("Loading on");
        Task task = LobbyNetworkManager.Instance.LeaveLobby(AuthenticationService.Instance.PlayerId);
        yield return new WaitUntil(() => task.IsCompleted);

        NetworkManager.Singleton.Shutdown();
        yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);

        MainNetworkManager.Instance.CleanUp();
        Loader.Load(Loader.Scene.HomeScreen);
    }

    public void LeaveGameOnDisconnect()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;

        LoadingScreen.SetActive(true);
        Debug.Log("Loading on");
        // LobbyNetworkManager.Instance.LeaveLobby();
        // NetworkManager.Singleton.Shutdown();
        // MainNetworkManager.Instance.CleanUp();
        Loader.Load(Loader.Scene.HomeScreen);
    }

    // public void OnPlayerDisconnection(ulong _clientID)
    // {
    //     MainNetworkManager.Instance.DisconnectPlayer(_clientID);

    //     // StartCoroutine(ReconnectCoroutine(_clientID));
    // }


    public IEnumerator ReconnectCoroutine(ulong _clientID = default)
    {
        // if (NetworkManager.Singleton.LocalClientId != _clientID)
        // {
        //     Debug.Log("Not the disconnected client");
        //     yield break;
        // }
        // If not on first attempt, wait some time before trying again, so that if the issue causing the disconnect
        // is temporary, it has time to fix itself before we try again. Here we are using a simple fixed cooldown
        // but we could want to use exponential backoff instead, to wait a longer time between each failed attempt.
        // See https://en.wikipedia.org/wiki/Exponential_backoff

        yield return new WaitForSeconds(1);

        Debug.Log("Lost connection to host, trying to reconnect...");
        NetworkManager.Singleton.Shutdown();
        yield return new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress); // wait until NetworkManager completes shutting down
                                                                                       // Debug.Log($"Reconnecting attempt {m_NbAttempts + 1}/{m_ConnectionManager.NbReconnectAttempts}...");
                                                                                       // m_ReconnectMessagePublisher.Publish(new ReconnectMessage(m_NbAttempts, m_ConnectionManager.NbReconnectAttempts));
                                                                                       // If first attempt, wait some time before attempting to reconnect to give time to services to update
                                                                                       // (i.e. if in a Lobby and the host shuts down unexpectedly, this will give enough time for the lobby to be
                                                                                       // properly deleted so that we don't reconnect to an empty lobby

        yield return new WaitForSeconds(1);

        // m_NbAttempts++;
        var reconnectingSetupTask = SetupClientReconnectionAsync();
        yield return new WaitUntil(() => reconnectingSetupTask.IsCompleted);

        if (!reconnectingSetupTask.IsFaulted && reconnectingSetupTask.Result.success)
        {
            // If this fails, the OnClientDisconnect callback will be invoked by Netcode
            var connectingTask = ConnectClientAsync();
            yield return new WaitUntil(() => connectingTask.IsCompleted);
        }
        else
        {
            if (!reconnectingSetupTask.Result.shouldTryAgain)
            {
                // setting number of attempts to max so no new attempts are made
                // m_NbAttempts = m_ConnectionManager.NbReconnectAttempts;
            }
            // Calling OnClientDisconnect to mark this attempt as failed and either start a new one or give up
            // and return to the Offline state
            StartCoroutine(ReconnectCoroutine(_clientID));
        }
    }
    public async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
    {
        if (LobbyNetworkManager.Instance.GetLobby() == null)
        {
            Debug.Log("Lobby does not exist anymore, stopping reconnection attempts.");
            return (false, false);
        }
        // When using Lobby with Relay, if a user is disconnected from the Relay server, the server will notify the
        // Lobby service and mark the user as disconnected, but will not remove them from the lobby. They then have
        // some time to attempt to reconnect (defined by the "Disconnect removal time" parameter on the dashboard),
        // after which they will be removed from the lobby completely.
        // See https://docs.unity.com/lobby/reconnect-to-lobby.html
        var lobby = await LobbyNetworkManager.Instance.ReconnectToLobbyAsync();
        var success = lobby != null;
        Debug.Log(success ? "Successfully reconnected to Lobby." : "Failed to reconnect to Lobby.");
        return (success, true); // return a success if reconnecting to lobby returns a lobby
    }
    public async Task ConnectClientAsync()
    {
        try
        {
            // Setup NGO with current connection method
            await LobbyNetworkManager.Instance.SetupClientConnectionAsync();
            // NGO's StartClient launches everything
            if (!NetworkManager.StartClient())
            {
                throw new Exception("NetworkManager StartClient failed");
            }
            else
            {
                SetPauseReadyServerRpc(true, default);
                Debug.Log("NetworkManager StartClient Successful!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error connecting client, see following exception");
            Debug.LogException(e);
            // StartingClientFailed();
            throw;
        }
    }


    // void OnApplicationPause(bool pauseStatus) 
    // {
    //     Debug.Log("Game pauseStatus: " + pauseStatus);

    //     if (pauseStatus)
    //     {
    //         AddToOutOfFocusListServerRpc(PlayerDataManager.Instance.GetPlayerNamePlayerData());
    //     }
    //     else
    //     {
    //         RemoveFromOutOfFocusListServerRpc();
    //     }
    // }


    // private void OnApplicationFocus(bool focusStatus) 
    // {
    //     IsGameOutOfFocuse = !focusStatus;

    //     Debug.Log("Game focusStatus: " + IsGameOutOfFocuse);

    //     if (IsGameOutOfFocuse && TouchScreenKeyboard.visible)
    //     {
    //         Debug.Log("App is out of focuse but the keyboard is opened!");
    //     }
    //     else if (IsGameOutOfFocuse && !TouchScreenKeyboard.visible)
    //     {
    //         Debug.Log("App is out of focuse and the keyboard is NOT opened!");
    //         // SetPauseReadyServerRpc(false, default);
    //         AddToOutOfFocusListServerRpc(PlayerDataManager.Instance.GetPlayerNamePlayerData());
    //     }
    //     else if (!IsGameOutOfFocuse)
    //     {
    //         Debug.Log("App is focused!");
    //         // SetPauseReadyServerRpc(true, default);
    //         RemoveFromOutOfFocusListServerRpc();
    //     }
    // }

    [ServerRpc(Delivery = RpcDelivery.Unreliable, RequireOwnership = false)]
    public void AddToOutOfFocusListServerRpc(string _playerName, ServerRpcParams _serverRpcParams = default)
    {
        Debug.Log("AddToOutOfFocusListServerRpc()");
        AddToOutOfFocusListClientRpc(_playerName, _serverRpcParams.Receive.SenderClientId);
    }
    [ClientRpc]
    public void AddToOutOfFocusListClientRpc(string _playerName, ulong _clientId)
    {
        Debug.Log("AddToOutOfFocusListClientRpc()");
        OutOfFocusPlayers.Add(new PlayerSessionInfo
        {
            ClientId = _clientId,
            PlayerName = _playerName,
            PlayerAnswer = RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount() - 1].questions[RoundManager.Instance.GetSubRoundCount() - 1].
                            defaultAnswer[Convert.ToInt32(_clientId.ToString())],
            DidPlayerAnswer = false,
            IsDefaultAnswer = true
        });
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemoveFromOutOfFocusListServerRpc(ServerRpcParams _serverRpcParams = default)
    {
        // if (OutOfFocusPlayers.Count != 0)
        // {
        //     foreach (var v in OutOfFocusPlayers)
        //     {
        //         if (v.ClientId == _serverRpcParams.Receive.SenderClientId)
        //         {
        //             OutOfFocusPlayers.Remove(v);
        //             break;
        //         }
        //     }
        // }


        RemoveFromOutOfFocusListClientRpc(_serverRpcParams.Receive.SenderClientId);
    }
    [ClientRpc]
    public void RemoveFromOutOfFocusListClientRpc(ulong _clientId)
    {
        if (OutOfFocusPlayers.Count != 0)
        {
            foreach (var v in OutOfFocusPlayers)
            {
                if (v.ClientId == _clientId)
                {
                    OutOfFocusPlayers.Remove(v);
                    break;
                }
            }
        }
    }



    [ServerRpc(RequireOwnership = false)]
    public void SetPauseReadyServerRpc(bool _isReady = true, ServerRpcParams serverRpcParams = default)
    {
        // if (_isReady && PlayerPauseReadyDictionary[serverRpcParams.Receive.SenderClientId] == true)
        //     return;

        SetPauseReadyClientRpc(_isReady, serverRpcParams.Receive.SenderClientId);
        PlayerPauseReadyDictionary[serverRpcParams.Receive.SenderClientId] = _isReady;
        bool allClientsReady = true;

        Debug.Log("NetworkManager.Singleton.ConnectedClientsIds.Count is: " + NetworkManager.Singleton.ConnectedClientsIds.Count);

        // for (int i = 0; i > NetworkManager.Singleton.ConnectedClientsList.Count; i++)
        // {
        //     Debug.Log($"NetworkManager.Singleton.ConnectedClients[{i}].ClientId = {NetworkManager.Singleton.ConnectedClients[i].ClientId}");
        // }

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Debug.Log($" {clientId}");

            if (!PlayerPauseReadyDictionary.ContainsKey(clientId) || !PlayerPauseReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (!allClientsReady)
        {
            // CurrentState    =   state.Value;
            // state.Value     =   State.GamePaused;

            IsGamePaused.Value = true;
        }

        if (allClientsReady)
        {
            // state.Value     =   CurrentState;
            // UnPauseGame();

            IsGamePaused.Value = false;
        }

        Debug.Log("allClientsReady: " + allClientsReady);
    }

    [ClientRpc]
    void SetPauseReadyClientRpc(bool _isReady = true, ulong _clientId = default)
    {
        PlayerPauseReadyDictionary[_clientId] = _isReady;
    }

    public IEnumerator ShowPlayerDisconnectedMessage()
    {
        if (state.Value == State.GameEnd)
            yield break;

        PlayerDisconnectedMessage.GetComponent<RectTransform>().DOAnchorPosY(-50, 1).SetUpdate(true).SetEase(Ease.InOutCubic);

        Debug.Log("PlayerDisconnectedMessage Show!");

        yield return new WaitForSeconds(1.25f);

        Debug.Log("PlayerDisconnectedMessage Hide!");

        PlayerDisconnectedMessage.GetComponent<RectTransform>().DOAnchorPosY(50, 1).SetUpdate(true).SetEase(Ease.InOutCubic);
    }

    public void PauseGame()
    {
        if (this != null)
            DisconnectionPanel.SetActive(true);

        if (AudioManager.Instance.QuestionAS.isPlaying)
            AudioManager.Instance.QuestionAS.Pause();

        // Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        if (this != null)
            DisconnectionPanel.SetActive(false);

        if (AudioManager.Instance.QuestionAS.isPlaying)
            AudioManager.Instance.QuestionAS.Play();

        // // Time.timeScale = 1;
    }

    public void RPCsChecker()
    {
        Debug.Log("RPCsChecker()");

        CheckSceneReadyServerRpcs();
        CheckQuestionAudioDonePlayingReadyServerRpcs();
        CheckDefaultAsnwerOptionServerRpcs();
        CheckPlayerAnswerReadyServerRpcs();
        CheckPlayerChooseReadyOnTimerFinishServerRpcs();
        CheckPlayerChooseReadyServerRpcs();
    }




    //----------------------------------------- *Helper Functions* -----------------------------------------//
    public async Task GroupPlayerAnswers(List<PlayerSessionInfo> playersInSessionList)
    {
        Debug.Log("GroupPlayerAnswers Function Started");

        await Task.Run(() =>
        {
            Debug.Log("GroupPlayerAnswers Task Started");
            var playerAnswerCount = playersInSessionList
                .GroupBy(player => player.PlayerAnswer)
                .ToDictionary(group => group.Key, group => group.Count());

            var duplicatedAnswers = new HashSet<string>(playerAnswerCount
                .Where(pair => pair.Value > 1)
                .Select(pair => pair.Key));

            // Group players with duplicated PlayerAnswer values
            GroupedPlayerAnswers = playersInSessionList
                .Where(player => duplicatedAnswers.Contains(player.PlayerAnswer))
                .GroupBy(player => player.PlayerAnswer)
                .Select(group => new PlayerAnswerGroup(group.Key) { Players = group.ToList() })
                .ToList();

            // Add players with unique answers as individual groups
            var uniqueAnswers = playerAnswerCount
                .Where(pair => pair.Value == 1)
                .Select(pair => pair.Key);

            foreach (var answer in uniqueAnswers)
            {
                var player = playersInSessionList.First(p => p.PlayerAnswer == answer);
                GroupedPlayerAnswers.Add(new PlayerAnswerGroup(answer) { Players = new List<PlayerSessionInfo> { player } });
            }

            // Set IsCorrect to true for players with the answer "Correct"
            foreach (var group in GroupedPlayerAnswers)
            {
                if (group.PlayerAnswer == RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount() - 1].questions[RoundManager.Instance.GetSubRoundCount() - 1].answer)
                {
                    group.IsCorrect = true;
                }
            }

            Debug.Log("GroupPlayerAnswers Task Finished");
        });

        Debug.Log("GroupPlayerAnswers Function Finished");
    }
    //-----------------------------------------------------------------------------------------------------//
}
