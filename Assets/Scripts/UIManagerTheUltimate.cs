using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerTheUltimate : MonoBehaviour
{
#region Instance
    private static UIManagerTheUltimate _instance;
    
    public static UIManagerTheUltimate Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<UIManagerTheUltimate>();
			return _instance;
		}
	}
#endregion

    [Header("Splash Screen Info")]
    public GameObject SplashScreen;
    public RectTransform SplashPanel;

    [Header("Main Info")]
    [SerializeField] RectTransform NextPanel;
    [SerializeField] RectTransform Previous;

    public float RightStartPosition;
    public float LeftStartPosition;
    public float CenterStartPosition;

    // [SerializeField] bool isAnimating;

    // public int PanelsCounter;

    // [Header("LogIn Info")]
    // public string Name;
    // public string Email;
    // public string PassWord;

    // [Header("Tutorial Screens Info")]
    // public GameObject TutorialScreen;

    // public RectTransform Tutorialpanel1;
    // public RectTransform Tutorialpanel2;
    // public RectTransform Tutorialpanel3;

    void Start() 
    {
        Canvas.ForceUpdateCanvases();

        CenterStartPosition = 0f;
        RightStartPosition = SplashPanel.rect.width;
        LeftStartPosition = -SplashPanel.rect.width;
    }
}
