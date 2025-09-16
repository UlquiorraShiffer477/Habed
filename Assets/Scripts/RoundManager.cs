using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTLTMPro;
using System;
using UnityEngine.EventSystems;


[Serializable]
public class Questions
{
    public string QuestionText;
    public string QuestionAnswer;
}

//--------------------------------------------------//

// [Serializable]
// public class CorrectQuestionAnswer
// {
//     public string answerText;
//     public List<PlayerInfo> playersWhoClicked = new List<PlayerInfo>();
// }

public class RoundManager : MonoBehaviour
{
     #region Instance
    private static RoundManager _instance;
    
    public static RoundManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<RoundManager>();

			return _instance;
		}
	}
    #endregion


    [Header("Main Info")]
    [SerializeField] int RoundCounter = 1;
    int SubRoundCounter = 1;
    string RoundCurrentName;

    [SerializeField] List<string> RoundNames;

    //-------------------------------

    [Header("Score Info")]
    int BasCorrect = 1000;
    int BasLie = 500;

    int CorrectScore = 1000;
    int LieScroe = 500;

    //-------------------------------

    // [Header("Answers Info")]
    // public List<AnswersShowing> answers = new List<AnswersShowing>();

    //-------------------------------

    [Header("Questions Info")]
    public List<Question> questions = new List<Question>();
    public List<Round> rounds = new List<Round>();

    public List<RTLTextMeshPro> QuestionFields;

    [Header("Demo Players Answers")]

    public List<string> CurrentPlayerAnswer = new List<string>();

    public List<string> R1A = new List<string>();
    public List<string> R2A = new List<string>();
    public List<string> R3A = new List<string>();


    public bool canplayaduio = true;

    
    //-------------------------------
    void Awake() 
    {
        SetRoundName();
    }
    // void Start() 
    // {
    //     SetRoundName();
    // }

    public void SetRoundScores()
    {
        if(RoundCounter <= 3)
        {
            CorrectScore = BasCorrect * RoundCounter;
            LieScroe = BasLie * RoundCounter;
        }
    }

    // ---------------------Round Helper Get/Set--------------------- //
    public void SetRoundName()
    {
        RoundCurrentName = RoundNames[RoundCounter-1];
    }
    public string GetRoundName()
    {
        return RoundCurrentName;
    }

    public void SetRoundCount()
    {
        RoundCounter++;
    }
    public int GetRoundCount()
    {
        return RoundCounter;
    }
    public void ResetRoundCount()
    {
        RoundCounter = 1;
    }


    // ---------------------Sub-Round Helper Get/Set--------------------- //
    public void SetSubRoundCount(bool _resetCounter = false)
    {
        if (_resetCounter)
        {
            ResetSubRoundCount();
            return;
        }
            
        SubRoundCounter++;
    }

    public int GetSubRoundCount()
    {
        return SubRoundCounter;
    }
    public void ResetSubRoundCount()
    {
        SubRoundCounter = 1;
    }


    // ---------------------Score Helper Get/Set--------------------- //
    public int GetCorrectScore()
    {
        return CorrectScore;
    }
    public int GetLieScore()
    {
        return LieScroe;
    }
    public void ResetScore()
    {
        LieScroe = 500;
        CorrectScore = 100;
    }


    // ---------------------Helper Functions--------------------- //
    
    public void SetNextQuestion(bool _isNewRound = false)
    {
        Debug.Log("NextQuestions");

        if (GetRoundCount() == 3)
        {
            Debug.Log($"Round Count Is: {GetRoundCount()}");
            GamePlayManager.Instance.RoundAnounser.SetActive(false);
            GamePlayManager.Instance.RoundResultsScreen.SetActive(true);
        }

        // state.Value = State.GameStart;

        if (_isNewRound)
        {
            SetRoundCount();
            SetSubRoundCount(true);
        }

        SetSubRoundCount();

        SetRoundName();

        GamePlayManager.Instance.roomSessionManager.ResetDependencies();
        SetRoundScores();

        AssignQuestionTextFields();
        // AssignKeyboardType();

        // foreach(RTLTextMeshPro v in QuestionFields)
        // {
        //     Debug.Log($"Assigning Question: round[{RoundCounter-1}].question[{SubRoundCounter-1}]");
        //     // v.text = rounds[RoundCounter-1].questions[SubRoundCounter-1].question;
        // }

        #region Old Code...
        // CurrentPlayerAnswer = R1A;

        // switch(RoundCounter)
        // {
        //     case 1:
        //     {
        //         foreach(RTLTextMeshPro v in QuestionFields)
        //         {
        //             v.text = rounds[0].questions[SubRoundCounter-1].question;
        //         }

        //         CurrentPlayerAnswer = R1A;
        //     }
        //     break;

        //     case 2:
        //     {
        //         foreach(RTLTextMeshPro v in QuestionFields)
        //         {
        //             v.text = rounds[1].questions[SubRoundCounter-1].question;
        //         }

        //         CurrentPlayerAnswer = R2A;
        //     }
        //     break;

        //     case 3:
        //     {
        //         foreach(RTLTextMeshPro v in QuestionFields)
        //         {
        //             v.text = rounds[2].questions[SubRoundCounter-1].question;
        //         }

        //         CurrentPlayerAnswer = R3A;
        //     }
        //     break;
        // }
        #endregion
    }

    public void AssignQuestionTextFields()
    {
        foreach(RTLTextMeshPro v in QuestionFields)
        {
            Debug.Log($"Assigning Question: round[{RoundCounter-1}].question[{SubRoundCounter-1}]");
            v.text = rounds[RoundCounter-1].questions[SubRoundCounter-1].question;
        }

        // AssignKeyboardType();
    }

    public void AssignKeyboardType(string _temp)
    {
        if (rounds[RoundCounter-1].questions[SubRoundCounter-1].isNumbersOnly)
        {
            GamePlayManager.Instance.AnswerField.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;

            // TouchScreenKeyboard.Open("",TouchScreenKeyboardType.NumberPad);
            Debug.Log("Keyboard type = PhonePad");
        }
        else
        {
            TouchScreenKeyboard.Open("",TouchScreenKeyboardType.Default);
            Debug.Log("Keyboard type = Default");
        }
    }
}
