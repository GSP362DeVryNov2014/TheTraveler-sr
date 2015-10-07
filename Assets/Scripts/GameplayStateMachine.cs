/*******************************************************************************
 *
 *  File Name: GameplayStateMachine.cs
 *
 *  Description: The brains of the program, controls the flow
 *
 *******************************************************************************/
using GSP.Char;
using GSP.Core;
using GSP.Entities.Neutrals;
using GSP.Items;
using GSP.Tiles;
using System;
using UnityEngine;
using UnityEngine.UI;
using GSP.Items.Inventories;
using GSP.Char.Allies;
using GSP.Entities.Friendlies;

namespace GSP
{

    /*******************************************************************************
     *
     * Name: GameplayStateMachine
     * 
     * Description: Controls the flow of the game through a finite state machine.
     * 
     *******************************************************************************/
    public class GameplayStateMachine : MonoBehaviour 
	{
		// The states that the GPSM runs on
		enum GamePlayState
		{
			BeginTurn,
			RollDice,
			CalcDistance,
			DisplayDistance,
			SelectPathToTake,
			DoAction,
			EndTurn,
			EndGame
		} //end GamePlayState

		// State Machine management variables
		GamePlayState gamePlayState;            	// The current state
		bool canInitAfterStart;			         	// Initialising values after Start()
		bool canRunEndStuff;                    	// Whether the end scene stuff should be ran during that state
        bool isAlliesOpen;                          // Whether the allies window is open
		int guiDiceDistVal;	                		// The dice value which is then onverted into a distance value
        bool isMovementInitialized;                 // Whether the movement class is initialised

		// State Machine input/output variables
		int guiNumOfPlayers; 	           			// The number of players playing
		string mapEventResultString; 		    	// If mapEvent is Item, what Resource is picked up? Null if not a resource event
		int guiPlayerTurn;  	                	// Whos turn is it
		int guiMaxWeight;							// Maximum weight player can carry
		int guiCurrentWeight;						// Total weight of player at the moment
        bool isPlayerAI;                            // Whether the player is an AI

		// HUD Elements
        // Current Player
		Text guiPlayerName;							// Name of current player
		Text guiTurnText; 			       			// The turn or event currently happening
		Text guiGold;		            			// The player's Gold Value
       	Text guiWeight;		                		// The player's actual weight
		Button actionButton;						// Button user presses to advance turn phase
		Text actionButtonText;						// Text of actionButton 
		bool actionButtonActive;					// Whether the action button is active or not
		GameObject acceptPanel;						// Panel for accepting an ally
		GameObject pauseMenu;						// Panel for pausing game
		GameObject instructionsSet;					// Panel that displays instructions
		int instructionsProgress;					// What slide should instructions show
		bool isPaused;								// Whether the game is paused or not
		// All Players
		GameObject imageParent;						// Panel with all player images
		GameObject textParent;						// Panel with all player names and gold
        // Inventory
        PlayerInventory inventory;                  // The inventory script for the player's Inventory
        AllyInventory allyInventory;                // The inventory script for the ally's Inventory
        RecycleBin recycleInventory;                // The inventory script for the recycle bin's Inventory
        // Aliies
        AllyTable allyTable;                        // The ally table script for the AllyTable

		// Game Objects
		Die die;       			                    // The Die to be used in the game
        GUIMovement guiMovement;		            // The GUIMovement component reference
		MapEvent guiMapEvent;						// The MapEvent component reference
		string mapEventResult;						// Result of the map event

