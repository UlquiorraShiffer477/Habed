using System;
using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections;
using Habed.Networking;

using System.Collections.Generic;

public class MediationAdvertismentsBase : MonoBehaviour
{
    public static Action<float> OnBannerSizeChanged;
    public delegate void VoidDelegate();

    public static MediationAdvertismentsBase Instance;

    public bool rewardedLoaded = false;
    public bool canRequestRewarded = true;
    public bool canRequestInterstitial = true;

    public delegate void IncentivizedCallback(bool status);
    IncentivizedCallback incentivizedCallback;
    VoidDelegate interstitialCallback;

    private InterstitialAd interstitial;
    private RewardedAd rewardBasedVideo;
    [SerializeField] private BannerView bannerView;
    private bool bannerLoaded = false;
    private bool waitingToShowBanner = false;

    private BannerAdConfig _bannerAdConfig = new BannerAdConfig();

    public static VoidDelegate RewardedAdLoadedCallback;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void InitAdMob()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return;
#endif
        incentivizedCallback = null;
        interstitialCallback = null;

        MobileAds.SetiOSAppPauseOnBackground(true);
        MobileAds.RaiseAdEventsOnUnityMainThread = true;

        var config = new RequestConfiguration();
        config.TestDeviceIds = new List<string> { "6E39C1531CAD7E10D24D0E7FDABECF0F" };
        MobileAds.SetRequestConfiguration(config);

        MobileAds.Initialize((InitializationStatus status) =>
        {
            if (status == null)
            {
                Debug.LogError("Google Mobile Ads initialization failed.");
                return;
            }

            Debug.Log("Google Mobile Ads initialization complete.");

            MobileAds.OpenAdInspector((AdInspectorError error) =>
            {
                // Error will be set if there was an issue and the inspector was not displayed.
            });
        });

