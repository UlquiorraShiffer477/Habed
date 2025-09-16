// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Firebase;
// using Firebase.Auth;
// using RTLTMPro;

// public class FireBaseManager : MonoBehaviour
// {
//     public static FireBaseManager Instance
// 	{
// 		get
// 		{
// 			if (!_instance)
// 				_instance = GameObject.FindObjectOfType<FireBaseManager>();

// 			return _instance;
// 		}
// 	}

//     private static FireBaseManager _instance;

//     [Header("Firebase")]
//     public FirebaseAuth auth;
//     public FirebaseUser user;

//     [Header("Login References")]
//     [SerializeField] RTLTextMeshPro loginOutputText;


//     [Header("Register References")]
//     [SerializeField] RTLTextMeshPro registerOutputText;


//     void Awake() 
//     {
//         DontDestroyOnLoad(gameObject);

//         FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(checkDependancyTask => 
//         {
//             var dependancyStatus = checkDependancyTask.Result;

//             if (dependancyStatus == DependencyStatus.Available)
//             {
//                 InitializeFirebase();
//             }
//             else
//             {
//                 Debug.Log($"Could not resolve all firebase dependancies: {dependancyStatus}");
//             }
//         });
//     }

//    void InitializeFirebase()
//    {
//         auth = FirebaseAuth.DefaultInstance;

//         auth.StateChanged += AuthStateChnaged;
//         AuthStateChnaged(this, null);
//    }

//     void AuthStateChnaged(object sender, System.EventArgs eventArgs)
//     {
//         if(auth.CurrentUser != user)
//         {
//             bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

//             if(!signedIn && user != null)
//             {
//                 Debug.Log("Signed Out");
//             }

//             user = auth.CurrentUser;

//             if (signedIn)
//             {
//                 Debug.Log($"Signed In: {user.DisplayName}");
//             }
//         }
//     }

//     public void ClearOutPut()
//     {
//         loginOutputText.text = "";
//         registerOutputText.text = "";
//     }

//     public void LoginButton(string _email, string _password)
//     {
//         StartCoroutine(LoginLogic(_email, _password));
//     }

//     public void RegisterButton(string _username, string _email, string _password)
//     {
//         StartCoroutine(RegisterLogic(_username, _email, _password));
//     }

//     IEnumerator LoginLogic(string _email, string _password)
//     {
//         Credential credential = EmailAuthProvider.GetCredential(_email, _password);

//         var loginTask = auth.SignInWithCredentialAsync(credential);

//         yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

//         if (loginTask.Exception != null)
//         {
//             FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
//             AuthError error = (AuthError)firebaseException.ErrorCode;
//             string output = "Unknown Error, Please Try Again";

//             switch(error)
//             {
//                 case AuthError.MissingEmail:
//                     output = "Please Enter Your Email";
//                     break;
//                 case AuthError.MissingPassword:
//                     output = "Please Enter Your Password";
//                     break;
//                 case AuthError.InvalidEmail:
//                     output = "Invalid Email";
//                     break;
//                 case AuthError.WrongPassword:
//                     output = "Incorrect Password";
//                     break;
//                 case AuthError.UserNotFound:
//                     output = "Account Does Not Exist";
//                     break;
//             }
//             loginOutputText.text = output;
//         }
//         else
//         {
//             if (user.IsEmailVerified)
//             {
//                 yield return new WaitForSeconds(1f);

//                 PlayerDataManager.Instance.SetPlayerID(user.UserId);
//                 PlayerDataManager.Instance.SetPlayerName(user.DisplayName);

//                 MainNetworkManager.Instance.Authenticate(PlayerDataManager.Instance.GetPlayerName());

//                 UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
//                 GlobalManager.Instance.LoadScene("Home Screen");
//             }
//             else
//             {
//                 // yield return new WaitForSeconds(1f);
//                 PlayerDataManager.Instance.SetPlayerID(user.UserId);
//                 PlayerDataManager.Instance.SetPlayerName(user.DisplayName);

//                 MainNetworkManager.Instance.Authenticate(PlayerDataManager.Instance.GetPlayerName());

//                 UIManagerTheUltimate.Instance.SplashScreen.SetActive(true);
//                 GlobalManager.Instance.LoadScene("Home Screen");
//             }
//         }
//     }

//     IEnumerator RegisterLogic(string _username, string _email, string _password)
//     {
//         if(_username == "")
//         {
//             registerOutputText.text = "Please Enter A Username";
//         }
//         else if (_username.ToLower() == "nigger")
//         {
//             registerOutputText.text = "That Username Is Inappropriate";
//         }
//         else
//         {
//             var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

//             yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

//             if(registerTask.Exception != null)
//             {
//                 FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
//                 AuthError error = (AuthError)firebaseException.ErrorCode;
//                 string output = "Unknown Error, Please Try Again";

//                 switch(error)
//                 {
//                     case AuthError.InvalidEmail:
//                         output = "Invalid Email";
//                         break;
//                     case AuthError.EmailAlreadyInUse:
//                         output = "Email Already In Use";
//                         break;
//                     case AuthError.WeakPassword:
//                         output = "Weak Password";
//                         break;
//                     case AuthError.MissingEmail:
//                         output = "Please Enter Your Email";
//                         break;
//                     case AuthError.MissingPassword:
//                         output = "Please Enter Your Password";
//                         break;
//                 }
//                 registerOutputText.text = output;
//             }
//             else
//             {
//                 UserProfile profile = new UserProfile
//                 {
//                     DisplayName = _username,
                    
//                     //Give Profile Pic
//                 };

//                 var defaultUserTask = user.UpdateUserProfileAsync(profile);

//                 yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);

//                 if(defaultUserTask.Exception != null)
//                 {
//                     user.DeleteAsync();
//                     FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
//                     AuthError error = (AuthError)firebaseException.ErrorCode;
//                     string output = "Unknown Error, Please Try Again";

//                     switch(error)
//                     {
//                         case AuthError.Cancelled:
//                             output = "Update User Canceled";
//                             break;
//                         case AuthError.SessionExpired:
//                             output = "Session Expired";
//                             break;
//                     }
//                     registerOutputText.text = output;
//                 }
//                 else
//                 {
//                     Debug.Log($"Firebase User Created Successfully: {user.DisplayName} ({user.UserId})");



//                     //Send Verification Email
//                 }
//             }
//         }
//     }
// }