        // Runs when the object if first instantiated, because this object will occur once through the game,
        // these values are the beginning of game values
        // Note: Values should be updated at the EndTurn State
        void Start()
		{
            //TODO: Damien: Replace Tile stuff later
            // Clear the tile dictionary
            TileDictionary.Clean();
            // Set the dimensions and generate/add the tiles
            TileManager.SetDimensions(64, 20, 16);
            TileManager.GenerateAndAddTiles();

			// Get HUD elements
            guiPlayerName = GameObject.Find("CurrentPlayer/PlayerNamePanel/PlayerName").GetComponent<Text>();
            guiTurnText = GameObject.Find("CurrentPlayer/TurnPhasePanel/TurnPhase").GetComponent<Text>();
            guiGold = GameObject.Find("CurrentPlayer/WeightGold/Gold").GetComponent<Text>();
            guiWeight = GameObject.Find("CurrentPlayer/WeightGold/Weight").GetComponent<Text>();
            actionButton = GameObject.Find("CurrentPlayer/ActionButton").GetComponent<Button>();
            actionButtonText = GameObject.Find("CurrentPlayer/ActionButton").GetComponentInChildren<Text>();
			acceptPanel = GameObject.Find("Accept");
			pauseMenu = GameObject.Find ("PauseMenu");
			instructionsSet = GameObject.Find ("Instructions");
            imageParent = GameObject.Find("AllPlayers/ImageOrganizer");
            textParent = GameObject.Find("AllPlayers/TextOrganizer");
            inventory = GameObject.Find("PlayerInventory").GetComponent<PlayerInventory>();
            allyInventory = GameObject.Find("AllyInventory").GetComponent<AllyInventory>();
            recycleInventory = GameObject.Find("RecycleBin").GetComponent<RecycleBin>();
            allyTable = GameObject.Find("Allies").GetComponent<AllyTable>();
			GameObject.Find ("Canvas").transform.Find ("Instructions").gameObject.SetActive (false);
			actionButtonActive = true;
			isPaused = false;
			instructionsProgress = 1;

            // Disable the other panels by default
            acceptPanel.SetActive(false);
			pauseMenu.SetActive (false);
            inventory.gameObject.SetActive(false);
            allyInventory.gameObject.SetActive(false);
            recycleInventory.gameObject.SetActive(false);
            allyTable.gameObject.SetActive(false);

            // Running the end stuff defaults to true
            canRunEndStuff = true;

            // The movement class is not initialised
            isMovementInitialized = false;

            // The ally window are closed by default
            isAlliesOpen = false;

            // Get the number of players
            guiNumOfPlayers = GameMaster.Instance.NumPlayers;

            // Set the turn
            guiPlayerTurn = GameMaster.Instance.Turn;

			// Set starting values             		            
			guiDiceDistVal = 0;

            // The player is not an AI by default
            isPlayerAI = false;

			// Create a die
			die = new Die();

            // Reseed the random number generator
            die.Reseed(Environment.TickCount);

			// Get movement and map event components
			guiMovement = GameObject.Find("Canvas").GetComponent<GUIMovement> ();
			guiMapEvent = GameObject.Find("Canvas").GetComponent<MapEvent> ();

			// There isn't a map event in the beginning
			mapEventResultString = string.Empty;

			// Initialise other things after Start() runs
			canInitAfterStart = true;
            
            // Set the state to the BeginTurn state
            gamePlayState = GamePlayState.BeginTurn;
		} // end Start

		// Initialises things after the Start() function runs
        void InitAfterStart()
		{
            // Add the player instances
            AddPlayers(guiNumOfPlayers);
            
            // Add the player instances
			AddItems(guiNumOfPlayers);

            // The game is no longer new
            GameMaster.Instance.IsNew = false;

			// Update the "All Players" HUD to match active players
			// Disable unused players
			for(int i = 3; i > guiNumOfPlayers - 1 ; i--)
			{
				imageParent.transform.GetChild(i).gameObject.SetActive(false);
				textParent.transform.GetChild(i).gameObject.SetActive(false);
			} //end for

			// Update the remaining fields with name and gold
			for(int i = 0; i < guiNumOfPlayers; i++)
			{
				// Set the player's merchant entity
				Merchant playerMerchant = (Merchant)GameMaster.Instance.GetPlayerScript(i).Entity;

				// Get image component and update with player sprite
				imageParent.transform.GetChild(i).GetComponent<Image>().sprite =
					playerMerchant.GetSprite(0);

				// Get text component and update with player name and gold
				textParent.transform.GetChild(i).GetComponent<Text>().text =
					GameMaster.Instance.GetPlayerName(i) + " - " + playerMerchant.Currency;
			} //end for

            // Change the interface element's colours
            ChangeColor();
		} // end InitAfterStart

