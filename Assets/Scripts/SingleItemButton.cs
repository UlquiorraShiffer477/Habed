using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
using TMPro;

using Unity.Services.Authentication;

namespace Spine.Unity.Examples
{
    // public enum ItemState
    // {
    //     notPurchased,
    //     owned,
    //     equipped
    // }

    public class SingleItemButton : MonoBehaviour
    {
        [SerializeField] bool IsSpecialItem;
        [SerializeField] bool DoClearAllAccessories;

        public int ItemIndex;
        public CustomizationContoller skinsSystem;
        public ItemTypes itemTypes;

        [Header("Item Properites")]
        public GameObject ItemCost_Button;
        public GameObject ItemEquip_Button;

        public TextMeshProUGUI Cost_Text;
        public RTLTextMeshPro EquipButton_Text;

        public ItemData itemData = new ItemData();


        void Awake()
        {
            itemData.ItemID = skinsSystem.mainGenders + "_" + itemData.itemTypes.ToString() + "_" + itemData.ItemIndex;
            Cost_Text.text = itemData.ItemCost.ToString();

            // Debug.Log("itemData.ItemID = " + itemData.ItemID);
            // ItemIndex = transform.GetSiblingIndex();
        }

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                // if (IsSpecialItem)
                //    skinsSystem.ClearItems(itemTypes);

                // skinsSystem.UpdateAnimation();

                // skinsSystem.Equip(ItemIndex, itemTypes, IsSpecialItem);

