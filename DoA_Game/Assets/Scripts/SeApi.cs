using UnityEngine;
using UnityEditor;
using Proyecto26;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Net.Http;
using System;
using System.Net.Http.Headers;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class SeApi : MonoBehaviour {

	GameManager gamemanager;
	RoundManager roundmanager;
	ListManager listmanager;

	void Start()
	{
		gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		roundmanager = GameObject.FindGameObjectWithTag("RoundManager").GetComponent<RoundManager>();
		listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();
	}

	// Add SE Leaderboard To The List
	public async Task GetLeaderboard()
	{
		int limit = 1000;
		int offset = 0;
		int total; 

		var client = new HttpClient();
		do
		{
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri("https://api.streamelements.com/kappa/v2/points/" + TwitchOAuth.seAccountId + "/top?limit=" + limit + "&offset=" + offset),
			Headers =
			{
				{ "Accept", "application/json" },
				{ "Authorization", "Bearer " + TwitchOAuth.jwtToken},
			},
		};
			using (var response = await client.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();
				var body = await response.Content.ReadAsStringAsync();

				var data = JsonConvert.DeserializeObject<RootClass>(body); 
				foreach (User user in data.users) 
				{
					listmanager.AddFromLeaderboardToList(user.username,user.points);  
				}
				Debug.Log("GetLeaderboard()");	

				total = data._total;
				offset += 1000;
			}
		}
		while(offset < total);
	}

	// Add Points For Winners
	public async void BulkPointsForWinners()
	{
		List<Dictionary<string, object>> players = new List<Dictionary<string, object>>();
		foreach(var winner in listmanager.entries)
		{
			if(winner.winner)
			{
				var obj = new Dictionary<string, object>();
				obj["username"] = winner.name;
				obj["current"] = gamemanager.multiplier;
				players.Add(obj);

				string winnersToShow = winner.name + " won " + gamemanager.multiplier + " points" + "\n";  
				roundmanager.winnerChat.text += winnersToShow;                                                          
				roundmanager.winnerCount += 1;     
				roundmanager.totalPointWon = gamemanager.multiplier * roundmanager.winnerCount;
			}
		}

		if (players.Count == 0)
		{
			Debug.Log("No winners found, skipping API call.");
			return;
		}
		else
		{
			var client = new HttpClient();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Put,
				RequestUri = new Uri("https://api.streamelements.com/kappa/v2/points/" + TwitchOAuth.seAccountId),
				Headers =
				{
					{ "Accept", "application/json" },
					{ "Authorization", "Bearer " + TwitchOAuth.jwtToken},
				},
				Content = new StringContent("{ \"users\": " + JsonConvert.SerializeObject(players) + ", \"mode\": \"add\" }")
				{
					Headers =
					{
						ContentType = new MediaTypeHeaderValue("application/json")
					}
				}
			};
			using (var response = await client.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();
				var body = await response.Content.ReadAsStringAsync();
			}
			Debug.Log("BulkPointsForWinners()");
		}
	}

	// Remove Bet Points From Players - entries
	public async void RemovePointsFromPlayers()
	{
		List<Dictionary<string, object>> players = new List<Dictionary<string, object>>();
		foreach (var entry in listmanager.entries)
		{
			foreach (var user in listmanager.leaderboard)
			{
				if (entry.name == user.name)
				{
					var obj = new Dictionary<string, object>();
					obj["username"] = entry.name;
					obj["current"] = user.points - entry.bet;
					players.Add(obj);
				}
			}
		}
		if (players.Count == 0)
		{
			Debug.Log("No winners found, skipping API call.");
			return;
		}
		else
		{
			var client = new HttpClient();
			var request = new HttpRequestMessage
			{
				Method = HttpMethod.Put,
				RequestUri = new Uri("https://api.streamelements.com/kappa/v2/points/" + TwitchOAuth.seAccountId),
				Headers =
				{
					{ "Accept", "application/json" },
					{ "Authorization", "Bearer " + TwitchOAuth.jwtToken},
				},
				Content = new StringContent("{ \"users\": " + JsonConvert.SerializeObject(players) + ", \"mode\": \"set\" }")
				{
					Headers =
					{
					ContentType = new MediaTypeHeaderValue("application/json")
					}
				}
			};
			using (var response = await client.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();
				var body = await response.Content.ReadAsStringAsync();
			}
			Debug.Log("RemovePoint()");
		}
	}	

	// Bot Message
	public async void SendBotMsg(string msg)
	{
		var client = new HttpClient();
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Post,
			RequestUri = new Uri("https://api.streamelements.com/kappa/v2/bot/" + TwitchOAuth.seAccountId + "/say"),
			Headers =
			{
				{ "Accept", "application/json" },
				{ "Authorization", "Bearer " + TwitchOAuth.jwtToken},
			},
			Content = new StringContent("{\n  \"message\": \"" + msg + "\"\n}")
			{
				Headers =
				{
					ContentType = new MediaTypeHeaderValue("application/json")
				}
			}
		};
		using (var response = await client.SendAsync(request))
		{
			response.EnsureSuccessStatusCode();
			var body = await response.Content.ReadAsStringAsync();
		}
	}
}
