using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum PanelPositioning
{
    Center,

    Right,

    Left
}
public class UIPanelController : MonoBehaviour
{
    public PanelPositioning panelPositioning;
    public RectTransform Panel;
    public Vector2 OriginalTransform;
    public float OriginalxPos;

    void Awake() 
    {
        Panel = GetComponent<RectTransform>();
    }
   
    void OnEnable() 
    {
        ArrangePanleOnStart();

        OriginalTransform = Panel.anchoredPosition;
        OriginalTransform.x = OriginalxPos;
    }

    void ArrangePanleOnStart()
    {
        switch(panelPositioning)
        {
            case PanelPositioning.Center:
            Panel.anchoredPosition = new Vector2(0, 0);
            break;

            case PanelPositioning.Right:
            Panel.anchoredPosition = new Vector2(UIManagerTheUltimate.Instance.RightStartPosition, 0);
            break; 

            case PanelPositioning.Left:
            Panel.anchoredPosition = new Vector2(UIManagerTheUltimate.Instance.LeftStartPosition, 0);
            break;
        }
    }
}