        // Adds the players to the game
        void AddPlayers(int numPlayers)
		{
			// Check is this is a new game
            if (GameMaster.Instance.IsNew)
            {
                // Create the players
                GameMaster.Instance.CreatePlayers();
            } // end if
            else
            {
                // Load the players
                GameMaster.Instance.LoadPlayers();
            } // end else

			// Loop over the number of players to add their instances
			for (int count = 0; count < numPlayers; count++)
			{
				// Give players an animator
				Animator animator = GameMaster.Instance.GetPlayerObject(count).AddComponent<Animator> ();
				animator.runtimeAnimatorController = 
					Resources.Load ("Animations/Player" + GameMaster.Instance.GetPlayerSprite(count)) 
						as RuntimeAnimatorController;

				// Get the player's script
				Player playerScript = GameMaster.Instance.GetPlayerScript(count);
				
				// Set the players's sprite sheet sprites
				playerScript.SetCharacterSprites(GameMaster.Instance.GetPlayerSprite(count));
				
				// Set the player's facing
				playerScript.Face(FacingDirection.South);

                // Set the player's player number
                ((Merchant)playerScript.Entity).PlayerNumber = count;

                // Create the list of items for the player
                inventory.CreatePlayerItemList(count);

                // Hard coded for a single ally right now
                // Create the inventory list for the ally
                allyInventory.CreateAllyItemList(count + 7);
			} // end for
		} // end AddPlayers

        // Adds the starting items to the players
        void AddItems(int numPlayers)
		{
            // Check is this is a new game
            if (GameMaster.Instance.IsNew)
            {
                // Loop over the number of players to give them the items
                for (int count = 0; count < numPlayers; count++)
                {
                    // Get the starting items; could use indices, but this way is future proof from moving the items around
                    // in the database
                    Item weapon = ItemDatabase.Instance.Items.Find(item => item.Type == WeaponType.Sword.ToString());
                    Item legs = ItemDatabase.Instance.Items.Find(item => item.Type == ArmorType.Chainlegs.ToString());

                    // Add the items to the player's inventory
                    inventory.AddItem(0, count, weapon.Id, SlotType.Inventory);
                    inventory.AddItem(0, count, legs.Id, SlotType.Inventory);

                    // Equip the items for the player
                    inventory.EquipItem(count, (Equipment)weapon);
                    inventory.EquipItem(count, (Equipment)legs);
                } // end for
            } // end if
		} // end AddItems

        // Gets the player's values for display on the In-Game UI; At the beginning of each turn the values are grabbed
        // from each player and stored into respective GUI variables.
        void GetPlayerValues()
		{
            // Set the player's merchant entity
            Merchant playerMerchant = (Merchant)GameMaster.Instance.GetPlayerScript(guiPlayerTurn).Entity;

			//Set Input variables
			// Total Weight
			guiCurrentWeight = playerMerchant.TotalWeight;

			// Max Weight
			guiMaxWeight = playerMerchant.MaxWeight;

			// Set HUD elements
			// Name
			guiPlayerName.text = GameMaster.Instance.GetPlayerName(guiPlayerTurn);

            // Gold value
            guiGold.text = "Gold: " + playerMerchant.Currency;

			// The Weight Values; Current weight / Max weight
            guiWeight.text = "Weight: " + guiCurrentWeight + "/" + guiMaxWeight;

            // Determine if the current player is an AI
            isPlayerAI = GetCurrentPlayer().GetComponent<Player>().IsAI;
		} // end GetPlayerValues

        // Updates the state machine and things; runs every frame
        void Update()
        {
            // TODO: Damien: This is disabled until it can be properly implemented into the game.
            //if (Input.GetKeyDown(KeyCode.M) && !isPaused)
            //{
            //    // Save the players
            //    GameMaster.Instance.SavePlayers();
                
            //    // Save the resources
            //    GameMaster.Instance.SaveResources();

            //    // Finally, tell the GameMaster to load the end scene
            //    GameMaster.Instance.LoadLevel("Market");
            //}
            
            // This was set to true at the end of Start()
            if (canInitAfterStart)
            {
                // DON'T change this value to true. ever! This is meant to only run once.
                canInitAfterStart = false;

                // Initialise stuff after start
                InitAfterStart();
            } // end if
			// Run State Machine only if not paused
            else if(!isPaused)
            {
                // Update any values that affect GUI before creating GUI
                StateMachine();
            } // end else if
        } // end Update

