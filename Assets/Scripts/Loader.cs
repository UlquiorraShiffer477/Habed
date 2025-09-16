using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public static class Loader 
{
    public enum Scene
    {
        SplashScreen,
        GamePlayScreen,
        HomeScreen,
        LobbyScene,
        RegistrationScreen
    }

    public static Scene CurrentScene;

    public static void Load(Scene _targetScene)
    {
        CurrentScene = _targetScene;
        SceneManager.LoadSceneAsync(_targetScene.ToString());
    }

    public static void LoadNetwork(Scene _targetScene)
    {
        CurrentScene = _targetScene;
        NetworkManager.Singleton.SceneManager.LoadScene(_targetScene.ToString(), LoadSceneMode.Single);
    }
}