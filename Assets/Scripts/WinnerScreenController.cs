using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTLTMPro;
using TMPro;
using UnityEngine.UI;

using DG.Tweening;

using Unity.Netcode;

public class WinnerScreenController : MonoBehaviour
{
    [Header("Screen Elements")]
    public GameObject AvatarPanel;
    public GameObject BackgroundImage;
    public GameObject KingOfHabedText;

    public RTLTextMeshPro WinnerName;

    public ParticleSystem LeftSpotLights;
    public ParticleSystem RightSpotLights;
    public ParticleSystem Confiti;

    public TextMeshProUGUI SkipAnnouncer_Text;
    public Button SkipToFinalResult_Button;

    void Start() 
    {
        GamePlayManager.Instance.BackToHomeButon.gameObject.SetActive(false);

        WinnerName.text = RoomSessionManager.Instance.Results[0].fullPlayerAnswer.PlayerName;

        AvatarPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(UIManagerTheUltimate.Instance.RightStartPosition + 100, 0);
        KingOfHabedText.GetComponent<RectTransform>().anchoredPosition = new Vector2(UIManagerTheUltimate.Instance.LeftStartPosition, KingOfHabedText.GetComponent<RectTransform>().anchoredPosition.y);

        float r = BackgroundImage.GetComponent<Image>().color.r;
        float g = BackgroundImage.GetComponent<Image>().color.g;
        float b = BackgroundImage.GetComponent<Image>().color.b;

        BackgroundImage.GetComponent<Image>().color = new Color(r, g, b, 0);

        StartCoroutine(StartWinningAnimation(2, 1.4f, 2));
    }

    public IEnumerator StartWinningAnimation(float _backgroundFadeDuration, float _avatarSlideInDuration, float _textSlideInDuration)
    {
        AudioManager.Instance.PlayAudioClip(AudioManager.Instance.FinalRoundLoop);

        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.OnShowingBoxSFX);
        BackgroundImage.GetComponent<Image>().DOFade(1, _backgroundFadeDuration);
        yield return new WaitForSeconds(_backgroundFadeDuration);

        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.OnShowingBoxSFX);
        AvatarPanel.transform.DOLocalMoveX(0, _avatarSlideInDuration).SetEase(Ease.InOutElastic, 0.5f);
        yield return new WaitForSeconds(_avatarSlideInDuration);

        AudioManager.Instance.PlayAudioClip(AudioManager.Instance.CheeringSFX);

        AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.OnShowingBoxSFX);
        KingOfHabedText.transform.DOLocalMoveX(0, _textSlideInDuration).SetEase(Ease.InOutElastic, 0.5f);
        yield return new WaitForSeconds(_textSlideInDuration);

        Confiti.Play();

        yield return new WaitForSeconds(1);

        LeftSpotLights.Play();
        RightSpotLights.Play();

        yield return new WaitForSeconds(2);
        SkipAnnouncer_Text.DOFade(1,0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutCubic);
        SkipToFinalResult_Button.onClick.AddListener(SkipToFinalResult);
        
    }

    void SkipToFinalResult()
    {
        // GamePlayManager.Instance.state.Value = GamePlayManager.State.FinalResults;
        RoomSessionManager.Instance.AssignFinalResultsControllerList();
    }
}
