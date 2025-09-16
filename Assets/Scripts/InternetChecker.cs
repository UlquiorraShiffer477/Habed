using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using Hellmade.Net;

public class InternetChecker : MonoBehaviour
{
    private void Awake()
    {
        // // Listen to event
        // EazyNetChecker.OnConnectionStatusChanged += OnNetStatusChanged;

        // // Start a check
        // CheckNow();
    }


    /// <summary>
    /// Function that starts a continuous check
    /// </summary>
    public void CheckNow()
    {
        // Start a continuous check. Note the second parameter that is passed. True is passed for interruptActiveChecks.
        // This is done because the function CheckNow is also used for the "Check Now" button in the demo.
        // This way, when pressed, it will cancel the previous continuous check (it does not have to wait for its next check), and will immediately start another check.
        EazyNetChecker.StartConnectionCheck(false, true);
    }

    private void OnNetStatusChanged()
    {
        // Change status text depending on the status
        switch (EazyNetChecker.Status)
        {
            case NetStatus.Connected:
                // statusText.text = "CONNECTED";
                // statusImg.color = Color.green;
                GamePlayManager.Instance.SetPauseReadyServerRpc(true, default);
                break;
            case NetStatus.NoDNSConnection:
                // statusText.text = "OFFLINE";
                // statusImg.color = Color.red;
                GamePlayManager.Instance.SetPauseReadyServerRpc(false, default);
                break;
            case NetStatus.WalledGarden:
                // statusText.text = "RESRTICTED";
                // statusImg.color = Color.yellow;
                GamePlayManager.Instance.SetPauseReadyServerRpc(false, default);
                break;
            case NetStatus.PendingCheck:
                // statusText.text = "PENDING";
                // statusImg.color = Color.grey;
                break;
        }
    }
}
