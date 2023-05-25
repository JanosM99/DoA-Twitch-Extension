using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class RoundManager : MonoBehaviour
{	
    // SCRIPTS
    SeApi seApi;
    GameManager gamemanager;
    ListManager listmanager;

    // TIMER
    public float timeRemaining = 10;                                
    public bool isTimerRunning = false;                             
    public Text timer_Text;                                         
    public GameObject objectTimerText;                              

    // UI
    public Text winnerChat;                                         
    public GameObject objectWinnerCount;                           
    public Text winnerCountText;                                    

    public int winnerCount = 0;                                     
    public int totalPointWon = 0;                                  
    public string winnerCard;                                       
    public Card winnerCardObject;                                   

    public bool helpToRemovePoints;                                 // RemovePointsFromPlayers()       

    string betDisabled = "A tétrakás véget ért. Sok szerencsét!";     

    void Start()
    {
        seApi = GameObject.FindGameObjectWithTag("SE_API").GetComponent<SeApi>();
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();
    }

    void Update()
    {
        if (isTimerRunning)                                         // Timer Starts
        {
            if (timeRemaining > 0)                                                     
            {
                timeRemaining -= Time.deltaTime; 
                DisplayTime(timeRemaining);          
                gamemanager.drawCardButton.interactable = false;                                  
            }
            else
            {
                if(!helpToRemovePoints)                             // Called Once - RemovePointsFromPlayers();
                {
                    seApi.RemovePointsFromPlayers();                // API - RemovePointsFromPlayers() - Entries
                    helpToRemovePoints = true;
                }
                timeRemaining = 0; 
                isTimerRunning = false;
                // Timer Ends Here
                gamemanager.drawCardButton.interactable = true; 
		        objectTimerText.SetActive(false);

                gamemanager.isBetEnabled = false;                   // Bet Disabled
                seApi.SendBotMsg(betDisabled);                      // Twitch Chat - betDisabled
		        gamemanager.info_Text.text = betDisabled;           // info_Text - betDisabled
            }
        }
        winnerCountText.text = winnerCount.ToString() + " Player(s) won " + totalPointWon.ToString() + " points";    
    }

    // Display The Timer 
    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timer_Text.text = seconds.ToString();
    }

    // Manage The Winners
    public async void RoundWinners() 
    {   
        listmanager.DeleteLeaderBoard();                                // Clear Leaderboard From StartRound()
        await seApi.GetLeaderboard();                                   // API - Leaderboard For RoundWinners()

        if(!string.IsNullOrEmpty(winnerCard))                           // Check If Card Exist
        {
            objectWinnerCount.SetActive(true);
            foreach(var e in listmanager.entries)                       // Check The Entries
            {
                foreach(var l in listmanager.leaderboard)               // Check Leaderboard
                {
                    if(e.name == l.name)                                // Match
                    {
                        if(winnerCardObject.GetComponent("RegularCard"))
                        {
                            if(e.card == "all")
                            {
                                e.winner = true;  
                            }
                        }
                        // ROWS
                        if(winnerCardObject.GetComponent("Row1"))
                        {
                            if(e.card == "row1" || e.card == "comb12" || e.card == "comb13" || e.card == "comb14")
                            {
                               e.winner = true;                                                                  
                            }
                        }
                        if(winnerCardObject.GetComponent("Row2"))
                        {
                            if(e.card == "row2" || e.card == "comb12" || e.card == "comb23" || e.card == "comb24")
                            {
                               e.winner = true;   
                            }
                        }
                        if(winnerCardObject.GetComponent("Row3"))
                        {
                            if(e.card == "row3" || e.card == "comb13" || e.card == "comb23" || e.card == "comb34")
                            {
                                e.winner = true;    
                            }
                        }
                        if(winnerCardObject.GetComponent("Row4"))
                        {
                            if(e.card == "row4" || e.card == "comb14" || e.card == "comb24" || e.card == "comb34")
                            {
                                e.winner = true;     
                            }
                        }
                        // COLUMNS
                        if(winnerCardObject.GetComponent("Column2"))
                        {
                            if(e.card == "column2" || e.card == "even" || e.card == "num")
                            {
                                e.winner = true;    
                            }
                        }
                        if(winnerCardObject.GetComponent("Column3"))
                        {
                            if(e.card == "column3" || e.card == "odd" || e.card == "num")
                            {
                                e.winner = true;    
                            }
                        }
                        if(winnerCardObject.GetComponent("Column4"))
                        {
                            if(e.card == "column4" || e.card == "even" || e.card == "num")
                            {
                                e.winner = true;    
                            }
                        }
                        if(winnerCardObject.GetComponent("Column5"))
                        {
                            if(e.card == "column5" || e.card == "odd" || e.card == "num")
                            {
                                e.winner = true;   
                            }
                        }
                        if(winnerCardObject.GetComponent("Column6"))
                        {
                            if(e.card == "column6" || e.card == "even" || e.card == "num")
                            {
                                e.winner = true;   
                            }
                        }
                        if(winnerCardObject.GetComponent("Column7"))
                        {
                            if(e.card == "column7" || e.card == "odd" || e.card == "num")
                            {
                                e.winner = true;  
                            }
                        }
                        if(winnerCardObject.GetComponent("Column8"))
                        {
                            if(e.card == "column8" || e.card == "even" || e.card == "num")
                            {
                                e.winner = true;    
                            }
                        }
                        if(winnerCardObject.GetComponent("Column9"))
                        {
                            if(e.card == "column9" || e.card == "odd" || e.card == "num")
                            {
                                e.winner = true;  
                            }
                        }
                        if(winnerCardObject.GetComponent("Column10"))
                        {
                            if(e.card == "column10" || e.card == "even" || e.card == "num")
                            {
                                e.winner = true;   
                            }
                        }
                        if(winnerCardObject.GetComponent("ColumnJ"))
                        {
                            if(e.card == "columnj" || e.card == "odd" || e.card == "face")
                            {
                                e.winner = true;  
                            }
                        }
                        if(winnerCardObject.GetComponent("ColumnQ"))
                        {                           
                            if(e.card == "columnq" || e.card == "even" || e.card == "face")
                            {
                                e.winner = true;   
                            }
                        }
                        if(winnerCardObject.GetComponent("ColumnK"))
                        {
                            if(e.card == "columnk" || e.card == "odd" || e.card == "face")
                            {
                                e.winner = true;   
                            }
                        }
                        if(winnerCardObject.GetComponent("ColumnA"))
                        {
                            if(e.card == "columna" || e.card == "even" || e.card == "face")
                            {
                                e.winner = true;  
                            }
                        }
                    }
                }
            }
            seApi.BulkPointsForWinners();              // API - BulkPointsForWinners()
        }   
        else
        {
            gamemanager.info_Text.text = "The winning card has not been drawn yet"; 
        }                                               
    }  
}