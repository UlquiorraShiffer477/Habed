using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace Habed.Networking
{
    public class ConnectivityTest : Common.Singleton<ConnectivityTest>
    {
        public static string URL = "google.com";
        public static string FIREBASE_IP = "35.201.97.85";

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CheckInternetConnection(BooleanCallback callback)
        {
            Instance.UnityPingHost(callback);
        }

        private Ping ping;
        private readonly List<BooleanCallback> callbacks = new List<BooleanCallback>();
        private bool isPinging;

        private void UnityPingHost(BooleanCallback callback)
        {
            ping = new Ping(URL);

            callbacks.Add(callback);

            if (isPinging) return;

            isPinging = true;

            if (Application.internetReachability == NetworkReachability.NotReachable)
                Result(false);
            else
                StartCoroutine(_UnityPingHost());
        }

        private IEnumerator _UnityPingHost()
        {
            float timeOut = 10.0f;
            float timer = 0;

            while (!ping.isDone)
            {
                if (timer > timeOut)
                {
                    Result(false);
                    yield break;
                }

                yield return null;
                timer += Time.deltaTime;
            }

            Result(true);
        }

        private void Result(bool result)
        {
            for (int i = 0; i < callbacks.Count; i++)
            {
                callbacks[i]?.Invoke(result);
            }

            callbacks.Clear();
            isPinging = false;
            ping.DestroyPing();
        }
    }
}