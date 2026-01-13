using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;

using RTLTMPro;
using TMPro;

using System.IO;

using Unity.Netcode;
using System;
using System.Text;
using Newtonsoft.Json;

using System.Net.Security;
using WebSocketSharp;

[Serializable]
public class JsonPostBody
{
    public int numberofuser;
}


public class BackEndManager : NetworkBehaviour
{
    #region Instance
    private static BackEndManager _instance;

    public static BackEndManager Instance
    {
        get
        {
            if (!_instance)
                _instance = GameObject.FindObjectOfType<BackEndManager>();

            return _instance;
        }
    }
    #endregion

    [SerializeField] string jsonURL_WithDefaultOptions;
    [SerializeField] string jsonURL_NoDefaultOptions;

    NetworkVariable<FixedString4096Bytes> jsonFixedtring = new NetworkVariable<FixedString4096Bytes>();

    UnityWebRequest WWWRequest;

    public JsonPostBody jsonPostBody;

    public JsonDataBase jsonDataBase;
    public List<Round> rounds;

    [Header("JSON Audio Info")]
    public AudioClip CurrentQuestionAudioClip;
    public List<AudioClip> AllAudiosRetrived;
    public int CurrentAudioIndex;

    [Header("Return Options")]
    [SerializeField] bool returnOptions = true;


    void Start()
    {
        if (!IsServer)
            return;

        Init();
    }

    public void Init()
    {
        // if(!IsServer)
        //     return;

        StartRetrivingCoroutineClientRpc();
    }

    [ClientRpc]
    public void StartRetrivingCoroutineClientRpc()
    {
        Debug.Log("Assigning Questions...");
        if (returnOptions)
            StartCoroutine(RetriveDataFromJson(jsonURL_WithDefaultOptions));
        else
            StartCoroutine(RetriveDataFromJson(jsonURL_NoDefaultOptions));
    }

