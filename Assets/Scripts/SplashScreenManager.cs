using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-1)]
public class SplashScreenManager : MonoBehaviour
{

    void Start() 
    {
        int GF = 0;
        StartLogoAnimation();

        Loader.Load(Loader.Scene.RegistrationScreen);
        // GlobalManager.Instance.LoadScene("RegistrationScreen");
    }
    public void StartLogoAnimation()
    {
        UIAnimator.Instance.AnimatLogo();
    }
}
