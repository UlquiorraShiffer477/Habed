using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using Unity.Netcode;

[Serializable]
public enum Gender
{
    NotSelected,
    Male,
    Female
}

[Serializable]
public struct PlayerInfo : INetworkSerializable, IEquatable<PlayerInfo>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerID;
    public FixedString128Bytes PlayerName;
    public FixedString32Bytes PlayerEmail;
    public int PlayerProfilePic;
    public int PlayerAvatar;
    public int PlayerScore;
    public int PlayerCoins;
    public int PlayerDance;
    public MainNetworkManager.PlayerAvatar playerAvatarSprite;  
    public bool IsMan;

    // Skins...
    public int HairIndex;
    public int LowerFaceIndex;
    public int ShirtIndex;
    public int PantsIndex;
    public int HeadAccessoriesIndex;
    public int EyeAccessoriesIndex;
    public int ShoulderAccessoriesIndex;
    public int BodyAccessoriesIndex;
    public int ShoesIndex;
    public int NoseIndex;

    public Gender gender;

    public bool FemaleSpecialShirtEquipped;
    public bool MaleSpecialShirtEquipped;

    public PlayerItems MaleItems;
    public PlayerItems FemaleItems;

    public PlayerInfo(ulong _clientId = 0, 
                        string _playerID = "", 
                        string _playerName = "", 
                        string _playerEmail = "", 
                        int _playerProfilePic = -1, 
                        int _playerAvatar = -1, 
                        int _playerScore = -1, 
                        int _playerCoins = -1, 
                        int _playerDance = -1, 
                        MainNetworkManager.PlayerAvatar _playerAvatarSprite = MainNetworkManager.PlayerAvatar.Male, 
                        bool _isMan = true, 
                        int _hairIndex = 1, 
                        int _lowerFaceIndex = 0, 
                        int _shirtIndex = 1, 
                        int _pantsIndex = 1, 
                        int _headAccessoriesIndex = 0, 
                        int _eyeAccessoriesIndex = 0, 
                        int _shoulderAccessoriesIndex = 0, 
                        int _bodyAccessoriesIndex = 0, 
                        int _shoesIndex = 0,
                        int _noseIndex = 0,
                        Gender _gender = Gender.NotSelected,
                        bool _femaleSpecialShirtEquipped = false,
                        bool _maleSpecialShirtEquipped = false,

                        PlayerItems maleItems = new PlayerItems(),
                        PlayerItems femaleItems = new PlayerItems())
    {
       ClientId = _clientId;
       PlayerID = _playerID;
       PlayerName = _playerName;
       PlayerEmail = _playerEmail;
       PlayerProfilePic = _playerProfilePic;
       PlayerAvatar = _playerAvatar;
       PlayerScore = _playerScore;
       PlayerCoins = _playerCoins;
       PlayerDance = _playerDance;
       IsMan = _isMan;
       HairIndex = _hairIndex;
       LowerFaceIndex = _lowerFaceIndex;
       ShirtIndex = _shirtIndex;
       PantsIndex = _pantsIndex;
       HeadAccessoriesIndex = _headAccessoriesIndex;
       EyeAccessoriesIndex = _eyeAccessoriesIndex;
       ShoulderAccessoriesIndex = _shoulderAccessoriesIndex;
       BodyAccessoriesIndex = _bodyAccessoriesIndex;
       ShoesIndex = _shoesIndex;
       NoseIndex = _noseIndex;

       MaleItems = maleItems;
       FemaleItems = femaleItems;

       playerAvatarSprite = MainNetworkManager.PlayerAvatar.Female;
       
       gender = _gender;

       Debug.Log("First Constructor called!");

       FemaleSpecialShirtEquipped   = _femaleSpecialShirtEquipped;
       MaleSpecialShirtEquipped     = _maleSpecialShirtEquipped;
    }

    

    public PlayerInfo(PlayerInfo _playerInfo)
    {
       ClientId =                   _playerInfo.ClientId;
       PlayerID =                   _playerInfo.PlayerID;
       PlayerName =                 _playerInfo.PlayerName;
       PlayerEmail =                _playerInfo.PlayerEmail;
       PlayerProfilePic =           _playerInfo.PlayerProfilePic;
       PlayerAvatar =               _playerInfo.PlayerAvatar;
       PlayerScore =                _playerInfo.PlayerScore;
       PlayerCoins =                _playerInfo.PlayerCoins;
       PlayerDance =                _playerInfo.PlayerDance;
       IsMan =                      _playerInfo.IsMan;
       HairIndex =                  _playerInfo.HairIndex;
       LowerFaceIndex =             _playerInfo.LowerFaceIndex;
       ShirtIndex =                 _playerInfo.ShirtIndex;
       PantsIndex =                 _playerInfo.PantsIndex;
       HeadAccessoriesIndex =       _playerInfo.HeadAccessoriesIndex;
       EyeAccessoriesIndex =        _playerInfo.EyeAccessoriesIndex;
       ShoulderAccessoriesIndex =   _playerInfo.ShoulderAccessoriesIndex;
       BodyAccessoriesIndex =       _playerInfo.BodyAccessoriesIndex;
       ShoesIndex =                 _playerInfo.ShoesIndex;
       NoseIndex =                  _playerInfo.NoseIndex;

       MaleItems =                  _playerInfo.MaleItems;
       FemaleItems =                _playerInfo.FemaleItems;

       playerAvatarSprite = _playerInfo.playerAvatarSprite;
       
       gender = _playerInfo.gender;

       FemaleSpecialShirtEquipped   = _playerInfo.FemaleSpecialShirtEquipped;
       MaleSpecialShirtEquipped     =  _playerInfo.MaleSpecialShirtEquipped;
    }



    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerID);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref PlayerEmail);
        serializer.SerializeValue(ref PlayerProfilePic);
        serializer.SerializeValue(ref PlayerAvatar);
        serializer.SerializeValue(ref PlayerScore);
        serializer.SerializeValue(ref PlayerCoins);
        serializer.SerializeValue(ref PlayerDance);
        serializer.SerializeValue(ref IsMan);
        serializer.SerializeValue(ref playerAvatarSprite);

        serializer.SerializeValue(ref HairIndex);
        serializer.SerializeValue(ref LowerFaceIndex);
        serializer.SerializeValue(ref ShirtIndex);
        serializer.SerializeValue(ref PantsIndex);
        serializer.SerializeValue(ref HeadAccessoriesIndex);
        serializer.SerializeValue(ref EyeAccessoriesIndex);
        serializer.SerializeValue(ref ShoulderAccessoriesIndex);
        serializer.SerializeValue(ref BodyAccessoriesIndex);
        serializer.SerializeValue(ref ShoesIndex);
        serializer.SerializeValue(ref NoseIndex);

        serializer.SerializeValue(ref MaleItems);
        serializer.SerializeValue(ref FemaleItems);

        serializer.SerializeValue(ref gender);

        serializer.SerializeValue(ref FemaleSpecialShirtEquipped);
        serializer.SerializeValue(ref MaleSpecialShirtEquipped);
    }

    public bool Equals(PlayerInfo other)
    {
        Debug.Log("Override");

        Debug.Log(MaleItems.Equals(other.MaleItems));
        Debug.Log(FemaleItems.Equals(other.FemaleItems));

        return ClientId == other.ClientId &&
            PlayerID == other.PlayerID &&
            PlayerName == other.PlayerName &&
            PlayerEmail == other.PlayerEmail &&
            PlayerProfilePic == other.PlayerProfilePic &&
            PlayerAvatar == other.PlayerAvatar &&
            PlayerScore == other.PlayerScore &&
            PlayerCoins == other.PlayerCoins &&
            PlayerDance == other.PlayerDance &&
            IsMan == other.IsMan &&
            playerAvatarSprite == other.playerAvatarSprite &&

            HairIndex == other.HairIndex &&
            LowerFaceIndex == other.LowerFaceIndex &&
            ShirtIndex == other.ShirtIndex &&
            PantsIndex == other.PantsIndex &&
            HeadAccessoriesIndex == other.HeadAccessoriesIndex &&
            EyeAccessoriesIndex == other.EyeAccessoriesIndex &&
            ShoulderAccessoriesIndex == other.ShoulderAccessoriesIndex &&
            BodyAccessoriesIndex == other.BodyAccessoriesIndex &&
            ShoesIndex == other.ShoesIndex &&
            NoseIndex == other.NoseIndex &&

            MaleItems.Equals(other.MaleItems)&&
            FemaleItems.Equals(other.FemaleItems)&&
            

            gender == other.gender &&

            FemaleSpecialShirtEquipped == other.FemaleSpecialShirtEquipped &&
            MaleSpecialShirtEquipped == other.MaleSpecialShirtEquipped;
    }
}

