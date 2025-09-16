using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using DG.Tweening;
using RTLTMPro;
using TMPro;

using UnityEngine.Events;

using AssetKits.ParticleImage;

using Spine.Unity.Examples;
using System;

using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

[Serializable]
public class ConsumableItem
{
    public string Name;
    public string Id;
    public string Desc;
    public string Price;
}


public class ShopManager : MonoBehaviour, IDetailedStoreListener
{
    #region Instance
    // ----------------------Instance Section---------------------- //
    public static ShopManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<ShopManager>();

            return _instance;
        }
    }
    private static ShopManager _instance;
    // ----------------------Instance Section---------------------- //
    #endregion


    [Header("In-App Purchase Info")]
    public UnityEvent PlayCoinAddingAnimation_1000;
    public UnityEvent PlayCoinAddingAnimation_6000;
    public UnityEvent PlayCoinAddingAnimation_15000;
    public UnityEvent PlayCoinAddingAnimation_35000;

    public ConsumableItem Coins_1000;
    public ConsumableItem Coins_6000;
    public ConsumableItem Coins_15000;
    public ConsumableItem Coins_35000;

    IStoreController m_StoreController;

    public bool IsDoubleRewards = false;




    public ParticleImage InAppPurchaseParticleSystem;

    public Button ShowCoinsPackages_Button;


    [Header("IAP Buttons")]
    public List<Transform> Packages_Buttons;


    public Button BackGround_Button;

    public bool IsAnimating;

    [Header("Coin Animations Properties")]
    [SerializeField] ParticleImage particleSystem_CP;
    [SerializeField] RectTransform iconHolder;
    [SerializeField] Vector3 punchScaleVector = new Vector3(.2f, .2f, .2f);
    [SerializeField] int vibrato = 1;
    [SerializeField] int elasticity = 1;

    public int CurrentCoinsCount;
    public RTLTextMeshPro Coins_Text;

    public AudioClip PurchaseAudioClip;
    public AudioClip OnCoinCounterHit;

    [SerializeField] RTLTextMeshPro increaseDecreaseAnimation_Text;




    // Start is called before the first frame update
    void Start()
    {
        // Assigning Main Coin Particle Events
        PlayCoinAddingAnimation_1000.AddListener(PlayCoinAnimation_1000);
        PlayCoinAddingAnimation_6000.AddListener(PlayCoinAnimation_6000);
        PlayCoinAddingAnimation_15000.AddListener(PlayCoinAnimation_15000);
        PlayCoinAddingAnimation_35000.AddListener(PlayCoinAnimation_35000);


        SetupBuilder();


        Debug.Log("PlayerDataManager.Instance.playerData.PlayerCoins = " + PlayerDataManager.Instance.playerData.PlayerCoins);

        CurrentCoinsCount = PlayerDataManager.Instance.playerData.PlayerCoins;
        Coins_Text.text = CurrentCoinsCount.ToString();

        ShowCoinsPackages_Button.onClick.AddListener(() =>
        {
            ShowPackageOnClick();
        });
        // BackGround_Button.onClick.AddListener(() =>
        // {
        // HidePackageOnClick();
        // });
    }



    // ------------------------------------------------------------------------------------------- //
    public void PlayCoinAnimation_1000()
    {
        int currentAmountBeforeAddition;
        int totalAmount;

        totalAmount = CurrentCoinsCount;
        currentAmountBeforeAddition = CurrentCoinsCount - 1000;

        Debug.Log($"CurrentCoinsCount = {CurrentCoinsCount} And totalAmount = {totalAmount}");

        StartCoroutine(TweenValue(currentAmountBeforeAddition, totalAmount, 2, 0, Coins_Text));

        // UpdatePlayerDataCoins(totalAmount);
    }
    public void PlayCoinAnimation_6000()
    {
        int currentAmountBeforeAddition;
        int totalAmount;

        totalAmount = CurrentCoinsCount;
        currentAmountBeforeAddition = CurrentCoinsCount - 6000;

        Debug.Log($"CurrentCoinsCount = {CurrentCoinsCount} And totalAmount = {totalAmount}");

        StartCoroutine(TweenValue(currentAmountBeforeAddition, totalAmount, 2, 0, Coins_Text));

        // UpdatePlayerDataCoins(totalAmount);
    }
    public void PlayCoinAnimation_15000()
    {
        int currentAmountBeforeAddition;
        int totalAmount;

        totalAmount = CurrentCoinsCount;
        currentAmountBeforeAddition = CurrentCoinsCount - 15000;

        Debug.Log($"CurrentCoinsCount = {CurrentCoinsCount} And totalAmount = {totalAmount}");

        StartCoroutine(TweenValue(currentAmountBeforeAddition, totalAmount, 2, 0, Coins_Text));

        // UpdatePlayerDataCoins(totalAmount);
    }
    public void PlayCoinAnimation_35000()
    {
        int currentAmountBeforeAddition;
        int totalAmount;

        totalAmount = CurrentCoinsCount;
        currentAmountBeforeAddition = CurrentCoinsCount - 35000;

        Debug.Log($"CurrentCoinsCount = {CurrentCoinsCount} And totalAmount = {totalAmount}");

        StartCoroutine(TweenValue(currentAmountBeforeAddition, totalAmount, 2, 0, Coins_Text));

        // UpdatePlayerDataCoins(totalAmount);
    }
    // ------------------------------------------------------------------------------------------- //



    public async void UpdatePlayerDataCoins(int _finalValue)
    {
        PlayerDataManager.Instance.playerData.PlayerCoins = _finalValue;
        await PlayerDataManager.Instance.SaveDataObject(PlayerDataManager.Instance.PLAYERDATA, PlayerDataManager.Instance.playerData);
        Debug.Log("Data got saved using cloud save!");
        CurrentCoinsCount = _finalValue;
        HomeMenuManager.Instance.HomeMenuPlayerData.PlayerCoins = CurrentCoinsCount;
        Debug.Log("CurrentCoinsCount = " + CurrentCoinsCount);
    }
    public void UpdateHomeMenuPlayerDataCoins(int _finalValue)
    {
        HomeMenuManager.Instance.HomeMenuPlayerData.PlayerCoins = _finalValue;
        CurrentCoinsCount = _finalValue;
    }
    public IEnumerator TweenValue(int _startValue, int _endValue, float _duration, float startDelay = 2, RTLTextMeshPro _valueText = null)
    {
        bool isDeduction = (_endValue - _startValue) < 0;
        if (isDeduction)
            startDelay = 0;

        yield return new WaitForSeconds(startDelay);

        ActivateValueEffect(_endValue - _startValue, isDeduction);

        int tempValue = _startValue;
        DOTween.To(() => tempValue, x => tempValue = x, _endValue, _duration).OnUpdate(() =>
        {
            if (_valueText)
            {
                _valueText.text = tempValue.ToString();
            }

            else
                Debug.LogError("No Text Field Assigned!");
        });

        yield return new WaitForSeconds(.15f);

        int tempPunchesCount = _endValue - _startValue;
        if (tempPunchesCount < 5)
        {
            _duration = _duration / 2;
        }

        if (tempPunchesCount > 0)
            for (int i = 0; i < tempPunchesCount; i++)
            {
                iconHolder.DOPunchScale(punchScaleVector, _duration / (tempPunchesCount * 2), vibrato, elasticity).SetEase(Ease.InOutCubic);
                yield return new WaitForSeconds(_duration / (tempPunchesCount * 2));
            }

        yield return null;
    }

    public void ActivateValueEffect(int _value, bool _isDeduction = false)
    {
        if (_isDeduction)
        {
            increaseDecreaseAnimation_Text.text = "" + _value;
            increaseDecreaseAnimation_Text.color = Color.red;

            DecreaseAnimation();
        }
        else
        {
            increaseDecreaseAnimation_Text.text = "+" + _value;
            increaseDecreaseAnimation_Text.color = Color.green;

            IncreaseAnimation();
        }
    }
    public void IncreaseAnimation()
    {
        increaseDecreaseAnimation_Text.GetComponent<RectTransform>().localPosition = Vector3.zero;
        increaseDecreaseAnimation_Text.alpha = 1;

        increaseDecreaseAnimation_Text.DOFade(0, 2.5f).SetEase(Ease.InOutCubic);
        increaseDecreaseAnimation_Text.GetComponent<RectTransform>().DOAnchorPosY(40, 2f).SetEase(Ease.OutCubic);
    }
    public void DecreaseAnimation()
    {
        increaseDecreaseAnimation_Text.GetComponent<RectTransform>().localPosition = Vector3.zero;
        increaseDecreaseAnimation_Text.alpha = 1;

        increaseDecreaseAnimation_Text.DOFade(0, 2.5f).SetEase(Ease.InOutCubic);
        increaseDecreaseAnimation_Text.GetComponent<RectTransform>().DOAnchorPosY(-40, 2f).SetEase(Ease.OutCubic);
    }


    public void Play_CPCoins(bool _doubleReward = false)
    {
        IsDoubleRewards = _doubleReward;
        // particleSystem_CP.SetBurst(0, 0, _count);
        particleSystem_CP.Play();
    }
    public void Play_CPCoins_InAppPurchase()
    {
        // particleSystem_CP.SetBurst(0, 0, _count);
        particleSystem_CP.Play();
    }
    public void PlayOneShot()
    {
        AudioManager.Instance.PlayAudioClip(OnCoinCounterHit);
    }

    public void AddCoinsOnPlayerWin()
    {
        if (GlobalManager.Instance.LastGameRank > 3)
            return;

        GlobalManager.Instance.CanAddCoins = false;

        int _coinsAmount = 0;
        int _totalAmount = 0;

        switch (GlobalManager.Instance.LastGameRank)
        {
            case 1:
                {
                    _coinsAmount = IsDoubleRewards ? (HomeMenuManager.Instance.FirstPlaceReword * 2) : HomeMenuManager.Instance.FirstPlaceReword;
                    break;
                }

            case 2:
                {
                    _coinsAmount = IsDoubleRewards ? (HomeMenuManager.Instance.SecondPlaceReword * 2) : HomeMenuManager.Instance.SecondPlaceReword;
                    break;
                }

            case 3:
                {
                    _coinsAmount = IsDoubleRewards ? (HomeMenuManager.Instance.ThirdPlaceReword * 2) : HomeMenuManager.Instance.ThirdPlaceReword;
                    break;
                }

            default:
                {
                    _coinsAmount = IsDoubleRewards ? (HomeMenuManager.Instance.OtherPlaceReword * 2) : HomeMenuManager.Instance.OtherPlaceReword;
                    break;
                }
        }

        _totalAmount = PlayerDataManager.Instance.playerData.PlayerCoins + _coinsAmount;

        StartCoroutine(TweenValue(CurrentCoinsCount, _totalAmount, 2, 0, Coins_Text));

        UpdatePlayerDataCoins(_totalAmount);
        
        GlobalManager.Instance.IsDoubleRewards = false;
    }


    public void PlayCoinsPurchasedSFX()
    {
        AudioManager.Instance.PlayAudioClip(PurchaseAudioClip);
    }


    // ------------------------------------------------------------------------------------------- //
    public void Invoke_1000Coins()
    {
        PlayCoinAddingAnimation_1000.Invoke();
    }
    public void Invoke_6000Coins()
    {
        PlayCoinAddingAnimation_6000.Invoke();
    }
    public void Invoke_15000Coins()
    {
        PlayCoinAddingAnimation_15000.Invoke();
    }
    public void Invoke_35000Coins()
    {
        PlayCoinAddingAnimation_35000.Invoke();
    }
    // ------------------------------------------------------------------------------------------- //
    public void AddCoins_1000()
    {
        HidePackageOnClick();

        int _totalAmount = 0;

        // Reseting Listeners...
        InAppPurchaseParticleSystem.onFirstParticleFinish.RemoveAllListeners();
        // Assigning The Proper One...
        InAppPurchaseParticleSystem.onFirstParticleFinish.AddListener(Invoke_1000Coins);

        InAppPurchaseParticleSystem.Play();
        PlayCoinsPurchasedSFX();

        _totalAmount = PlayerDataManager.Instance.playerData.PlayerCoins + 1000;
        UpdatePlayerDataCoins(_totalAmount);
    }
    public void AddCoins_6000()
    {
        HidePackageOnClick();

        int _totalAmount = 0;

        // Reseting Listeners...
        InAppPurchaseParticleSystem.onFirstParticleFinish.RemoveAllListeners();
        // Assigning The Proper One...
        InAppPurchaseParticleSystem.onFirstParticleFinish.AddListener(Invoke_6000Coins);

        InAppPurchaseParticleSystem.Play();
        PlayCoinsPurchasedSFX();

        _totalAmount = PlayerDataManager.Instance.playerData.PlayerCoins + 6000;
        UpdatePlayerDataCoins(_totalAmount);
    }
    public void AddCoins_15000()
    {
        HidePackageOnClick();

        int _totalAmount = 0;

        // Reseting Listeners...
        InAppPurchaseParticleSystem.onFirstParticleFinish.RemoveAllListeners();
        // Assigning The Proper One...
        InAppPurchaseParticleSystem.onFirstParticleFinish.AddListener(Invoke_15000Coins);

        InAppPurchaseParticleSystem.Play();
        PlayCoinsPurchasedSFX();

        _totalAmount = PlayerDataManager.Instance.playerData.PlayerCoins + 15000;
        UpdatePlayerDataCoins(_totalAmount);
    }
    public void AddCoins_35000()
    {
        HidePackageOnClick();

        int _totalAmount = 0;

        // Reseting Listeners...
        InAppPurchaseParticleSystem.onFirstParticleFinish.RemoveAllListeners();
        // Assigning The Proper One...
        InAppPurchaseParticleSystem.onFirstParticleFinish.AddListener(Invoke_35000Coins);

        InAppPurchaseParticleSystem.Play();
        PlayCoinsPurchasedSFX();

        _totalAmount = PlayerDataManager.Instance.playerData.PlayerCoins + 35000;
        UpdatePlayerDataCoins(_totalAmount);
    }
    // ------------------------------------------------------------------------------------------- //

    public void Coins_1000_Btn_Pressed()
    {
        if (!IsAnimating)
            m_StoreController.InitiatePurchase(Coins_1000.Id);
    }
    public void Coins_6000_Btn_Pressed()
    {
        if (!IsAnimating)
            m_StoreController.InitiatePurchase(Coins_6000.Id);
    }
    public void Coins_15000_Btn_Pressed()
    {
        if (!IsAnimating)
            m_StoreController.InitiatePurchase(Coins_15000.Id);
    }
    public void Coins_35000_Btn_Pressed()
    {
        if (!IsAnimating)
            m_StoreController.InitiatePurchase(Coins_35000.Id);
    }




    public void ShowPackageOnClick()
    {
        StartCoroutine(ShowPackagesPanel());
    }
    public IEnumerator ShowPackagesPanel()
    {
        if (IsAnimating)
            yield break;

        IsAnimating = true;

        BackGround_Button.gameObject.SetActive(true);
        BackGround_Button.GetComponent<Image>().DOFade(1, 0.25f).SetEase(Ease.InOutCubic);

        //yield return new WaitForSeconds(0.25f);

        foreach (var v in Packages_Buttons)
        {
            v.DOScale(Vector3.one, 0.25f).SetEase(Ease.InOutCubic);
            yield return new WaitForSeconds(0.25f);
        }

        foreach (var v in Packages_Buttons)
        {
            v.GetComponent<Button>().enabled = true;
        }

        IsAnimating = false;
    }

    public void HidePackageOnClick()
    {
        // No need for this. As in the new design, we'll keep the panel open always...
        // StartCoroutine(HidePackagesPanel());
    }
    public IEnumerator HidePackagesPanel()
    {
        if (IsAnimating)
            yield break;

        IsAnimating = true;

        foreach (var v in Packages_Buttons)
        {
            v.GetComponent<Button>().enabled = false;
        }

        BackGround_Button.GetComponent<Image>().DOFade(0, 0.5f).SetEase(Ease.InOutCubic);

        foreach (var v in Packages_Buttons)
        {
            v.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InOutCubic);
            yield return new WaitForSeconds(0.1f);
        }

        //BackGround_Button.GetComponent<Image>().DOFade(0, 0.25f).SetEase(Ease.InOutCubic);

        yield return new WaitForSeconds(0.25f);

        BackGround_Button.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        IsAnimating = false;
    }





    // ------------------------ In-App Purchase Functions ------------------------ //

    public void SetupBuilder()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(Coins_1000.Id, ProductType.Consumable);
        builder.AddProduct(Coins_6000.Id, ProductType.Consumable);
        builder.AddProduct(Coins_15000.Id, ProductType.Consumable);
        builder.AddProduct(Coins_35000.Id, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }




    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        Debug.Log("In-App Purchase Initialized Succesfully!");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        var product = purchaseEvent.purchasedProduct;

        if (product.definition.id == Coins_1000.Id)
        {
            AddCoins_1000();
        }
        else if (product.definition.id == Coins_6000.Id)
        {
            AddCoins_6000();
        }
        else if (product.definition.id == Coins_15000.Id)
        {
            AddCoins_15000();
        }
        else if (product.definition.id == Coins_35000.Id)
        {
            AddCoins_35000();
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("Initialize Failed: " + error);
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.Log("Initialize Failed: " + error + " " + message);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log("Purchase Failed: " + product + " " + failureReason);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.Log("Purchase Failed: " + product + " " + failureDescription);
    }
}
