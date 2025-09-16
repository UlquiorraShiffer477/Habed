using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using LeTai.TrueShadow;
using RTLTMPro;

using Spine.Unity.Examples;
public class ImageSwipe : MonoBehaviour
{
    public float speed = 1;
    public Color[] colors;
    public GameObject scrollbar, imageContent;
    public RectTransform LoginBlueStrock;
    public float[] EndValues;

    public RectTransform LoginBlueStrock_Customization;
    public RectTransform[] EndValues_Customization;
    
    // public RTLTextMeshPro ContinueButtonText;
    // public string[] Texts;
    private float scroll_pos = 0;
    float[] pos;
    private bool runIt = false;
    private float time;
    private Button takeTheBtn;
    int btnNumber;

    public bool StartAtCenter;
    public bool CanSwipe = true;

    public bool IsLoginScreen;
    public List<GameObject> LoginButtons;

    public List<LayoutElement> ContentPages;

    public Button StoreButton;
    public Button HomeButton;

    float distance;
    
    // Start is called before the first frame update

    void OnEnable() 
    {
        // if(HomeMenuManager.Instance.StartAtCenter)
        //     scroll_pos = 0.25f;
    }

    private void Awake() {
        pos = new float[transform.childCount];
    }

    void Start()
    {
        if(HomeMenuManager.Instance.StartAtCenter)
            scroll_pos = 0.25f;

        if(ContentPages.Count != 0)
        {
            foreach(LayoutElement LE in ContentPages)
            {
                LE.preferredHeight = UIManagerTheUltimate.Instance.SplashPanel.rect.height;
                LE.preferredWidth = UIManagerTheUltimate.Instance.SplashPanel.rect.width;
            }
        }
        else
            Debug.Log("The list of pages is empty... Please fill it!");
    }
    // Update is called once per frame
    void Update()
    {
        if (runIt)
        {
            pos = new float[transform.childCount];
            distance = 1f / (pos.Length - 1f);
            GecisiDuzenle(distance, pos, takeTheBtn);
            time += Time.deltaTime;
            if (time > 1f)
            {
                time = 0;
                runIt = false;
            }
        }
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }

        if (CanSwipe) //Edited Option.
        {
            if (Input.GetMouseButton(0))
            {
                scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
            }
            else
            {
                for (int i = 0; i < pos.Length; i++)
                {
                    if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                    {
                        scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                    }
                }
            }
        }
        
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                //transform.GetChild(i).localScale = Vector2.Lerp(transform.GetChild(i).localScale, new Vector2(1f, 1f), 0.1f);
                //imageContent.transform.GetChild(i).localScale = Vector2.Lerp(imageContent.transform.GetChild(i).localScale, new Vector2(1.2f, 1.2f), 0.1f);

                if(imageContent.transform.GetChild(i).GetComponent<TrueShadow>())
                    imageContent.transform.GetChild(i).GetComponent<TrueShadow>().enabled = true;

                if(IsLoginScreen)
                {
                    foreach (GameObject go in LoginButtons)
                    {
                        go.SetActive(false);
                    }
                    LoginButtons[i].SetActive(true);
                }

                if(LoginBlueStrock != null)
                    UIAnimator.Instance.Move_X_Animation(LoginBlueStrock, EndValues[i], .5f);

                if(LoginBlueStrock_Customization != null)
                    UIAnimator.Instance.Move_X_Animation(LoginBlueStrock_Customization, EndValues_Customization[i].localPosition.x, .5f);

                if(imageContent.transform.GetChild(i).GetComponent<Image>())
                    imageContent.transform.GetChild(i).GetComponent<Image>().color = colors[1];
                
                for (int j = 0; j < pos.Length; j++)
                {
                    if (j != i)
                    {
                        if(imageContent.transform.GetChild(j).GetComponent<TrueShadow>())
                            imageContent.transform.GetChild(j).GetComponent<TrueShadow>().enabled = false;

                        // if(ContinueButtonText != null)
                        //     ContinueButtonText.text = Texts[j];

                        if(IsLoginScreen)
                        {
                            foreach (GameObject go in LoginButtons)
                            {
                                go.SetActive(false);
                            }
                            LoginButtons[i].SetActive(true);
                        }

                        // if(LoginBlueStrock != null)
                        //     UIAnimator.Instance.Move_X_Animation(LoginBlueStrock, EndValues[j], 0f);

                        // if(LoginBlueStrock_Customization != null)
                        //     UIAnimator.Instance.Move_X_Animation(LoginBlueStrock_Customization, EndValues_Customization[j], 0f);

                        if(imageContent.transform.GetChild(j).GetComponent<Image>())
                            imageContent.transform.GetChild(j).GetComponent<Image>().color = colors[0];
                        
                        //imageContent.transform.GetChild(j).localScale = Vector2.Lerp(imageContent.transform.GetChild(j).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                        //transform.GetChild(j).localScale = Vector2.Lerp(transform.GetChild(j).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                    }
                }
            }
        }
    }
    private void GecisiDuzenle(float distance, float[] pos, Button btn)
    {
        // btnSayi = System.Int32.Parse(btn.transform.name);
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[btnNumber], 1f * speed * Time.deltaTime);
                
            }
        }
        for (int i = 0; i < btn.transform.parent.transform.childCount; i++)
        {
            btn.transform.name = ".";
        }
    }
    public void WhichBtnClicked(Button btn)
    {
        //Play Button Audios..
        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
        //------------

        btn.transform.name = "clicked";
        for (int i = 0; i < btn.transform.parent.transform.childCount; i++)
        {
            if (btn.transform.parent.transform.GetChild(i).transform.name == "clicked")
            {
                btnNumber = i;
                Debug.Log("btnNumber" + i);
                takeTheBtn = btn;
                time = 0;
                scroll_pos = (pos[btnNumber]);
                runIt = true;
            }
        }

        // HomeMenuManager.Instance.GoToCharacterCustomizationScreen();
    }



    public void GoToStore()
    {
        takeTheBtn = StoreButton;
        time = 0;
        scroll_pos = (pos[0]);
        runIt = true;
    }
    public void GoToHomeScreen()
    {
        takeTheBtn = HomeButton;
        time = 0;
        scroll_pos = (pos[1]);
        runIt = true;
    }
}
    


