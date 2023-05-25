using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	// SCRIPTS
	SeApi seApi;
    RoundManager roundmanager;
    ListManager listmanager;
	TwitchConnect twitchconnect;
	ChatCommand chatcommand;

	// DECK 
	public List<Card> deck;
	public Transform[] cardSlots;
	public bool[] availableCardSlots;

	// UI
	public Text multiplier_Text;					
	public int multiplier = 20;

	public Text info_Text;							
    public bool isBetEnabled = false; 	

	public Button drawCardButton;
	public Button startRoundButton;
	public Texture2D bountyCursor;

	string startRoundMsg = "A kör elkezdődött. Rakja meg tétjeit.";

	// BOUNTY EVENT
	bool bountyEvent = false;
	public GameObject gameCanvas , bountyCanvas , bountycards;
	public Button leftBountyButton , midBountyButton , rightBountyButton;
	public Transform leftBountySlot , midBountySlot , rightBountySlot;
	bool isLeftBountySelected , isMidBountySelected , isRightBountySelected;
	Card leftBountyCard , midBountyCard , rightBountyCard;
	Card randomLeftBountyCard , randomMidBountyCard , randomRightBountyCard;


	public void Start()
	{
		seApi = GameObject.FindGameObjectWithTag("SE_API").GetComponent<SeApi>();
        roundmanager = GameObject.FindGameObjectWithTag("RoundManager").GetComponent<RoundManager>();
        listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();
		twitchconnect = GameObject.FindGameObjectWithTag("TwitchConnect").GetComponent<TwitchConnect>();
		chatcommand = GameObject.FindGameObjectWithTag("ChatCommand").GetComponent<ChatCommand>();
	}

	public void Update()
	{
		if(bountyEvent == true) 													// BountyEvent Begins
		{
			bountyCanvas.SetActive(true); 											
			gameCanvas.SetActive(false); 											
			Cursor.SetCursor(bountyCursor, Vector2.zero, CursorMode.ForceSoftware);
			if(isLeftBountySelected || isMidBountySelected || isRightBountySelected)  
			{
				StartCoroutine(CoroutineForWantedCard());
			}
		}
		else 																		// BountyEvent Ends 
		{
			gameCanvas.SetActive(true); 											
			bountyCanvas.SetActive(false); 											
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

			EnableWantedButtons();  												// Set Buttons Back
			isLeftBountySelected = false;                    						// leftBountyButton
			isMidBountySelected = false;											// midBountyButton
			isRightBountySelected = false;											// rightBountyButton
		}
	}
	
	// Start The Round
	public async void StartRound()
    {
		twitchconnect.ClearLogs();						
        isBetEnabled = true;                       		// Enable The Chat Commands
        seApi.SendBotMsg(startRoundMsg);  		 		// Chat - startRoundMsg
		info_Text.text = startRoundMsg;       			
		startRoundButton.interactable = false;			
		roundmanager.objectTimerText.SetActive(true);	// Show The Timer
        roundmanager.isTimerRunning = true;             // Start The timer

		roundmanager.helpToRemovePoints = false;		// roundmanager - seApi.set_points()
		await seApi.GetLeaderboard();					// API - Leaderboard For RemovePointsFromPlayers()
    }

	// Draw A Card From The Deck
	public void DrawCard()
	{
		Card randomCard = deck[Random.Range(0, deck.Count)];  													// Pick A Random Card
		for (int i = 0; i < availableCardSlots.Length; i++)
		{
			if (availableCardSlots[i] == true) 																	// If Slots Are Available
			{
                Card pickedCard = Instantiate(randomCard,cardSlots[i].position,Quaternion.identity); 			// Instantiate A Card
				pickedCard.transform.parent = cardSlots[i].transform; 											// Make The Slot[i] The Card Parent
				pickedCard.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID(cardSlots[i].name);  // Sort The Cards Layer 
				availableCardSlots[i] = false; 																	// Set The Slot[i] NOT Avaialable
				DrawnCardMultiplier(pickedCard); 																// Check Which Card Drawn And Manage The Multiplier
				return;
			}
		}
	}

	// Shuffle The Deck
	public void Shuffle()
	{
		DestroyAll("Card"); 									// Destroy All Gameobjects With The Tag "Card"
		SetMultiplierToDefault(); 								// Set Multiplier To Default
		for(int i = 0; i < availableCardSlots.Length; i++)
		{
			availableCardSlots[i] = true; 						// Set The Slots To Available
		}

		listmanager.DeleteList();                               // Clear The Entries List
		listmanager.DeleteLeaderBoard();						// Clear The Leaderboard List

		isBetEnabled = false;									// Disable ChatCommands
        roundmanager.timeRemaining = 60;                        // Set Time To x sec
        roundmanager.winnerChat.text = "";                      // Clear The Winners Chat
		roundmanager.isTimerRunning = false; 					// Set IsTimerRunning To False

		info_Text.text = "";                                    // Clear The info_Text
		roundmanager.objectTimerText.SetActive(false);          // Disable The Timer text

		roundmanager.winnerCount = 0; 							// winnerCount Reset
		roundmanager.totalPointWon = 0;                       	// totalPointWon Reset
		roundmanager.objectWinnerCount.SetActive(false); 		// Disable WinnerCountText

		drawCardButton.interactable = true;
		startRoundButton.interactable = true;
	}

	// Destroy All Gameobjects With The Tag "Card"
	public void DestroyAll(string tag)
    {
		foreach (var gameObj in GameObject.FindGameObjectsWithTag("Card"))
		{
			Destroy(gameObj);
		}
    }

	// Add +x To The Multiplier
	public void UpdateMultiplier(int x)
	{
		multiplier += x;
		multiplier_Text.text = multiplier.ToString() + "x";
	}

	// Set Multiplier To Default
	public void SetMultiplierToDefault()
	{
		multiplier = 20;
		multiplier_Text.text = "";
	}

	// Manage The Drawn Card
	public void DrawnCardMultiplier(Card pickedCard)
	{
		if((pickedCard.GetComponent("RegularCard")))															
		{
			UpdateMultiplier(0);												// Add +0x To The Multiplier
			SpriteRenderer sr = pickedCard.GetComponent<SpriteRenderer>();  	// Get The Drawn RegularCard SpriteName
			roundmanager.winnerCard = sr.sprite.name; 							// Set The SpriteName To WinnerCard
			info_Text.text = "Card - " + roundmanager.winnerCard;			
			roundmanager.winnerCardObject = pickedCard; 						// Set The WinnerCard
			drawCardButton.interactable = false;								
			roundmanager.RoundWinners(); 										// Manage The Winners
		}
		else if((pickedCard.GetComponent("TwoXCard")))															
		{
			UpdateMultiplier(2);						 						// Add +2x To The Multiplier										
		}
		else if((pickedCard.GetComponent("ThreeXCard")))															
		{
			UpdateMultiplier(3);												// Add +3x To The Multiplier													
		}
		else if((pickedCard.GetComponent("FiveXCard")))															
		{
			UpdateMultiplier(5);												// Add +5x To The Multiplier													
		}
		else if((pickedCard.GetComponent("TenXCard")))															
		{
			UpdateMultiplier(10);					 							// Add +10x To The Multiplier														
		}
		else if((pickedCard.GetComponent("DoubleCard")))															
		{
			UpdateMultiplier(multiplier);										// Double The Multiplier														
		}
		else if((pickedCard.GetComponent("BountyCard")))															
		{
			bountyEvent = true;													// Bounty Event Begins		 	
		}
	}

	// Create 3 Random WantedCards 
	public void CreateWantedCards()
	{
		randomLeftBountyCard = deck[Random.Range(52, 95)]; 													// Random Bonus Card
		randomMidBountyCard = deck[Random.Range(52, 95)];											
		randomRightBountyCard = deck[Random.Range(52, 95)];										

        leftBountyCard = Instantiate(randomLeftBountyCard,leftBountySlot.position,Quaternion.identity); 	// Instantiate A Random Bonus Card
		leftBountyCard.transform.parent = bountycards.transform; 	 										// Set Bonus Cards Parent To BountyCards

        midBountyCard = Instantiate(randomMidBountyCard,midBountySlot.position,Quaternion.identity); 
		midBountyCard.transform.parent = bountycards.transform; 

		rightBountyCard = Instantiate(randomRightBountyCard,rightBountySlot.position,Quaternion.identity); 
		rightBountyCard.transform.parent = bountycards.transform; 
	}

	// OnClick Event For BountyButtons
	public void OnClickLeftWantedButton()
	{
		isLeftBountySelected = true;
		CreateWantedCards();
		DisableWantedButtons();
	}

	public void OnClickMidWantedButton()
	{
		isMidBountySelected = true; 
		CreateWantedCards();
		DisableWantedButtons();
	}

	public void OnClickRightWantedButton()
	{
		isRightBountySelected = true;
		CreateWantedCards();
		DisableWantedButtons();
	}

	// Called in Update() When Bounty Is Selected
	IEnumerator CoroutineForWantedCard()
    {
		yield return new WaitForSeconds(3);							// Wait 3s
		if(leftBountyCard != null && isLeftBountySelected) 			// Check If LeftBountyCard Exist And Selected
		{
			DrawnCardMultiplier(leftBountyCard); 					// Add The BountyCard x To Multiplier
		}
		else if(midBountyCard != null && isMidBountySelected)
		{
			DrawnCardMultiplier(midBountyCard);
		}
		else if(rightBountyCard != null && isRightBountySelected)
		{
			DrawnCardMultiplier(rightBountyCard);
		}
		bountyEvent = false;										// BountyEvent Ends
		foreach(Transform child in bountycards.transform) 			// Delete The BountyCards Gameobjects
		{
			if(child.gameObject != null)
			{
				Destroy(child.gameObject);
			}
		}
    }


	// Disable Wanted Buttons
	public void DisableWantedButtons()
	{
		leftBountyButton.interactable = false; 					
		leftBountyButton.enabled = false; 						
		leftBountyButton.gameObject.SetActive(false); 		

		midBountyButton.interactable = false; 
		midBountyButton.enabled = false; 
		midBountyButton.gameObject.SetActive(false);

		rightBountyButton.interactable = false; 
		rightBountyButton.enabled = false; 
		rightBountyButton.gameObject.SetActive(false); 
	}
	
	// Enable Wanted Buttons
	public void EnableWantedButtons()
	{
		leftBountyButton.interactable = true;
		leftBountyButton.enabled = true;
		leftBountyButton.gameObject.SetActive(true); 

		midBountyButton.interactable = true; 
		midBountyButton.enabled = true; 
		midBountyButton.gameObject.SetActive(true);

		rightBountyButton.interactable = true; 
		rightBountyButton.enabled = true; 
		rightBountyButton.gameObject.SetActive(true); 
	}
}
