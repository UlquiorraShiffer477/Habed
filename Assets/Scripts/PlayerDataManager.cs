using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

using Unity.Services.CloudSave;
using System.Threading.Tasks;

using Unity.Collections;

using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PlayerDataManager : MonoBehaviour 
{
    #region Instance
	// ----------------------Instance Section---------------------- //
    public static PlayerDataManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<PlayerDataManager>();

			return _instance;
		}
	}
	private static PlayerDataManager _instance;
    // ----------------------Instance Section---------------------- //
#endregion

    public readonly string PLAYERDATA = "Player_Data";


    [Header("Player Data Related"), Space]
    public PlayerData playerData;

    public PlayerData TempPlayerData;


    [Header("Objects")]
    public PlayerInfo playerInfo;

    [Header("Others")]
    public bool IsSpecialItemEquipped;
    // public int DefaultActiveIndex = 1;

    void Start() 
    {
        // playerInfo = new PlayerInfo{PantsIndex = 1};
    }

    #region Helper Functions...
    public void SetPlayerData(PlayerData _playerData)
    {
        playerData = DeepClone(_playerData);
    }
    public PlayerData GetClonedPlayerData()
    {
        PlayerData temp = DeepClone(playerData);
        return temp;
    }

    private T DeepClone<T>(T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", nameof(source));
        }

        // Perform deep copy using serialization and deserialization
        using (MemoryStream memoryStream = new MemoryStream())
        {
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, source);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(memoryStream);
        }
    }

    private T CloneObject<T>(T source)
    {
        T clone = Activator.CreateInstance<T>();

        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            field.SetValue(clone, field.GetValue(source));
        }

        return clone;
    }

    // public PlayerData GetPlayerData()
    // {
    //     return playerData;
    // }

    // public void AddItemToOwnedItemList(ItemData _itemData)
    // {
    //     playerData.OwnedItems.Add(_itemData);
    // }
    // public void SetOwnedItemsList(List<ItemData> _ownedItems)
    // {
    //     playerData.OwnedItems = _ownedItems;
    // }
    // public List<ItemData> GetOwnedItemsList()
    // {
    //     return playerData.OwnedItems;
    // }
    

    
    // public void SetPlayerGender(global :: Gender _gender)
    // {
    //     playerData.playerInfo.gender = _gender;
    // }
    public global :: Gender GetPlayerGender()
    {
        return playerData.playerInfo.gender;
    }

     public void SetPlayerNamePlayerData(string _name)
    {
        playerData.PlayerName = _name;
    }
    public string GetPlayerNamePlayerData()
    { 
        return playerData.PlayerName;
    }
    #endregion


    #region Cloud Save Functions
    public async Task SaveDataObject(string _key, PlayerData _object)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                {_key, _object}
            };

            await CloudSaveService.Instance.Data.ForceSaveAsync(data);

            Debug.Log($"Successfully saved {_key}:{_object}");
        }

        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }

        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }

        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<T> RetrieveSpecificData<T>(string key)
    {
        try
        {
            var results = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> {key});
            if (results.TryGetValue(key, out string value))
            {
                return JsonUtility.FromJson<T>(value);
            }
            else
            {
                Debug.Log($"There is no such key as {key}!");
            }
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
        return default;
    }
    #endregion

    public PlayerInfo GetPlayerInfo()
    {
        return playerData.playerInfo;
    }
}