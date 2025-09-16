using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using Spine.Unity.Examples;
public class HomeMenuCleanUp : MonoBehaviour
{
    void Awake() 
    {
        if (NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);
        
        if (HomeMenuManager.Instance != null)
            Destroy(HomeMenuManager.Instance.gameObject);
    }
}