        // Controls the flow of the game through various states
        void StateMachine()
		{
            // Switch over the GamePlayState's
			switch (gamePlayState) 
			{
                // The player begins their turn.
                case GamePlayState.BeginTurn:
                    {
                        // Get the player's values and update them
                        GetPlayerValues();

						// Clear the map event text
						mapEventResult = "";

						// Check if the player isn't an AI
                        if (isPlayerAI)
                        {
                            // Disable the buttons for AI as they don't use them
                            actionButton.interactable = false;
                            actionButtonActive = false;
                            GameObject.Find("CurrentPlayer/PauseButton").GetComponent<Button>().interactable = false;
                            GameObject.Find("CurrentPlayer/InvAlly/Inventory").GetComponent<Button>().interactable = false;
                            GameObject.Find("CurrentPlayer/InvAlly/Ally").GetComponent<Button>().interactable = false;
                        } // end if
                        else
                        {
                            // Otherwise, Verify the action button is enabled
                            actionButton.interactable = true;
                            actionButtonActive = true;
                            // Enable the pause, inventory, and ally window buttons
                            GameObject.Find("CurrentPlayer/PauseButton").GetComponent<Button>().interactable = true;
                            GameObject.Find("CurrentPlayer/InvAlly/Inventory").GetComponent<Button>().interactable = true;
                            GameObject.Find("CurrentPlayer/InvAlly/Ally").GetComponent<Button>().interactable = true;
                        } // end else

                        // The movement class is not initialised for the current player
                        isMovementInitialized = false;

                        // Switch the state to the RollDie state
                        gamePlayState = GamePlayState.RollDice;
                        break;
                    } // end Case BeginTurn
                
                // The player rolls the dice
                case GamePlayState.RollDice:
                    {
                        // Set the turn text
                        guiTurnText.text = "Player " + guiPlayerName.text + ", roll dice.";

                        // Set the action button's text to roll dice
                        actionButtonText.text = "Roll Dice";
				        break;
                    } // end Case RollDice
                
                // Calculate the allowed disance to travel
                case GamePlayState.CalcDistance:
                    {
                        // Set the turn text
                        guiTurnText.text = "Calculating distance...";

                        // Get the dice's value and calculate the allowed movement
                        guiDiceDistVal = guiDiceDistVal * (guiMaxWeight - guiCurrentWeight) / guiMaxWeight;

                        // Change the state to the DisplayDistance state
                        gamePlayState = GamePlayState.DisplayDistance;
                        break;
                    } // end Case CalcDistance
                
                // Display the distance allowed to travel
                case GamePlayState.DisplayDistance:
                    {
                        // Set the player's Player script
                        Player player = GameMaster.Instance.GetPlayerScript(guiPlayerTurn);

                        // Set the state text
                        guiTurnText.text = "Displaying distance...";

                        // Display the movement arrows
                        guiMovement.InitThis(player, guiDiceDistVal, isPlayerAI);

                        // The movement class is now initialised for the current player
                        isMovementInitialized = true;

						// Make sure the player isn't an AI
                        if (!isPlayerAI)
                        {
                            // Make the action button clickable again
                            actionButton.interactable = true;
                            actionButtonActive = true;
                        } // end if

						// Change the state to the SelectPathToTake state
						gamePlayState = GamePlayState.SelectPathToTake;
                        break;
                    } // end Case DisplayDistance
                
                // The player selects their path to take on the map
                case GamePlayState.SelectPathToTake:
                    {
                        // Set the state text
                        guiTurnText.text = "Movement Left: " + guiDiceDistVal + "\nPress 'End Turn' when finished.";

                        // Set the action button's text to end turn
                        actionButtonText.text = "End Turn";

                        // Update the value of allowed travel distance upon the player pressing a move button
                        guiDiceDistVal = guiMovement.RemainingTravelDistance;
                        break;
                    } // end Case SelectPathToTake
                
                // Deal with a MapEvent action
                case GamePlayState.DoAction:
                    {
                        // Set the state text
                        guiTurnText.text = "Resolving map event...";

						// Get and resolve the map event
						mapEventResult = guiMapEvent.DetermineEvent(guiPlayerTurn, die);

						// If it's an ally, allow player to accept
						if(mapEventResult.Contains("Ally"))
						{
							// Enable the panel
							acceptPanel.SetActive(true);

                            // Check if the player is an AI
                            if (isPlayerAI)
                            {
                                // Disable the buttons on the accept panel
                                acceptPanel.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = false;
                                acceptPanel.transform.GetChild(3).gameObject.GetComponent<Button>().interactable = false;
                            } // end if
                            else
                            {
                                // Otherwise re-enable the buttons for the player
                                acceptPanel.transform.GetChild(2).gameObject.GetComponent<Button>().interactable = true;
                                acceptPanel.transform.GetChild(3).gameObject.GetComponent<Button>().interactable = true;
                            } // end else
					
							// Update the text
							Text eventText = GameObject.Find("EventText").GetComponent<Text>();
							eventText.text = "Porter ally found.\nWould you like to add them?";
						} //end if
						else 
				   		{
							// Return the result for now
							guiTurnText.text = mapEventResult;

                            // Make sure the player isn't an AI
                            if (!isPlayerAI)
                            {
                                // Make the action button clickable again
                                actionButton.interactable = true;
                                actionButtonActive = true;
                            } // end if
						} //end if

						// Change the state to the EndTurn state
						gamePlayState = GamePlayState.EndTurn;
                        break;
                    } // end Case DoAction
                
                // The player has ended their turn
                case GamePlayState.EndTurn:
                    {
						// Get the player's values
						GetPlayerValues();

						// Set Action Button text to prompt next player
						actionButtonText.text = "Next Player";
                    
						// Clear the board of any highlight tiles
                        Highlight.ClearHighlight();
                        break;
                    } // end Case EndTurn
                
                // Its the end of the game
                case GamePlayState.EndGame:
                    {
                        // Set the state text
                        guiTurnText.text = "Universe Ending";

                        // Check whether to run the end stuff
                        if (canRunEndStuff)
                        {
                            // Only run this once
                            canRunEndStuff = false;

                            // Force the inventory closed
                            inventory.gameObject.SetActive(false);

                            // Force the allies window and ally inventory closed
                            allyTable.gameObject.SetActive(false);
                            allyInventory.gameObject.SetActive(false);

                            // Loop through and sell the players's resources and their ally's resources
                            // Note: Ally resources are not setup to pickup or sell right now
                            for (int playerSellIndex = 0; playerSellIndex < guiNumOfPlayers; playerSellIndex++)
                            {
                                // Get the player's merchant entity
                                Merchant playerMerchant = (Merchant)GameMaster.Instance.GetPlayerScript(playerSellIndex).Entity;

                                Porter allyPorter;  // The ally's porter entity

                                // Check if the player has an ally
                                if (playerMerchant.NumAllies > 0)
                                {
                                    // Check if the ally type is porter
                                    if (playerMerchant.GetAlly(0).GetComponent<PorterMB>() != null)
                                    {
                                        // THe ally type is porter to get its entity
                                        allyPorter = (Porter)playerMerchant.GetAlly(0).GetComponent<PorterMB>().Entity;

                                        // Sell the ally's resources
                                        allyPorter.SellResources();

                                        // Transfer the currency to the player's merchant
                                        allyPorter.TransferCurrency<Merchant>(playerMerchant, allyPorter.Currency);
                                    } // end if playerMerchant.GetAlly(0).GetComponent<PorterMB>() != null
                                } // end if

                                // We need to access the character script at the given index and sell the resources
                                playerMerchant.SellResources();
                            } // end for

                            // Save the players
                            GameMaster.Instance.SavePlayers();

                            // Finally, tell the GameMaster to load the end scene
                            GameMaster.Instance.LoadLevel("EndScene");
                        }
                        break;
                    } // end Case EndGame
                
                // Shouldn't reach here, this is reached if the state isn't listed above
                default:
                    {
                        Debug.LogWarning("Hit the default case in GamePlayStateMachine!");
                        break;
                    } // end Case default
            } // end switch gamePlayState
		} // end StateMachine
		
