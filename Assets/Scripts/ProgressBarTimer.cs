using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProgressBarTimer : MonoBehaviour
{
#region Instance...
    public static ProgressBarTimer Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<ProgressBarTimer>();

			return _instance;
		}
	}
    private static ProgressBarTimer _instance;
#endregion

    public delegate void TimerEnded();
    public static event TimerEnded OnTimerEnded;

    public Image progressBar;
    public float maxTime;
    private float currentTime;
    private float previousTime;

    public bool AutoPlayAnimation = true;

    bool CanPlay;

    // public GameObject SplashScreen;

    void OnEnable() 
    {
        currentTime = 0f;
        previousTime = 0f;
        
        //old...
        if (AutoPlayAnimation)
            CanPlay = true;

        
    }

    void Start() 
    {
        // SplashScreen = FindObjectOfType<LogoAnimation>().gameObject;
    }

    //old...
    void FixedUpdate() 
    {
        if(CanPlay)
        {
            if (currentTime >= maxTime)
            {
                // if(SceneManager.GetActiveScene().name == "Splash Screen")
                //     GlobalManager.Instance.LoadScene("Registration Screen");
                
                OnTimerEnded?.Invoke();

                UIManagerTheUltimate.Instance.SplashScreen.SetActive(false);
                // SplashScreen.SetActive(false);
                return;
            }

            currentTime += Time.deltaTime;
            
            if (currentTime != previousTime)
            {
                progressBar.fillAmount = currentTime / maxTime;
                previousTime = currentTime;
            }
        }
    }

    // public void StartProgressBar()
    // {
    //     CanPlay = true;
    // }
}
