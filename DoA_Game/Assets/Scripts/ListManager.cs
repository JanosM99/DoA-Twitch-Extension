using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
 using System;
public class ListManager : MonoBehaviour
{   
    [SerializeField]
    public List<Entry> entries = new List<Entry>();               // Entries List
    [SerializeField]
    public List<Viewer> leaderboard = new List<Viewer>();         // Leaderboard List

    // Add Player To Entries List
    public void AddNameToList(string user, int bet, string card)
    {
        var item = entries.FirstOrDefault(x => x.name == user);                     // Check If User Already Betted (ONLY 1 BET / ROUND)
        if(item != null || !CheckUserPoints(user,bet))                              // Check If User Has Enough Point To Bet
        {
            Debug.Log("The user already betted or does not have enough points.");
        }
        else
        {
            entries.Add(new Entry(user,bet,card,false));
        }
    }

    // Clear Entries List
    public void DeleteList()
    {
        entries.Clear();
    }

    // Add  Viewer To Leaderboard List
    public void AddFromLeaderboardToList(string user, int points)
    {
        leaderboard.Add(new Viewer(user,points));
    }

    // Clear Leaderboard List
    public void DeleteLeaderBoard()
    {
        leaderboard.Clear();
    }

    // Check Points - AddNameToList()
    bool CheckUserPoints(string user,int bet)
    {
        foreach(var l in leaderboard)
        {
            if(l.name == user && l.points >= bet) 
            {
                return true;
            }
        }
        return false;
    }
}

// Entry Class - entries List
[System.Serializable]
public class Entry
{
    public string name;
    public int bet;
    public string card; 
    public bool winner;

    public Entry(string name, int bet, string card, bool winner)
    {
        this.name = name;
        this.bet = bet;
        this.card = card;
        this.winner = winner;
    }
}

// Viewer Class - leaderboard List
[System.Serializable]
public class Viewer
{
    public string name; 
    public int points; 

    public Viewer(string name, int points)
    {
        this.name = name;
        this.points = points;
    }
}