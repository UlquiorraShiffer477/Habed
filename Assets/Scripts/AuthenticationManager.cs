using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

// External dependencies
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using System.Text;

using UnityEngine.SocialPlatforms;

using Unity.Services.Authentication;
using Unity.Services.Core;

using Unity.Services.CloudSave;
using CloudSaveSample;

using RTLTMPro;
using TMPro;
using JetBrains.Annotations;
// using UnityEditorInternal.VR;
using System;



#if UNITY_EDITOR
using ParrelSync;
#endif

public enum UserLoggedInState
{
    None,
    Anonymously,
    Google,
    Apple
}
[DefaultExecutionOrder(0)]
public class AuthenticationManager : MonoBehaviour
{
    #region Instance
    public static AuthenticationManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<AuthenticationManager>();

            return _instance;
        }
    }
    private static AuthenticationManager _instance;
    #endregion

    public UserLoggedInState userLoggedInState;

    //Google Token...
    public string PlayerToken_Google;
    string DefaultUserName = "هبيد جديد";

    // #if UNITY_IOS
    IAppleAuthManager m_AppleAuthManager;
    public string PlayerName_Apple;
    public string PlayerToken_Apple { get; private set; }
    public string Error { get; private set; }
    public bool loggedIn = false;
    public void Initialize()
    {
        var deserializer = new PayloadDeserializer();
        m_AppleAuthManager = new AppleAuthManager(deserializer);
    }

    private async void Start()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions();

#if UNITY_EDITOR
                options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

                await UnityServices.InitializeAsync(options);
                Debug.Log("UnityServices.InitializeAsync completed");


            }
            else
            {
                Debug.Log("UnityServices already initialized");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in Start method: {ex.Message}");
            Debug.LogException(ex);
        }
    }
    public async void checkLogIn()
    {
        int authMethod = PlayerPrefManager.PlayerAuthData.GetAuthMethod();
        Debug.Log($"PlayerPrefManager.PlayerAuthData.GetAuthMethod(): {authMethod}");
        switch (authMethod)
        {
            case -1:
                Debug.Log("No auth method selected");
                break;

            case 1:
                Debug.Log("Authenticating anonymously");
                AuthenticateAnonymouslyAsync();
                break;

            case 2:
                Debug.Log("Starting Google login");
                await LoginGoogleAsync();  // Note: await added here
                break;

            case 3:
                Debug.Log("Starting Apple login");
                LoginToApple();
                break;

            case 0:
                Debug.Log("Auth method 0 selected");
                break;

            default:
                Debug.Log($"Unexpected auth method: {authMethod}");
                break;
        }
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Player signed in successfully: {AuthenticationService.Instance.PlayerId}");
        };
    }
    public void Update()
    {
        if (m_AppleAuthManager != null)
        {
            m_AppleAuthManager.Update();
        }
    }

    private const string APPLE_TOKEN_KEY = "AppleAuthToken";

    public async void LoginToApple()
    {
        // First try to get saved token
        string savedToken = PlayerPrefs.GetString(APPLE_TOKEN_KEY, null);

        if (!string.IsNullOrEmpty(savedToken))
        {
            Debug.Log("Attempting to login with saved token");
            RegistrationManager.Instance.BufferingScreen.SetActive(true);

            try
            {
                // Try to authenticate with saved token
                if (userLoggedInState == UserLoggedInState.None)
                    await AuthenticateWithApple(savedToken);
                else if (userLoggedInState == UserLoggedInState.Anonymously)
                    await LinkWithAppleAsync(savedToken);

                // If we got here, token was valid
                PlayerToken_Apple = savedToken;
                RegistrationManager.Instance.BufferingScreen.SetActive(false);
                return;
            }
            catch (Exception e)
            {
                Debug.Log("Saved token failed, falling back to manual login: " + e.Message);
                RegistrationManager.Instance.BufferingScreen.SetActive(false);
                PlayerPrefs.DeleteKey(APPLE_TOKEN_KEY);
                // Continue to manual login below
            }
        }

        // Initialize the Apple Auth Manager for manual login
        if (m_AppleAuthManager == null)
        {
            Debug.Log("Initializing AppleAuthManager");
            Initialize();
        }

        var loginArgs = new AppleAuthLoginArgs();

        RegistrationManager.Instance.BufferingScreen.SetActive(true);
        m_AppleAuthManager.LoginWithAppleId(
            loginArgs,
            async credential =>
            {
                var appleIDCredential = credential as IAppleIDCredential;
                if (appleIDCredential != null)
                {
                    var idToken = Encoding.UTF8.GetString(
                        appleIDCredential.IdentityToken,
                        0,
                        appleIDCredential.IdentityToken.Length);

                    Debug.Log("Sign-in with Apple successfully done. IDToken: " + idToken);
                    PlayerToken_Apple = idToken;

                    // Save the new token
                    PlayerPrefs.SetString(APPLE_TOKEN_KEY, idToken);
                    PlayerPrefs.Save();

                    if (userLoggedInState == UserLoggedInState.None)
                        await AuthenticateWithApple(PlayerToken_Apple);
                    else if (userLoggedInState == UserLoggedInState.Anonymously)
                        await LinkWithAppleAsync(PlayerToken_Apple);

                    RegistrationManager.Instance.BufferingScreen.SetActive(false);
                }
                else
                {
                    RegistrationManager.Instance.BufferingScreen.SetActive(false);
                    Debug.Log("Sign-in with Apple error. Message: appleIDCredential is null");
                    Error = "Retrieving Apple Id Token failed.";
                }
            },
            error =>
            {
                RegistrationManager.Instance.BufferingScreen.SetActive(false);
                Debug.Log("Sign-in with Apple error. Message: " + error);
                Error = "Retrieving Apple Id Token failed.";
            }
        );
    }
    // #endif


    void Awake()
    {
        //DontDestroyOnLoad(this.gameObject);
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
#endif
    }


    // #if UNITY_ANDROID
    //     //Enable This Once Switched Back To Android Patform.......
    //     void InitializePlayGamesLogin()
    //     {
    //         var config = new PlayGamesClientConfiguration.Builder()
    //             // Requests an ID token be generated.  
    //             // This OAuth token can be used to
    //             // identify the player to other services such as Firebase.
    //             .RequestIdToken()
    //             .Build();

    //         PlayGamesPlatform.InitializeInstance(config);
    //         PlayGamesPlatform.DebugLogEnabled = true;
    //         PlayGamesPlatform.Activate();
    //     }
    //     //Enable This Once Switched Back To Android Patform.......
    // #endif


    // Method to trigger Google Login
    private bool isAuthenticating = false;
    //public GameObject BufferingScreen;
    public async Task LoginGoogleAsync()
    {
#if UNITY_ANDROID
        //BufferingScreen.SetActive(true);
        RegistrationManager.Instance.BufferingScreen.SetActive(true);
        await UnityServices.InitializeAsync();

        Debug.Log("LoginGoogleAsync");
        await LoginGooglePlayGames();

        if (userLoggedInState == UserLoggedInState.None)
            await SignInWithGooglePlayGamesAsync(PlayerToken_Google);
        else if (userLoggedInState == UserLoggedInState.Anonymously)
            await LinkWithGoogleAsync(PlayerToken_Google);
#endif
    }

    // Callback method for login result