		// Gets the guiDiceDistVal
		public int GetDist()
		{
			return guiDiceDistVal;
		} // end GetDist
		
		// Gets the current state
        public int GetState()
		{
            return (int)gamePlayState;
		} // end GetState
		
		// Changes the current state to the next state
        public void NextState()
		{
			gamePlayState++;
		} // end NextState

		// Changes the state to EndGame which ends the game.
		public void EndGame()
		{
			gamePlayState = GamePlayState.EndGame;
		} // end EndGame
        
		// Resets values (For testing, not useful practically)
        void ResetValues()
		{
			actionButtonText.text = "N/A";
            guiGold.text = "-1";
            guiWeight.text = "-1";
            guiMaxWeight = -1;
            guiDiceDistVal = 0;
		} // end ResetValues

        // Change the colour of the interface elements
        void ChangeColor()
        {
            // Get the player's color
            Color playerColor = Utility.InterfaceColorToColor(GameMaster.Instance.GetPlayerColor(guiPlayerTurn));

            // Set the interface element's colours to the current player's colour
            GameObject.Find("Background").GetComponent<Image>().color = playerColor;
            acceptPanel.transform.GetChild(0).GetComponent<Image>().color = playerColor;
            pauseMenu.transform.GetChild(0).GetComponent<Image>().color = playerColor;
        } // end ChangeColor

