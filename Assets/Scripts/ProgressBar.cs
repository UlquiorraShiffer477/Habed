using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    //public Slider slider;
    
    public int maximum;
    public float current;
    public Image mask;

    [SerializeField] bool animating50, animating85, animating100;

    Tween tween;

    [SerializeField] TMP_Text loadingText;

    void Start()
    {
        //UpdateTo50();
    }

    // Update is called once per frame

    public void FirstUpdateText(string _text)
    {
        loadingText.text = _text;
    }
    public void UpdateText()
    {
        loadingText.text = "LOADING " + current + "%";
    }

    public void FirstProgressBarUpdate(float _amount)
	{
		// current += 1;
        // FirstetCurrentFill();
        mask.fillAmount = _amount;
	}

    public void ProgressBarUpdate()
	{
		current += 1;
        GetCurrentFill();
	}

    void FirstetCurrentFill()
    {
        float fillAmount = (float)current / (float)maximum;
        mask.fillAmount = fillAmount;

        loadingText.text = "Downloading...  " + (int)(fillAmount * 100) + "%";
    }

    void GetCurrentFill()
    {
        float fillAmount = (float)current / (float)maximum;
        mask.fillAmount = fillAmount;

        loadingText.text = "LOADING " + (int)(fillAmount * 100) + "%";
    }
    
    public void BarReset()
    {
        loadingText.text = "LOADING " + 0 + "%";
        // mask.fillAmount = 0;
        //this.GetComponent<Animator>().Play("Empty");
    }

    public void BarInit(int _current, int _max)
    {
        loadingText.text = "LOADING " + 0 + "%";

        current = _current;
        maximum = _max;        
        //this.GetComponent<Animator>().Play("Empty");
    }

    public void UpdateTo50()
    {
        if(animating50)
            return;

        animating50 = true;

        float value = 0;

        if(tween != null)
        {
            tween.Kill();
            tween = null;
        }

        tween = DOTween.To(() => value, x => value = x, 50, 1f)
        .OnUpdate(() => 
        {
            mask.fillAmount = value * 0.01f;
            loadingText.text = "LOADING " + value + "%";
        });
    }

    public void UpdateTo85()
    {
        if(animating85)
            return;

        animating85 = true;

        float value = 50;

        if(tween != null)
        {
            tween.Kill();
            tween = null;
        }

        tween = DOTween.To(() => value, x => value = x, 85, 1f)
        .OnUpdate(() => 
        {
            // mask.fillAmount = value * 0.01f;
            loadingText.text = "LOADING " + value + "%";
        });
    }

    public void UpdateTo100()
    {
        if(animating100)
            return;

        animating100 = true;

        float value = 85;

        if(tween != null)
        {
            tween.Kill();
            tween = null;
        }

        DOTween.To(() => value, x => value = x, 100, 1f)
        .OnUpdate(() => 
        {
            // mask.fillAmount = value * 0.01f;
            loadingText.text = "LOADING " + value + "%";
        });
    }
}