#if UNITY_ANDROID
    public Task LoginGooglePlayGames()
    {
        Debug.Log("LoginGooglePlayGames");
        var tcs = new TaskCompletionSource<object>();
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");
                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    PlayerToken_Google = code;
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                    tcs.SetResult(null);
                });
                // RegistrationManager.Instance.BufferingScreen.SetActive(false);
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
                //BufferingScreen.SetActive(false);
                RegistrationManager.Instance.BufferingScreen.SetActive(false);
                tcs.SetException(new Exception("Failed"));
            }
        });
        return tcs.Task;
    }


    async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}"); //Display the Unity Authentication PlayerID
            Debug.Log("SignIn is successful.");
            await LoadPlayerData();
            userLoggedInState = UserLoggedInState.Google;
            PlayerPrefManager.PlayerAuthData.SetAuthMethod((int)userLoggedInState);
            if (RegistrationManager.Instance != null)
                RegistrationManager.Instance.OnLoginContinueClicked(false);
            // BufferingScreen.SetActive(false);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
            Debug.LogException(ex);
        }
    }

    //    public async Task Authenticate()
    //     {
    //         try 
    //         {
    //             PlayGamesPlatform.Activate();
    //             await UnityServices.InitializeAsync();

    //             var signInTask = new TaskCompletionSource<SignInStatus>();

    //             PlayGamesPlatform.Instance.Authenticate((status) =>
    //             {
    //                 signInTask.SetResult(status);
    //             });

    //             var success = await signInTask.Task;
    //             var localUser = PlayGamesPlatform.Instance.localUser;

    //             if (success == SignInStatus.Success)
    //             {
    //                 Debug.Log("Login with Google was successful.");

    //                 var serverAccessTask = new TaskCompletionSource<string>();
    //                 PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
    //                 {
    //                     serverAccessTask.SetResult(code);
    //                 });

    //                 PlayerToken_Google = await serverAccessTask.Task;
    //                 Debug.Log($"Auth code is {PlayerToken_Google}");

    //                 DefaultUserName = localUser.userName;

    //                 if (userLoggedInState == UserLoggedInState.None)
    //                 {
    //                     await AuthenticateWithGoogle(); // Now properly awaitable
    //                 }
    //                 else if (userLoggedInState == UserLoggedInState.Anonymously)
    //                 {
    //                     Debug.Log("Already authenticated anonymously");
    //                     // await LinkWithGoogleAsync(PlayerToken_Google);
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.Log($"Unsuccessful login: {success}");
    //                 if (RegistrationManager.Instance != null)
    //                 {
    //                     RegistrationManager.Instance.BufferingScreen.SetActive(false);
    //                 }
    //             }
    //         }
    //         catch (Exception ex)
    //         {
    //             Debug.LogError($"Authentication error: {ex.Message}");
    //             if (RegistrationManager.Instance != null)
    //             {
    //                 RegistrationManager.Instance.BufferingScreen.SetActive(false);
    //             }
    //         }
    //     }
