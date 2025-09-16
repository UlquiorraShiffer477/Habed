using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<GlobalManager>();

			return _instance;
		}
	}
	
	private static GlobalManager _instance;


	[Header("Loading Screen")]
    public GameObject BufferingScreen;

	[Header("Main Info")]
    [SerializeField] PlayerDataManager playerDataManager; 

	public bool isLobby;

	public bool IsMale;

	public bool CanAnimateCharacter = true;



	public int LastGameRank;
	public bool CanAddCoins;

	public bool IsDoubleRewards;


	public bool IsNewPlayer = true;

	public bool CanPlayMusic;
	public bool CanPlaySFX;

	public bool IsChangeUserNameSection;



	// public async void LoadSceneAsync(string sceneName)
	// {
	// 	// bar.BarReset();
	// 	// FadedBG.Instance.FadeIn(0.4f);
	// 	// //bar.GetComponent<Animator>().SetTrigger("StartLoadingP1");

	// 	// bar.gameObject.SetActive(true);
	// 	// bar.current = 0;

	// 	// var scene = SceneManager.LoadSceneAsync(sceneName);
	// 	// scene.allowSceneActivation = false;

	// 	// await Task.Delay(1000);

	// 	// do
	// 	// {
	// 	// 	await Task.Delay(200);

	// 	// 	bar.current = (int)(scene.progress * 100);
	// 	// 	bar.UpdateText();

	// 	// 	if(scene.progress > 0.85)
	// 	// 	{
	// 	// 		bar.UpdateTo100();
	// 	// 	}
	// 	// 	else
	// 	// 		if(scene.progress > 0.5)
	// 	// 		{
    //     //             bar.UpdateTo85();
	// 	// 			//bar.GetComponent<Animator>().SetTrigger("StartLoadingP2");
	// 	// 		}
	// 	// }
	// 	// while(scene.progress < 0.9f);

	// 	// await Task.Delay(2000);
	// 	// FadedBG.Instance.FadeOut(0.4f);

	// 	// scene.allowSceneActivation = true;
	// }

    void Awake()
    {
        DontDestroyOnLoad(this);


		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		// PlayerPrefs.DeleteAll();
    }

	private void Start() 
	{
		// Application.runInBackground = true;
	}

	// ----------------------General Functions---------------------- //

	

    public void StartLogoAnimation()
    {
        UIAnimator.Instance.AnimatLogo();
    }

	public void LoadScene(string sceneName)
	{
		SceneManager.LoadSceneAsync(sceneName);
	}
}
