using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;


public class LoadingInBetweenScenesController : MonoBehaviour
{
    #region Instance
    public static LoadingInBetweenScenesController Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<LoadingInBetweenScenesController>();

			return _instance;
		}
	}
	private static LoadingInBetweenScenesController _instance;
    #endregion

    [SerializeField] bool IsOnline;

    void Start() 
    {
        if (IsOnline)
            if (GamePlayManager.Instance)
                GamePlayManager.Instance.OnSceneLoaded += GamePlayManager_OnSceneLoaded;
    }

    private void GamePlayManager_OnSceneLoaded(object sender, EventArgs e)
    {
        Hide();

        Debug.Log("LoadingInBetweenScenesController Hide();");
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
