using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TestLineSwipe : MonoBehaviour
{
    public RectTransform line;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float animationDuration = 1f;

    private void Start()
    {
        startPosition = line.anchoredPosition;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endPosition = Input.mousePosition;

            float swipeDistance = endPosition.x - startPosition.x;

            if (swipeDistance > 0)
            {
                // Move line to right
                endPosition = new Vector2(line.anchoredPosition.x + 100, line.anchoredPosition.y);
                line.DOAnchorPos(endPosition, animationDuration);
            }
            else if (swipeDistance < 0)
            {
                // Move line to left
                endPosition = new Vector2(line.anchoredPosition.x - 100, line.anchoredPosition.y);
                line.DOAnchorPos(endPosition, animationDuration);
            }
        }
    }
}
