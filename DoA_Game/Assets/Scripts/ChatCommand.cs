using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

using WebSocketSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class ChatCommand : MonoBehaviour
{
    SeApi seApi;
    GameManager gamemanager;
    ListManager listmanager;

    private WebSocket _ws;
    private string viewerName;
    private string betClass;

    int rowPoint = 13;
    int columnPoint = 4;
    int combPoint = 26;
    int evenPoint = 28;
    int oddPoint = 24;
    int numPoint = 36;
    int facePoint = 16;
    int allPoint = 52;

    string[] row_class = new string[] {"row1", "row2", "row3", "row4"};
    string[] column_class = new string[] {"column2", "column3", "column4", "column5", "column6", "column7", "column8","column9", "column10", "columnj", "columnq", "columnk", "columna"};
    string[] combination_class = new string[] {"comb12", "comb13", "comb14", "comb23", "comb24", "comb34", "even", "odd", "num", "face"};
    string all_class = "all";

    void Start()
    {
        seApi = GameObject.FindGameObjectWithTag("SE_API").GetComponent<SeApi>();
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();

        // API
        _ws = new WebSocket("YOUR SOCKET");
        _ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
          
        Debug.Log("[WebSocket Status]: " + _ws.ReadyState);
        _ws.Connect();
        Debug.Log("[WebSocket Status]: " + _ws.ReadyState);
        
        _ws.OnMessage += (sender, e) =>
        {
            var regex = new Regex(@"\s+");

            // Split the string into an array of strings
            var strings = regex.Split($"{e.Data}");
            // strings[0] - name, strings[1] - betclass
            checkBetClass(strings[0],strings[1]);
        };
             
    }

    // Check the API
    private void Update()
    {
        if(_ws == null) 
        {
            return;
        }
    }

    // Manage the incoming bets
    void bet(string user, int point, string bet_class)
    {
        listmanager.AddNameToList(user, point, bet_class);
    }

    // Send startround msg to the extension
    public void SendRoundStartMessage()
    {
        var msg = new 
        {
            action = "sendmessage",
            message = "startround"
        };
        _ws.Send(JsonConvert.SerializeObject(msg));
    }

    // Check if bet is valid and call bet()
    void checkBetClass(string get_name, string get_bet_class)
    {
        if(gamemanager.isBetEnabled)
        {
            switch(get_bet_class)
            {
                // ROWS
                case string row1 when row1.Contains(row_class[0]): 
                    bet(get_name, rowPoint, row_class[0]);
                break;
                case string row2 when row2.Contains(row_class[1]): 
                    bet(get_name, rowPoint, row_class[1]);
                break;
                case string row3 when row3.Contains(row_class[2]): 
                    bet(get_name, rowPoint, row_class[2]);
                break;
                case string row4 when row4.Contains(row_class[3]): 
                    bet(get_name, rowPoint, row_class[3]);
                break;

                // COLUMNS       
                case string column2 when column2.Contains(column_class[0]): 
                    bet(get_name, columnPoint, column_class[0]);
                break;
                case string column3 when column3.Contains(column_class[1]): 
                    bet(get_name, columnPoint, column_class[1]);                
                break;
                case string column4 when column4.Contains(column_class[2]): 
                    bet(get_name, columnPoint, column_class[2]);               
                break;
                case string column5 when column5.Contains(column_class[3]): 
                    bet(get_name, columnPoint, column_class[3]);               
                break;
                case string column6 when column6.Contains(column_class[4]): 
                    bet(get_name, columnPoint, column_class[4]);                
                break;
                case string column7 when column7.Contains(column_class[5]): 
                    bet(get_name, columnPoint, column_class[5]);                
                break;
                case string column8 when column8.Contains(column_class[6]): 
                    bet(get_name, columnPoint, column_class[6]);               
                break;
                case string column9 when column9.Contains(column_class[7]): 
                    bet(get_name, columnPoint, column_class[7]);               
                break;
                case string column10 when column10.Contains(column_class[8]): 
                    bet(get_name, columnPoint, column_class[8]);                
                break;
                case string columnj when columnj.Contains(column_class[9]): 
                    bet(get_name, columnPoint, column_class[9]);                
                break;
                case string columnq when columnq.Contains(column_class[10]): 
                    bet(get_name, columnPoint, column_class[10]);               
                break;
                case string columnk when columnk.Contains(column_class[11]): 
                    bet(get_name, columnPoint, column_class[11]);             
                break;
                case string columna when columna.Contains(column_class[12]): 
                    bet(get_name, columnPoint, column_class[12]);               
                break;

                // COMBINATIONS
                case string comb12 when comb12.Contains(combination_class[0]): 
                    bet(get_name, combPoint, combination_class[0]);               
                break;
                case string comb13 when comb13.Contains(combination_class[1]): 
                    bet(get_name, combPoint, combination_class[1]);               
                break;
                case string comb14 when comb14.Contains(combination_class[2]): 
                    bet(get_name, combPoint, combination_class[2]);               
                break;
                case string comb23 when comb23.Contains(combination_class[3]): 
                    bet(get_name, combPoint, combination_class[3]);               
                break;
                case string comb24 when comb24.Contains(combination_class[4]): 
                    bet(get_name, combPoint, combination_class[4]);               
                break;
                case string comb34 when comb34.Contains(combination_class[5]): 
                    bet(get_name, combPoint, combination_class[5]);               
                break;
                case string even when even.Contains(combination_class[6]): 
                    bet(get_name, evenPoint, combination_class[6]);               
                break;
                case string odd when odd.Contains(combination_class[7]): 
                    bet(get_name, oddPoint, combination_class[7]);               
                break;
                case string num when num.Contains(combination_class[8]): 
                    bet(get_name, numPoint, combination_class[8]);               
                break;
                case string face when face.Contains(combination_class[9]): 
                    bet(get_name, facePoint, combination_class[9]);               
                break;
                
                // ALL
                case string all when all.Contains(all_class): 
                    bet(get_name, allPoint, all_class); 
                break;
            }
        }   
        else
        {
            return;
        }
    }
}
