using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using DG.Tweening;

public class GamePlayManagerUI : MonoBehaviour
{
    #region Instance
    // ----------------------Instance Section---------------------- //
    private static GamePlayManagerUI _instance;

    public static GamePlayManagerUI Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<GamePlayManagerUI>();

			return _instance;
		}
	}
    // ----------------------Instance Section---------------------- //
    #endregion

    public GameObject LoadingBar;
    public RectTransform LoadingBarHolder;
    public RectTransform AnswerInputField;

    public void ShowQuestionCounterAnimation()
    {
        AnswerInputField.DOAnchorPosY(207, 0.5f).SetEase(Ease.InOutCubic);
        LoadingBarHolder.DOAnchorPosY(138.5f, 0.5f).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            LoadingBar.SetActive(true);
        });
    }
    
    public void HideQuestionCounterAnimation()
    {
        AnswerInputField.DOAnchorPosY(-45, 0);
        LoadingBarHolder.DOAnchorPosY(-145, 0);
        LoadingBar.SetActive(false);
    }
}
