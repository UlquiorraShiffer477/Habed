using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Application.targetFrameRate = 60;
        
        #if UNITY_IPHONE
        PlayerSettings.iOS.hideHomeButton = true;
        #endif
        
        QualitySettings.vSyncCount = 0;

        MediationAdvertismentsBase.Instance.InitAdMob();
    }
}