        // RequestInterstitial();
        RequestRewardBasedVideo();
    }

    private AdRequest CreateAdRequest()
    {
        return new AdRequest();
    }

    #region Interstitial

    public bool IsDisabledShowingInterstitial()
    {
        bool IsDisabled = false;

        // IsDisabled = !DataManager.Instance.GetPlayerData().HasAds;

        return IsDisabled;
    }

    public void RequestInterstitial()
    {
#if UNITY_EDITOR
        return;
#endif
        if (!canRequestInterstitial || Application.internetReachability == NetworkReachability.NotReachable)
            return;

        if (interstitial != null)
        {
            interstitial.Destroy();
        }

        InterstitialAd.Load(MediationStatics.adUnitIdInterstitial, CreateAdRequest(), (InterstitialAd ad, LoadAdError loadAdError) =>
        {
            if (loadAdError != null || ad == null)
            {
                // GameStatics.GeneralDebugLogger.LogError($"InterstitialFailedToLoad: {loadAdError}");
                Invoke(nameof(RequestInterstitial), loadAdError.GetMessage().ToLower().Contains("cap") ? 60 * 5 : 10);
                return;
            }

            interstitial = ad;

            interstitial.OnAdFullScreenContentOpened += HandleInterstitialOpened;
            interstitial.OnAdFullScreenContentClosed += HandleInterstitialClosed;
        });
    }

    public void ShowInterstitial(VoidDelegate callback = null, int numberOfRepetitions = 0)
    {
#if UNITY_EDITOR
        callback?.Invoke();
        return;
#endif

        if (callback != null) interstitialCallback = callback;

        if (IsDisabledShowingInterstitial())
        {
            interstitialCallback = null;
            if (callback != null) callback();
            return;
        }

        if (interstitial != null && interstitial.CanShowAd())
        {
            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, false, Carrom.CRR_AudioManager.MASTER_DEFAULT_VALUE);
            interstitial.Show();
        }
        else
        {
            interstitialCallback = null;
            callback?.Invoke();

            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, true, Carrom.CRR_AudioManager.MASTER_DEFAULT_VALUE);
            RequestInterstitial();

            // GameStatics.GeneralDebugLogger.Log("Interstitial is not ready yet");
        }
    }

    #endregion

    #region Banner

    public bool IsDisabledShowingBanner()
    {
        bool IsDisabled = false;

        // Debug.Log("HasAds = " + DataManager.Instance.GetPlayerData().HasAds);

        // IsDisabled = !DataManager.Instance.GetPlayerData().HasAds;

        // Debug.Log("IsDisabled = " + IsDisabled);

        return IsDisabled;
    }

    public void SetBannerConfig(BannerAdConfig bannerAdConfig)
    {
        _bannerAdConfig = bannerAdConfig;
    }

    public void ShowBanner()
    {
        Debug.Log("Showing Banner Function!");
        // #if UNITY_EDITOR
        //         return;
        // #endif
        if (IsDisabledShowingBanner())
            return;

        waitingToShowBanner = true;

        // Always request a new banner instead of trying to reuse hidden ones
        if (bannerView == null || bannerView.IsDestroyed || !bannerLoaded)
        {
            Debug.Log("ShowBanner :: Requesting new banner");
            RequestBanner();
        }
        else
        {
            try
            {
                // Try to show existing banner first
                if (IsBannerValid())
                {
                    Debug.Log("ShowBanner :: Showing existing banner");
                    bannerView.Show();

                    float bannerHeight = bannerView.GetHeightInPixels();
                    OnBannerSizeChanged?.Invoke(bannerHeight);
                }
                else
                {
                    Debug.Log("ShowBanner :: Banner invalid, requesting new one");
                    RequestBanner();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error showing banner: {ex.Message}");
                // If showing fails, request a new banner
                bannerView = null;
                bannerLoaded = false;
                RequestBanner();
            }
        }
    }

    public void HideBanner()
    {
        Debug.Log("Hiding Banner Function!");
        // #if UNITY_EDITOR
        //         return;
        // #endif

        waitingToShowBanner = false;

        try
        {
            if (IsBannerValid())
            {
                bannerView.Hide();
                Debug.Log("Banner hidden successfully");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error hiding banner: {ex.Message}");
        }
        finally
        {
            // Always notify that banner is hidden
            OnBannerSizeChanged?.Invoke(0f);
        }
    }

    // Better approach: Destroy and recreate banner when hiding
    public void HideBannerCompletely()
    {
        Debug.Log("Hiding Banner Completely (Destroy)");
        // #if UNITY_EDITOR
        //         return;
        // #endif

        waitingToShowBanner = false;

        try
        {
            if (bannerView != null)
            {
                if (!bannerView.IsDestroyed)
                {
                    bannerView.Destroy();
                }
                NotchManager.OnUpdateNotch -= UpdateBannerToNotch;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error destroying banner: {ex.Message}");
        }
        finally
        {
            bannerView = null;
            bannerLoaded = false;
            OnBannerSizeChanged?.Invoke(0f);
        }
    }

    // Alternative method: Keep banner loaded but move it off-screen
    public void MoveBannerOffScreen()
    {
        Debug.Log("Moving Banner Off Screen");
        // #if UNITY_EDITOR
        //         return;
        // #endif

        try
        {
            if (IsBannerValid())
            {
                // Move banner way off screen instead of hiding
                bannerView.SetPosition(0, -5000);
                OnBannerSizeChanged?.Invoke(0f); // Tell UI it's "hidden"
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error moving banner off screen: {ex.Message}");
        }
    }

    public void MoveBannerBackToPosition()
    {
        Debug.Log("Moving Banner Back to Position");
        // #if UNITY_EDITOR
        //         return;
        // #endif

        try
        {
            if (IsBannerValid())
            {
                bannerView.SetPosition(AdPosition.Bottom);
                float bannerHeight = bannerView.GetHeightInPixels();
                OnBannerSizeChanged?.Invoke(bannerHeight);
            }
            else
            {
                // Banner doesn't exist, create new one
                RequestBanner();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error moving banner back: {ex.Message}");
            RequestBanner();
        }
    }

    // Enhanced RequestBanner that handles re-showing
    public void RequestBanner()
    {
        // #if UNITY_EDITOR
        //         return;
        // #endif

        Debug.Log("RequestBanner called");

        if (_bannerAdConfig == null || !_bannerAdConfig.ShowAd || Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("RequestBanner :: Failed due to config or connectivity");
            return;
        }



        // Clean up existing banner completely
        CleanupExistingBanner();

        // Create new banner
        var safeArea = NotchManager.GetSafeAreaSize(NotchManager.SidesAffected.All);
        AdSize adSize = AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        bannerView = new BannerView(MediationStatics.adUnitIdBanner, adSize, AdPosition.Bottom);

        bannerView.OnBannerAdLoaded += HandleBannerLoaded;
        bannerView.OnBannerAdLoadFailed += HandleBannerFailedToLoad;

        Debug.Log("RequestBanner :: Loading new banner ad");
        bannerView.LoadAd(CreateAdRequest());

        NotchManager.OnUpdateNotch += UpdateBannerToNotch;



    }

    private void CleanupExistingBanner()
    {
        try
        {
            if (bannerView != null)
            {
                bannerView.OnBannerAdLoaded -= HandleBannerLoaded;
                bannerView.OnBannerAdLoadFailed -= HandleBannerFailedToLoad;
                NotchManager.OnUpdateNotch -= UpdateBannerToNotch;

                // Unsubscribe from events
                if (MediationAdvertismentsBase.Instance != null)
                {
                    Debug.Log("Unsubbed :: OnBannerSizeChanged");
                    MediationAdvertismentsBase.OnBannerSizeChanged -= OnBannerSizeChanged;
                }

                if (!bannerView.IsDestroyed)
                {
                    bannerView.Destroy();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Error cleaning up banner: {ex.Message}");
        }
        finally
        {
            bannerView = null;
            bannerLoaded = false;
        }
    }

    private bool IsBannerValid()
    {
        try
        {
            return bannerView != null && !bannerView.IsDestroyed;
        }
        catch
        {
            return false;
        }
    }

    public void UpdateBannerToNotch()
    {
        // #if UNITY_EDITOR
        //         return;
        // #endif
        if (bannerView == null || _bannerAdConfig == null)
        {
            Debug.Log("bannerView = " + bannerView + " / " + "_bannerAdConfig = " + _bannerAdConfig);
            // return;
        }

        var safeArea = NotchManager.GetSafeAreaSize(NotchManager.SidesAffected.All);
        bannerView.SetPosition(AdPosition.Bottom);
        // switch (_bannerAdConfig.Position.ToLower())
        // {
        //     case "top":
        //         int bannerPositionX = 0;
        //         int bannerPositionY = (int)ConvertPixelsToDP(65.0f * NotchManager.CanvasScaleFactor + safeArea.offsetMax.x);
        //         bannerView.SetPosition(bannerPositionX, bannerPositionY);
        //         break;
        //     case "bottom":
        //         bannerView.SetPosition(AdPosition.Bottom);
        //         break;
        // }
    }

    public float GetBannerHeighInPixels()
    {
        if (bannerView == null || bannerView.IsDestroyed) return ConvertDPToPixels(50f);
        return bannerView.GetHeightInPixels();
    }

    #endregion

    #region RewardedVideo

    bool shouldReward = false;
    bool isShowingAdmobRewarded = false;
    private object callback;

    public void RequestRewardBasedVideo()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return;
#endif

        if (!canRequestRewarded || Application.internetReachability == NetworkReachability.NotReachable)
            return;

        RewardedAd.Load(MediationStatics.adUnitIdRewarded, CreateAdRequest(), (RewardedAd ad, LoadAdError loadAdError) =>
        {
            if (loadAdError != null || ad == null)
            {
                // GameStatics.GeneralDebugLogger.LogError($"RewardedAdFailedToLoad: {loadAdError}");
                Invoke(nameof(RequestRewardBasedVideo), loadAdError.GetMessage().ToLower().Contains("cap") ? 60 * 5 : 10);

                canRequestRewarded = true;
                rewardedLoaded = false;

                return;
            }

            // GameStatics.GeneralDebugLogger.Log($"RewardedAdLoaded");

            canRequestRewarded = false;
            rewardedLoaded = true;

            RewardedAdLoadedCallback?.Invoke();
            rewardBasedVideo = ad;

            rewardBasedVideo.OnAdFullScreenContentOpened += HandleRewardBasedVideoOpened;
            rewardBasedVideo.OnAdFullScreenContentClosed += HandleRewardBasedVideoClosed;
        });
    }

    public bool IsRewardedAdAvailable()
    {
        return rewardBasedVideo != null && rewardBasedVideo.CanShowAd();
    }

    public void ShowRewardBasedVideo(IncentivizedCallback callback = null, int numberOfRepetitions = 0)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        callback?.Invoke(false);
        callback = null;
        return;
#endif

        incentivizedCallback = callback;

        if (rewardBasedVideo.CanShowAd())
        {
            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, false, Carrom.CRR_AudioManager.MASTER_DEFAULT_VALUE);
            isShowingAdmobRewarded = true;
            rewardBasedVideo.Show(HandleRewardBasedVideoRewarded);
        }
        else if (numberOfRepetitions < 2)
        {
            StartCoroutine(DelayedShowRewarded(numberOfRepetitions));
        }
        else
        {
            // GameStatics.GeneralDebugLogger.Log("Reward based video ad is not ready yet");

            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, true, Carrom.CRR_AudioManager.MASTER_DEFAULT_VALUE);
            incentivizedCallback?.Invoke(false);
            incentivizedCallback = null;

            RequestRewardBasedVideo();
        }
    }

    IEnumerator DelayedShowRewarded(int numberOfRepetitions)
    {
        yield return new WaitForSeconds(0.5f);
        ShowRewardBasedVideo(incentivizedCallback, numberOfRepetitions + 1);
    }

    #endregion

    #region Interstitial callback handlers

    public void HandleInterstitialOpened()
    {
        // UnityMainThreadDispatcher.Instance.Enqueue(() => CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, false, CRR_AudioManager.MASTER_DEFAULT_VALUE));
        print("HandleInterstitialOpened event received");
    }

    public void HandleInterstitialClosed()
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            canRequestInterstitial = true;

            interstitialCallback?.Invoke();
            interstitialCallback = null;

            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, true, CRR_AudioManager.MASTER_DEFAULT_VALUE);

            interstitial.Destroy();
            RequestInterstitial();
        });

        print("HandleInterstitialClosed event received");
    }

    #endregion

    #region RewardBasedVideo callback handlers

    public void HandleRewardBasedVideoOpened()
    {
        print("HandleRewardBasedVideoOpened event received");
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, false, CRR_AudioManager.MASTER_DEFAULT_VALUE);
            rewardedLoaded = false;
        });
    }

    public void HandleRewardBasedVideoClosed()
    {
        print("HandleRewardBasedVideoClosed event received");
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, true, CRR_AudioManager.MASTER_DEFAULT_VALUE);
            canRequestRewarded = true;
            rewardedLoaded = false;
            RequestRewardBasedVideo();
            isShowingAdmobRewarded = false;

            // FirebaseAnalyticsClient.LogEvent(FirebaseAnalyticsClient.EventName.rewarded_ad_complete, new Dictionary<string, object>()
            // {
            //     { "reward_granted", false },
            // });
        });
    }

    public void HandleRewardBasedVideoRewarded(Reward reward)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            string type = reward.Type;
            double amount = reward.Amount;

            shouldReward = true;
            isShowingAdmobRewarded = false;

            incentivizedCallback?.Invoke(shouldReward);
            incentivizedCallback = null;

            shouldReward = false;

            // CRR_AudioManager.Instance.ToggleGroupVolume(CRR_AudioManager.Group.Master, true, CRR_AudioManager.MASTER_DEFAULT_VALUE);
            print("HandleRewardBasedVideoRewarded event received for " + amount.ToString() + " " + type);
        });
    }

    #endregion

    #region Banner callback handlers

    public void HandleBannerLoaded()
    {
        print("HandleBannerLoaded event received");

        bannerLoaded = true;
        if (waitingToShowBanner)
            bannerView.Show();

        OnBannerSizeChanged?.Invoke(bannerView.GetHeightInPixels());
    }

    public void HandleBannerFailedToLoad(LoadAdError loadAdError)
    {
        print("HandleBannerFailedToLoad event received with message: " + loadAdError.GetMessage());

        if (loadAdError.GetMessage().ToLower() == "no fill")
        {
            bannerLoaded = false;
        }
    }

    public void HandleBannerOpened(object sender, EventArgs args)
    {
        print("HandleBannerOpened event received");
    }

    #endregion

    public static float ConvertPixelsToDP(float px)
    {
        return (px * 160.0f) / Screen.dpi;
    }

    public static float ConvertDPToPixels(float dp)
    {
        Debug.Log("ConvertDPToPixels :: " + (dp * Screen.dpi) / 160.0f);
        return (dp * Screen.dpi) / 160.0f;
    }
}

