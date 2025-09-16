using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JsonDataBase
{
    public List<Round> rounds;
}

[Serializable]
public class Round
{
    public List<Question> questions;
}

[Serializable]
public class Question
{
    public string _id;
    public int round;
    public string answer;
    public string question;
    public string questionAudioPath;

    public List<string> defaultAnswer;

    public bool isNumbersOnly;
}