        // Gets the current player from the players list
        public GameObject GetCurrentPlayer()
		{
			return	GameMaster.Instance.GetPlayerObject(guiPlayerTurn);
		} // end GetCurrentPlayer

		// Button Functions
		// Action Button - Used to move the turn through its phases
		public void ActionButton()
		{
            if(gamePlayState == GamePlayState.RollDice)
			{
                // Roll the die
				guiDiceDistVal = die.Roll(2, 5);
				
				//Play dieRoll sound
				AudioManager.Instance.PlayDie();

				// Set Action Button text while disabled
				actionButtonText.text = "Processing...";

				//Disable button while distance is being calculated
				actionButton.interactable = false;
				actionButtonActive = false;

				// Change to the CalcDistance State
				gamePlayState = GamePlayState.CalcDistance;
			} //end if
			else if(gamePlayState == GamePlayState.SelectPathToTake)
			{
				// Set Action Button text while disabled
				actionButtonText.text = "Processing...";

				//Disable buttons while turn is ending
				actionButton.interactable = false;
				actionButtonActive = false;
				guiMovement.DisableButtons();

				// Change the state to the DoAction state
				gamePlayState = GamePlayState.DoAction;
			} //end else if
			else if(gamePlayState == GamePlayState.EndTurn)
			{
				// Close the inventory if it's open
                if (inventory.IsOpen)
                {
                    inventory.IsOpen = false;
                    inventory.gameObject.SetActive(false);
                } // end if

                // Close the ally's window if it's open
                if (isAlliesOpen)
                {
                    isAlliesOpen = false;
                    allyTable.gameObject.SetActive(false);
                } // end if

                // Close the ally's inventory if it's open
                if (allyInventory.IsOpen)
                {
                    allyInventory.IsOpen = false;
                    allyInventory.gameObject.SetActive(false);
                } // end if
                
                // Update the turn
				guiPlayerTurn = GameMaster.Instance.NextTurn();

                // Change the interface element's colours
                ChangeColor();

				// Change the state to the BeginTurn state
				gamePlayState = GamePlayState.BeginTurn;
			} //end else if
		} //end ActionButton

