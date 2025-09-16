using System.Collections;
using System.Collections.Generic;
using Spine.Unity.Examples;
using UnityEngine;
using UnityEngine.UI;

public class UIBannerManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform mainButtonPanel; // Your panel with Home, Store, Dances, Settings
    public RectTransform mainCharactersItems; // Your panel with Home, Store, Dances, Settings




    // [Header("Banner Settings")]
    // public float panelBottomMargin = 0f; // Extra margin between panel and banner

    private Vector2 originalButtonsPanelPosition;
    private Vector2 originalItemPanelPosition;

    private bool hasStoredOriginalPosition = false;

    public void Start()
    {
        // Store the original position of the panel
        if (mainButtonPanel != null)
        {
            originalButtonsPanelPosition = mainButtonPanel.anchoredPosition;
        }

        if (mainCharactersItems != null)
        {
            originalItemPanelPosition = mainCharactersItems.anchoredPosition;
        }

        hasStoredOriginalPosition = true;

        // Subscribe to banner size changes
        if (MediationAdvertismentsBase.Instance != null)
        {
            Debug.Log("Subbed :: OnBannerSizeChanged");
            MediationAdvertismentsBase.OnBannerSizeChanged += OnBannerSizeChanged;
        }

        // Subscribe to notch updates
        NotchManager.OnUpdateNotch += UpdateUILayout;

        ShowBannerAndLiftPanel();
    }

    private void OnDestroy()
    {
        HideBannerAndResetPanel();

        // Unsubscribe from events
        if (MediationAdvertismentsBase.Instance != null)
        {
            Debug.Log("Unsubbed :: OnBannerSizeChanged");
            MediationAdvertismentsBase.OnBannerSizeChanged -= OnBannerSizeChanged;
        }

        NotchManager.OnUpdateNotch -= UpdateUILayout;
    }

    private void OnBannerSizeChanged(float bannerHeightInPixels)
    {
        LiftPanelWithOffset(bannerHeightInPixels);
    }

    public void ShowBannerAndLiftPanel()
    {
        // Show the banner
        if (MediationAdvertismentsBase.Instance != null)
        {
            MediationAdvertismentsBase.Instance.ShowBanner();
        }
    }

    public void HideBannerAndResetPanel()
    {
        // Hide the banner
        if (MediationAdvertismentsBase.Instance != null)
        {
            MediationAdvertismentsBase.Instance.HideBannerCompletely();
        }

        // Reset panel to original position
        // LiftPanelWithOffset(0f);
    }

    // private void LiftPanelUp(float bannerHeightInPixels)
    // {
    //     if (mainButtonPanel == null || !hasStoredOriginalPosition) return;

    //     // Convert banner height from pixels to canvas units
    //     float bannerHeightInCanvasUnits = ConvertPixelsToCanvasUnits_V2(bannerHeightInPixels);

    //     // Calculate how much to lift the panel (banner height + margin)
    //     float liftAmount = bannerHeightInCanvasUnits;

    //     // Get safe area bottom offset to account for notches
    //     var safeArea = NotchManager.GetSafeAreaSizeScaled(NotchManager.SidesAffected.Down);
    //     float safeAreaOffset = safeArea.offsetMin.y;

    //     // Apply the lift: original position + lift amount + safe area offset
    //     Vector2 newPosition = new Vector2(
    //         originalItemPanelPosition.x,
    //         originalItemPanelPosition.y + liftAmount + safeAreaOffset
    //     );

    //     mainButtonPanel.anchoredPosition = newPosition;

    //     Debug.Log($"Panel Lifted - Banner Height: {bannerHeightInPixels}px, Lift Amount: {liftAmount}, New Y: {newPosition.y}");
    // }

    private void UpdateUILayout()
    {
        // Re-calculate position when notch updates occur
        if (MediationAdvertismentsBase.Instance != null)
        {
            float currentBannerHeight = MediationAdvertismentsBase.Instance.GetBannerHeighInPixels();
            LiftPanelWithOffset(currentBannerHeight);
        }
    }

    private float ConvertPixelsToCanvasUnits(float pixels)
    {
        if (pixels <= 0) return 0f;

        float scaleFactor = GetCanvasScaleFactor();
        Debug.Log("scaleFactor = " + scaleFactor);
        return pixels / scaleFactor;
    }

    // // Option 1: Use different conversion method
    // private float ConvertPixelsToCanvasUnits_V2(float pixels)
    // {
    //     // Method 1: Using DPI
    //     return pixels * (160f / Screen.dpi);
    // }


    // Option 4: Subtract a fixed offset to eliminate gap
    private void LiftPanelWithOffset(float bannerHeightInPixels)
    {
        float bannerHeightInCanvasUnits = ConvertPixelsToCanvasUnits(bannerHeightInPixels);

        Vector2 newPosition_Buttons = new Vector2(
            originalButtonsPanelPosition.x,
            originalButtonsPanelPosition.y + bannerHeightInCanvasUnits
        );

        Vector2 newPosition_Items = new Vector2(
            originalItemPanelPosition.x,
            originalItemPanelPosition.y + bannerHeightInCanvasUnits
        );

        if (mainButtonPanel)
            mainButtonPanel.anchoredPosition = newPosition_Buttons;
        
        if (mainCharactersItems)
            mainCharactersItems.anchoredPosition = newPosition_Items;
        
        // if (mainItemsPanel_Male)
        //     mainItemsPanel_Male.anchoredPosition = newPosition_Items;

        // if (mainItemsPanel_Female)
        //     mainItemsPanel_Female.anchoredPosition = newPosition_Items;

        // Debug.Log($"Simple Lift - Banner: {bannerHeightInPixels}px -> {bannerHeightInCanvasUnits} canvas units, New Y: {newPosition.y}");

    }

    private float GetCanvasScaleFactor()
    {
        // Fallback to NotchManager's scale factor
        return NotchManager.CanvasScaleFactor;
    }

    // // Add this method for manual testing without safe area
    // public void LiftPanelUpSimple(float bannerHeightInPixels)
    // {
    //     if (mainButtonPanel == null || !hasStoredOriginalPosition) return;

    //     float bannerHeightInCanvasUnits = ConvertPixelsToCanvasUnits(bannerHeightInPixels);

    //     Vector2 newPosition = new Vector2(
    //         originalPanelPosition.x,
    //         originalPanelPosition.y + bannerHeightInCanvasUnits
    //     );

    //     mainButtonPanel.anchoredPosition = newPosition;

    //     Debug.Log($"Simple Lift - Banner: {bannerHeightInPixels}px -> {bannerHeightInCanvasUnits} canvas units, New Y: {newPosition.y}");
    // }
}
