using System.Collections;
using System.Collections.Generic;


using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using RTLTMPro;
using TMPro;

public class RegistrationManager : MonoBehaviour
{
    #region Instance
    public static RegistrationManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<RegistrationManager>();

            return _instance;
        }
    }

    private static RegistrationManager _instance;
    #endregion


    [Header("Main Canvas")]
    [SerializeField] RectTransform MainCanvas;

    [Header("Loading Screen")]
    public GameObject BufferingScreen;


    [Header("***Main Info***")]
    [SerializeField] GameObject TurtoralScreen;
    [SerializeField] GameObject LoginScreen;


    [Header("***Tutorial Screen Info***")]
    public Image[] dots;
    public GameObject[] panels;
    public float animationDuration = 0.5f;
    public float dotScale = 1f;
    public float dotScaleSelected = 1.5f;
    public float dotPadding = 0f;

    int currentDot = 0;
    RectTransform[] dotRectTransforms;
    Vector2[] dotOriginalPos;

    Vector2 panelOffscreenPosRight;
    Vector2 panelOffscreenPosLeft;
    Vector2 panelOnscreenPos;
    private RectTransform[] panelRectTransforms;

    int TutorialPageCounter;

    [Header("***Login Info***")]
    public string Name;
    public string Email;
    public string PassWord;

    [Header("Objects")]
    // public LoginInfo loginInfo;

    public PlayerInfo playerInfo;

    public RectTransform Blue_Strock;

    // public string SignUp_Text;
    // public string SignIn_Text;

    public Button GoogleSignIn_Button;
    public Button AppleSignIn_Button;

    // public RTLTextMeshPro ContinueText;

    public TMP_InputField Login_Email_Input;
    public TMP_InputField Login_PassWord_Input;


    public string SignUp_Name_Input_Temp;

    public TMP_InputField SignUp_Name_Input;
    public TMP_InputField SignUp_Email_Input;
    public TMP_InputField SignUp_PassWord_Input;

    public bool IsNameEntered = false;

    public bool IsLoginButtonPressed;

    [SerializeField] Button NextButton;

    [SerializeField] RectTransform[] LoginScreens;



    public bool IsNewPlayer;


    public void SwitchBetweenLoginScreens()
    {
        LoginScreens[0].DOAnchorPosX(panelOffscreenPosLeft.x, .5f).SetEase(Ease.InOutCubic);
        LoginScreens[1].DOAnchorPosX(panelOnscreenPos.x, .5f).SetEase(Ease.InOutCubic);
    }
    public void LoginScreensStartUp()
    {
        LoginScreens[0].anchoredPosition = panelOnscreenPos;
        LoginScreens[1].anchoredPosition = panelOffscreenPosRight;
    }
    // private void Awake() {
    //     if(!AuthenticationManager.Instance.loggedIn)
    //     {
    //         AuthenticationManager.Instance.checkLogIn();
    //         AuthenticationManager.Instance.loggedIn = true;
    //     }
            
    // }
    void Start()
    {
        if(!AuthenticationManager.Instance.loggedIn)
        {
            AuthenticationManager.Instance.checkLogIn();
            AuthenticationManager.Instance.loggedIn = true;
        }

        NextButton.onClick.AddListener(() =>
        {
            OnLoginContinueClicked();
        });

        if (PlayerPrefManager.PlayerAuthData.GetAuthMethod() == 0 || PlayerPrefManager.PlayerAuthData.GetAuthMethod() == -1)
            StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.GameStart));

        if (Application.platform == RuntimePlatform.Android)
            AppleSignIn_Button.gameObject.SetActive(false);
        
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            GoogleSignIn_Button.gameObject.SetActive(false);

        if (PlayerPrefManager.PlayerFirstTimeData.GetTutorialFirstTime())
        {
            TurtoralScreen.SetActive(false);
            LoginScreen.SetActive(true);
        }
        else
        {
            PlayerPrefManager.PlayerFirstTimeData.SetTutorialFirstTime();
        }

        // Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(MainCanvas);

        NextButton.interactable = false;

        SetUpTheDotes();

        LoginScreensStartUp();

        if (GlobalManager.Instance.IsChangeUserNameSection)
        {
            SignUp_Name_Input.text = PlayerDataManager.Instance.GetPlayerNamePlayerData();
            SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = PlayerDataManager.Instance.GetPlayerNamePlayerData();

            GlobalManager.Instance.IsChangeUserNameSection = false;
            SwitchBetweenLoginScreens();
        }
            

        // Login_Email_Input.onEndEdit.AddListener(AssignEmail);
        // Login_PassWord_Input.onEndEdit.AddListener(AssignPassWord);
        // SignUp_Email_Input.onEndEdit.AddListener(AssignEmail);
        // SignUp_PassWord_Input.onEndEdit.AddListener(AssignPassWord);
        // SignUp_Name_Input.onEndEdit.AddListener(AssignName);
    }


    // ------------------------------LogIn Section----------------------------//

    public void OnGuestLoginButtonPressed()
    {
        BufferingScreen.SetActive(true);

        // if (AuthenticationManager.Instance.userLoggedInState != UserLoggedInState.Anonymously)
            AuthenticationManager.Instance.AuthenticateAnonymouslyAsync();
        // else
        // {
        //     BufferingScreen.SetActive(false);
        //     await AuthenticationManager.Instance.LoadPlayerData();
        //     Debug.Log("Player Is Already SignedIn As A Guest!");
        //     SwitchBetweenLoginScreens();
        // }

    }

    public void OnGoogleButtonPressed()
    {
#if UNITY_ANDROID
        //Enable This Once Switched Back To Android Patform.......
        AuthenticationManager.Instance.LoginGoogleAsync();
        //Enable This Once Switched Back To Android Patform.......
#endif
    }

    public void OnAppleButtonPressed()
    {
// #if UNITY_IOS
        AuthenticationManager.Instance.LoginToApple();
// #endif
    }


    public void OnSignUpPressed()
    {
        //Play Button Audios..
        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        //------------

        // ContinueText.text = SignUp_Text;
        // LoginIn_Button.gameObject.SetActive(false);
        // SignUp_Button.gameObject.SetActive(true);

        UIAnimator.Instance.Move_X_Animation(Blue_Strock, -100, 1);

        // FireBaseManager.Instance.ClearOutPut();
    }

    public void OnSignInPressed()
    {
        //Play Button Audios..
        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        //------------

        // ContinueText.text = SignIn_Text;
        // LoginIn_Button.gameObject.SetActive(true);
        // SignUp_Button.gameObject.SetActive(false);

        UIAnimator.Instance.Move_X_Animation(Blue_Strock, 95f, 1);

        // FireBaseManager.Instance.ClearOutPut();
    }

    public void AssignName(string _value)
    {
        // //Play Button Audios..
        // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        // //------------

        Name = _value;
        playerInfo.PlayerName = _value;

        IsNameEntered = true;
    }
    public void AssignEmail(string _value)
    {
        // //Play Button Audios..
        // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        // //------------

        Email = _value;
        playerInfo.PlayerEmail = _value;
    }
    public void AssignPassWord(string _value)
    {
        // //Play Button Audios..
        // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        // //------------

        PassWord = _value;
    }

    // public async void SkipCustomization()
    // {
    //         ChoosingChatacrerScreen.SetActive(false);
    //         CharacterCutomizationScreen_Female.SetActive(false);
    //         CharacterCutomizationScreen_Male.SetActive(false);

    //         UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

    //         StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.HomeMenu));


    //         if(GlobalManager.Instance.IsMale)
    //         {

    //             // PlayerDataManager.Instance.MaleCharacter.GetComponent<CustomizationContoller>().SetAllCharacterSkins(PlayerDataManager.Instance.GetPlayerInfo());

    //             // Old...
    //             PlayerDataManager.Instance.playerInfo.IsMan = true;

    //             // New...
    //             PlayerDataManager.Instance.playerData.IsMan = true;

    //             // NetworkManager.Singleton.AddNetworkPrefab(PlayerDataManager.Instance.MaleCharacter);

    //             MaleDanceScreen.SetActive(true);
    //             FemaleDanceScreen.SetActive(false);
    //         }

    //         else
    //         {
    //             // PlayerDataManager.Instance.FemaleCharacter.GetComponent<CustomizationContoller>().SetAllCharacterSkins(PlayerDataManager.Instance.GetPlayerInfo());

    //             // Old...
    //             PlayerDataManager.Instance.playerInfo.IsMan = false;

    //             // New...
    //             PlayerDataManager.Instance.playerData.IsMan = false;

    //             // NetworkManager.Singleton.AddNetworkPrefab(PlayerDataManager.Instance.FemaleCharacter);

    //             FemaleDanceScreen.SetActive(true);
    //             MaleDanceScreen.SetActive(false);
    //         }

    //         LobbyScreen.SetActive(true);

    //         GlobalManager.Instance.isLobby = true;
    // }

    public async void OnLoginContinueClicked(bool _checkIfNameEmpty = true) // Assigned in the inspector...
    {
        if (_checkIfNameEmpty)
        {
            if (SignUp_Name_Input.text == "")
                return;
        }
        if (IsLoginButtonPressed)
            return;

        IsLoginButtonPressed = true;


        // Old...
        // PlayerDataManager.Instance.SetPlayerName(SignUp_Name_Input.text);

        // New...
        PlayerDataManager.Instance.SetPlayerNamePlayerData(SignUp_Name_Input.text);

        // Check if the name is new.
        if (PlayerDataManager.Instance.GetPlayerNamePlayerData() != SignUp_Name_Input_Temp)
        {
            await PlayerDataManager.Instance.SaveDataObject(PlayerDataManager.Instance.PLAYERDATA, PlayerDataManager.Instance.playerData);
            Debug.Log("Data got saved using cloud save!");
        }
        Debug.Log($"Player Name Is: {PlayerDataManager.Instance.GetPlayerNamePlayerData()}");

        BufferingScreen.SetActive(false);
        UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);

        // StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.CharacterCreation));

        Loader.Load(Loader.Scene.HomeScreen);
    }

    // public void OnRegisterClicked()
    // {
    //     Debug.Log("Reg Button Pressed");

    //     // Old...
    //     // PlayerDataManager.Instance.SetPlayerName(SignUp_Name_Input.text);

    //     // New...
    //     PlayerDataManager.Instance.SetPlayerNamePlayerData(SignUp_Name_Input.text);

    //     AuthenticationManager.Instance.AuthenticateWithGoogle();
    //     UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
    //     Loader.Load(Loader.Scene.GamePlayScreen);
    // }

    public void CheckIfUserNameInputIsEmpty()
    {
        if (SignUp_Name_Input.text == "")
            NextButton.interactable = false;

        else
            NextButton.interactable = true;
    }

    // ------------------------------Tutorial Section----------------------------//

    void SetUpTheDotes()
    {
        dotRectTransforms = new RectTransform[dots.Length];
        dotOriginalPos = new Vector2[dots.Length];
        panelRectTransforms = new RectTransform[panels.Length];

        panelOffscreenPosRight = new Vector2(UIManagerTheUltimate.Instance.RightStartPosition, 0);
        panelOffscreenPosLeft = new Vector2(UIManagerTheUltimate.Instance.LeftStartPosition, 0);
        panelOnscreenPos = Vector2.zero;

        for (int i = 0; i < dots.Length; i++)
        {
            dotRectTransforms[i] = dots[i].GetComponent<RectTransform>();
            dotOriginalPos[i] = dotRectTransforms[i].anchoredPosition;
            if (i != currentDot)
                dots[i].color = new Color(1f, 1f, 1f, 0.5f);
            else
                dots[i].color = new Color(1f, 1f, 1f, 1f);

            panelRectTransforms[i] = panels[i].GetComponent<RectTransform>();
            panelRectTransforms[i].anchoredPosition = panelOffscreenPosRight;
        }

        panelRectTransforms[0].anchoredPosition = panelOnscreenPos;

        // dotRectTransforms[currentDot].DOScale(dotScaleSelected, animationDuration);
        // dotRectTransforms[currentDot].DOAnchorPos(dotOriginalPos[currentDot] + new Vector2(0, dotPadding), animationDuration);
    }

    public void NextPanel()
    {
        int nextDot = (currentDot + 1) % dots.Length;
        AnimateDot(currentDot, nextDot);
        currentDot = nextDot;
    }

    public void PrevPanel()
    {
        int prevDot = (currentDot + dots.Length - 1) % dots.Length;
        AnimateDot(currentDot, prevDot);
        currentDot = prevDot;
    }

    void AnimateDot(int from, int to)
    {
        if (TutorialPageCounter > 1)
        {
            ShowLoginScreen();
            return;
        }

        // //Play Button Audios..
        // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        // //------------

        dots[from].color = new Color(1f, 1f, 1f, 0.5f);
        dots[to].color = new Color(1f, 1f, 1f, 1f);

        // dotRectTransforms[from].DOScale(dotScale, animationDuration);
        // dotRectTransforms[to].DOScale(dotScaleSelected, animationDuration);
        // dotRectTransforms[from].DOAnchorPos(dotOriginalPos[from], animationDuration);
        // dotRectTransforms[to].DOAnchorPos(dotOriginalPos[to] + new Vector2(0, dotPadding), animationDuration);

        panelRectTransforms[to].DOAnchorPos(panelOnscreenPos, animationDuration);
        panelRectTransforms[from].DOAnchorPos(panelOffscreenPosLeft, animationDuration);

        TutorialPageCounter++;
    }

    public void ShowLoginScreen()
    {
        // //Play Button Audios..
        // AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        // //------------

        TurtoralScreen.SetActive(false);
        UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
        LoginScreen.SetActive(true);
    }

    public void SetInactive(RTLTextMeshPro _text)
    {
        _text.color = Color.white;
    }
}
