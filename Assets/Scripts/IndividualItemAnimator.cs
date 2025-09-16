using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class IndividualItemAnimator : MonoBehaviour
{
    RectTransform item;
    Vector3 originalScale;

    void Awake() 
    {
        item = GetComponent<RectTransform>();

        originalScale = GetComponent<RectTransform>().localScale;
    }

    public void ScaleUp()
    {
        ScaleUp_SelectedItem(item, new Vector3(1.1f ,1.1f ,1.1f));
    }
    public void ScaleBack()
    {
        ResetScale_SelectedItem(item, originalScale);
    }

    public void ScaleUp_SelectedItem(RectTransform _rect, Vector3 _endValue, float _duration = 0.3f)
    {
        _rect.DOScale(_endValue, _duration).SetEase(Ease.InOutCubic);
    }

    public void ResetScale_SelectedItem(RectTransform _rect, Vector3 _endValue, float _duration = 0.3f)
    {
        _rect.DOScale(_endValue, _duration).SetEase(Ease.InOutCubic);
    }
}
