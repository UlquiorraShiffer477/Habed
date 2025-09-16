using System.Collections;
using System.Collections.Generic;
using Spine.Unity.Examples;
using UnityEngine;

public class LobbyScreenController : MonoBehaviour
{
    void OnEnable() 
    {
        if (GlobalManager.Instance.CanAddCoins)
        {
            ShopManager.Instance.Play_CPCoins(GlobalManager.Instance.IsDoubleRewards);
        }
    }
}
