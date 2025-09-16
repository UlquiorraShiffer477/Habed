// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using UnityEngine;

// [Serializable]
// public class PlayerSessionInfo_Player
// {
//     public int ClientId;
//     public string PlayerName;
//     public string PlayerAnswer;
//     public bool DidPlayerAnswer;
// }

// // [Serializable]
// // public class PlayerAnswerGroup
// // {
// //     public string PlayerAnswer;
// //     public List<PlayerSessionInfo_Player> Players;

// //     public PlayerAnswerGroup(string playerAnswer)
// //     {
// //         PlayerAnswer = playerAnswer;
// //         Players = new List<PlayerSessionInfo_Player>();
// //     }
// // }

// public class DuplicatedListTest : MonoBehaviour
// {
//     public List<PlayerSessionInfo_Player> PlayersInSessionList = new List<PlayerSessionInfo_Player>();
//     public List<PlayerAnswerGroup> GroupedPlayerAnswers = new List<PlayerAnswerGroup>();

//     async void Start()
//     {
//         // Example data
//         PlayersInSessionList.Add(new PlayerSessionInfo_Player { ClientId = 1, PlayerName = "Player1", PlayerAnswer = "Answer1", DidPlayerAnswer = true });
//         PlayersInSessionList.Add(new PlayerSessionInfo_Player { ClientId = 2, PlayerName = "Player2", PlayerAnswer = "Answer1", DidPlayerAnswer = true });
//         PlayersInSessionList.Add(new PlayerSessionInfo_Player { ClientId = 3, PlayerName = "Player3", PlayerAnswer = "Answer2", DidPlayerAnswer = true });
//         PlayersInSessionList.Add(new PlayerSessionInfo_Player { ClientId = 4, PlayerName = "Player4", PlayerAnswer = "Answer3", DidPlayerAnswer = true });
//         PlayersInSessionList.Add(new PlayerSessionInfo_Player { ClientId = 5, PlayerName = "Player5", PlayerAnswer = "Answer2", DidPlayerAnswer = true });
//         PlayersInSessionList.Add(new PlayerSessionInfo_Player { ClientId = 6, PlayerName = "Player6", PlayerAnswer = "Answer2", DidPlayerAnswer = true });

//         await GroupPlayerAnswers();

//         foreach (var group in GroupedPlayerAnswers)
//         {
//             Debug.Log($"PlayerAnswer: {group.PlayerAnswer}");
//             foreach (var player in group.Players)
//             {
//                 Debug.Log($"ClientId: {player.ClientId}, PlayerName: {player.PlayerName}, PlayerAnswer: {player.PlayerAnswer}");
//             }
//         }
//     }

//     public async Task GroupPlayerAnswers()
//     {
//         await Task.Run(() =>
//         {
//             var playerAnswerCount = PlayersInSessionList
//                 .GroupBy(player => player.PlayerAnswer)
//                 .ToDictionary(group => group.Key, group => group.Count());

//             var duplicatedAnswers = new HashSet<string>(playerAnswerCount
//                 .Where(pair => pair.Value > 1)
//                 .Select(pair => pair.Key));

//             // Group players with duplicated PlayerAnswer values
//             GroupedPlayerAnswers = PlayersInSessionList
//                 .Where(player => duplicatedAnswers.Contains(player.PlayerAnswer))
//                 .GroupBy(player => player.PlayerAnswer)
//                 .Select(group => new PlayerAnswerGroup(group.Key) { Players = group.ToList() })
//                 .ToList();

//             // Add players with unique answers as individual groups
//             var uniqueAnswers = playerAnswerCount
//                 .Where(pair => pair.Value == 1)
//                 .Select(pair => pair.Key);

//             foreach (var answer in uniqueAnswers)
//             {
//                 var player = PlayersInSessionList.First(p => p.PlayerAnswer == answer);
//                 GroupedPlayerAnswers.Add(new PlayerAnswerGroup(answer) { Players = new List<PlayerSessionInfo_Player> { player } });
//             }
//         });
//     }
// }
