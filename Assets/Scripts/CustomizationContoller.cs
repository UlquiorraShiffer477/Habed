using System.Collections;
using System.Collections.Generic;
using Spine.Unity.AttachmentTools;
using UnityEngine;

using Spine;

namespace Spine.Unity.Examples {
	public enum ItemTypes
    {
		Hair,
        LowerFace,
        Shirt,
        Pants,
        HeadAccessories,
        EyeAccessories,
        ShoulderAccessories,
        BodyAccessories,
        Shoes,
		Nose,
		None,
    }

	public enum MainGenders
	{
		Male,
		Female
	}

public class CustomizationContoller : MonoBehaviour
{
	[Header("General Info")]
	public MainGenders mainGenders;

	[Header("Hair prosperities")]
	[SpineSkin] public string [] Hair = {};
	public int activeHairIndex = 0;
	[SerializeField] ParticleSystem Hair_PS;

	[Header("LowerFace prosperities")]
	[SpineSkin] public string [] LowerFace = {};
	public int activeLowerFaceIndex = 0;
	[SerializeField] ParticleSystem LowerFace_PS;

	[Header("Shirt prosperities")]
	[SpineSkin] public string [] Shirt = {};
	public int activeShirtIndex = 0;
	[SerializeField] ParticleSystem Shirt_PS;

	[Header("Pants prosperities")]
	[SpineSkin] public string [] Pants = {};
	public int activePantsIndex = 0;
	[SerializeField] ParticleSystem Pants_PS;

	[Header("HeadAccessories prosperities")]
	[SpineSkin] public string [] HeadAccessories = {};
	public int activeHeadAccessoriesIndex = 0;
	[SerializeField] ParticleSystem HeadAccessories_PS;

	[Header("EyeAccessories prosperities")]
	[SpineSkin] public string [] EyeAccessories = {};
	public int activeEyeAccessoriesIndex = 0;
	[SerializeField] ParticleSystem EyeAccessories_PS;

	[Header("ShoulderAccessories prosperities")]
	[SpineSkin] public string [] ShoulderAccessories = {};
	public int activeShoulderAccessoriesIndex = 0;
	[SerializeField] ParticleSystem ShoulderAccessories_PS;

	[Header("BodyAccessories prosperities")]
	[SpineSkin] public string [] BodyAccessories = {};
	public int activeBodyAccessoriesIndex = 0;
	[SerializeField] ParticleSystem BodyAccessories_PS;

	[Header("Shoes prosperities")]
	[SpineSkin] public string [] Shoes = {};
	public int activeShoesIndex = 0;
	[SerializeField] ParticleSystem Shoes_PS;

	[Header("Nose")]
	[SpineSkin] public string [] Nose = {};
	public int activeNoseIndex = 0;
	[SerializeField] ParticleSystem Nose_PS;


	// public enum ItemTypes
    // {
	// 	Hair,
    //     LowerFace,
    //     Shirt,
    //     Pants,
    //     HeadAccessories,
    //     EyeAccessories,
    //     ShoulderAccessories,
    //     BodyAccessories,
    //     Shoes,
	// 	Nose
    // }

	// equipment skins
	public enum ItemType {
		Cloth,
		Pants,
		Bag,
		Hat
	}

	[SerializeField] SkeletonGraphic skeletonAnimation;
	// This "naked body" skin will likely change only once upon character creation,
	// so we store this combined set of non-equipment Skins for later re-use.
	Skin characterSkin;

	// for repacking the skin to a new atlas texture
	public Material runtimeMaterial;
	public Texture2D runtimeAtlas;

	[SpineAnimation]
	public List<string> EquipAnim;
	[SpineAnimation]
	public string SelectedAnim;
	[SpineAnimation]
	public string IdleAnim;

	//PlayerInfo
	public PlayerInfo playerInfo;

	public bool IsMan;

	public global :: Gender gender = global :: Gender.NotSelected;