		// Inventory button - Displays equipment and resources
		public void ShowInventory()
		{
			// Toggle the inventory
            inventory.IsOpen = !inventory.IsOpen;

            // Check if the inventory is open
            if (inventory.IsOpen)
            {
                // Set the inventory window up before displaying it
                inventory.SetPlayer(guiPlayerTurn);
                
                // Open the inventory window
                inventory.gameObject.SetActive(true);
            }
            else
            {
                // Otherwise, close the inventory window
                inventory.gameObject.SetActive(false);
            } // end if
		} // end ShowInventory
		
		// Ally button - Displays allies
		public void ShowAllies()
		{
			// Toggle the allies window
            isAlliesOpen = !isAlliesOpen;

            // Check if the ally window is open
            if (isAlliesOpen)
            {
                // Set the ally window up before displaying it
                allyTable.SetPlayer(guiPlayerTurn);
                
                // Open the ally window
                allyTable.gameObject.SetActive(true);
            } // end if
            else
            {
                // Otherwise, close the ally window
                allyTable.gameObject.SetActive(false);
            } // end else
		} // end ShowAllies

        // Ally inventory button - Displays the ally's inventory
        public void ShowAllyInventory()
        {
            // Toggle the ally's inventory window
            allyInventory.IsOpen = !allyInventory.IsOpen;

            // Check if the ally window is open
            if (allyInventory.IsOpen)
            {
                // Set the ally window up before displaying it
                allyInventory.SetPlayer(guiPlayerTurn);

                // Open the ally window
                allyInventory.gameObject.SetActive(true);
            } // end if
            else
            {
                // Otherwise, close the ally window
                allyInventory.gameObject.SetActive(false);

                // Close the inventory window if it's open
                if (inventory.IsOpen)
                {
                    ShowInventory();
                } // end if
            } // end else
        } // end ShowAllyInventory

        // Trade with Player button - Toggles the player's inventory and allies windows
        public void AllyTradeWithPlayer()
        {
            // Check if the allies window is open without the player inventory window
            if (isAlliesOpen && !inventory.IsOpen)
            {
                // Toggle the ally window
                ShowAllies();

                // Toggle the inventory
                ShowInventory();
            } // end if
            // Check if the allies window is closed and the player inventory is opened
            else if (!isAlliesOpen && inventory.IsOpen)
            {
                // Toggle the inventory
                ShowInventory();

                // Toggle the ally window
                ShowAllies();
            } // end else if
            // Check if both windows are open at the same time
            else if (isAlliesOpen && inventory.IsOpen)
            {
                // Toggle the ally window
                ShowAllies();
            } // end else if
        } // end AllyTradeWithPlayer

        // Accept/Cancel button - Handles the Accept/Cancel clicks
        public void AcceptCancelRecycle(bool isRecycling)
        {
            // Tell the inventory to recycle the player's items based upon the given value
            recycleInventory.RecycleItems(isRecycling);

            // Hide the inventory after if it's open
            if (recycleInventory.IsOpen)
            {
                ShowRecycleBin();
            } // end if
        } // end AcceptCancelRecycle

        // Recycle Bin button - Displays the recycle bin window
        public void ShowRecycleBin()
        {
            // Toggle the recycle bin's inventory window
            recycleInventory.IsOpen = !recycleInventory.IsOpen;

            // Check if the recycle bin's window is open
            if (recycleInventory.IsOpen)
            {
                // Set the recycle bin window up before displaying it
                recycleInventory.SetPlayer(guiPlayerTurn);

                // Open the recycle bin window
                recycleInventory.gameObject.SetActive(true);
            } // end if
            else
            {
                // Otherwise, close the recycle bin window
                recycleInventory.gameObject.SetActive(false);
            } // end else
        } // end ShowRecycleBin

