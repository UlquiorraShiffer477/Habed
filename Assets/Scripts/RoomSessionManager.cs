using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;

using Unity.Netcode;
using Unity.Collections;
using System.Threading.Tasks;
using System.Linq;

using RTLTMPro;


// [Serializable]
// public class PlayerAnswer
// {
//     public PlayerInput playerInput;
//     public List<string> playersWhoClicked;
// }


//New Struct...
[Serializable]
public struct RoomSessionPlayerAnswer : INetworkSerializable, IEquatable<RoomSessionPlayerAnswer>
{
    public ulong ClientID;
    public FixedString128Bytes answerText;
    public PlayerInfo ownerInfo;
    // public List<PlayerInfo> playersWhoClicked;
    public bool IsCorrect;

    public RoomSessionPlayerAnswer(ulong _clientId = 0,
                                    string _answerText = "",
                                    PlayerInfo _ownerInfo = new PlayerInfo(),
                                    // List<PlayerInfo> _playersWhoClicked = new List<PlayerInfo>(),
                                    bool _isCorrect = false)
    {
        ClientID = _clientId;
        answerText = _answerText;
        ownerInfo = _ownerInfo;
        // playersWhoClicked = _playersWhoClicked;
        IsCorrect = _isCorrect;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref answerText);
        serializer.SerializeValue(ref ownerInfo);
        // serializer.SerializeValue(ref playersWhoClicked);
        serializer.SerializeValue(ref IsCorrect);
    }

    public bool Equals(RoomSessionPlayerAnswer other)
    {
        return ClientID == other.ClientID &&
                answerText == other.answerText &&
                ownerInfo.Equals(other.ownerInfo) &&
                IsCorrect == other.IsCorrect;
    }
}

// Old Class...
// [Serializable]
// public class RoomSessionPlayerAnswer
// {
//     public string answerText;
//     public PlayerInfo ownerInfo = new PlayerInfo();
//     public List<PlayerInfo> playersWhoClicked = new List<PlayerInfo>();
//     public bool IsCorrect = false;
// }



public class RoomSessionManager : NetworkBehaviour
{
    #region Instance
    private static RoomSessionManager _instance;

    public static RoomSessionManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<RoomSessionManager>();

