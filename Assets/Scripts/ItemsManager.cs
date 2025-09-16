using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

using Unity.Services.Authentication;

using Spine.Unity.Examples;

using RTLTMPro;
using DA_Assets.Shared.CodeHelpers;

public class ItemsManager : MonoBehaviour
{
    #region Instance
	// ----------------------Instance Section---------------------- //
    public static ItemsManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<ItemsManager>();

			return _instance;
		}
	}
	private static ItemsManager _instance;
    // ----------------------Instance Section---------------------- //
#endregion

    public List<SingleItemButton> AllItems;

    // public List<SingleItemButton> ConflictItems = new List<SingleItemButton>();

    [Header("Items Catagories"), Space]
    public List<SingleItemButton> Catagory_Hair = new List<SingleItemButton>();
    public List<SingleItemButton> Catagory_HeadAccessories = new List<SingleItemButton>();
    public List<SingleItemButton> Catagory_Shirt = new List<SingleItemButton>();
    public List<SingleItemButton> Catagory_Pants = new List<SingleItemButton>();

    [Header("Default Items")]
    public SingleItemButton DefaultFemalePants;
    public SingleItemButton DefaultMalePants;

    [Header("Player Saved Data"), Space]
    public List<ItemData> OwnedItems = new List<ItemData>();
    public List<SingleItemButton> EquippedItems = new List<SingleItemButton>();

    [Header("Items Related Properties"), Space]
    public Image SelectedImage;

    public string Equip_String = "ارتداء";
    public string Equipped_String = "عدم الارتداء";
    // public string UnEquip_String = "عدم الارتداء";

    [Header("Popup Properties")]
    public GameObject PopupTent;
    public GameObject PopupContainer;
    public Button ContinueButton;
    public float Duration = 0.5f;
    public List<AudioClip> ShowPopupAudioClips;
    public AudioClip HidePopupAudioClip;

    void Awake() {
        // OwnedItems = PlayerDataManager.Instance.playerData.OwnedItems;
        // OwnedItems = HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems;
    }

    void Start() 
    {
        ContinueButton.onClick.AddListener(() => 
        {
            CloseInsufficientFundPopup();
        });

        // PlayerData tempPlayerData = new PlayerData();
        // tempPlayerData = await PlayerDataManager.Instance.RetrieveSpecificData<PlayerData>(PlayerDataManager.Instance.PlayerData);

        // if (tempPlayerData != null)
        // {
        //     PlayerDataManager.Instance.playerData = tempPlayerData;
        //     OwnedItems = PlayerDataManager.Instance.GetOwnedItemsList();
        //     Debug.Log("Retrieved Data Successfully!");
        // }
        // else
        //     Debug.Log("Nothing to retrieve!");

        // foreach (var v in OwnedItems)
        // {
        //     Debug.Log("ItemsManager - " + v.ItemID + " / " + "IsPurchased = " + v.IsPurchased + " / " + "IsEquipped = " + v.IsEquipped);
        // }

        // Invoke(nameof(SetUpItems), 0.1f);       
    }

    public void SetUpItems()
    {
        if (OwnedItems.Count == 0)
        {
            for (int i = 0; i < AllItems.Count; i++)
            {
                if (AllItems[i].itemData.IsDefaultItem)
                {
                    Debug.Log("Default Item Added!");

                    AllItems[i].itemData.IsEquipped = true;

                    OwnedItems.Add(AllItems[i].itemData);
                }
            }

            // foreach (var v in OwnedItems)
            // {
            //     Debug.Log("SetUpItems - " + v.ItemID + " / " + "ItemType - " + v.itemTypes);
            // }

            StartAssigningAllItemsPhase2();
        }
        else
        {
            StartAssigningAllItemsPhase2();
        }

        
    }

    public void StartAssigningAllItemsPhase2()
    {
        Debug.Log("StartAssigningAllItemsPhase2");

        for (int i = 0; i < AllItems.Count; i++)
        {
            for (int j = 0; j < OwnedItems.Count; j++)
            {
                if (OwnedItems[j].ItemID == AllItems[i].itemData.ItemID)
                {
                    // Debug.Log($"OwnedItems[{j}].ItemID = " + OwnedItems[j].ItemID + " / " + $"AllItems[{i}].itemData.ItemID = " + AllItems[i].itemData.ItemID);

                    if (!AllItems[i].itemData.IsDefaultItem)
                    {
                        AllItems[i].ShowItemEquip_Layout();
                    }
                    
                    AllItems[i].itemData.IsPurchased = true;

                    // Debug.Log($"AllItems[{i}].itemData.IsPurchased = " + AllItems[i].itemData.IsPurchased);

                    if (OwnedItems[j].IsEquipped)
                    {
                        // if (!AllItems[i].itemData.IsDefaultItem)
                        // {
                            // Debug.Log($"AllItems[{i}].EquipButton_Text.text = " + Equipped_String);

                            SetSelectedItemImageParent(AllItems[i].transform);
                            AllItems[i].EquipButton_Text.text = Equipped_String;
                            AllItems[i].itemData.IsEquipped = true;
                            // AllItems[i].skinsSystem.Equip(AllItems[i].itemData.ItemIndex, AllItems[i].itemData.itemTypes, AllItems[i].itemData.IsSpecialItem, false);
                            EquippedItems.Add(AllItems[i]);
                        // }
                    }
                    else
                    {
                        if (!AllItems[i].itemData.IsDefaultItem)
                            AllItems[i].EquipButton_Text.text = Equip_String;
                    }

                    // Debug.Log($"{AllItems[i].itemData.ItemID} is OWNED!");
                    break;
                }
            }

            // yield return new WaitForSeconds(0.1f);
        }
    }

    public void SetToDefaultItem(ItemTypes _itemType, MainGenders _mainGenders, int _itemIdex, bool _stopEquipping = true)
    {
        GetDefaultItem(_itemType, _mainGenders, _itemIdex).EquipItem(_stopEquipping);
    }

    public SingleItemButton GetDefaultItem(ItemTypes _itemType, MainGenders _mainGenders, int _itemIdex)
    {
        for (int i = 0; i < AllItems.Count; i++)
        {
            if (AllItems[i].itemData.itemTypes == _itemType && AllItems[i].skinsSystem.mainGenders == _mainGenders && AllItems[i].itemData.ItemIndex == _itemIdex)
            {
                Debug.Log("DefaultItem = " + AllItems[i].itemData.ItemID);
                return AllItems[i];
            }
        }

        return null;
    }

    public void ReplaceItemInEquippedList(SingleItemButton _oldItem, SingleItemButton _newItem)
    {
        int index = EquippedItems.FindIndex(item => item == _oldItem);

        if (index != -1)
        {
            EquippedItems[index] = _newItem;
            Debug.Log(_oldItem.ItemIndex + " replaced with " + _newItem.ItemIndex);
        }
        else
        {
            Debug.Log(_oldItem + " not found in the list.");
        }
    }

    public void SetSelectedItemImageParent(Transform _parentTransform)
    {
        SelectedImage.transform.SetParent(_parentTransform);
        SelectedImage.transform.SetSiblingIndex(0);
        SelectedImage.transform.localPosition = Vector3.zero;
        // SelectedImage.transform.localScale = Vector3.one;
        
        ShowSelectedImage();
    }
    public void ShowSelectedImage()
    {
        SelectedImage.DOFade(0.0f,0);
        SelectedImage.transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f), 0.0f);

        SelectedImage.DOFade(1, 0.7f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo);
        SelectedImage.transform.DOScale(Vector3.one, 0.7f).SetEase(Ease.InOutCubic).SetLoops(-1, LoopType.Yoyo);
    }

    public void ShowInsufficientFundPopup()
    {
        PopupTent.SetActive(true);
        AudioManager.Instance.PlayRandomAudioFromPool(ShowPopupAudioClips);
        PopupContainer.transform.DOScale(Vector3.one, Duration).SetEase(Ease.OutBounce, 2);
    }
    public void CloseInsufficientFundPopup()
    {
        AudioManager.Instance.PlayAudioClip(HidePopupAudioClip);
        PopupContainer.transform.DOScale(Vector2.zero, Duration).SetEase(Ease.InBounce, 2).OnComplete(() =>
        {
            PopupTent.gameObject.SetActive(false);
        });
    }
}