public class MediationStatics
{
    #region AppID

#if UNITY_ANDROID
    public static string appId = "ca-app-pub-8640126183830435~9237427199";
#elif UNITY_IPHONE
    public static string appId = "ca-app-pub-8640126183830435~8573398007";
#else
    public static string appId = "unexpected_platform";
#endif

    #endregion

    #region InterstitialIDs

#if UNITY_EDITOR
    public static string adUnitIdInterstitial = "unused";
#elif UNITY_ANDROID
    public static string adUnitIdInterstitial = "ca-app-pub-8640126183830435/5524212151";
#elif UNITY_IPHONE
    public static string adUnitIdInterstitial = "ca-app-pub-8640126183830435/4338342741";
#else
    public static string adUnitIdInterstitial = "unexpected_platform";
#endif

    #endregion

    #region RewardedIDs

#if UNITY_EDITOR
    public static string adUnitIdRewarded = "unused";
#elif UNITY_ANDROID
    public static string adUnitIdRewarded = "ca-app-pub-8640126183830435/2932422036";
#elif UNITY_IPHONE
    public static string adUnitIdRewarded = "ca-app-pub-8640126183830435/5126791794";
#else
    public static string adUnitIdRewarded = "unexpected_platform";
#endif

    #endregion

    #region BannerIDs

#if UNITY_EDITOR
    public static string adUnitIdBanner = "ca-app-pub-3940256099942544/9214589741"; // Testing ID From Google...
#elif UNITY_ANDROID
    public static string adUnitIdBanner = "ca-app-pub-8640126183830435/9036953893";
#elif UNITY_IPHONE
    public static string adUnitIdBanner = "ca-app-pub-8640126183830435/6837293826";
#else
    public static string adUnitIdBanner = "unexpected_platform";
#endif

    #endregion
}

[Serializable]
public class BannerAdConfig
{
    public bool ShowAd = true;
    public string Position = "bottom";
}