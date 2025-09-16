using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class PlayerPrefManager
{
    public const string prefsBuildVersion = "0.0.0";
    private const string prefsHashingKey = "2023_1_1";

    public static class PlayerFirstTimeData
    {
        public const string PlayerFirstTimeDataKey = "PlayerFirstTimeDataKey";
        public const string PlayerFirstTimeDataInstanceKey = "PlayerFirstTimeDataInstanceKey";

        public static void SetTutorialFirstTime()
        {
            PlayerPrefs.SetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerFirstTimeDataKey + "_" + PlayerFirstTimeDataInstanceKey), 0);
        }

        public static bool GetTutorialFirstTime()
        {
            return (PlayerPrefs.GetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerFirstTimeDataKey + "_" + PlayerFirstTimeDataInstanceKey), -1) > -1);
        }
    }

    public static class PlayerInternalSettings
    {
        public const string PlayerInternalSettingsDataKey = "PlayerInternalSettingsDataKey";
        public const string PlayerInternalSettingsDataInstanceKey = "PlayerInternalSettingsDataInstanceKey";

        public const string PlayerInternalSettingsDataInstanceKey_Music = "PlayerInternalSettingsDataInstanceKey_Music";
        public const string PlayerInternalSettingsDataInstanceKey_VFX = "PlayerInternalSettingsDataInstanceKey_VFX";

        public static void SetMusicOn(int _value = -1)
        {
            PlayerPrefs.SetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerInternalSettingsDataKey + "_" + PlayerInternalSettingsDataInstanceKey + "_" + PlayerInternalSettingsDataInstanceKey_Music), _value);
        }

        public static bool GetMusicOn()
        {
            return PlayerPrefs.GetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerInternalSettingsDataKey + "_" + PlayerInternalSettingsDataInstanceKey + "_" + PlayerInternalSettingsDataInstanceKey_Music), -1) > -1;
        }

        public static void SetVFXOn(int _value = -1)
        {
            PlayerPrefs.SetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerInternalSettingsDataKey + "_" + PlayerInternalSettingsDataInstanceKey + "_" + PlayerInternalSettingsDataInstanceKey_VFX), _value);
        }

        public static bool GetVFXOn()
        {
            return PlayerPrefs.GetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerInternalSettingsDataKey + "_" + PlayerInternalSettingsDataInstanceKey + "_" + PlayerInternalSettingsDataInstanceKey_VFX), -1) > -1;
        }
    }

    public static class PlayerAuthData
    {
        public const string PlayerAuthDataKey = "PlayerAuthDataKey";

        public static void SetAuthMethod(int _authMethod)
        {
            PlayerPrefs.SetInt((prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerAuthDataKey), _authMethod);
        }

        public static int GetAuthMethod()
        {
            return PlayerPrefs.GetInt(prefsHashingKey + "_" + prefsBuildVersion + "_" + PlayerAuthDataKey);
        }
    }
    

    public static class PlayerItemsData
    {
        public const string PlayerItemsDataKey = "PlayerItemsDataKey";
        public const string PlayerItemsDataInstanceKey = "PlayerItemsDataInstanceKey";

        public static void SetOwnedItemsList(List<string> _ownedItemsList)
        {
            // Convert the list to a single string with '|' as a delimiter
            string serializedStringList = string.Join("|", _ownedItemsList.ToArray());
            PlayerPrefs.SetString(prefsBuildVersion + "_" + prefsHashingKey + "_" + PlayerItemsDataKey + "_" + PlayerItemsDataInstanceKey, serializedStringList);
            PlayerPrefs.Save(); // Persist the changes immediately
        }

        public static List<string> GetOwnedItemsList()
        {
            if (PlayerPrefs.HasKey(prefsBuildVersion + "_" + prefsHashingKey + "_" + PlayerItemsDataKey + "_" + PlayerItemsDataInstanceKey))
            {
                string serializedStringList = PlayerPrefs.GetString(prefsBuildVersion + "_" + prefsHashingKey + "_" + PlayerItemsDataKey + "_" + PlayerItemsDataInstanceKey);
                // Split the saved string by the delimiter '|' and convert it back to a list of strings
                string[] stringArray = serializedStringList.Split('|');
                List<string> loadedStringList = new List<string>(stringArray);
                return loadedStringList;
            }
            else
            {
                // Return an empty list if the key is not found in PlayerPrefs
                return new List<string>();
            }
        }
    }

    public static void DeleteAllPlayerPrefsData()
    {
        PlayerPrefs.DeleteAll();
    }
}