[Serializable]
public class PlayerAnswerGroup
{
    public string PlayerAnswer;
    public bool IsCorrect;
    public List<string> PlayersNames_WhoChooseOwnerAnswer = new List<string>();
    public List<ulong> PlayersID_WhoChooseThis = new List<ulong>();
    public List<PlayerSessionInfo> Players;

    public PlayerAnswerGroup(string playerAnswer = "")
    {
        PlayerAnswer = playerAnswer;
        Players = new List<PlayerSessionInfo>();
    }
}

[Serializable]
public class PlayerSessionInfo /*: INetworkSerializable, IEquatable<PlayerSessionInfo>*/
{
    public ulong ClientId;
    public string PlayerName;
    public string PlayerAnswer;
    public List<string> PlayersWhoChooseOwnerAnswer = new List<string>();
    public List<ulong> PlayersIDWhoChooseThis = new List<ulong>();
    public int PlayerScore;
    public int PlayerRank;
    public bool IsCorrect;

    public bool IsDefaultAnswer;
    public bool DidPlayerAnswer;

    public PlayerInfo playerInfo;
}

[Serializable]
public class PlayerData
{
    public string PlayerID = "";
    public string PlayerName = "";
    public bool IsMan = false;
    public int PlayerCoins = 2000;
    public List<ItemData> OwnedItems = new List<ItemData>();