    public IEnumerator RetriveDataFromJson(string _url)
    {
        jsonPostBody.numberofuser = MainNetworkManager.Instance.PlayerInfoNetworkList.Count;

        WWWRequest = UnityWebRequest.PostWwwForm(_url, "");
        WWWRequest.SetRequestHeader("Content-Type", "application/json");
        WWWRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jsonPostBody)));
        WWWRequest.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log("www: " + _url);

        yield return WWWRequest.SendWebRequest();

        if (WWWRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(WWWRequest.error);
        }
        else
        {
            string responseText = WWWRequest.downloadHandler.text;
            Debug.Log("Raw Response: " + responseText);

            if (IsServer)
            {
                jsonFixedtring.Value = responseText;
            }
            else
            {
                while (jsonFixedtring.Value.ToString().IsNullOrEmpty())
                {
                    Debug.Log("Waiting to sync [jsonFixedtring] with host...");
                    yield return new WaitForSeconds(0.1f);
                }
            }

            Debug.Log("jsonFixedtring.Value = " + jsonFixedtring.Value.ToString());

            // CRITICAL: Make a local copy BEFORE parsing to avoid race conditions
            string jsonToParse = jsonFixedtring.Value.ToString();

            while (jsonToParse.ToString().IsNullOrEmpty())
            {
                Debug.Log("Waiting to assign [jsonToParse]...");
                yield return new WaitForSeconds(0.1f);
            }

            // Small delay to ensure the NetworkVariable is fully synced on clients
            yield return new WaitForSeconds(0.1f);

            // Make another copy to be safe
            jsonToParse = jsonFixedtring.Value.ToString();

            Debug.Log("jsonToParse = " + jsonToParse);

            if (string.IsNullOrEmpty(jsonToParse) || string.IsNullOrWhiteSpace(jsonToParse))
            {
                Debug.LogError("JSON value is empty or null!");
                yield break;
            }

            try
            {
                Debug.Log("Parsing JSON: [" + jsonToParse + "]");
                jsonDataBase = JsonUtility.FromJson<JsonDataBase>(jsonToParse);
                rounds = jsonDataBase.rounds;
                RoundManager.Instance.rounds = rounds;
                RoundManager.Instance.AssignQuestionTextFields();
                SaveFilesFromJSON();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JSON Parse Error: " + ex.Message);
                Debug.LogError("Failed JSON: " + jsonToParse);
            }
        }
    }

    public void SaveFilesFromJSON()
    {
        StartCoroutine(LoadAudio());
    }

    private IEnumerator LoadAudio()
    {
        string audioFilePath;

        foreach (Round round in rounds)
        {
            foreach (Question question in round.questions)
            {
                if (!string.IsNullOrEmpty(question.questionAudioPath))
                {
                    audioFilePath = question.questionAudioPath;

                    //  Debug.Log("Load Audio");
                    // Create a new WWW object to load the audio file
                    UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioFilePath, AudioType.UNKNOWN);

                    // Wait for the audio file to be loaded
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {

                        // Debug.Log("Audios Got loaded!");

                        AllAudiosRetrived.Add(DownloadHandlerAudioClip.GetContent(www));

                    }
                    else
                    {
                        Debug.Log("Error loading audio: " + www.error);
                    }
                }

                yield return new WaitForSeconds(0.001f);
            }
        }

        AssignCurrentQuestionAudioClip();

        Debug.Log($"isServer: {IsServer} - ID: {NetworkManager.Singleton.LocalClientId}");
        GamePlayManager.Instance.Test_SetPlayerSceneReadyClientRpc();
    }

    public void AssignCurrentQuestionAudioClip()
    {
        // Debug.Log("AssignCurrentQuestionAudioClip 1");
        CurrentQuestionAudioClip = AllAudiosRetrived[CurrentAudioIndex];
        // Debug.Log("AssignCurrentQuestionAudioClip 2");

        if (CurrentAudioIndex < AllAudiosRetrived.Count - 1)
        {
            // Debug.Log("AssignCurrentQuestionAudioClip 1");
            CurrentAudioIndex++;
        }
    }

    public void DeleteSavedFiles()
    {
        // Get the path to the saved audio files
        string audioSavePath = Path.Combine(Application.persistentDataPath, "audio.wav");
        string textSavePath = Path.Combine(Application.persistentDataPath, "text.txt");

        // Check if the audio file exists and delete it
        if (File.Exists(audioSavePath))
        {
            File.Delete(audioSavePath);
            Debug.Log("Audio file deleted.");
        }

        // Check if the text file exists and delete it
        if (File.Exists(textSavePath))
        {
            File.Delete(textSavePath);
            Debug.Log("Text file deleted.");
        }
    }

    IEnumerator WatForResponse(UnityWebRequest request)
    {
        while (!request.isDone)
        {
            // FadedBG.Instance.Init((int)request.downloadProgress);

            //progressBar.value = request.downloadProgress;
            // FadedBG.Instance.FirstProgressBarUpdate(request.downloadProgress);

            // FadedBG.Instance.UpdateTextInBar("Setting up questions... "/*+ (int)(request.downloadProgress * 100f) + "%"*/);

            // FadedBG.Instance.UpdateAssetNames("Downloading Products...");

            yield return new WaitForSeconds(0.01f);
        }
    }

    // public struct FixedPlayerName : INetworkSerializable, IEquatable<FixedPlayerName>{
    // ForceNetworkSerializeByMemcpy<FixedString32Bytes> m_Name; // using ForceNetworkSerializeByMemcpy to force compatibility between FixedString and NetworkSerializable
    // public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
    //     serializer.SerializeValue(ref m_Name);
    // }

    // public override string ToString() {
    //     return m_Name.Value.ToString();
    // }

    // public static implicit operator string(FixedPlayerName s) => s.ToString();
    // public static implicit operator FixedPlayerName(string s) => new FixedPlayerName() { m_Name = new FixedString32Bytes(s) };

    // public bool Equals(FixedPlayerName other)
    // {
    //     return m_Name.ToString() == other.m_Name.ToString();   
    // }
    // }

    //List Serialization...
    // int lenght = 0;
    // FixedPlayerName[] Array; 
    // if (!serializer.IsReader)
    // {
    //     Array = PlayersWhoChooseOwnerAnswer.ToArray();
    //     lenght = Array.Length;
    // }
    // else
    // {
    //     Array = new FixedPlayerName[lenght];
    // }
    // serializer.SerializeValue(ref lenght);

    // for (int i = 0; i < lenght; ++i)
    // {
    //     serializer.SerializeValue(ref Array[i]);
    // }

    // if (serializer.IsReader)
    // {
    //     PlayersWhoChooseOwnerAnswer = Array.ToList();
    // }

}
