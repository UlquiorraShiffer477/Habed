using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;

public class FadedBG : MonoBehaviour
{
	private static FadedBG _instance;
    
    public static FadedBG Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<FadedBG>();

			return _instance;
		}
	}

	// [SerializeField] Image fadedbackground;
	private readonly float fadeInEndValue = 1f;
	// [SerializeField] CanvasGroup TransitionPanel;
	// [SerializeField] RTLTextMeshPro assetName;
	public ProgressBar bar;


	public void UpdateTextInBar(string _text)
	{
		bar.FirstUpdateText(_text);
	}
	public void Init(int _max)
	{
		bar.BarReset();
		bar.BarInit(0, _max);
	}

	public void FirstProgressBarUpdate(float _amount)
	{
		bar.FirstProgressBarUpdate(_amount);
		
	}

	public void ProgressBarUpdate()
	{
		bar.ProgressBarUpdate();
	}

	public void FadeIn(float speed)
	{
		StopCoroutine(nameof(FadeProcess));
		StartCoroutine(FadeProcess(speed, true));
		// TransitionPanel.blocksRaycasts = true;
	}

	public void FadeOut(float speed)
	{
		StopCoroutine(nameof(FadeProcess));
		StartCoroutine(FadeProcess(speed, false));
		// TransitionPanel.blocksRaycasts = false;
	}

	// public void UpdateAssetNames(string _name)
	// {
	// 	assetName.text = _name;
	// }

	private IEnumerator FadeProcess(float speed, bool fadeIn)
	{
		// fadedbackground.DOFade((fadeIn) ? fadeInEndValue : 0f, speed).OnUpdate(()=>
		// {
		// 	// TransitionPanel.alpha = fadedbackground.color.a;
		// });
		
		yield return new WaitForSeconds(speed); // when done fading process
		// fadedbackground.raycastTarget = fadeIn;
	}
}