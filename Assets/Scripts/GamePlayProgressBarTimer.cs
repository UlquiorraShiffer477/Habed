using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using UnityEngine.EventSystems;
using System;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.PlayerLoop;

public class GamePlayProgressBarTimer : NetworkBehaviour
{
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


    // public delegate void TimerEnded();
    public static event EventHandler OnTimerEnded;
    // public event EventHandler OnPlayersInSessionListChanged;

    public Image progressBar;
    public float maxTime;
    public NetworkVariable<float> currentTime = new NetworkVariable<float> (0f);
    private float previousTime;

    public bool DoesHaveWarrningSFX = false;
    public bool IsWarrningAudioPlayed = false;

    bool CanPlay;

    public override void OnNetworkSpawn()
    {
        currentTime.OnValueChanged += CurrentTime_OnValueChanged;
        
        if (NetworkManager.Singleton.IsServer)
            GamePlayManager.Instance.Test_NetworkVariable.Value = 0f;

        previousTime = 0f;
    }

    private void CurrentTime_OnValueChanged(float previousValue, float newValue)
    {
        
    }

    void Start() 
    {
        Debug.Log("Start GamePlayProgressBarTimer IsServer: " + IsServer);

        if (NetworkManager.Singleton.IsServer)
            GamePlayManager.Instance.Test_NetworkVariable.Value = 0f;

        previousTime = 0f;
    }

    void OnEnable() 
    {
        Debug.Log("OnEnable GamePlayProgressBarTimer IsServer: " + NetworkManager.Singleton.IsServer);

        if (NetworkManager.Singleton.IsServer)
            GamePlayManager.Instance.Test_NetworkVariable.Value = 0f;

        previousTime = 0f;
        
        IsWarrningAudioPlayed = false;
        CanPlay = true;
    }

    void FixedUpdate() 
    {
        // Debug.Log("Update GamePlayProgressBarTimer IsServer: " + NetworkManager.Singleton.IsServer);

        // Debug.Log("Is Server GamePlayProgressBarTimer");

        if(CanPlay)
        {
            if (GamePlayManager.Instance.Test_NetworkVariable.Value >= maxTime)
            {
                // if(SceneManager.GetActiveScene().name == "Splash Screen")
                //     GlobalManager.Instance.LoadScene("Registration Screen");
                CanPlay = false;
                Debug.Log("OnTimerEnded Invoked!");
                OnTimerEnded?.Invoke(this, EventArgs.Empty);

                // UIManagerTheUltimate.Instance.SplashScreen.SetActive(false);
                return;
            }

            if (NetworkManager.Singleton)
                if (NetworkManager.Singleton.IsServer && GamePlayManager.Instance.Test_NetworkVariable != null)
                    GamePlayManager.Instance.Test_NetworkVariable.Value += Time.deltaTime;
            
            if (GamePlayManager.Instance.Test_NetworkVariable.Value != previousTime)
            {
                progressBar.fillAmount = GamePlayManager.Instance.Test_NetworkVariable.Value / maxTime;
                previousTime = GamePlayManager.Instance.Test_NetworkVariable.Value;

                if (previousTime > maxTime * .75f)
                {
                    // Debug.Log("Fill Amount = 8");

                    if (DoesHaveWarrningSFX)
                        if (!IsWarrningAudioPlayed)
                        {
                            AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.NarratorOnLowTimeAudioClips);
                            IsWarrningAudioPlayed = true;
                        } 
                }  
            }
        }
    }

    private void OnDisable() 
    {
        if (NetworkManager.Singleton)
            if (NetworkManager.Singleton.IsServer)
                GamePlayManager.Instance.Test_NetworkVariable.Value = 0f;
    }

    public void StartProgressBar()
    {
        CanPlay = true;
    }
}