    public PlayerInfo playerInfo = new PlayerInfo{
				HairIndex 						= 1,
				LowerFaceIndex 					= 0,
				ShirtIndex 						= 1,
				PantsIndex 						= 1,
				HeadAccessoriesIndex 			= 0,
				EyeAccessoriesIndex 			= 0,
				ShoulderAccessoriesIndex 		= 0,
				BodyAccessoriesIndex 			= 0,
				ShoesIndex 						= 0,
				NoseIndex 						= 0,
                FemaleSpecialShirtEquipped      = false,
                MaleSpecialShirtEquipped        = false,

                MaleItems = new PlayerItems{
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

                FemaleItems = new PlayerItems{
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

    public PlayerData()
    {
        // Debug.Log("Third Constructor called!");
        PlayerID = "";
        PlayerName = "";
        IsMan = false;
        PlayerCoins = 2000;
        OwnedItems = new List<ItemData>();
        playerInfo = new PlayerInfo{
				HairIndex 						= 1,
				LowerFaceIndex 					= 0,
				ShirtIndex 						= 1,
				PantsIndex 						= 1,
				HeadAccessoriesIndex 			= 0,
				EyeAccessoriesIndex 			= 0,
				ShoulderAccessoriesIndex 		= 0,
				BodyAccessoriesIndex 			= 0,
				ShoesIndex 						= 0,
				NoseIndex 						= 0,
                FemaleSpecialShirtEquipped      = false,
                MaleSpecialShirtEquipped        = false,

                MaleItems = new PlayerItems{
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

                FemaleItems = new PlayerItems{
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

    public PlayerData(PlayerData _playerData)
    {
        PlayerID =      _playerData.PlayerID;
        PlayerName =    _playerData.PlayerName;
        IsMan =         _playerData.IsMan;
        PlayerCoins =   _playerData.PlayerCoins;
        OwnedItems =    new List<ItemData>(_playerData.OwnedItems);
        playerInfo =    new PlayerInfo(_playerData.playerInfo);
    }

    // public bool Equals(ItemData other)
    // {
    //     throw new NotImplementedException();
    // }

    // public override bool Equals(object obj)
    // {
    //     if (obj == null || GetType() != obj.GetType())
    //     {
    //         return false;
    //     }
        
    //     PlayerData other = (PlayerData)obj;

    //     return (PlayerID == other.PlayerID && 
    //             PlayerName == other.PlayerName &&
    //             IsMan == other.IsMan &&
    //             PlayerCoins == other.PlayerCoins &&
    //             playerInfo.Equals(other.playerInfo));
    // }
}

[Serializable]
public struct PlayerItems: INetworkSerializable, IEquatable<PlayerItems>
{
    // Skins...
    public int HairIndex_PlayerItems;
    public int LowerFaceIndex_PlayerItems;
    public int ShirtIndex_PlayerItems;
    public int PantsIndex_PlayerItems;
    public int HeadAccessoriesIndex_PlayerItems;
    public int EyeAccessoriesIndex_PlayerItems;
    public int ShoulderAccessoriesIndex_PlayerItems;
    public int BodyAccessoriesIndex_PlayerItems;
    public int ShoesIndex_PlayerItems;
    public int NoseIndex_PlayerItems;

    public PlayerItems(
        int hairIndex                   = 1,
        int lowerFaceIndex              = 0,
        int shirtIndex                  = 0,
        int pantsIndex                  = 0,
        int headAccessoriesIndex        = 0,
        int eyeAccessoriesIndex         = 0,
        int shoulderAccessoriesIndex    = 0,
        int bodyAccessoriesIndex        = 0,
        int shoesIndex                  = 0,
        int noseIndex                   = 0
    )
    {
        HairIndex_PlayerItems = hairIndex;
        LowerFaceIndex_PlayerItems = lowerFaceIndex;        
        ShirtIndex_PlayerItems = shirtIndex;       
        PantsIndex_PlayerItems = pantsIndex;           
        HeadAccessoriesIndex_PlayerItems = headAccessoriesIndex;
        EyeAccessoriesIndex_PlayerItems = eyeAccessoriesIndex; 
        ShoulderAccessoriesIndex_PlayerItems = shoulderAccessoriesIndex;
        BodyAccessoriesIndex_PlayerItems = bodyAccessoriesIndex;
        ShoesIndex_PlayerItems = shoesIndex;   
        NoseIndex_PlayerItems = noseIndex;                
    }

    public PlayerItems(PlayerItems _playerItems)
    {
        HairIndex_PlayerItems =                 _playerItems.HairIndex_PlayerItems;
        LowerFaceIndex_PlayerItems =            _playerItems.LowerFaceIndex_PlayerItems;        
        ShirtIndex_PlayerItems =                _playerItems.ShirtIndex_PlayerItems;       
        PantsIndex_PlayerItems =                _playerItems.PantsIndex_PlayerItems;           
        HeadAccessoriesIndex_PlayerItems =      _playerItems.HeadAccessoriesIndex_PlayerItems;
        EyeAccessoriesIndex_PlayerItems =       _playerItems.EyeAccessoriesIndex_PlayerItems; 
        ShoulderAccessoriesIndex_PlayerItems =  _playerItems.ShoulderAccessoriesIndex_PlayerItems;
        BodyAccessoriesIndex_PlayerItems =      _playerItems.BodyAccessoriesIndex_PlayerItems;
        ShoesIndex_PlayerItems =                _playerItems.ShoesIndex_PlayerItems;   
        NoseIndex_PlayerItems =                 _playerItems.NoseIndex_PlayerItems;          
    }

    public bool Equals(PlayerItems other)
    {
        Debug.Log("PlayerItems EqualsOverride");

        return  HairIndex_PlayerItems == other.HairIndex_PlayerItems &&
                LowerFaceIndex_PlayerItems == other.LowerFaceIndex_PlayerItems &&
                ShirtIndex_PlayerItems == other.ShirtIndex_PlayerItems &&
                PantsIndex_PlayerItems == other.PantsIndex_PlayerItems &&
                HeadAccessoriesIndex_PlayerItems == other.HeadAccessoriesIndex_PlayerItems &&
                EyeAccessoriesIndex_PlayerItems == other.EyeAccessoriesIndex_PlayerItems &&
                ShoulderAccessoriesIndex_PlayerItems == other.ShoulderAccessoriesIndex_PlayerItems &&
                BodyAccessoriesIndex_PlayerItems == other.BodyAccessoriesIndex_PlayerItems &&
                ShoesIndex_PlayerItems == other.ShoesIndex_PlayerItems &&
                NoseIndex_PlayerItems == other.NoseIndex_PlayerItems;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref HairIndex_PlayerItems);
        serializer.SerializeValue(ref LowerFaceIndex_PlayerItems);
        serializer.SerializeValue(ref ShirtIndex_PlayerItems);
        serializer.SerializeValue(ref PantsIndex_PlayerItems);
        serializer.SerializeValue(ref HeadAccessoriesIndex_PlayerItems);
        serializer.SerializeValue(ref EyeAccessoriesIndex_PlayerItems);
        serializer.SerializeValue(ref ShoulderAccessoriesIndex_PlayerItems);
        serializer.SerializeValue(ref BodyAccessoriesIndex_PlayerItems);
        serializer.SerializeValue(ref ShoesIndex_PlayerItems);
        serializer.SerializeValue(ref NoseIndex_PlayerItems);
    }

}