            return _instance;
        }
    }
    #endregion


    [Header("Players")]
    public int PlayersCount;
    public List<PlayerInfo> PlayersInRoom;



    [Header("Prefabs")]
    public AnswerController answersController = new AnswerController();

    public AnswerShowingPanelController answerShowingPanelController = new AnswerShowingPanelController();

    public ResultsController resultsController = new ResultsController();
    public List<FinalResultsController> finalResultsControllerList = new List<FinalResultsController>();



    public List<AnswerController> roundAnswersGOs = new List<AnswerController>();
    public NetworkList<int> RandomIndex;

    public NetworkList<int> Scores;


    [Header("Spawning Positions")]
    public RectTransform answersControllerParentPos;
    public RectTransform AnswesPanelParentPos;
    public RectTransform ResultsControllerParentPos;

    [Header("Answers Lists")]
    public List<RoomSessionPlayerAnswer> playersAnswer;
    public List<PlayerAnswerGroup> FinalPlayerAnswerGroup = new List<PlayerAnswerGroup>();

    public List<AnswerShowingPanelController> Panels = new List<AnswerShowingPanelController>();
    public List<ResultsController> Results = new List<ResultsController>();


    [Header("Questions Info")]
    public List<Round> rounds;
    public List<Question> currentRoundQuestions;


    void Awake()
    {
        playersAnswer = new List<RoomSessionPlayerAnswer>();
        FinalPlayerAnswerGroup = new List<PlayerAnswerGroup>();

        RandomIndex = new NetworkList<int>();
        Scores = new NetworkList<int>();
    }

    // void OnEnable() 
    // {
    //     if(GamePlayManager.Instance.roundManager.GetRoundCount() == 1)
    //         AddDemoPlayerInfo(DemoPlayers, GamePlayManager.Instance.roundManager.R1A);
    //     else if(GamePlayManager.Instance.roundManager.GetRoundCount() == 2)
    //         AddDemoPlayerInfo(DemoPlayers, GamePlayManager.Instance.roundManager.R2A);
    //     else if(GamePlayManager.Instance.roundManager.GetRoundCount() == 3)
    //         AddDemoPlayerInfo(DemoPlayers, GamePlayManager.Instance.roundManager.R3A);
    // }

    // public void Init()
    // {
    //     PlayersInRoom = new List<PlayerInfo>();

    //     // RoundManager.Instance.SetNextQuestion();

    //     for (int i = 0 ; i < 3 ; i++)
    //     {
    //         CorrectQuestionAnswer tempCQA = new CorrectQuestionAnswer();

    //         CQA.Add(tempCQA);
    //     }

    //     for (int i = 0 ; i < CQA.Count ; i++)
    //     {
    //         CQA[i].answerText = GamePlayManager.Instance.roundManager.rounds[i].questions[i].question;
    //     }

    //     PlayersCount = PlayersInRoom.Count;
    //     AddPlayerToList(PlayerDataManager.Instance.GetPlayerInfo());

    //     foreach(var v in DemoPlayers)
    //     {
    //         AddPlayerToList(v.ownerInfo);
    //     }

    //     AddDemoPlayerInfo(DemoPlayers, GamePlayManager.Instance.roundManager.CurrentPlayerAnswer);

    // }

    public void AddPlayerToList(PlayerInfo _player)
    {
        PlayersInRoom.Add(_player);
    }

    public void ResetDependencies()
    {
        GamePlayManager.Instance.areAnswersCreated = false;

        ClearRandomIndexClientRpc();

        foreach (AnswerController go in roundAnswersGOs)
        {
            Destroy(go.gameObject);
        }

        foreach (AnswerShowingPanelController go in Panels)
        {
            Destroy(go.gameObject);
        }

        if (RoundManager.Instance.GetRoundCount() <= 3)
            foreach (ResultsController go in Results)
            {
                Destroy(go.gameObject);
            }

        GamePlayManager.Instance.PlayersInSessionList.Clear();
        GamePlayManager.Instance.GroupedPlayerAnswers.Clear();


        playersAnswer.Clear();
        FinalPlayerAnswerGroup.Clear();

        roundAnswersGOs.Clear();
        Panels.Clear();

        if (RoundManager.Instance.GetRoundCount() <= 3)
            Results.Clear();
    }

    public void AddAnswer(PlayerInfo player, string answer)
    {
        RoomSessionPlayerAnswer pa = new RoomSessionPlayerAnswer();
        pa.ownerInfo = player;
        pa.answerText = answer;

        playersAnswer.Add(pa);
    }

    public void CreateAnswerOptions()
    {
        GamePlayManager.Instance.PlayersInSessionList.Add(new PlayerSessionInfo
        {
            ClientId = 100,
            PlayerName = "الاجابة الصحيحة",
            PlayerAnswer = RoundManager.Instance.rounds[RoundManager.Instance.GetRoundCount() - 1].questions[RoundManager.Instance.GetSubRoundCount() - 1].answer,
            DidPlayerAnswer = true
        });

        GamePlayManager.Instance.LoadingScreen.SetActive(true);
        Debug.Log("Loading on");
        StartCoroutine(CreateAnswerOptionsCoroutine());
    }

    private IEnumerator CreateAnswerOptionsCoroutine()
    {
        yield return new WaitForTask(GamePlayManager.Instance.GroupPlayerAnswers(GamePlayManager.Instance.PlayersInSessionList));

        // Add a wait loop to ensure GroupedPlayerAnswers is populated
        int maxAttempts = 10;
        int attempts = 0;
        while (GamePlayManager.Instance.GroupedPlayerAnswers.Count == 0 && attempts < maxAttempts)
        {
            yield return new WaitForSeconds(0.1f); // Wait for 100ms
            attempts++;
        }

        if (GamePlayManager.Instance.GroupedPlayerAnswers.Count == 0)
        {
            Debug.LogError("GroupedPlayerAnswers is still empty after waiting. Check the grouping logic.");
            // Handle this error case appropriately
        }

        foreach (var group in GamePlayManager.Instance.GroupedPlayerAnswers)
        {
            Debug.Log($"PlayerAnswer: {group.PlayerAnswer}");
            foreach (var player in group.Players)
            {
                Debug.Log($"ClientId: {player.ClientId}, PlayerName: {player.PlayerName}, PlayerAnswer: {player.PlayerAnswer}");
            }
        }
        GamePlayManager.Instance.LoadingScreen.SetActive(false);
        Debug.Log("Loading of");

        if (!IsServer)
        {
            yield break;
        }

        Debug.Log("CreateAnswerOptions **");
        CreateAnswerOptionsClientRpc();
    }

    // Helper class to wait for a Task in a coroutine
    public class WaitForTask : CustomYieldInstruction
    {
        private Task _task;

        public WaitForTask(Task task)
        {
            _task = task;
        }

        public override bool keepWaiting
        {
            get
            {
                return !_task.IsCompleted;
            }
        }
    }


    // Old...
    // public void CreateCorrectAnswerOption()
    // {
    //     GameObject go = GameObject.Instantiate(answersController.gameObject, answersControllerParentPos) as GameObject;

    //     AnswerController ac = go.GetComponent<AnswerController>();
    //     ac.playerSessionInfo.IsCorrect = true;
    //     ac.playerSessionInfo.ClientId = 100;

    //     roundAnswersGOs.Add(ac);
    // }   

    // -----------------------------------------------------Network Functions----------------------------------------------------- //

    // [ClientRpc]
    // void CreateAnswerOptionsClientRpc()
    // {
    //     Debug.Log("CreateAnswerOptionsClientRpc Instantiating Answers");

    //     // Creating the fixed correct option... 
    //     // CreateCorrectAnswerOption();


    //     Debug.Log("CreateAnswerOptionsClientRpc Instantiating Answers = " + GamePlayManager.Instance.GroupedPlayerAnswers.Count);

    //     // Intsantiating players options...
    //     if (GamePlayManager.Instance.GroupedPlayerAnswers.Count != 0)
    //     {
    //         Debug.Log("Instantiating Answers");

    //         foreach (PlayerAnswerGroup _playerAnswerGroup in GamePlayManager.Instance.GroupedPlayerAnswers)
    //         {   
    //             GameObject go = GameObject.Instantiate(answersController.gameObject, answersControllerParentPos) as GameObject;

    //             AnswerController ac = go.GetComponent<AnswerController>();

    //             ac.playerAnswerGroup = _playerAnswerGroup;           
    //             roundAnswersGOs.Add(ac);
    //         }
    //     }


    //     /*// // Check if all the players have answered and present in the PlayersInSessionList List...
    //     // if ((GamePlayManager.Instance.PlayersInSessionList.Count) != GamePlayManager.Instance.OriginalPlayersInSession.Count)
    //     // {
    //     //     for (int i = 0; i < GamePlayManager.Instance.OriginalPlayersInSession.Count; i++)
    //     //     {
    //     //         GameObject go = GameObject.Instantiate(answersController.gameObject, answersControllerParentPos) as GameObject;

    //     //         AnswerController ac = go.GetComponent<AnswerController>();

    //     //         ac.playerSessionInfo.PlayerName = "Computer";
    //     //         ac.playerSessionInfo.IsDefaultAnswer = true;
    //     //         ac.playerSessionInfo.ClientId = 200;

    //     //         int tempIndex = Convert.ToInt32(NetworkManager.Singleton.LocalClientId.ToString());

    //     //         Debug.Log($"tempIndex = {tempIndex} \n rounds[{RoundManager.Instance.GetRoundCount()-1}].questions[{RoundManager.Instance.GetSubRoundCount()-1}].defaultAnswer[{tempIndex}]");

    //     //         ac.playerSessionInfo.PlayerAnswer = rounds[RoundManager.Instance.GetRoundCount()-1].questions[RoundManager.Instance.GetSubRoundCount()-1].
    //     //         defaultAnswer[tempIndex];

    //     //         roundAnswersGOs.Add(ac);

    //     //         if ((GamePlayManager.Instance.OriginalPlayersInSession.Count + 1) == roundAnswersGOs.Count)
    //     //             break;
    //     //     }
    //     // }
    //     */

    //     ShuffleNetworkListServerRpc(); 
    // } 

    [ClientRpc]
    void CreateAnswerOptionsClientRpc()
    {
        Debug.Log("CreateAnswerOptionsClientRpc Instantiating Answers");

        // Add a short delay and recheck
        if (GamePlayManager.Instance.GroupedPlayerAnswers.Count == 0)
        {
            StartCoroutine(DelayedAnswerCreation());
        }
        else
        {
            CreateAnswers();
        }
    }

    private IEnumerator DelayedAnswerCreation()
    {
        yield return new WaitForSeconds(0.5f);
        if (GamePlayManager.Instance.GroupedPlayerAnswers.Count != 0)
        {
            CreateAnswers();
        }
        else
        {
            Debug.LogError("GroupedPlayerAnswers is still empty after delay. Check the grouping logic.");
            // Handle this error case appropriately
        }
    }

    private void CreateAnswers()
    {
        // Move the answer creation logic here
        foreach (PlayerAnswerGroup _playerAnswerGroup in GamePlayManager.Instance.GroupedPlayerAnswers)
        {
            GameObject go = GameObject.Instantiate(answersController.gameObject, answersControllerParentPos) as GameObject;

            AnswerController ac = go.GetComponent<AnswerController>();

            ac.playerAnswerGroup = _playerAnswerGroup;
            roundAnswersGOs.Add(ac);
        }
        // ... rest of the answer creation code ...
        // ShuffleNetworkListServerRpc();
    }

    // public IEnumerator AddToRandomIndex()
    // {
    //     // if (IsServer)
    //     // {
    //     for (int i = 0; i < roundAnswersGOs.Count; i++)
    //     {
    //         RandomIndex.Add(i);
    //     }

    //     ShuffleNetworkList();
    //     // }

    //     yield return new WaitForSeconds(0.02f);

    //     AssignAfterShuffle();
    //     // AssignAfterShuffleServerRpc();
    //     // AssignAfterShuffleClientRpc();
    // }

    [ClientRpc]
    public void ClearRandomIndexClientRpc()
    {
        ClearRandomIndex();
    }
    public void ClearRandomIndex()
    {
        if (IsServer)
        {
            RandomIndex.Clear();
        }
    }

    // [ClientRpc]
    // public void ShuffleNetworkListClientRpc()
    // {
    //     StartCoroutine(AddToRandomIndex());

    //     // if (IsServer)
    //     // {
    //     //     for (int i = 0; i < roundAnswersGOs.Count; i++)
    //     //     {
    //     //         RandomIndex.Add(i);
    //     //     }

    //     //     ShuffleNetworkList();
    //     // }

    //     // // AssignAfterShuffle();
    //     // // AssignAfterShuffleServerRpc();
    //     // AssignAfterShuffleClientRpc();
    // }


    [ServerRpc(RequireOwnership = false)]
    public void ShuffleNetworkListServerRpc()
    {
        // if (IsServer)
        // {
        for (int i = 0; i < roundAnswersGOs.Count; i++)
        {
            RandomIndex.Add(i);
        }

        ShuffleNetworkList();
        // }


        // AssignAfterShuffle();
        // AssignAfterShuffleServerRpc();
        AssignAfterShuffleClientRpc();
    }


    // public void AssignAfterShuffle()
    // {
    //     Debug.Log("RandomIndex.Count = " + RandomIndex.Count);

    //     for (int i = 0; i < roundAnswersGOs.Count; i++)
    //     {
    //         roundAnswersGOs[i].transform.SetSiblingIndex(RandomIndex[i]);
    //     }
    // }

    [ServerRpc(RequireOwnership = false)]
    public void AssignAfterShuffleServerRpc()
    {
        Debug.Log("RandomIndex.Count = " + RandomIndex.Count);

        for (int i = 0; i < roundAnswersGOs.Count; i++)
        {
            roundAnswersGOs[i].transform.SetSiblingIndex(RandomIndex[i]);
        }
    }

    [ClientRpc]
    public void AssignAfterShuffleClientRpc()
    {
        Debug.Log("RandomIndex.Count = " + RandomIndex.Count);

        for (int i = 0; i < roundAnswersGOs.Count; i++)
        {
            roundAnswersGOs[i].transform.SetSiblingIndex(RandomIndex[i]);
        }
    }


    // -----------------------------------------------------Panels----------------------------------------------------- //


    public IEnumerator CreateAnswerPanels()
    {
        AudioClip audioClip;

        Debug.Log($"FinalPlayerAnswerGroup List Count is: {FinalPlayerAnswerGroup.Count}");

        for (int i = 0; i < FinalPlayerAnswerGroup.Count; i++)
        {
            answerShowingPanelController.FullPlayerAnswerGroup = FinalPlayerAnswerGroup[i];
            AnswerShowingPanelController apc = Instantiate(answerShowingPanelController, AnswesPanelParentPos);
            Panels.Add(apc);

            Debug.Log($"Panels Added");
        }

        yield return new WaitForSeconds(.01f);

        foreach (AnswerShowingPanelController answerPanelController in Panels)
        {
            Debug.Log($"First answerPanelController");


            Debug.Log($"First fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count {answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count}");

            if (answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count > 0 && !answerPanelController.FullPlayerAnswerGroup.IsCorrect)
            {
                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingBoxSFX);

                answerPanelController.gameObject.SetActive(true);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
                {

                });

                AudioManager.Instance.PlayAudioClip(audioClip);


                yield return new WaitForSeconds(audioClip.length);

                yield return new WaitForSeconds(.5f);

                // play audio....
                // yeild wait for the audio lenght....
                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingNamesSFX);

                for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
                {
                    answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

                    AudioManager.Instance.PlayAudioClip(audioClip);

                    yield return new WaitForSeconds(audioClip.length);

                    yield return new WaitForSeconds(0.35f);

                    Debug.Log($"Showing box for every player choose this");
                }

                // play audio.....
                // yeidl wait for the aduio lenght....
                yield return new WaitForSeconds(1f);

                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnLiesSFX);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    answerPanelController.AnswerField.GetComponent<RectTransform>().localPosition = answerPanelController.AnswerPosition.localPosition;



                    if (answerPanelController.IsCorrectAnswer)
                    {
                        answerPanelController.CorrectAnswerText.gameObject.SetActive(true);

                        answerPanelController.GetComponent<Image>().sprite = answerPanelController.CorrectAnswerImage;
                        answerPanelController.Thumb.SetActive(true);

                        Debug.Log($"Showing thumb on correct answer");

                        for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
                        {

                            answerPanelController.PlayersWhoChoseThis[i].transform.localPosition = answerPanelController.NamesPostitions_Truth[i].localPosition;
                        }
                    }

                    for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.Players.Count; i++)
                    {
                        if (answerPanelController.PlayersWhoMadeThis[i].text == "الاجابة الصحيحة")
                            continue;

                        answerPanelController.PlayersWhoMadeThis[i].gameObject.SetActive(true);
                        
                        if (answerPanelController.FullPlayerAnswerGroup.Players[i].ClientId == 100)
                            answerPanelController.PlayersWhoMadeThis[i].gameObject.SetActive(false);
                    }
                    answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutCubic);
                });

                AudioManager.Instance.PlayAudioClip(audioClip);

                yield return new WaitForSeconds(audioClip.length);

                for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
                {

                    answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);

                    bool IsDefaultAnswer = false;

                    foreach (var playerAnswer in answerPanelController.FullPlayerAnswerGroup.Players)
                    {
                        if (playerAnswer.IsDefaultAnswer)
                        {
                            AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.FullPlayerAnswerGroup.PlayersID_WhoChooseThis[i]), -(GamePlayManager.Instance.roundManager.GetLieScore()));
                            IsDefaultAnswer = true;
                        }
                        else
                        {
                            Debug.Log("NoOne Chose His Owen Answer!");
                            AddRemoveScore(
                                P: GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(playerAnswer.ClientId),
                                amount: GamePlayManager.Instance.roundManager.GetLieScore()
                                /**answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count*/);
                            IsDefaultAnswer = false;
                        }
                    }

                    StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>(), IsDefaultAnswer));
                }

                yield return new WaitForSeconds(2f);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic);
            }
        }

        foreach (AnswerShowingPanelController answerPanelController in Panels)
        {
            Debug.Log($"Second ap2");



            Debug.Log($"Second fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count {answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count}");
            if (answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count > 0 && answerPanelController.FullPlayerAnswerGroup.IsCorrect)
            {
                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingBoxSFX);

                answerPanelController.gameObject.SetActive(true);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);

                AudioManager.Instance.PlayAudioClip(audioClip);

                yield return new WaitForSeconds(audioClip.length);

                yield return new WaitForSeconds(.5f);

                // play audio....

                // yeild wait for the audio lenght....

                for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
                {
                    answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

                    yield return new WaitForSeconds(0.35f);
                }

                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnCorrectAnswerSFX);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    answerPanelController.AnswerField.GetComponent<RectTransform>().localPosition = answerPanelController.AnswerPosition.localPosition;

                    if (answerPanelController.IsCorrectAnswer)
                    {
                        answerPanelController.CorrectAnswerText.gameObject.SetActive(true);

                        answerPanelController.GetComponent<Image>().sprite = answerPanelController.CorrectAnswerImage;
                        answerPanelController.Thumb.SetActive(true);

                        for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
                        {
                            answerPanelController.PlayersWhoChoseThis[i].transform.localPosition = answerPanelController.NamesPostitions_Truth[i].localPosition;
                        }
                    }

                    for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.Players.Count; i++)
                    {
                        if (answerPanelController.PlayersWhoMadeThis[i].text == "الاجابة الصحيحة")
                            continue;

                        answerPanelController.PlayersWhoMadeThis[i].gameObject.SetActive(true);
                        if (answerPanelController.FullPlayerAnswerGroup.Players[i].ClientId == 100)
                            answerPanelController.PlayersWhoMadeThis[i].gameObject.SetActive(false);
                    }

                    answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutCubic);
                });


                AudioManager.Instance.PlayAudioClip(audioClip);

                yield return new WaitForSeconds(audioClip.length);


                for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count; i++)
                {

                    answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);

                    Debug.Log($"Scores Added On Correct Answer For: {GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.FullPlayerAnswerGroup.PlayersID_WhoChooseThis[i]).PlayerName}");

                    AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.FullPlayerAnswerGroup.PlayersID_WhoChooseThis[i]), GamePlayManager.Instance.roundManager.GetCorrectScore());

                    StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>()));
                }
                for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.Players.Count; i++)
                {
                    if (answerPanelController.FullPlayerAnswerGroup.Players[i].ClientId != 100)
                        AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.FullPlayerAnswerGroup.Players[i].ClientId), GamePlayManager.Instance.roundManager.GetCorrectScore());
                }

                yield return new WaitForSeconds(2f);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic);
            }

            else if (answerPanelController.FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count == 0 && answerPanelController.FullPlayerAnswerGroup.IsCorrect)
            {
                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingBoxSFX);

                answerPanelController.gameObject.SetActive(true);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
                {
                    AudioManager.Instance.PlayAudioClip(audioClip);
                });

                yield return new WaitForSeconds(audioClip.length);

                yield return new WaitForSeconds(.5f);

                yield return new WaitForSeconds(1f);


                // Assign Audio To Play Later On...
                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnCorrectAnswerEmptySFX);


                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                {
                    answerPanelController.AnswerField.GetComponent<RectTransform>().localPosition = answerPanelController.AnswerPosition.localPosition;

                    if (answerPanelController.IsCorrectAnswer)
                    {
                        answerPanelController.CorrectAnswerText.gameObject.SetActive(true);
                        answerPanelController.GetComponent<Image>().sprite = answerPanelController.CorrectAnswerImage;
                        answerPanelController.Thumb.SetActive(true);
                    }

                    for (int i = 0; i < answerPanelController.FullPlayerAnswerGroup.Players.Count; i++)
                    {
                        if (answerPanelController.PlayersWhoMadeThis[i].text == "الاجابة الصحيحة")
                            continue;
                        answerPanelController.PlayersWhoMadeThis[i].gameObject.SetActive(true);
                        if (answerPanelController.FullPlayerAnswerGroup.Players[i].ClientId == 100)
                            answerPanelController.PlayersWhoMadeThis[i].gameObject.SetActive(false);
                    }


                    answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one, 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
                    {
                        AudioManager.Instance.PlayAudioClip(AudioManager.Instance.Fail);
                        AudioManager.Instance.PlayAudioClip(audioClip);
                    });
                });

                yield return new WaitForSeconds(audioClip.length);

                audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.NarratorOnNoPickCorrectAnswerAudioClips);

                AudioManager.Instance.PlayAudioClip(audioClip);

                yield return new WaitForSeconds(audioClip.length);

                yield return new WaitForSeconds(0.5f);

                answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic);
            }
        }

        int NotClicked_PositionsIndex = 0;

        for (int i = 0; i < Panels.Count; i++)
        {
            if (Panels[i].FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count == 0)
            {
                Panels[i].gameObject.SetActive(true);

                Panels[i].GetComponent<RectTransform>().localPosition = GamePlayManager.Instance.NotClicked_Positions[NotClicked_PositionsIndex].localPosition;

                Panels[i].GetComponent<RectTransform>().DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0).SetEase(Ease.OutBounce);

                NotClicked_PositionsIndex++;
            }
        }

        yield return new WaitForSeconds(3f);

        for (int i = 0; i < Panels.Count; i++)
        {
            if (Panels[i].FullPlayerAnswerGroup.PlayersNames_WhoChooseOwnerAnswer.Count == 0)
            {
                Panels[i].GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InOutCubic);

                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(3);

        if (!IsServer)
            yield break;

        GamePlayManager.Instance.state.Value = GamePlayManager.State.Results;



        #region Old...

        // foreach (AnswerShowingPanelController answerPanelController in Panels)
        // {
        //     Debug.Log($"First answerPanelController");


        //     Debug.Log($"First fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count {answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count}");

        //     if (answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count > 0 && !answerPanelController.fullPlayerAnswer.IsCorrect)
        //     {
        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingBoxSFX);

        //         answerPanelController.gameObject.SetActive(true);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one , 0.5f).SetEase(Ease.OutBounce).OnComplete(()=>
        //         {

        //         });

        //         AudioManager.Instance.PlayAudioClip(audioClip);


        //         yield return new WaitForSeconds(audioClip.length);

        //         yield return new WaitForSeconds(.5f);

        //         // play audio....

        //         // yeild wait for the audio lenght....

        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingNamesSFX);

        //         // AudioClip audioClip1 = AudioManager.Instance.GetRandomAudioClip(AudioManager.Instance.Drums);

        //         // yield return new WaitForSeconds(2f);

        //         for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //         {
        //             answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

        //             AudioManager.Instance.PlayAudioClip(audioClip);

        //             yield return new WaitForSeconds(audioClip.length);

        //             yield return new WaitForSeconds(0.35f);

        //             Debug.Log($"Showing box for every player choose this");
        //         }

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //     // ^
        //     // ^
        //     // ^
        //         // AudioClip audioClip1 = AudioManager.Instance.GetRandomAudioClip(AudioManager.Instance.Drums);



        //         // AudioManager.Instance.playon(AudioManager.Instance.OnLiesSFX);

        //         yield return new WaitForSeconds(1f);

        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnLiesSFX);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        //         {
        //             answerPanelController.AnswerField.GetComponent<RectTransform>().localPosition = answerPanelController.AnswerPosition.localPosition;



        //             if(answerPanelController.IsCorrectAnswer)
        //             {
        //                 // audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnCorrectAnswerSFX);

        //                 answerPanelController.GetComponent<Image>().sprite = answerPanelController.CorrectAnswerImage;
        //                 answerPanelController.Thumb.SetActive(true);

        //                 Debug.Log($"Showing thumb on correct answer");

        //                 for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //                 {
        //                     answerPanelController.PlayersWhoChoseThis[i].transform.localPosition = answerPanelController.NamesPostitions_Truth[i].localPosition;
        //                 }
        //             }

        //             answerPanelController.PlayerWhoMadeIt.gameObject.SetActive(true);

        //             answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one , 0.2f).SetEase(Ease.InOutCubic);

        //             // if (answerPanelController.IsCorrectAnswer)


        //         });

        //         // AudioManager.Instance.PlayAudioClip(AudioManager.Instance.Drums);

        //         // yield return new WaitForSeconds(1);

        //         AudioManager.Instance.PlayAudioClip(audioClip);

        //         yield return new WaitForSeconds(audioClip.length);

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //     // ^
        //     // ^
        //     // ^
        //         // yield return new WaitForSeconds(1f);


        //         for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //         {

        //             answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);

        //             if (answerPanelController.fullPlayerAnswer.IsDefaultAnswer)
        //             {
        //                 AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.fullPlayerAnswer.PlayersIDWhoChooseThis[i]), - (GamePlayManager.Instance.roundManager.GetLieScore()));
        //                 StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>(), true));
        //             }
        //             else
        //             {
        //                 Debug.Log("NoOne Chose His Owen Answer!");
        //                 AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.fullPlayerAnswer.ClientId), GamePlayManager.Instance.roundManager.GetLieScore()/**answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count*/);
        //                 StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>()));
        //             }

        //             // StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>()));
        //         }

        //         // AddRemoveScore(answerPanelController.fullPlayerAnswer, GamePlayManager.Instance.roundManager.GetLieScore()*answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count);

        //         yield return new WaitForSeconds(2f);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic);
        //     }
        // }

        // foreach (AnswerShowingPanelController answerPanelController in Panels)
        // {
        //     Debug.Log($"Second ap2");



        //     Debug.Log($"Second fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count {answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count}");
        //     if (answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count > 0 && answerPanelController.fullPlayerAnswer.IsCorrect)
        //     {
        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingBoxSFX);

        //         answerPanelController.gameObject.SetActive(true);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one , 0.5f).SetEase(Ease.OutBounce);

        //         // AudioManager.Instance.PlayAudioClip(AudioManager.Instance.Drums);

        //         // yield return new WaitForSeconds(2f);

        //         AudioManager.Instance.PlayAudioClip(audioClip);

        //         yield return new WaitForSeconds(audioClip.length);

        //         yield return new WaitForSeconds(.5f);

        //         // play audio....

        //         // yeild wait for the audio lenght....

        //         for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //         {
        //             answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

        //             yield return new WaitForSeconds(0.35f);
        //         }

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //     // ^
        //     // ^
        //     // ^


        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnCorrectAnswerSFX);





        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        //         {
        //             answerPanelController.AnswerField.GetComponent<RectTransform>().localPosition = answerPanelController.AnswerPosition.localPosition;

        //             if(answerPanelController.IsCorrectAnswer)
        //             {
        //                 // audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnCorrectAnswerSFX);

        //                 answerPanelController.GetComponent<Image>().sprite = answerPanelController.CorrectAnswerImage;
        //                 answerPanelController.Thumb.SetActive(true);

        //                 for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //                 {
        //                     answerPanelController.PlayersWhoChoseThis[i].transform.localPosition = answerPanelController.NamesPostitions_Truth[i].localPosition;
        //                 }
        //             }

        //             answerPanelController.PlayerWhoMadeIt.gameObject.SetActive(true);

        //             answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one , 0.2f).SetEase(Ease.InOutCubic);
        //         });




        //         AudioManager.Instance.PlayAudioClip(audioClip);

        //         yield return new WaitForSeconds(audioClip.length);

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //     // ^
        //     // ^
        //     // ^
        //         // yield return new WaitForSeconds(1f);

        //         for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //         {

        //             answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);

        //             Debug.Log($"Scores Added On Correct Answer For: {GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.fullPlayerAnswer.PlayersIDWhoChooseThis[i]).PlayerName}");

        //             // if (answerPanelController.fullPlayerAnswer.IsDefaultAnswer)
        //             // {
        //             //     AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.fullPlayerAnswer.PlayersIDWhoChooseThis[i]), - (GamePlayManager.Instance.roundManager.GetCorrectScore()));
        //             //     StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>(), true));
        //             // }
        //             // else
        //             // {
        //             //     AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.fullPlayerAnswer.PlayersIDWhoChooseThis[i]), GamePlayManager.Instance.roundManager.GetCorrectScore());
        //             //     StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>()));
        //             // }

        //             AddRemoveScore(GamePlayManager.Instance.GetOriginalPlayersInSessionFromClientID(answerPanelController.fullPlayerAnswer.PlayersIDWhoChooseThis[i]), GamePlayManager.Instance.roundManager.GetCorrectScore());
        //             StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>()));
        //         }

        //         yield return new WaitForSeconds(2f);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic);
        //     }

        //     else if (answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count == 0 && answerPanelController.fullPlayerAnswer.IsCorrect)
        //     {
        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnShowingBoxSFX);

        //         answerPanelController.gameObject.SetActive(true);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one , 0.5f).SetEase(Ease.OutBounce).OnComplete(() =>
        //         {
        //             AudioManager.Instance.PlayAudioClip(audioClip);
        //         });

        //         yield return new WaitForSeconds(audioClip.length);

        //         yield return new WaitForSeconds(.5f);

        //         // play audio....

        //         // yeild wait for the audio lenght....

        //         // for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //         // {
        //         //     answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

        //         //     yield return new WaitForSeconds(0.35f);
        //         // }

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //     // ^
        //     // ^
        //     // ^
        //         yield return new WaitForSeconds(1f);


        //         // Assign Audio To Play Later On...
        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.OnCorrectAnswerEmptySFX);
        //         // audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.NarratorOnNoPickCorrectAnswerAudioClips);


        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        //         {
        //             answerPanelController.AnswerField.GetComponent<RectTransform>().localPosition = answerPanelController.AnswerPosition.localPosition;

        //             if(answerPanelController.IsCorrectAnswer)
        //             {
        //                 answerPanelController.GetComponent<Image>().sprite = answerPanelController.CorrectAnswerImage;
        //                 answerPanelController.Thumb.SetActive(true);

        //                 // for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //                 // {
        //                 //     answerPanelController.PlayersWhoChoseThis[i].transform.localPosition = answerPanelController.NamesPostitions_Truth[i].localPosition;
        //                 // }
        //             }

        //             answerPanelController.PlayerWhoMadeIt.gameObject.SetActive(true);

        //             answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.one , 0.2f).SetEase(Ease.InOutCubic).OnComplete(() =>
        //             {
        //                 // Debug.Log("No One Picked Correct Answer Audio Clip Played");
        //                 AudioManager.Instance.PlayAudioClip(AudioManager.Instance.Fail);
        //                 AudioManager.Instance.PlayAudioClip(audioClip);
        //             });
        //         });

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //     // ^
        //     // ^
        //     // ^
        //         yield return new WaitForSeconds(audioClip.length);

        //         audioClip = AudioManager.Instance.GetRandomAudioClipFromPool(AudioManager.Instance.NarratorOnNoPickCorrectAnswerAudioClips);

        //         AudioManager.Instance.PlayAudioClip(audioClip);

        //         yield return new WaitForSeconds(audioClip.length);

        //         // for (int i = 0 ; i < answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count ; i++)
        //         // {

        //         //     answerPanelController.PlayersWhoChoseThis[i].DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutCubic);

        //         //     AddRemoveScore(answerPanelController.fullPlayerAnswer, GamePlayManager.Instance.roundManager.GetCorrectScore()*answerPanelController.fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count);

        //         //     StartCoroutine(TextAnimation(answerPanelController.ScoreText[i].GetComponent<RectTransform>(), answerPanelController.GetComponent<RectTransform>()));
        //         // }

        //         yield return new WaitForSeconds(0.5f);

        //         answerPanelController.GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic);
        //     }
        // }

        // int NotClicked_PositionsIndex = 0;

        // for(int i = 0; i < Panels.Count; i++)
        // {
        //     if (Panels[i].fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count == 0)
        //     {
        //         Panels[i].gameObject.SetActive(true);

        //         Panels[i].GetComponent<RectTransform>().localPosition = GamePlayManager.Instance.NotClicked_Positions[NotClicked_PositionsIndex].localPosition;

        //         Panels[i].GetComponent<RectTransform>().DOScale(new Vector3(0.5f, 0.5f, 0.5f) , 0).SetEase(Ease.OutBounce);

        //         //yield return new WaitForSeconds(.5f);

        //         // play audio....

        //         // yeild wait for the audio lenght....

        //         // play audio.....
        //         // yeidl wait for the aduio lenght....
        //         // ^
        //         // ^
        //         // ^

        //         //Panels[i].GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic);

        //         NotClicked_PositionsIndex++;
        //     }


        // }

        // yield return new WaitForSeconds(3f);

        // for(int i = 0; i < Panels.Count; i++)
        // {
        //     if (Panels[i].fullPlayerAnswer.PlayersWhoChooseOwnerAnswer.Count == 0)
        //     {
        //         Panels[i].GetComponent<RectTransform>().DOScale(Vector3.zero , 0.2f).SetEase(Ease.InOutCubic);

        //         yield return new WaitForSeconds(0.1f);
        //     }
        // }

        // // if(GamePlayManager.Instance.roundManager.GetRoundCount() == 3)
        // // {
        // //     GamePlayManager.Instance.WinnerScreen.SetActive(true);
        // //     GamePlayManager.Instance.ShowingAnswersScreen.SetActive(false);

        // //     yield break;
        // // }

        // if(!IsServer)
        //     yield break;

        // GamePlayManager.Instance.state.Value = GamePlayManager.State.Results;

        // // StartCoroutine(CreateResults());

        // // // ResetRound();

        // // GamePlayManager.Instance.RoundResultsScreen.SetActive(true);
        // // GamePlayManager.Instance.ShowingAnswersScreen.SetActive(false);

        // yield return new WaitForSeconds(3);

        #endregion

    }



    public IEnumerator TextAnimation(RectTransform _rect, RectTransform _rect2, bool _isWrong = false)
    {
        if (_isWrong)
            _rect.GetComponent<RTLTextMeshPro>().color = Color.red;

        _rect.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

        yield return new WaitForSeconds(1);

        _rect.DOAnchorPosY(-60, 0.2f);
        _rect.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.2f).OnComplete(() =>
        {
            AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.OnAddingScoresSFX);

            _rect.DOAnchorPosY(20, 0.2f);
            _rect.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                // _rect.DOAnchorPosY(-25, 0.2f);
                _rect2.DOAnchorPosY(-47, 0.2f).OnComplete(() =>
                {
                    _rect2.DOAnchorPosY(-57, 0.2f);
                });
            });
        });

        yield return new WaitForSeconds(1.2f);
    }


    // ----------------------------------------------------- -----------------------------------------------------//

    // public void AddToFinalPlayerAnswersList(PlayerSessionInfo _playerAnswers)
    // {
    //     finalPlayersAnswer.Add(_playerAnswers);
    // }

    public void PlayerAnsweredThis(AnswerController ac)
    {
        // ac.playerAnswer.playersWhoClicked.Add(PlayerDataManager.Instance.GetPlayerInfo());
        //To server -> SendPlayerChoice
    }

    // ------------------------------------Results Functions------------------------------------ //

    public void CreateResults()
    {
        foreach (PlayerSessionInfo _pi in GamePlayManager.Instance.OriginalPlayersInSession)
        {
            if (!_pi.IsCorrect)
            {
                if (IsServer)
                {
                    Scores.Add(_pi.PlayerScore);
                }

                resultsController.fullPlayerAnswer = _pi;
                ResultsController rc = Instantiate(resultsController, ResultsControllerParentPos);

                // rc.score_Int = Scores[i];

                Results.Add(rc);
            }
        }

        Results = Results.OrderByDescending(results => results.fullPlayerAnswer.PlayerScore).ToList();

        for (int i = 0; i < Results.Count; i++)
        {
            Results[i].transform.SetSiblingIndex(i);

            int rank = i;

            Results[i].PlayerRank = rank + 1;
            Debug.Log(Results[i].PlayerRank);

            Results[i].RankText.text = Results[i].PlayerRank.ToString();
        }

        for (int i = 0; i < GamePlayManager.Instance.OriginalPlayersInSession.Count; i++)
        {
            for (int j = 0; j < Results.Count; j++)
            {
                if (GamePlayManager.Instance.OriginalPlayersInSession[i].ClientId == Results[j].fullPlayerAnswer.ClientId)
                {
                    Debug.Log("Matched!");
                    GamePlayManager.Instance.OriginalPlayersInSession[i].PlayerRank = Results[j].PlayerRank;
                    Debug.Log(GamePlayManager.Instance.OriginalPlayersInSession[i].PlayerRank);
                }
            }
        }

        AudioManager.Instance.PlayAudioClip(AudioManager.Instance.ScoreCounter);
        // yield return new WaitForSeconds(3);
    }

    public void AssignFinalResultsControllerList()
    {
        for (int i = 0; i < finalResultsControllerList.Count; i++)
        {
            int rank = i;
            finalResultsControllerList[i].WinnersPlayersInfo = GamePlayManager.Instance.GetPlayerSessionInfoFromPlayerRank(rank + 1);
        }

        if (IsServer)
            GamePlayManager.Instance.state.Value = GamePlayManager.State.GameEnd;

        GamePlayManager.Instance.WinnerScreen.SetActive(false);
        GamePlayManager.Instance.FinalResultsScreen.SetActive(true);
    }

    public void ResetPlayersScores()
    {
        // foreach(var v in DemoPlayers)
        // {
        //     // v.ownerInfo.PlayerScore = 0;
        // }

        // PlayerDataManager.Instance.SetPlayerScore(0);
    }


    // amount is: **roundmanager.getscore()*fullplayeranswer.playerswhoclicked.count**
    // P is: **fullplayeranswer.ownerinfo**
    public void AddRemoveScore(PlayerSessionInfo P, int amount)
    {
        for (int i = 0; i < GamePlayManager.Instance.OriginalPlayersInSession.Count; i++)
        {
            if (GamePlayManager.Instance.OriginalPlayersInSession[i].ClientId == P.ClientId)
            {
                // var PIR = GamePlayManager.Instance.PlayersInSessionList[i];
                Debug.Log("Scores Added");
                // PIR.PlayerScore += amount;
                GamePlayManager.Instance.OriginalPlayersInSession[i].PlayerScore += amount;

                // PlayersInRoom[i].PlayerScore += amount;

                break;
            }
        }

    }


    public void AddPlayersWhoClicked(RoomSessionPlayerAnswer P)
    {
        int temp = playersAnswer.IndexOf(P);
        // playersAnswer[temp].playersWhoClicked = P.playersWhoClicked;
    }

    // public List<AnswerController> Shuffle(List<AnswerController> go, bool _addToNewList = false)
    // {
    //     List<AnswerController> tempList = go;
    //     for (int i = 0; i < go.Count; i++)
    //     {
    //         AnswerController tempObject = go[i];
    //         int randomIndex = UnityEngine.Random.Range(i, go.Count);
    //         go[i] = go[randomIndex];
    //         go[randomIndex] = tempObject;

    //         // if (_addToNewList)
    //         // {
    //         //     finalPlayersAnswer.Add(go[i].playerSessionInfo);
    //         // }
    //     }

    //     return tempList;
    // }
    public void ShuffleVoid(List<AnswerController> go, bool _addToNewList = false)
    {
        for (int i = 0; i < go.Count; i++)
        {
            // AnswerController tempObject = go[i];
            int randomIndex = UnityEngine.Random.Range(i, go.Count);
            // go[i] = go[randomIndex];
            // go[randomIndex] = tempObject;

            RandomIndex.Add(randomIndex);
            Debug.Log("RandomIndex : " + RandomIndex[i]);
        }
    }

    public void Shuffle(List<AnswerController> list)
    {
        // if (!IsServer)
        //     return;

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            AnswerController value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public void ShuffleNetworkList()
    {
        // if (!IsServer)
        //     return;

        int n = RandomIndex.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            int value = RandomIndex[k];
            RandomIndex[k] = RandomIndex[n];
            RandomIndex[n] = value;
        }
    }

    // ---------------------------------------------------------------------------------------------------------//

    public void AssignSessionRounds(List<Round> _rounds)
    {
        rounds = _rounds;
        AssignSessionQuestions();
    }

    public void AssignSessionQuestions()
    {
        switch (RoundManager.Instance.GetRoundCount())
        {
            case 1:
                currentRoundQuestions = rounds[0].questions;
                break;

            case 2:
                currentRoundQuestions = rounds[1].questions;
                break;

            case 3:
                currentRoundQuestions = rounds[2].questions;
                break;
        }
    }

    public void FindDuplicates(List<string> strings)
    {
        HashSet<string> uniqueStrings = new HashSet<string>();
        HashSet<string> duplicateStrings = new HashSet<string>();

        foreach (string str in strings)
        {
            if (!uniqueStrings.Add(str)) // If it fails to add, it means it's a duplicate
            {
                duplicateStrings.Add(str);
            }
        }

        if (duplicateStrings.Count > 0)
        {
            Debug.Log("Duplicates found:");
            foreach (string str in duplicateStrings)
            {
                Debug.Log(str);
            }
        }
        else
        {
            Debug.Log("No duplicates found.");
        }
    }
}