		// Pause button - Displays pause menu and pauses game
		public void PauseGame()
		{
			if(!pauseMenu.activeInHierarchy)
			{
				isPaused = true;
				pauseMenu.SetActive (true);
				guiMovement.DisableButtons();
				actionButton.interactable = false;
                if (inventory.IsOpen)
				{
					ShowInventory();
				} //end if
                if(isAlliesOpen)
                {
                    ShowAllies();
                } // end if
                if (allyInventory.IsOpen)
                {
                    ShowAllyInventory();
                } // end if
                if (recycleInventory.IsOpen)
                {
                    ShowRecycleBin();
                } // end if
                GameObject.Find("CurrentPlayer/InvAlly/Inventory").GetComponent<Button>().interactable = false;
                GameObject.Find("CurrentPlayer/InvAlly/Ally").GetComponent<Button>().interactable = false;
			} //end if
			else
			{
				isPaused = false;
				pauseMenu.SetActive(false);
                GameObject.Find("CurrentPlayer/InvAlly/Inventory").GetComponent<Button>().interactable = true;
                GameObject.Find("CurrentPlayer/InvAlly/Ally").GetComponent<Button>().interactable = true;
				if(actionButtonActive)
				{
					actionButton.interactable = true;
				} //end if
				if(gamePlayState == GamePlayState.SelectPathToTake)
				{
					guiMovement.TravelDistanceLeft();
				} //end if
			} //end else
		} //end PauseGame

		// Instructions button - Displays instructions panel
		public void Instructions()
		{
			instructionsSet.SetActive (!instructionsSet.activeInHierarchy);
			instructionsSet.transform.GetChild(0).gameObject.SetActive(true);
			instructionsSet.transform.GetChild(1).gameObject.SetActive(false);
			instructionsSet.transform.GetChild(2).gameObject.SetActive(false);
			instructionsProgress = 1;
		} //end Instructions

		// Continue button - goes through instruction slides
		public void Continue()
		{
			if(instructionsProgress == 0)
			{
				instructionsSet.transform.GetChild(0).gameObject.SetActive(true);
				instructionsSet.transform.GetChild(1).gameObject.SetActive(false);
				instructionsSet.transform.GetChild(2).gameObject.SetActive(false);
			} //end if
			else if(instructionsProgress == 1)
			{
				instructionsSet.transform.GetChild(0).gameObject.SetActive(false);
				instructionsSet.transform.GetChild(1).gameObject.SetActive(true);
				instructionsSet.transform.GetChild(2).gameObject.SetActive(false);
			} //end else if
			else if(instructionsProgress == 2)
			{
				instructionsSet.transform.GetChild(0).gameObject.SetActive(false);
				instructionsSet.transform.GetChild(1).gameObject.SetActive(false);
				instructionsSet.transform.GetChild(2).gameObject.SetActive(true);
			} //end else if
			instructionsProgress++;
			if(instructionsProgress > 2)
			{
				instructionsProgress = 0;
			} //end if
		} //end Continue

		public void MainMenu()
		{
			Entities.EntityManager.Instance.Dispose ();
			while(GameMaster.Instance.Turn != 0)
			{
				GameMaster.Instance.NextTurn();
			} //end while
			GameMaster.Instance.NumPlayers = 0;
			AudioManager.Instance.PlayMenu ();
			GameMaster.Instance.LoadLevel ("MenuScene");
		} //end MainMenu

		// Yes Button - Accepts whatever is presented
		public void Yes()
		{
			if(mapEventResult.Contains("Ally"))
			{
				guiTurnText.text = guiMapEvent.ResolveAlly("YES");
			} //end if
			acceptPanel.SetActive (false);

            // Make sure the player isn't an AI
            if (!isPlayerAI)
            {
                // Make the action button clickable again
                actionButton.interactable = true;
                actionButtonActive = true;
            } // end if
		} //end Yes

		// No Button - Declines whatever is presented
		public void No()
		{
			if(mapEventResult.Contains("Ally"))
			{
				guiTurnText.text = guiMapEvent.ResolveAlly("NO");
			} //end if
			acceptPanel.SetActive (false);

            // Make sure the player isn't an AI
            if (!isPlayerAI)
            {
                // Make the action button clickable again
                actionButton.interactable = true;
                actionButtonActive = true;
            } // end if
		} //end No

        // Gets whether the movement class is initialised for the current player
        public bool IsMovementInitialized
        {
            get { return isMovementInitialized; }
        } // end IsMovementInitialized
	} // end GameplayStateMachine
} // end GSP
