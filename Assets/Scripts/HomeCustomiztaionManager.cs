using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeCustomiztaionManager : MonoBehaviour
{
    public static HomeCustomiztaionManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<HomeCustomiztaionManager>();

			return _instance;
		}
	}
	
	private static HomeCustomiztaionManager _instance;

    [Header("Item Lists")]
    public List<GameObject> BeardItems;
    public List<GameObject> ShirtItems;
    public List<GameObject> PantsItems;
    public List<GameObject> HeadAccessoriesItems;
    public List<GameObject> EyeAccessoriesItems;
    public List<GameObject> ShoulderAccessoriesItems;
    public List<GameObject> BodyAccessoriesItems;
    public List<GameObject> ShoesItems;
}
