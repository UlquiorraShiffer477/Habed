using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LogoAnimation : MonoBehaviour
{
    [SerializeField] Image Logo_Text;
    [SerializeField] Image logo_Image;
    [SerializeField] float duration_Text;
    [SerializeField] float duration_Image;

    void OnEnable() 
    {
        GlobalManager.Instance.StartLogoAnimation();
    }

    public void AnimatLogo()
    {
        Logo_Text.DOFade(1, duration_Text).SetLoops(2, LoopType.Yoyo);

        // Scale up and down
        Logo_Text.transform.DOScale(1.2f, duration_Text).SetLoops(2, LoopType.Yoyo);

        // Rotate continuously
        logo_Image.transform.DORotate(new Vector3(0, 0, -80), duration_Image).SetLoops(-1, LoopType.Yoyo);
    }
}
