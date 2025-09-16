using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LeTai.TrueShadow;

public class UIAnimator : MonoBehaviour
{
    [Header("Logo Info")]
    public Image Logo1;
    public Image Logo2;
    public float Logo1_Duration;
    public float Logo2_Duration;

    [Header("Main Info")]
    [SerializeField] RectTransform NextPanel;
    [SerializeField] RectTransform Previous;

    [SerializeField] bool isAnimating;

    [Header("Tutorial Screens Info")]
    public GameObject TutorialScreen;

    public RectTransform Tutorialpanel1;
    public RectTransform Tutorialpanel2;
    public RectTransform Tutorialpanel3;


    [Header("Main")]
    // public RectTransform SplashPanel;
    public float StartxPosToTheRight;
    public float StartxPosToTheLeft;
    public float StartxPosInTheCenter;
    public Canvas MainCanvas;
    public RectTransform CenterPanel;

    public bool IsAvatarSelectedAtStart;

    [Header("Tutorial Screens")]
    public int TutorialCounter;
    public GameObject Dots1;
    public GameObject Dots2;
    public GameObject Dots3;

    // public GameObject TutorialScreen;

    // public RectTransform Tutorialpanel1;
    // public RectTransform Tutorialpanel2;
    // public RectTransform Tutorialpanel3;

    [Header("SignIn Screens Info")]
    public GameObject SignInScreen;
    public RectTransform SignInpanel1;
    public RectTransform SignInpanel2;
    public RectTransform Strock;

    [Header("Choose Avatar Screen Info")]
    public GameObject ChooseAvatarScreen;

    public RectTransform Male;
    public RectTransform Female;

    public bool IsMale;

    public bool IsAvatarChoosen;

    [Header("Avatar Customisation Male")]
    public GameObject AvatarCustomisationMaleScreen;

    [Header("Avatar Customisation Female")]
    public GameObject AvatarCustomisationFemaleScreen;

    [Header("Lobby Screen")]
    public GameObject LobbyScreen;
    public RectTransform LobbyPanel;
    public RectTransform DailyRewardsPanel;

    public TrueShadow SettingsTab;
    public TrueShadow DancesTab;
    public TrueShadow MyStuffTab;
    public TrueShadow StoreTab;

    public RectTransform SettingsPanel;
    public RectTransform DancesPanel;
    public RectTransform MyStuffPanel;
    public RectTransform StorePanel;

    [Header("Add Answer Info")]
    public GameObject AddAnswerScreen;

    [Header("Choose Answer Info")]
    public GameObject ChooseAnswerrScreen;

    [Header("Answers Info")]
    public GameObject AnswersScreen;
    [Header("Leadrer Board info")]
    public GameObject LeadrerBoard;




    [Header("Others")]
    public bool animatePanel1;
    public bool animatePanel2;

    private static UIAnimator _instance;
    
