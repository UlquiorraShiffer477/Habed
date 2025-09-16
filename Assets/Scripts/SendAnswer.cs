using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendAnswer : MonoBehaviour
{
    public string Answer;
    public Sprite Choosen;

    public void OnClick()
    {
        GetComponent<Image>().sprite = Choosen;
        SetAnswer(Answer);
    }

    public void SetAnswer(string _answer)
    {
        GamePlayManager.Instance.AnswerShowing.text = _answer;
    }
}
