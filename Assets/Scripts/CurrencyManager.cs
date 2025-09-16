using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
#region Instance
    public static CurrencyManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<CurrencyManager>();

			return _instance;
		}
	}
	
	private static CurrencyManager _instance;
#endregion

    public int CurrentPlayerCoinsAmount;
    
    
    void Start() 
    {
        
    }


    public void AddCoins(int _coinsAmount)
    {

    }

    public void SubstractCoins(int _coinsAmount)
    {
        
    }

    public void UpdateCoinsText()
    {

    }

    public void UpdatePlayerPrefs()
    {

    }
}
