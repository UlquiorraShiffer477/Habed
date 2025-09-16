using System.Collections;
using System.Collections.Generic;
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
        QualitySettings.vSyncCount = 0;

        MediationAdvertismentsBase.Instance.InitAdMob();
    }
}