                ReviewItem();
            });

            if (ItemCost_Button != null)
                ItemCost_Button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (!(HomeMenuManager.Instance.HomeMenuPlayerData.PlayerCoins >= itemData.ItemCost))
                        ShowInsufficientFundPopup();
                    else
                        PurchaseItem();
                });

            if (ItemEquip_Button != null)
                ItemEquip_Button.GetComponent<Button>().onClick.AddListener(() =>
                {
                    // EquipItem();
                    ToggleEquipUnEquipItem();
                });
        }


        public void ReviewItem()
        {
            // stop current tween...
            // 
            // Assigning new tween...
            ItemsManager.Instance.SetSelectedItemImageParent(this.transform);

            if (itemData.IsDefaultItem)
            {
                if (!itemData.IsEquipped)
                    EquipItem();
            }
            else
            {
                skinsSystem.Equip(itemData.ItemIndex, itemTypes, IsSpecialItem);
                skinsSystem.UpdateAnimation();
            }
        }


        public void PurchaseItem()
        {
            int finalCoinCounte = HomeMenuManager.Instance.HomeMenuPlayerData.PlayerCoins - itemData.ItemCost;
            Debug.Log("Player Coins After Purchasing = " + finalCoinCounte);

            StartCoroutine(ShopManager.Instance.TweenValue(ShopManager.Instance.CurrentCoinsCount, finalCoinCounte, 2, 0, ShopManager.Instance.Coins_Text));
            AudioManager.Instance.PlayAudioClip(ShopManager.Instance.PurchaseAudioClip);

            ShopManager.Instance.UpdateHomeMenuPlayerDataCoins(finalCoinCounte);

            Debug.Log("CurrentCoinsCount = " + ShopManager.Instance.CurrentCoinsCount);

            if (!itemData.IsPurchased)
            {
                SetUpItemDataState(true, false);

                ShowItemEquip_Layout();
                EquipButton_Text.text = ItemsManager.Instance.Equip_String;

                HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems.Add(itemData);

                EquipItem();
            }
            else
            {
                Debug.Log("Item is owned! Can't purchase again!");
            }
        }

        public void ToggleEquipUnEquipItem()
        {
            if (itemData.IsEquipped)
                UnEquipItem();
            else
                EquipItem();
        }

        public void EquipItem(bool _stopEquipping = false)
        {
            // If item already equipped, do nothing...
            // if (itemData.IsEquipped)
            //     return;

            // if pressed on another item with same catagory then unequip the current equipped item
            // else -> if pressed on another item with different catagory, keep the current equipped item + equip the new item.


            // Check if the item is purchase before starting the actions...(I dont think we need this, but just in case)
            if (itemData.IsPurchased)
            {
                // Starting equipping actions...
                SetUp_EquipItem();

                // Prepair the temp variables...
                bool foundExactItem = false;
                bool foundSameItemType = false;
                SingleItemButton oldItem = null;

                // Start checking action, we're checking here if the item is already in the list of the equipped items, if not, we check once again if the itemtype
                // matches any other item's itemtype. If yes, we replace the old one with the new one to store only one item per itemtype in this list.
                // after all of that, if no other item in the list matches this item's itemtype, we just add this item as a new item to the equipped items list...
            #region Checking Operations...
                for (int i = 0; i < ItemsManager.Instance.EquippedItems.Count; i++)
                {
                    if (ItemsManager.Instance.EquippedItems.Contains(this))
                    {
                        foundExactItem = true;
                        Debug.Log("foundExactItem = " + foundExactItem);
                        break;
                    }
                    else
                    {
                        if (ItemsManager.Instance.EquippedItems[i].itemData.itemTypes == itemData.itemTypes 
                            && ItemsManager.Instance.EquippedItems[i].skinsSystem.mainGenders == skinsSystem.mainGenders)
                        {
                            foundSameItemType = true;
                            Debug.Log("foundSameItemType = " + foundSameItemType);
                            oldItem = ItemsManager.Instance.EquippedItems[i];
                            break;
                        }
                    }
                }

                if (!foundExactItem)
                {
                    if (!foundSameItemType)
                    {
                        ItemsManager.Instance.EquippedItems.Add(this);
                        Debug.Log("New equipped item added.");
                    }
                    else
                    {
                        oldItem.SetUp_UnEquipItem();
                        ItemsManager.Instance.ReplaceItemInEquippedList(oldItem, this);
                    }
                }
            #endregion

                // Update the owneditems list and store it to the cloud...
                for (int i = 0; i < HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems.Count; i++)
                {
                    if (oldItem)
                    {
                        if (HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems[i].ItemID == oldItem.itemData.ItemID)
                        {
                            // Same item found...
                            HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems[i] = oldItem.itemData;
                        }
                    }

                    for (int j = 0; j < ItemsManager.Instance.EquippedItems.Count; j++)
                    {
                        if (ItemsManager.Instance.EquippedItems[j].itemData.ItemID == HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems[i].ItemID)
                        {
                            HomeMenuManager.Instance.HomeMenuPlayerData.OwnedItems[i] = ItemsManager.Instance.EquippedItems[j].itemData;
                            Debug.Log("Updated the owneditemslist!");
                        }
                    }
                }

                skinsSystem.Equip(itemData.ItemIndex, itemTypes, IsSpecialItem, true, true, this, _stopEquipping);
                skinsSystem.UpdateAnimation();

                SavePlayerSkinsIndex(IsSpecialItem);
            }
        }


        public void UnEquipItem(bool _setToDefaultItem = true, bool _stopEquipping = false)
        {
            // If item already NOT equipped, do nothing...
            // if (!itemData.IsEquipped)
            //     return;

            int itemIndexTemp = 0;

            for (int i = 0; i < ItemsManager.Instance.EquippedItems.Count; i++)
            {
                /*
                // if (ItemsManager.Instance.EquippedItems[i].itemData.ItemID == itemData.ItemID)
                // {
                    // switch (itemData.itemTypes)
                    // {
                    //     case ItemTypes.Hair:
                    //         skinsSystem.Equip(1, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.Pants:
                    //         skinsSystem.Equip(1, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.Shirt:
                    //         skinsSystem.Equip(1, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.Shoes:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.EyeAccessories:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.BodyAccessories:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.HeadAccessories:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.ShoulderAccessories:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.LowerFace:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;

                    //     // ------------------------------- //

                    //     case ItemTypes.Nose:
                    //         skinsSystem.Equip(0, itemTypes, IsSpecialItem);
                    //         skinsSystem.UpdateAnimation();

                    //         SavePlayerDefaultSkinsIndex(IsSpecialItem);
                    //         break;
                    // }

                    // ItemsManager.Instance.EquippedItems[i].SetUp_UnEquipItem();
                    // ItemsManager.Instance.EquippedItems.Remove(this);
                // }
                */
                
                if (ItemsManager.Instance.EquippedItems[i].itemData.ItemID == itemData.ItemID)
                {
                    if(itemData.itemTypes == ItemTypes.Hair || itemData.itemTypes == ItemTypes.Pants || itemData.itemTypes == ItemTypes.Shirt)
                    {
                        if (_setToDefaultItem)
                        {
                            Debug.Log("UnEquipItem Function / ItemType = Hair||Pants||Shirt , _setToDefaultItem = true", this.gameObject);
                            itemIndexTemp = 1;
                            ItemsManager.Instance.SetToDefaultItem(itemData.itemTypes, skinsSystem.mainGenders, itemIndexTemp, _stopEquipping);
                        }
                        else
                        {
                            Debug.Log("UnEquipItem Function / ItemType = Hair||Pants||Shirt , _setToDefaultItem = false", this.gameObject);
                            ItemsManager.Instance.SetToDefaultItem(itemData.itemTypes, skinsSystem.mainGenders, itemIndexTemp);
                        }
                    }
                    else
                    {
                        if (itemData.itemTypes == ItemTypes.Shoes)
                        {
                            Debug.Log("UnEquipItem Function / ItemType = Shoes", this.gameObject);
                            ItemsManager.Instance.SetToDefaultItem(itemData.itemTypes, skinsSystem.mainGenders, itemIndexTemp);
                        }
                        else
                        {
                            Debug.Log("UnEquipItem Function / ItemType = else " + itemData.itemTypes.ToString(), this.gameObject);
                            skinsSystem.Equip(itemIndexTemp, itemTypes, IsSpecialItem, true, true, this, _stopEquipping);
                            skinsSystem.UpdateAnimation();

                            SavePlayerDefaultSkinsIndex(IsSpecialItem);

                            ItemsManager.Instance.EquippedItems[i].SetUp_UnEquipItem();
                            ItemsManager.Instance.EquippedItems.Remove(this);

                            break;
                        }
                    }
                }
            }
        }




        public void SetUp_EquipItem()
        {
            itemData.IsEquipped = true;
            EquipButton_Text.text = ItemsManager.Instance.Equipped_String;
            ItemsManager.Instance.SetSelectedItemImageParent(this.transform);
        }
        public void SetUp_UnEquipItem()
        {
            itemData.IsEquipped = false;
            EquipButton_Text.text = ItemsManager.Instance.Equip_String;
        }

        public void SetUpItemDataState(bool _isPurchased, bool _isEquipped)
        {
            itemData.IsPurchased = _isPurchased;
            itemData.IsEquipped = _isEquipped;
        }


        public void SavePlayerDefaultSkinsIndex(bool _isSpecialItem = false)
        {
            if (_isSpecialItem)
            {
                PlayerDataManager.Instance.IsSpecialItemEquipped = _isSpecialItem;
            }

            PlayerItems playerItems = CheckGenderThenSaveActiveIndex();

            switch (itemTypes)
            {
                case ItemTypes.Hair:
                    {
                        playerItems.HairIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.HairIndex = 0;
                        break;
                    }
                case ItemTypes.LowerFace:
                    {
                        playerItems.LowerFaceIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.LowerFaceIndex = 0;
                        break;
                    }
                case ItemTypes.Shirt:
                    {
                        playerItems.ShirtIndex_PlayerItems = 1;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.ShirtIndex = 1;
                        break;
                    }
                case ItemTypes.Pants:
                    {
                        playerItems.PantsIndex_PlayerItems = 1;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.PantsIndex = 1;
                        break;
                    }
                case ItemTypes.HeadAccessories:
                    {
                        Debug.Log("SavePlayerDefaultSkinsIndex HeadAccessoriesIndex = 0");
                        playerItems.HeadAccessoriesIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.HeadAccessoriesIndex = 0;
                        break;
                    }
                case ItemTypes.EyeAccessories:
                    {
                        playerItems.EyeAccessoriesIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.EyeAccessoriesIndex = 0;
                        break;
                    }
                case ItemTypes.ShoulderAccessories:
                    {
                        playerItems.ShoulderAccessoriesIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.ShoulderAccessoriesIndex = 0;
                        break;
                    }
                case ItemTypes.BodyAccessories:
                    {
                        playerItems.BodyAccessoriesIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.BodyAccessoriesIndex = 0;
                        break;
                    }
                case ItemTypes.Shoes:
                    {
                        playerItems.ShoesIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.ShoesIndex = 0;
                        break;
                    }
                case ItemTypes.Nose:
                    {
                        playerItems.NoseIndex_PlayerItems = 0;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.NoseIndex = 0;
                        break;
                    }
            }

            SetGenderThenSaveActiveIndex(playerItems);
        }

        // ----------------------------------------------------------------------------------------------------------------- //

        public void SavePlayerSkinsIndex(bool _isSpecialItem = false)
        {
            if (_isSpecialItem)
            {
                PlayerDataManager.Instance.IsSpecialItemEquipped = _isSpecialItem;
            }

            PlayerItems playerItems = CheckGenderThenSaveActiveIndex();

            // if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.gender == Gender.Male)
            //     playerItems = HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.MaleItems;
            // else if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.gender == Gender.Female)
            //     playerItems = HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.FemaleItems;

            switch (itemTypes)
            {
                case ItemTypes.Hair:
                    {
                        Debug.Log("SavePlayerSkinsIndex Hair");
                        playerItems.HairIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.HairIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.LowerFace:
                    {
                        playerItems.LowerFaceIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.LowerFaceIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.Shirt:
                    {
                        playerItems.ShirtIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.ShirtIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.Pants:
                    {
                        playerItems.PantsIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.PantsIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.HeadAccessories:
                    {
                        Debug.Log("HeadAccessories Hair");
                        playerItems.HeadAccessoriesIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.HeadAccessoriesIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.EyeAccessories:
                    {
                        playerItems.EyeAccessoriesIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.EyeAccessoriesIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.ShoulderAccessories:
                    {
                        playerItems.ShoulderAccessoriesIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.ShoulderAccessoriesIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.BodyAccessories:
                    {
                        playerItems.BodyAccessoriesIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.BodyAccessoriesIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.Shoes:
                    {
                        playerItems.ShoesIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.ShoesIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
                case ItemTypes.Nose:
                    {
                        playerItems.NoseIndex_PlayerItems = itemData.ItemIndex;
                        HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.NoseIndex = itemData.ItemIndex; // Keep Maybe?
                        break;
                    }
            }

            SetGenderThenSaveActiveIndex(playerItems);
        }

        public PlayerItems CheckGenderThenSaveActiveIndex()
        {
            if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.gender == Gender.Male)
            {
                Debug.Log("MALEITEMS!!!!!", this.gameObject);
                return HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.MaleItems;
            }

            else if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.gender == Gender.Female)
            {
                Debug.Log("FEMALEITEMS!!!!!", this.gameObject);
                return HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.FemaleItems;
            }

            else
            {
                Debug.Log("DEFAULT!!!!!", this.gameObject);
                return default;
            }
        }

        public void SetGenderThenSaveActiveIndex(PlayerItems _playerItems)
        {
            if (HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.gender == Gender.Male)
            {
                HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.MaleItems = _playerItems;
            }
            else
            {
                HomeMenuManager.Instance.HomeMenuPlayerData.playerInfo.FemaleItems = _playerItems;
            }
        }

        public void PurchaseItem(ItemData _itemData)
        {

        }

        public void ShowItemEquip_Layout()
        {
            ItemEquip_Button.SetActive(true);
            ItemCost_Button.SetActive(false);
        }
        public void ShowItemCost_Layout()
        {
            ItemCost_Button.SetActive(true);
            ItemEquip_Button.SetActive(false);
        }

        public void ShowInsufficientFundPopup()
        {
            ItemsManager.Instance.ShowInsufficientFundPopup();
        }
    }
}