    public static UIAnimator Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<UIAnimator>();
			return _instance;
		}
	}

    void Awake() 
    {

    }

    void Start() 
    {
        Canvas.ForceUpdateCanvases();
        
        // StartxPosToTheRight = SplashPanel.rect.width;
        // StartxPosToTheLeft = -SplashPanel.rect.width;
        StartxPosInTheCenter = 0;
        
        // AnimatLogo();
        // StartCoroutine(StartFirstPanel());
    }

    public void OnNextTutorial()
    {
        // PanelSliding();
    }

    public void PanelSliding(RectTransform _panel, float _endPosition, float _duration)
    {
        isAnimating = true;
        _panel.DOAnchorPosX(_endPosition, _duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            
        });
    }

    public IEnumerator StartFirstPanel()
    {
        yield return new WaitForSeconds(3);

        TutorialScreen.SetActive(true);
        //SplashPanel.gameObject.SetActive(false);
    }

    public void AnimatePanels(RectTransform _panel1, RectTransform _panel2, float xPos1 = 0, float xPos2 = 0)
    {
        // Animate panel1 to the right edge of panel2
        // float xPos = _panel2.anchoredPosition.x + _panel2.rect.width;
        _panel1.DOAnchorPosX(xPos1, 0.5f).SetEase(Ease.InOutQuad);

        // Animate panel2 to the center of the screen
        _panel2.DOAnchorPosX(xPos2, 0.5f).SetEase(Ease.InOutQuad);
    }

    public void AnimateSinglePanel(RectTransform _panel, float xPos = 0)
    {
        // Animate panel2 to the center of the screen
        _panel.DOAnchorPosX(xPos, 0.5f).SetEase(Ease.InOutQuad);
    }

    public void OnTutorialNextClick()
    {
        if(TutorialCounter == 3)
            return;

        if(TutorialCounter == 0)
        {
            AnimatePanels(Tutorialpanel1,Tutorialpanel2 , StartxPosToTheLeft , StartxPosInTheCenter);
        }

        if(TutorialCounter == 1)
        {
            AnimatePanels(Tutorialpanel2,Tutorialpanel3 , StartxPosToTheLeft , StartxPosInTheCenter);
        }

        if(TutorialCounter == 2)
        {
            SignInScreen.SetActive(true);
            TutorialScreen.SetActive(false);
        }

        TutorialCounter++;
    }

    public void OnSignUpTextClick()
    {
        AnimatePanels(SignInpanel2,SignInpanel1,StartxPosInTheCenter,StartxPosToTheLeft);
        Strock.DOAnchorPosX(85,.5f).SetEase(Ease.InOutCubic);
    }

    public void OnSignInTextClick()
    {
        AnimatePanels(SignInpanel1,SignInpanel2,StartxPosInTheCenter,StartxPosToTheRight);
        Strock.DOAnchorPosX(-85,.5f).SetEase(Ease.InOutCubic);
    }

    public void OnSignInClick()
    {
        if(!IsAvatarSelectedAtStart)
        {
            ChooseAvatarScreen.SetActive(true);
            SignInScreen.SetActive(false);
        }
        // else
        // {
        //     LobbyScreen.SetActive(true);
        //     SignInScreen.SetActive(false);
        // }
    }

    public void OnAvatarChoseMale()
    {
        IsAvatarChoosen = true;
        Male.GetComponent<Image>().DOFade(1 , .3f);
        Female.GetComponent<Image>().DOFade(.5f , .3f);
        Male.DOScale(Vector3.zero , 0.4f).OnComplete(() =>
        {
            
        });
        Male.DOPunchAnchorPos(new Vector2(1f,1f), .7f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            //Optional OnComplete Code...
        });
    }

    public void OnAvatarChoseFemale()
    {
        IsAvatarChoosen = true;
        Female.GetComponent<Image>().DOFade(1 , .3f);
        Male.GetComponent<Image>().DOFade(.5f , .3f);
        Female.DOPunchAnchorPos(new Vector2(1f,1f), .7f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            //Optional OnComplete Code...
        });
    }

    public void OnNextAvatarChooseClick()
    {
        if(IsAvatarChoosen)
        {
            if(IsMale)
            {
                AvatarCustomisationMaleScreen.SetActive(true);
                ChooseAvatarScreen.SetActive(false);
            }
            else
            {
                AvatarCustomisationFemaleScreen.SetActive(true);
                ChooseAvatarScreen.SetActive(false);
            }
        }
        else
            return;
    }

    public void OnFinishCustomizationClick()
    {
        AvatarCustomisationMaleScreen.SetActive(false);
        AvatarCustomisationFemaleScreen.SetActive(false);

        LobbyScreen.SetActive(true);
        CenterPanel = LobbyPanel;
        StartCoroutine(ScaleDailyRewardsPanelUp());
    }

    public IEnumerator ScaleDailyRewardsPanelUp()
    {
        yield return new WaitForSeconds(0.5f);
        DailyRewardsPanel.DOScale(Vector2.one, 0.3f).SetEase(Ease.InOutCubic);
    }

    public void OnDailyRewardsClick()
    {
        DailyRewardsPanel.DOScale(Vector2.zero, 0.3f).SetEase(Ease.InOutCubic);
    }      

    public void OnSettingsClick()
    {
        SettingsTab.enabled = true;
        DancesTab.enabled = false;
        MyStuffTab.enabled = false;
        StoreTab.enabled = false;

        SettingsPanel.gameObject.SetActive(true);
        DancesPanel.gameObject.SetActive(false);
        MyStuffPanel.gameObject.SetActive(false);
        StorePanel.gameObject.SetActive(false);
        LobbyPanel.gameObject.SetActive(false);

        CenterPanel = SettingsPanel;
    }
    public void OnDancesClick()
    {
        SettingsTab.enabled = false;
        DancesTab.enabled = true;
        MyStuffTab.enabled = false;
        StoreTab.enabled = false;

        SettingsPanel.gameObject.SetActive(false);
        DancesPanel.gameObject.SetActive(true);
        MyStuffPanel.gameObject.SetActive(false);
        StorePanel.gameObject.SetActive(false);
        LobbyPanel.gameObject.SetActive(false);

        CenterPanel = DancesPanel;
    }
    public void OnMyStuffClick()
    {
        SettingsTab.enabled = false;
        DancesTab.enabled = false;
        MyStuffTab.enabled = true;
        StoreTab.enabled = false;

        SettingsPanel.gameObject.SetActive(false);
        DancesPanel.gameObject.SetActive(false);
        MyStuffPanel.gameObject.SetActive(true);
        StorePanel.gameObject.SetActive(false);
        LobbyPanel.gameObject.SetActive(false);

        CenterPanel = MyStuffPanel;
    }
    public void OnMyStoreClick()
    {
        SettingsTab.enabled = false;
        DancesTab.enabled = false;
        MyStuffTab.enabled = false;
        StoreTab.enabled = true;

        SettingsPanel.gameObject.SetActive(false);
        DancesPanel.gameObject.SetActive(false);
        MyStuffPanel.gameObject.SetActive(false);
        StorePanel.gameObject.SetActive(true);
        LobbyPanel.gameObject.SetActive(false);

        CenterPanel = StorePanel;
    }

    public void OnTabsBackClick()
    {
        SettingsTab.enabled = false;
        DancesTab.enabled = false;
        MyStuffTab.enabled = false;
        StoreTab.enabled = false;

        SettingsPanel.gameObject.SetActive(false);
        DancesPanel.gameObject.SetActive(false);
        MyStuffPanel.gameObject.SetActive(false);
        StorePanel.gameObject.SetActive(false);
        LobbyPanel.gameObject.SetActive(true);
    }

    public void OnPlayClick()
    {
        LobbyScreen.SetActive(false);
        AddAnswerScreen.SetActive(true);
        StartCoroutine(OpenChooseAmswerScreen());
    }

    public IEnumerator OpenChooseAmswerScreen()
    {
        yield return new WaitForSeconds(5);
        AddAnswerScreen.SetActive(false);
        ChooseAnswerrScreen.SetActive(true);
    }

    public void OnChooseAnswerClicked()
    {
        ChooseAnswerrScreen.SetActive(false);
        AnswersScreen.SetActive(true);
        StartCoroutine(OpenLeadrerBoard());
    }

    public IEnumerator OpenLeadrerBoard()
    {
        yield return new WaitForSeconds(3);
        AnswersScreen.SetActive(false);
        LeadrerBoard.SetActive(true);
    }

    public void ReturnToMainMenuFromResults()
    {
        LeadrerBoard.SetActive(false);
        LobbyScreen.SetActive(true);
    }
       
    public void AnimatLogo()
    {
        // logo.DOFade(0.5f, duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        // logo.rectTransform.DOScale(new Vector3(1.2f, 1.2f, 1), duration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);

        Logo1.DOFade(1, Logo1_Duration).SetLoops(2, LoopType.Yoyo);

        // Scale up and down
        Logo1.transform.DOScale(1.2f, Logo1_Duration).SetLoops(2, LoopType.Yoyo);

        // Rotate continuously
        // Logo2.transform.DORotate(new Vector3(0, 0, -70), Logo2_Duration).SetEase(Ease.InOutCubic).OnComplete(() =>
        // {
        //     Logo2.transform.DORotate(new Vector3(0, 0, 70), Logo2_Duration).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo);
        // });
    }

    //-----------------------------------------General Animating Functions-----------------------------------------//

    public void ScaleUp_SelectedItem(RectTransform _rect, Vector3 _endValue, float _duration = 0.3f)
    {
        _rect.DOScale(_endValue, _duration).SetEase(Ease.InOutCubic);
    }

    public void ResetScale_SelectedItem(RectTransform _rect, Vector3 _endValue, float _duration = 0.3f)
    {
        _rect.DOScale(_endValue, _duration).SetEase(Ease.InOutCubic);
    }

    public void Move_X_Animation(RectTransform _rect, float _endValue, float _duration)
    {
        _rect.DOAnchorPosX(_endValue, _duration).SetEase(Ease.OutCubic);
    }

    public void Move_AnchorPos(RectTransform _rect, Vector2 _endValue, float _duration)
    {
        _rect.DOAnchorPos(_endValue, _duration).SetEase(Ease.OutCubic);
    }
}