#endif
    public string GooglePlayError;



    public async Task LoadPlayerData()
    {
        // Load saved data...
        if (await PlayerDataManager.Instance.RetrieveSpecificData<PlayerData>(PlayerDataManager.Instance.PLAYERDATA) != null)
        {
            // Debug.Log(PlayerDataManager.Instance.RetrieveSpecificData<PlayerData>(PlayerDataManager.Instance.PLAYERDATA));
            PlayerDataManager.Instance.playerData = await PlayerDataManager.Instance.RetrieveSpecificData<PlayerData>(PlayerDataManager.Instance.PLAYERDATA);

            RegistrationManager.Instance.SignUp_Name_Input.text = PlayerDataManager.Instance.playerData.PlayerName;
            RegistrationManager.Instance.SignUp_Name_Input_Temp = PlayerDataManager.Instance.playerData.PlayerName;
            RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = PlayerDataManager.Instance.playerData.PlayerName;
            Debug.Log("Authenticated Player Has Data!");

            if (PlayerDataManager.Instance.playerData.playerInfo.gender == global::Gender.NotSelected)
                GlobalManager.Instance.IsNewPlayer = true;
            else
                GlobalManager.Instance.IsNewPlayer = false;
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
        else
        {
            Debug.Log("Authenticated Player Does Not Have Data!");
            GlobalManager.Instance.IsNewPlayer = true;

            PlayerDataManager.Instance.playerData.PlayerID = AuthenticationService.Instance.PlayerId;
            PlayerDataManager.Instance.playerData.PlayerCoins = 2000;

            // Debug.Log(DefaultUserName);

            DefaultUserName = "هبيد جديد" + " - " + UnityEngine.Random.Range(0, 10) + UnityEngine.Random.Range(0, 10) + UnityEngine.Random.Range(0, 10) + UnityEngine.Random.Range(0, 10) + UnityEngine.Random.Range(0, 10);
            Debug.Log(DefaultUserName);

            PlayerDataManager.Instance.playerData.PlayerName = DefaultUserName;

            RegistrationManager.Instance.SignUp_Name_Input.text = PlayerDataManager.Instance.playerData.PlayerName;
            RegistrationManager.Instance.SignUp_Name_Input_Temp = PlayerDataManager.Instance.playerData.PlayerName;
            RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = PlayerDataManager.Instance.playerData.PlayerName;

            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
    }


    public async void AuthenticateAnonymouslyAsync()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        await LoadPlayerData();

        userLoggedInState = UserLoggedInState.Anonymously;

        PlayerPrefManager.PlayerAuthData.SetAuthMethod(1);

        // RegistrationManager.Instance.SignUp_Name_Input.text = DefaultUserName;
        // RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = DefaultUserName;

        Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

        // RegistrationManager.Instance.SwitchBetweenLoginScreens();
        RegistrationManager.Instance.OnLoginContinueClicked(false);
    }

    public async Task AuthenticateWithApple(string idToken)
    {
        try
        {
            Debug.Log("5");

            await AuthenticationService.Instance.SignInWithAppleAsync(idToken);

            Debug.Log("6");

            await LoadPlayerData();

            // RegistrationManager.Instance.SignUp_Name_Input.text = PlayerName_Apple;
            // RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = PlayerName_Apple;

            userLoggedInState = UserLoggedInState.Apple;

            PlayerPrefManager.PlayerAuthData.SetAuthMethod(3);

            // RegistrationManager.Instance.SignUp_Name_Input.text = DefaultUserName;
            // RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = DefaultUserName;

            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            RegistrationManager.Instance.BufferingScreen.SetActive(false);

            // RegistrationManager.Instance.SwitchBetweenLoginScreens();
            RegistrationManager.Instance.OnLoginContinueClicked(false);


            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
            Debug.LogException(ex);
        }
    }

    public async Task LinkWithAppleAsync(string idToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithAppleAsync(idToken);

            await LoadPlayerData();

            userLoggedInState = UserLoggedInState.Apple;

            PlayerPrefManager.PlayerAuthData.SetAuthMethod(3);

            // RegistrationManager.Instance.SignUp_Name_Input.text = DefaultUserName;
            // RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = DefaultUserName;

            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            Debug.Log("Link is successful.");

            RegistrationManager.Instance.BufferingScreen.SetActive(false);

            // RegistrationManager.Instance.SwitchBetweenLoginScreens();
            RegistrationManager.Instance.OnLoginContinueClicked(false);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
    }

    public void AppleSignOut()
    {
        // #if UNITY_IOS
        m_AppleAuthManager.SetCredentialsRevokedCallback(async result =>
        {
            // Sign in with Apple Credentials were revoked.
            // Discard credentials/user id and go to login screen.

            // We Need This!!! Uncomment!
            // await AuthenticationService.Instance.UnlinkAppleAsync(idToken);

            AuthenticationService.Instance.ClearSessionToken();
            userLoggedInState = UserLoggedInState.None;

            PlayerPrefManager.PlayerAuthData.SetAuthMethod(-1);
            Loader.Load(Loader.Scene.RegistrationScreen);
        });
        // #endif
    }

    // Call This On Mobile Devices....
    public async Task AuthenticateWithGoogle()
    {
        try
        {
            Debug.Log("PlayerToken_Google " + PlayerToken_Google);

            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(PlayerToken_Google);

            await LoadPlayerData();

            userLoggedInState = UserLoggedInState.Google;

            PlayerPrefManager.PlayerAuthData.SetAuthMethod(2);

            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            // Check for null to prevent NullReferenceException
            if (RegistrationManager.Instance != null)
            {
                RegistrationManager.Instance.OnLoginContinueClicked(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Google authentication failed: {ex.Message}");
            if (RegistrationManager.Instance != null)
            {
                RegistrationManager.Instance.BufferingScreen.SetActive(false);
            }
            throw; // Re-throw the exception to be handled by the caller
        }
    }

    public async Task LinkWithGoogleAsync(string idToken)
    {
        try
        {
            await AuthenticationService.Instance.LinkWithGooglePlayGamesAsync(idToken);

            await LoadPlayerData();

            userLoggedInState = UserLoggedInState.Google;

            PlayerPrefManager.PlayerAuthData.SetAuthMethod(2);

            // RegistrationManager.Instance.SignUp_Name_Input.text = DefaultUserName;
            // RegistrationManager.Instance.SignUp_Name_Input.placeholder.GetComponent<RTLTextMeshPro>().text = DefaultUserName;

            Debug.Log("Signed in! " + AuthenticationService.Instance.PlayerId);

            Debug.Log("Link is successful.");

            // RegistrationManager.Instance.SwitchBetweenLoginScreens();

            RegistrationManager.Instance.OnLoginContinueClicked(false);
            //BufferingScreen.SetActive(false);
        }
        catch (AuthenticationException ex) when (ex.ErrorCode == AuthenticationErrorCodes.AccountAlreadyLinked)
        {
            // Prompt the player with an error message.
            Debug.LogError("This user is already linked with another account. Log in instead.");
            //BufferingScreen.SetActive(false);
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }

        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            //BufferingScreen.SetActive(false);
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
            //BufferingScreen.SetActive(false);
            RegistrationManager.Instance.BufferingScreen.SetActive(false);
        }
    }
    public void GoogleSignOut()
    {
#if UNITY_ANDROID
        // sign out
        //PlayGamesPlatform.Instance.SignOut();
        AuthenticationService.Instance.SignOut(true);
        AuthenticationService.Instance.ClearSessionToken();
        userLoggedInState = UserLoggedInState.None;

        PlayerPrefManager.PlayerAuthData.SetAuthMethod(-1);
        Loader.Load(Loader.Scene.RegistrationScreen);
#endif
    }
    public void GuestSignOut()
    {
        AuthenticationService.Instance.SignOut();
        // AuthenticationService.Instance.ClearSessionToken();
        userLoggedInState = UserLoggedInState.None;

        PlayerPrefManager.PlayerAuthData.SetAuthMethod(-1);
        Loader.Load(Loader.Scene.RegistrationScreen);
    }

    public async void DeletePlayaerAccount()
    {
        await AuthenticationService.Instance.DeleteAccountAsync();

        PlayerPrefManager.PlayerAuthData.SetAuthMethod(-1);

        StartCoroutine(AudioManager.Instance.PlayBackgroundMusic(AudioManager.BGMTypes.GameStart));

        PlayerDataManager.Instance.playerData = new PlayerData();
        await PlayerDataManager.Instance.SaveDataObject(PlayerDataManager.Instance.PLAYERDATA, PlayerDataManager.Instance.playerData);
        Debug.Log("Data got saved using cloud save!");
        
        userLoggedInState = UserLoggedInState.None;
        MainNetworkManager.Instance.CleanUp();

        Loader.Load(Loader.Scene.RegistrationScreen);
    }
}