	void Awake () 
	{
		skeletonAnimation = this.GetComponent<SkeletonGraphic>();

		playerInfo = new PlayerInfo
			{
				HairIndex 					= 1,
				LowerFaceIndex 				= 0,
				ShirtIndex 					= 1,
				PantsIndex 					= 1,
				HeadAccessoriesIndex 		= 0,
				EyeAccessoriesIndex 		= 0,
				ShoulderAccessoriesIndex 	= 0,
				BodyAccessoriesIndex 		= 0,
				ShoesIndex 					= 0,
				NoseIndex 					= 0,

				MaleItems 					= new PlayerItems
				{
                    HairIndex_PlayerItems 						= 1,
                    LowerFaceIndex_PlayerItems 					= 0,
                    ShirtIndex_PlayerItems  					= 1,
                    PantsIndex_PlayerItems  					= 1,
                    HeadAccessoriesIndex_PlayerItems  			= 0,
                    EyeAccessoriesIndex_PlayerItems  			= 0,
                    ShoulderAccessoriesIndex_PlayerItems  		= 0,
                    BodyAccessoriesIndex_PlayerItems  			= 0,
                    ShoesIndex_PlayerItems  					= 0,
                    NoseIndex_PlayerItems  						= 0,
                },

				FemaleItems = new PlayerItems
				{
                    HairIndex_PlayerItems 						= 1,
                    LowerFaceIndex_PlayerItems 					= 0,
                    ShirtIndex_PlayerItems  					= 1,
                    PantsIndex_PlayerItems  					= 1,
                    HeadAccessoriesIndex_PlayerItems  			= 0,
                    EyeAccessoriesIndex_PlayerItems  			= 0,
                    ShoulderAccessoriesIndex_PlayerItems  		= 0,
                    BodyAccessoriesIndex_PlayerItems  			= 0,
                    ShoesIndex_PlayerItems  					= 0,
                    NoseIndex_PlayerItems  						= 0,
                }
			};
	}

	void OnEnable() 
	{
		if (PlayerDataManager.Instance.playerData.playerInfo.gender != Gender.NotSelected)
		{
			SetAllCharacterSkins(PlayerDataManager.Instance.playerData.playerInfo);

			// Debug.Log("Previous Customization Found");
		}
			
		else
		{
			SetAllCharacterSkins(
				playerInfo
			);

			Debug.Log("NO Previous Customization Found", this.gameObject);
		}
	}

	// void Start () 
	// {
	// 	SetAllCharacterSkins(PlayerDataManager.Instance.playerData.playerInfo);

	// 	UpdateCharacterSkin();
	// 	UpdateCombinedSkin();
	// }

	public void Equip (int _itemIndex, ItemTypes _itemType, bool _isSpecialCase, bool _playVFX = true, bool _playSFX = true, SingleItemButton _singleItemButton = null, bool _stopEquipping = false) 
	{
		// Debug.Log("_stopEquipping = " + _stopEquipping, this.gameObject);

		switch(_itemType)
		{
			//------------------------------------------------
			
			case ItemTypes.Hair:
			if(Hair_PS != null && _playVFX)
				Hair_PS.Play();

			if (!_stopEquipping)
			{
				for (int i = 0; i < ItemsManager.Instance.Catagory_HeadAccessories.Count; i++)
				{
					if (ItemsManager.Instance.Catagory_HeadAccessories[i].skinsSystem.mainGenders == mainGenders)
					{
						// Debug.Log(ItemsManager.Instance.Catagory_HeadAccessories[i].skinsSystem.mainGenders + " = " + mainGenders);

						if (ItemsManager.Instance.Catagory_HeadAccessories[i].skinsSystem.gender == Gender.Female)
						{
							// Debug.Log("Female");

							if (_singleItemButton != null)
							{
								// Debug.Log("Female1");
								if (_singleItemButton.itemData.IsSpecialItem)
								{
									// Debug.Log("Female2");
									if (ItemsManager.Instance.Catagory_HeadAccessories[i].itemData.IsEquipped)
									{
										// Debug.Log("Female3");
										ItemsManager.Instance.Catagory_HeadAccessories[i].UnEquipItem(true, true);
										Debug.Log("Found A Special Item! Female Hair Pressed", this.gameObject);
										break;
									}
								}
							}
						}
						else
						{
							// Debug.Log("Male");

							if (ItemsManager.Instance.Catagory_HeadAccessories[i].itemData.IsSpecialItem)
							{
								// Debug.Log("Male1");
								if (ItemsManager.Instance.Catagory_HeadAccessories[i].itemData.IsEquipped)
								{
									// Debug.Log("Male2");
									ItemsManager.Instance.Catagory_HeadAccessories[i].UnEquipItem(false, true);
									Debug.Log("Found A Special Item! Male Hair Pressed", this.gameObject);
									break;
								}
							}
						}
					}
				}
			}


			activeHairIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.LowerFace:
			if(LowerFace_PS != null && _playVFX)
				LowerFace_PS.Play();
			activeLowerFaceIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.Shirt:
			if(Shirt_PS != null && _playVFX)
				Shirt_PS.Play();

			if (!_stopEquipping)
			{
				for (int i = 0; i < ItemsManager.Instance.Catagory_Pants.Count; i++)
				{
					if (ItemsManager.Instance.Catagory_Pants[i].skinsSystem.mainGenders == mainGenders)
					{
						if (_singleItemButton != null)
						{
							if (_singleItemButton.itemData.IsSpecialItem)
							{
								if (_singleItemButton.itemData.IsEquipped)
								{
									if (gender == Gender.Male)
									{
										// ItemsManager.Instance.Catagory_Pants[i].UnEquipItem();
										HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.MaleSpecialShirtEquipped = true;
										Debug.Log("Found A Special Item!", this.gameObject);
										activePantsIndex = 0;
										break;
									}
									else if (gender == Gender.Female)
									{
										// ItemsManager.Instance.Catagory_Pants[i].UnEquipItem();
										HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.FemaleSpecialShirtEquipped = true;
										Debug.Log("Found A Special Item!", this.gameObject);
										activePantsIndex = 0;
										break;
									}
								}
							}

							else
							{
								if (gender == Gender.Male)
								{	
									Debug.Log("MaleSpecialShirtEquipped = false", this.gameObject);
									if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.MaleSpecialShirtEquipped)
									{
										HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.MaleSpecialShirtEquipped = false;
										ItemsManager.Instance.DefaultMalePants.EquipItem(true);
									}
								}
								else if (gender == Gender.Female)
								{
									Debug.Log("FemaleSpecialShirtEquipped = false", this.gameObject);
									if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.FemaleSpecialShirtEquipped)
									{
										HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.FemaleSpecialShirtEquipped = false;
										ItemsManager.Instance.DefaultFemalePants.EquipItem(true);
									}
								}
							}
						}
					}
				}
			}

			

			

			activeShirtIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.Pants:
			if(Pants_PS != null && _playVFX)
				Pants_PS.Play();

			if (!_stopEquipping)
			{
				for (int i = 0; i < ItemsManager.Instance.Catagory_Shirt.Count; i++)
				{
					if (ItemsManager.Instance.Catagory_Shirt[i].skinsSystem.mainGenders== mainGenders)
					{
						if (ItemsManager.Instance.Catagory_Shirt[i].itemData.IsSpecialItem)
						{
							if (ItemsManager.Instance.Catagory_Shirt[i].itemData.IsEquipped)
							{
								Debug.Log("Found A Special Item!", this.gameObject);
								ItemsManager.Instance.Catagory_Shirt[i].UnEquipItem(true, false);
								break;
							}
						}
					}
				}
			}

			activePantsIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.HeadAccessories:
			if(HeadAccessories_PS != null && _playVFX)
				HeadAccessories_PS.Play();

			if (!_stopEquipping)
			{
				for (int i = 0; i < ItemsManager.Instance.Catagory_Hair.Count; i++)
				{
					if (ItemsManager.Instance.Catagory_Hair[i].skinsSystem.mainGenders == mainGenders)
					{
						if (ItemsManager.Instance.Catagory_Hair[i].skinsSystem.gender == Gender.Female)
						{
							if (ItemsManager.Instance.Catagory_Hair[i].itemData.IsSpecialItem)
							{
								if (ItemsManager.Instance.Catagory_Hair[i].itemData.IsEquipped)
								{
									ItemsManager.Instance.Catagory_Hair[i].UnEquipItem(true, true);
									// activeHairIndex = 0;
									Debug.Log("Found A Special Item! Female Head HeadAccessories Pressed", this.gameObject);
									break;
								}
								// else
								// {
								// 	if (activeHairIndex == 0)
								// 		activeHairIndex = 1;
								// }
							}
						}
						else
						{
							if (_singleItemButton != null)
							{
								if (_singleItemButton.itemData.IsSpecialItem)
								{
									if (ItemsManager.Instance.Catagory_Hair[i].itemData.IsEquipped)
									{
										ItemsManager.Instance.Catagory_Hair[i].UnEquipItem(false, true);
										// activeHairIndex = 0;
										Debug.Log("Found A Special Item! Male Head HeadAccessories Pressed", this.gameObject);
										break;
									}
								}
							}
						}
					}
				}
			}

			activeHeadAccessoriesIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.EyeAccessories:
			if(EyeAccessories_PS != null && _playVFX)
				EyeAccessories_PS.Play();
			activeEyeAccessoriesIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.ShoulderAccessories:
			if(ShoulderAccessories_PS != null && _playVFX)
				ShoulderAccessories_PS.Play();
			activeShoulderAccessoriesIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.BodyAccessories:
			if(BodyAccessories_PS != null && _playVFX)
				BodyAccessories_PS.Play();
			activeBodyAccessoriesIndex = _itemIndex;
			break;

			//------------------------------------------------

			case ItemTypes.Shoes:
			if(Shoes_PS != null && _playVFX)
				Shoes_PS.Play();
			activeShoesIndex = _itemIndex;
			break;
			
			//------------------------------------------------

			case ItemTypes.Nose:
			if(Nose_PS != null && _playVFX)
				Nose_PS.Play();
			activeNoseIndex = _itemIndex;
			break;
		}

		UpdateCharacterSkin();
		UpdateCombinedSkin();

		if (_playSFX)
			AudioManager.Instance.PlayAudioClip(AudioManager.Instance.EquipSFX);
	}

	public void SetAllCharacterSkins(PlayerInfo _playerInfo)
	{
		playerInfo = _playerInfo;
		
		if (mainGenders == MainGenders.Male)
		{
			// Debug.Log("SetSkins Male", this.gameObject);
			SetSkins(_playerInfo.MaleItems, _playerInfo);
		}
		else if (mainGenders == MainGenders.Female)
		{
			// Debug.Log("SetSkins Female", this.gameObject);
			SetSkins(_playerInfo.FemaleItems, _playerInfo);
		}
		
		else
			SetDefaultSkins(_playerInfo);

		playerInfo.gender = _playerInfo.gender;

		UpdateCharacterSkin();
		UpdateCombinedSkin();
	}

	public void SetDefaultSkins(PlayerInfo _playerInfo)
	{
		// Debug.Log("Set Default Items", this.gameObject);
		if (_playerInfo.HairIndex == 0)
			activeHairIndex = 								1;
		else
			activeHairIndex = 								_playerInfo.HairIndex;
		//-----------------------------------------------------------
		activeLowerFaceIndex = 								_playerInfo.LowerFaceIndex;
		//-----------------------------------------------------------
		if (_playerInfo.ShirtIndex == 0)
			activeShirtIndex = 								1;
		else
			activeShirtIndex = 								_playerInfo.ShirtIndex;
		//-----------------------------------------------------------
		if (_playerInfo.PantsIndex == 0)
			activePantsIndex = 								1;
		else
			activePantsIndex = 								_playerInfo.PantsIndex;
		//-----------------------------------------------------------
		activeHeadAccessoriesIndex = 						_playerInfo.HeadAccessoriesIndex;
		activeEyeAccessoriesIndex = 						_playerInfo.EyeAccessoriesIndex;
		activeShoulderAccessoriesIndex = 					_playerInfo.ShoulderAccessoriesIndex;
		activeBodyAccessoriesIndex = 						_playerInfo.BodyAccessoriesIndex;
		activeShoesIndex = 									_playerInfo.ShoesIndex;
		activeNoseIndex = 									_playerInfo.NoseIndex;
	}

	public void SetSkins(PlayerItems _playerItems, PlayerInfo _playerInfo)
	{
		// Debug.Log("SetSkins Items");

		// if (_playerItems.HairIndex_PlayerItems == 0)
		// 	activeHairIndex = 								1;
		// else
			activeHairIndex = 								_playerItems.HairIndex_PlayerItems;
			// Debug.LogError("_playerItems.HairIndex_PlayerItems = " + _playerItems.HairIndex_PlayerItems);
		//-----------------------------------------------------------
		activeLowerFaceIndex = 								_playerItems.LowerFaceIndex_PlayerItems;
		//-----------------------------------------------------------
		if (_playerItems.ShirtIndex_PlayerItems == 0)
			activeShirtIndex = 								1;
		else
			activeShirtIndex = 								_playerItems.ShirtIndex_PlayerItems;
		//-----------------------------------------------------------
		if (_playerItems.PantsIndex_PlayerItems == 0)
			activePantsIndex = 								1;
		else
		{
			bool specialItemEquipped = false; 

			if (_playerInfo.MaleSpecialShirtEquipped && gender != Gender.Female)
			{
				
				// Debug.Log("This Piece Of **** Male", this.gameObject);
				activePantsIndex = 0;
				specialItemEquipped = true;
				
			}
			else 
				if (_playerInfo.FemaleSpecialShirtEquipped && gender != Gender.Male)
				{
					// Debug.Log("This Piece Of **** Female", this.gameObject);
					activePantsIndex = 0;
					specialItemEquipped = true;
				}
				else			
					if (!specialItemEquipped)
						activePantsIndex = 								_playerItems.PantsIndex_PlayerItems;
		}
			
		//-----------------------------------------------------------
		activeHeadAccessoriesIndex = 						_playerItems.HeadAccessoriesIndex_PlayerItems;
		activeEyeAccessoriesIndex = 						_playerItems.EyeAccessoriesIndex_PlayerItems;
		activeShoulderAccessoriesIndex = 					_playerItems.ShoulderAccessoriesIndex_PlayerItems;
		activeBodyAccessoriesIndex = 						_playerItems.BodyAccessoriesIndex_PlayerItems;
		activeShoesIndex = 									_playerItems.ShoesIndex_PlayerItems;
		activeNoseIndex = 									_playerItems.NoseIndex_PlayerItems;
	}

	// public void SetCharacterSkins(
	// 	int _activeHairIndex,
	// 	int _activeLowerFaceIndex,
	// 	int _activeShirtIndex,
	// 	int _activePantsIndex,
	// 	int _activeHeadAccessoriesIndex,
	// 	int _activeEyeAccessoriesIndex,
	// 	int _activeShoulderAccessoriesIndex,
	// 	int _activeBodyAccessoriesIndex,
	// 	int _activeShoesIndex
	// )
	// {
	// 	activeHairIndex = _activeHairIndex;
	// 	activeLowerFaceIndex = _activeLowerFaceIndex;
	// 	activeShirtIndex = _activeShirtIndex;
	// 	activePantsIndex = _activePantsIndex;
	// 	activeHeadAccessoriesIndex = _activeHeadAccessoriesIndex;
	// 	activeEyeAccessoriesIndex = _activeEyeAccessoriesIndex;
	// 	activeShoulderAccessoriesIndex = _activeShoulderAccessoriesIndex;
	// 	activeBodyAccessoriesIndex = _activeBodyAccessoriesIndex;
	// 	activeShoesIndex = _activeShoesIndex;

	// 	UpdateCharacterSkin();
	// 	UpdateCombinedSkin();
	// }
	

	public void OptimizeSkin () {
		// Create a repacked skin.
		var previousSkin = skeletonAnimation.Skeleton.Skin;
		// Note: materials and textures returned by GetRepackedSkin() behave like 'new Texture2D()' and need to be destroyed
		if (runtimeMaterial)
			Destroy(runtimeMaterial);
		if (runtimeAtlas)
			Destroy(runtimeAtlas);
		Skin repackedSkin = previousSkin.GetRepackedSkin("Repacked skin", skeletonAnimation.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial, out runtimeMaterial, out runtimeAtlas);
		previousSkin.Clear();
		// Use the repacked skin.
		skeletonAnimation.Skeleton.Skin = repackedSkin;
		skeletonAnimation.Skeleton.SetSlotsToSetupPose();
		skeletonAnimation.AnimationState.Apply(skeletonAnimation.Skeleton);
		// `GetRepackedSkin()` and each call to `GetRemappedClone()` with parameter `premultiplyAlpha` set to `true`
		// cache necessarily created Texture copies which can be cleared by calling AtlasUtilities.ClearCache().
		// You can optionally clear the textures cache after multiple repack operations.
		// Just be aware that while this cleanup frees up memory, it is also a costly operation
		// and will likely cause a spike in the framerate.
		AtlasUtilities.ClearCache();
		Resources.UnloadUnusedAssets();
	}

	void UpdateCharacterSkin () 
	{
		var skeleton = skeletonAnimation.Skeleton;
		var skeletonData = skeleton.Data;
		characterSkin = new Skin("character-base");
		// Note that the result Skin returned by calls to skeletonData.FindSkin()
		// could be cached once in Start() instead of searching for the same skin
		// every time. For demonstration purposes we keep it simple here.
		
		// Debug.Log($"Hair[{activeHairIndex}] = {Hair[activeHairIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(Hair[activeHairIndex]));
		

		// Debug.Log($"LowerFace[{activeLowerFaceIndex}] = {LowerFace[activeLowerFaceIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(LowerFace[activeLowerFaceIndex]));


		// Debug.Log($"Shirt[{activeShirtIndex}] = {Shirt[activeShirtIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(Shirt[activeShirtIndex]));


		// Debug.Log($"Pants[{activePantsIndex}] = {Pants[activePantsIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(Pants[activePantsIndex]));


		// Debug.Log($"HeadAccessories[{activeHeadAccessoriesIndex}] = {HeadAccessories[activeHeadAccessoriesIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(HeadAccessories[activeHeadAccessoriesIndex]));


		// Debug.Log($"EyeAccessories[{activeEyeAccessoriesIndex}] = {EyeAccessories[activeEyeAccessoriesIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(EyeAccessories[activeEyeAccessoriesIndex]));


		// Debug.Log($"ShoulderAccessories[{activeShoulderAccessoriesIndex}] = {ShoulderAccessories[activeShoulderAccessoriesIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(ShoulderAccessories[activeShoulderAccessoriesIndex]));


		// Debug.Log($"BodyAccessories[{activeBodyAccessoriesIndex}] = {BodyAccessories[activeBodyAccessoriesIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(BodyAccessories[activeBodyAccessoriesIndex]));


		// Debug.Log($"Shoes[{activeShoesIndex}] = {Shoes[activeShoesIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(Shoes[activeShoesIndex]));


		// Debug.Log($"Nose[{activeNoseIndex}] = {Nose[activeNoseIndex]}");
		characterSkin.AddSkin(skeletonData.FindSkin(Nose[activeNoseIndex]));

	}

	void CheckSpecialCase(bool _isSpecialCase)
	{
		if (_isSpecialCase)
		{
			activePantsIndex = 0;
		}
	}

	public void ClearItems(ItemTypes itemTypes)
	{
		// switch (itemTypes)
		// {
		// 	case ItemTypes.Hair:
		// 	activeHeadAccessoriesIndex = 0;
		// 	break;

		// 	case ItemTypes.Shirt:
		// 	activePantsIndex = 0;
		// 	break;
		// }
	}
	
	void UpdateCombinedSkin () 
	{
		var skeleton = skeletonAnimation.Skeleton;
		var resultCombinedSkin = new Skin("character-combined");
		resultCombinedSkin.AddSkin(characterSkin);
		// AddEquipmentSkinsTo(resultCombinedSkin);
		skeleton.SetSkin(resultCombinedSkin);
		skeleton.SetSlotsToSetupPose();
	}

	// Callback Function (Attach It To Buttons)
	public void UpdateAnimation(string _animationName = "")
	{
		// Debug.Log("Animation Updated");
		// skeletonAnimation.startingAnimation = _animationName;

		var state = skeletonAnimation.AnimationState;
		
		// skeletonAnimation.AnimationState.SetAnimation(0, GetRandomAnimationFromList(EquipAnim), true);

		state.SetAnimation(0, IdleAnim, true);
		state.SetEmptyAnimation(1, 0);
		state.AddAnimation(1, GetRandomAnimationFromList(EquipAnim), false, 0).MixDuration = 0.4f;

		var duration = state.GetCurrent(1).AnimationTime;

		// Debug.Log($"duration = {duration}");

		state.AddEmptyAnimation(1, 0.4f, duration);

		// Debug.Log($"duration = {duration}");

		// skeletonAnimation.Skeleton.SetSlotsToSetupPose();
		// skeletonAnimation.LateUpdate();
	}

	public void UpdateSelectedAnimation(string _firstAnimationName, string _secondAnimationName, float _delay = 0f, float _mixDuration = 0.4f)
	{
		// Debug.Log("Animation Updated");
		
		var state = skeletonAnimation.AnimationState;

		state.SetAnimation(0, _firstAnimationName, true);
		state.SetEmptyAnimation(1, 0);
		state.AddAnimation(1, _secondAnimationName, false, _delay).MixDuration = _mixDuration;

		var duration = state.GetCurrent(1).AnimationTime;

		// Debug.Log($"duration = {duration}");

		state.AddEmptyAnimation(1, _mixDuration, duration);

		// Debug.Log($"duration = {duration}");
	}

	public string GetRandomAnimationFromList(List<string> _animations)
	{
		return _animations[Random.Range(0, _animations.Count)];
	}
}
}
