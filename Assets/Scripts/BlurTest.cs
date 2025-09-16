using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using QFSW.QC.UI;

using DG.Tweening;

[ExecuteInEditMode]
public class BlurTest : MonoBehaviour
{
    [SerializeField] private Material _blurMaterial = null;
    [SerializeField] GameObject PanelToBlur;
    [SerializeField] private float _blurRadius = 1f;
    [SerializeField] private Vector2 _referenceResolution = new Vector2(1920, 1080);

    [SerializeField] Button CloseButton;
    [SerializeField] Image Panel;

    void Awake() 
    {
        PanelToBlur.GetComponent<Image>().material = _blurMaterial;
    }

    void Start() 
    {
        _blurRadius = 1;

        // Panel.GetComponent<Button>().onClick.AddListener(() =>
        // {
        //     OnBlurEnds();
        // });
        CloseButton.onClick.AddListener(() =>
        {
            OnBlurEnds();
        });
    }

    public void OnBlurStart(float _blurSpeed = 1)
    {
        Panel.raycastTarget = true;

        Vector2 resolution = new Vector2(Screen.width, Screen.height);
        float correction = resolution.y / _referenceResolution.y;

        Panel.DOFade(0.3f, _blurSpeed).SetEase(Ease.InOutCubic).OnComplete(() =>
        {
            // Panel.GetComponent<Button>().onClick.RemoveAllListeners();
            // Panel.GetComponent<Button>().onClick.AddListener(() =>
            // {
            //     OnBlurEnds();
            // });
            CloseButton.gameObject.SetActive(true);
        });

        DOTween.To(() => _blurRadius, x => _blurRadius = x, 10, _blurSpeed).OnUpdate(() => 
        {
            if (_blurMaterial)
            {
                _blurMaterial.SetFloat("_Radius", _blurRadius);
                _blurMaterial.SetFloat("_BlurMultiplier", correction);     
            }
        });
    }

    public void OnBlurEnds(float _blurSpeed = 1)
    {
        Panel.raycastTarget = false;

        CloseButton.gameObject.SetActive(false);

        Vector2 resolution = new Vector2(Screen.width, Screen.height);
        float correction = resolution.y / _referenceResolution.y;

        //Animate the alpha of the image...
        Panel.DOFade(0f, _blurSpeed).SetEase(Ease.InOutCubic);

        //Animate the blurRaduis...
        DOTween.To(() => _blurRadius, x => _blurRadius = x, 1, _blurSpeed).OnUpdate(() => 
        {
            if (_blurMaterial)
            {
                _blurMaterial.SetFloat("_Radius", _blurRadius);
                _blurMaterial.SetFloat("_BlurMultiplier", correction);     
            }
        });
    }

    private void LateUpdate()
    {
        if (_blurMaterial)
        {
            Vector2 resolution = new Vector2(Screen.width, Screen.height);
            float correction = resolution.y / _referenceResolution.y;
            _blurMaterial.SetFloat("_Radius", _blurRadius);
            _blurMaterial.SetFloat("_BlurMultiplier", correction);
        }
    }
}
