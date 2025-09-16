using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;
public class GameManager : MonoBehaviour
{
    #region  Instance
    public static GameManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<GameManager>();

			return _instance;
		}
	}
    private static GameManager _instance;
    #endregion
    
    public Transform Tutorial_1;
    public Transform Tutorial_2;
    public Transform Tutorial_3;

    public GameObject Dots_1;
    public GameObject Dots_2;
    public GameObject Dots_3;

    public int Counter = 1;

    public void OnNextClicked()
    {
        switch(Counter)
        {
            case 1:
            {
                PlaySlidingAnimaion(Tutorial_1 , Tutorial_2, -390, 0);

                Dots_1.SetActive(false);
                Dots_2.SetActive(true);
            }
            break;

            case 2:
            {
                PlaySlidingAnimaion(Tutorial_2 , Tutorial_3, -390 , 0);

                Dots_2.SetActive(false);
                Dots_3.SetActive(true);
            }
            break;

            case 3:
            {
                
            }
            break;

            case 4:
            {

            }
            break;
        }

        Counter++;
    }

    public void PlaySlidingAnimaion(Transform _t1, Transform _t2 , float _x1, float _x2)
    {
        _t1.DOLocalMoveX(_x1 , .7f).SetEase(Ease.InOutCubic);
        _t2.DOLocalMoveX(_x2 , .7f).SetEase(Ease.InOutCubic);
    }

    // public ProgressBar bar;



    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Application.runInBackground = true;

        // PlayerPrefManager.DeleteAllPlayerPrefsData();

        // DontDestroyOnLoad(this);
    }
}
