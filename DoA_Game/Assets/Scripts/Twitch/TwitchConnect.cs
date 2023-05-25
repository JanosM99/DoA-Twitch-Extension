using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;



public class TwitchConnect : MonoBehaviour
{
    // Variables
    private TcpClient tcpClient;
    private StreamReader streamReader;
    private StreamWriter streamWriter;

    // Event For When A Chat Message Is Received
    public UnityEvent<string,string> OnChatMessage;
    // Check If The Connection Is Running
    public bool isRunning = false;
    // Display The Connection Status
    public Text status;

   // Functions
    private void Awake()
    {
        // Set The Frame Rate And Run The Game In The Background
        Application.targetFrameRate = 30;
        Application.runInBackground = true;
    }

    // Check For New Messages Every Frame
    private void Update()
    {
        if(isRunning)
        {
            if (NewTwitchMessage(out string newMessage))
            {
                if(newMessage != null)
                {
                    MessageFormate(newMessage);
                }
            }
        }
    }

    // Connect to Twitch
    public async void ConnectToTwitch()
    {
        tcpClient = new TcpClient();
        await tcpClient.ConnectAsync("irc.chat.twitch.tv", 6667);
        streamReader = new StreamReader(tcpClient.GetStream());
        streamWriter= new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };

        await streamWriter.WriteLineAsync("PASS " + "oauth:" + TwitchOAuth._authToken);
        await streamWriter.WriteLineAsync("NICK " + "oauth:" + TwitchOAuth._authToken);
        await streamWriter.WriteLineAsync("JOIN #" + TwitchOAuth.twitchChannelName.ToLower());
        // Set The isRunning To True And Start Reading
        isRunning = true;
        ReadMessages();
    }

    // Last Line Received From Twitch
    private string lastLine;
    // List Of Logs Received From Twitch
    [SerializeField] private List<string> logs = new List<string>();
     // Index Of The Last Log Checked
    [SerializeField] private int logsIndex;
    // Flag To Clear The Logs
    [SerializeField] private bool isClearLogs = false;

    private async void ReadMessages()
    {
        try
        {
            // Clear The Log And Reset The Logs Index
            logs.Clear();
            logsIndex = 1;
            while (true)
            {
                // Check If The Logs Should Be Cleared
                if (isClearLogs)
                {
                    logs.Clear();
                    logsIndex = 1;
                    isClearLogs = false;

                    if(lastLine !=null)
                    {
                        logs.Add(lastLine);
                    }
                }
                // Read The Next Line From Twitch
                lastLine = await streamReader.ReadLineAsync();
                if(lastLine !=null)
                {
                    logs.Add(lastLine);
                    Debug.Log(lastLine);
                    status.text = "Connected";
                }
                // Check If The Line Is A Ping Message And Respond With A Pong
                if (lastLine != null && lastLine.StartsWith("PING"))
                {
                    lastLine.Replace("PING", "PONG");
                    await streamWriter.WriteLineAsync(lastLine);
                    Debug.Log(lastLine);
                }
            }
        }
        catch(Exception ex)
        {
            //  If An Exception Occurs
            status.text = "Connection lost...";
            Debug.LogError("ReadMessages(): " + ex.Message);
            await HandleReconnection();
        }
    }

    // Handle Reconnection
    private async Task HandleReconnection()
    {
        CloseConnection();                 // Close The Current Connection
        await Task.Delay(5000);           // Wait Before Reconnecting
        status.text = "Reconnecting...";
        Debug.Log("Reconnecting...");
        ConnectToTwitch();                 // Connect To Twitch 
    }

    // Close Connection
    private void CloseConnection()
    {
        streamReader.Close();
        streamWriter.Close();
        tcpClient.Close();
        status.text = "Connection closed";
        Debug.Log("Connection closed");
    }

    // Called - StartRound()
    // Function To Clear Logs
    public void ClearLogs()
    {
        isClearLogs = true;
    }

    // Check For New Messages
    public bool NewTwitchMessage(out string newMessage)
    {
        // If There Are No New Messages, Return False
        if (logs.Count < logsIndex | logs.Count==0)
        {
            newMessage = "";
            return false;
        }
        // Iterate Through The Logs And Find The Next Message
        for (int i = logsIndex; i <= logs.Count; i++)
        {
            if (logs[i - 1].Contains("PRIVMSG"))
            {
                logsIndex = i + 1;
                newMessage = logs[i - 1];
                return true;
            }
        }
        // Update The logsIndex And Return The Message
        logsIndex = logs.Count + 1;
        newMessage = "";
        // If No Message Is Found
        return false;
    }

    // Message 
    public string[] messageDetails(string twitchMessage)
    {
        if (twitchMessage == null)
        {
            return new string[1] { "" };
        }
        if (!twitchMessage.Contains("PRIVMSG"))
        {
            return new string[1] { "" };
        }
        else
        {
            string[] firstSplit = twitchMessage.Split(' ');
            string channelName = firstSplit[2].Substring(1);

            string[] messageSplit = twitchMessage.Split(':');
            string message = messageSplit[2];

            string first = firstSplit[0];
            string[] secondSplit = first.Split('!');
            string user = secondSplit[0].Substring(1);

            return new string[2] {user, message};
        }
    }

    private void MessageFormate(string twitchLine)
    {
        if(twitchLine!=null)
        {
            string[] message = messageDetails(twitchLine);
            
            OnChatMessage?.Invoke(message[0],message[1]);
            Debug.Log(message[0] + ": " + message[1]);
        }
    }
}