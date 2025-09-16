using Habed;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class NotchManager
{
    [System.Flags]
    public enum SidesAffected
    {
        None = 0,

        Right = 1,
        Left = 2,
        Up = 4,
        Down = 8,

        All = ~0,
    }

    public static UnityEngine.Events.UnityAction OnUpdateNotch;

    private static float _canvasScaleFactor = -1.0f;
    public static float CanvasScaleFactor
    {
        get
        {
            if (_canvasScaleFactor < 0) _canvasScaleFactor = CalculateScaleFactor(ScreenMatchMode.MatchWidthOrHeight);
            return _canvasScaleFactor;
        }
    }

    public static SafeAreaSize GetSafeAreaSize(SidesAffected sidesAffected)
    {
        var topOffset = sidesAffected.HasFlag(SidesAffected.Up) ? Screen.height - GetScreenSafeArea().height - GetScreenSafeArea().y : 0;
        var bottomOffset = sidesAffected.HasFlag(SidesAffected.Down) ? GetScreenSafeArea().y : 0;
        var rightOffset = sidesAffected.HasFlag(SidesAffected.Right) ? 0 : 0;
        var leftOffset = sidesAffected.HasFlag(SidesAffected.Left) ? 0 : 0;

        if (NotchEnabled)
        {
            return new SafeAreaSize(
                new Vector2(rightOffset, topOffset),
                new Vector2(leftOffset, bottomOffset)
                );
        }
        else
        {
            return new SafeAreaSize(
                Vector2.zero,
                Vector2.zero
                );
        }
    }

    public static SafeAreaSize GetSafeAreaSizeScaled(SidesAffected sidesAffected, Canvas canvas = null)
    {
        return GetSafeAreaSize(sidesAffected) / (canvas == null ? CanvasScaleFactor : canvas.scaleFactor);
    }

    private static float CalculateScaleFactor(ScreenMatchMode matchMode, float match = 1f)
    {
        Vector2 refrenceSize = new Vector2(390, 844);
        float scaleFactor = 0;
        var screenSize = new Vector2(Screen.width, Screen.height);

        switch (matchMode)
        {
            case ScreenMatchMode.MatchWidthOrHeight:
                float logWidth = Mathf.Log(screenSize.x / refrenceSize.x, 2);
                float logHeight = Mathf.Log(screenSize.y / refrenceSize.y, 2);
                float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, match);
                scaleFactor = Mathf.Pow(2, logWeightedAverage);
                break;
            case ScreenMatchMode.Expand:
                scaleFactor = Mathf.Min(screenSize.x / refrenceSize.x, screenSize.y / refrenceSize.y);
                break;
            case ScreenMatchMode.Shrink:
                scaleFactor = Mathf.Max(screenSize.x / refrenceSize.x, screenSize.y / refrenceSize.y);
                break;
        }

        return scaleFactor;
    }

    public static bool NotchEnabled
    {
        get
        {
            return true;
        }
        set
        {
            OnUpdateNotch?.Invoke();
        }
    }

    private static Rect GetScreenSafeArea()
    {
        return true ? Screen.safeArea : new Rect(0, 155, Screen.width, Screen.height - 155 - 100);
    }
}

public struct SafeAreaSize
{
    public Vector2 offsetMax;
    public Vector2 offsetMin;

    public SafeAreaSize(Vector2 offsetMax, Vector2 offsetMin)
    {
        this.offsetMax = offsetMax;
        this.offsetMin = offsetMin;
    }

    public static SafeAreaSize operator /(SafeAreaSize safeAreaSize, float canvasScaleFactor)
    {
        return new SafeAreaSize(safeAreaSize.offsetMax / -canvasScaleFactor, safeAreaSize.offsetMin / canvasScaleFactor);
    }
}