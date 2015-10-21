/*******************************************************************************
 *
 *  File Name: BrentsStateMachine.cs
 *
 *  Description: Brains of the menu's logic; controls the flow of the menu
 *
 *******************************************************************************/
using GSP.Core;
using GSP.Items.Inventories;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace GSP
{
    /*******************************************************************************
     *
     * Name: BrentsStateMachine
     * 
     * Description: The logic for the menu.
     * 
     *******************************************************************************/
    public class BrentsStateMachine : MonoBehaviour
	{
        // Note: Class variables should always be private. For outside access, use properties with get and/or set - Damien
        //       An exception is setting assets/values through the unity editor; however not always the best way to do it
        
        // State machine variables
		OverallState programState;          // Current overall state
		MenuState menuState;				// Current menu state
		float timeHolder;					// Waiting time

        // Audio prefab holder
        public GameObject audioManager;

		// Menu Objects
		GameObject introSet;				// Intro panel
		GameObject menuSet;					// Main Menu panel
		GameObject mapSet;					// Map selection panel
		GameObject creditsSet;				// Credits panel
		GameObject instructionsSet;			// Instructions panel
		GameObject optionsSet;				// Options panel
        GameObject colorSet;                // Colors panel
		GameObject charactersSet;			// Character selection panel
		GameObject backButton;				// Back button
        GameObject continueButton;          // Continue button for the colours
		Text 	   guideText;				// Map selection text
		bool isMenuDisplayed;				// Whether menu is displayed or not
		bool isInstructionsDisplayed;		// Whether instructions are displayed or not
		int  instructionsProgress;			// What slide the player is on
		bool isCreditsDisplayed;			// Whether Credits are displayed or not
		bool isOptionsDisplayed;			// Whether Options are displayed or not
		string mapSelection;		        // Scene name of map
		bool isMapsDisplayed;				// Whether selection maps are displayed or not
        bool isColorsDisplayed;				// Whether the colours are displayed or not
		bool isCharactersDisplayed;			// Whether the character selection is displayed or not
		int playerSpriteNum;				// What player pushed the sprite selection button

		// Initialize variables
		void Start()
		{
            // Init Menu variables
			programState = OverallState.Intro;				// Initial beginning of game
			menuState = MenuState.Home;						// Prevents triggers from occuring before called
			isMenuDisplayed = true;							// Menu begins displayed
			isCreditsDisplayed = false;						// Credits begins hidden
			isOptionsDisplayed = false;						// Options begin hidden
			isInstructionsDisplayed = false;				// Instructions begin hidden
			instructionsProgress = 1;						// No progress made, prepare to show second slide
			timeHolder = Time.time + 3.0f;					// Initialize first wait period
			mapSelection = "nothing";						// Nothing has been chosen yet
			isMapsDisplayed = false;						// Map selection begins hidden
            isColorsDisplayed = false;						// Colours begins hidden
			isCharactersDisplayed = false;					// Character selection begins hidden
			playerSpriteNum = -1;							// Initialize to -1 as no player has pushed it

			// Obtain references to hierarchy panels and objects
            introSet = GameObject.Find("Intro");
            menuSet = GameObject.Find("MainMenu");
            mapSet = GameObject.Find("MapSelection");
            creditsSet = GameObject.Find("Credits");
            instructionsSet = GameObject.Find("Instructions");
			optionsSet = GameObject.Find ("Options");
            colorSet = GameObject.Find("Colours");
			charactersSet = GameObject.Find ("Characters");
            backButton = GameObject.Find("BackButton");
            continueButton = GameObject.Find("ColorsContinue");
            guideText = GameObject.Find("GuideText").GetComponent<Text>();

			// Disable everything but intro and menu
            DisableMapSelection();
            DisableInstructions();
            DisableCredits();
			DisableOptions ();
            DisableColors();
			DisableCharacters ();
            backButton.SetActive(false);
            continueButton.SetActive(false);
            guideText.gameObject.SetActive(false);

			// Set audio to active
			if(AudioManager.Instance.IsMusicMuted())
			{
				AudioManager.Instance.MuteMusic();
			} //end if

			if (AudioManager.Instance.IsSFXMuted ())
			{
				AudioManager.Instance.MuteSFX();
			} //end if

			AudioManager.Instance.MusicVolume (1.0f);
			AudioManager.Instance.SFXVolume (1.0f);

            // Initialise the GameMaster in the menu
            // Note: Don't remove this
            if (GameMaster.Instance != null) {/* Leave this empty. :P */}

            // Initialise the ItemDatabase in the menu
            // Note: Don't remove this
            if (ItemDatabase.Instance != null) { /* Leave this empty. :P */}
		} // end Start
		
		// Main function for controlling game
		void Update()
		{
			// PROGRAM ENTRY POINT
			switch(programState)
			{
				// Intro
                case OverallState.Intro:
                    {
                        // INTRO ENTRY POINT
                        
                        // Make sure an AudioManager exists
                        if (AudioManager.Instance == null)
                        {
                            Instantiate(audioManager);
                        } // end if
                        
                        // After intro finishes, move to menu
                        if (Time.time > timeHolder)
                        {
                            // Change state
                            programState = OverallState.Menu;

                            // Show main menu 
                            DisableTitle();
                        } // end if
                        break;
                    } // end case Intro
				// Menu
                case OverallState.Menu:
                    {
                        // MENU ENTRY POINT
                        switch (menuState)
                        {
                            case MenuState.Home:
                                {
                                    // Home - Menu hub, displays all buttons for menu
                                    if (!isMenuDisplayed)
                                    {
                                        EnableMainMenu();
                                    } // end if
                                    break;
                                } // end case Home
                            // Solo - Single Player game
                            case MenuState.Solo:
                                {
                                    // Hide menu if not cleared yet
                                    if (isMenuDisplayed)
                                    {
                                        DisableMainMenu();
                                    } // end if

                                    // Display maps for selection if not done yet
                                    if (!isMapsDisplayed)
                                    {
                                        EnableMapSelection();
                                    } // end if

                                    // Only continue once a map has been chosen
                                    if (mapSelection != "nothing")
                                    {
                                        // Pick player amount
                                        // Set the number of players to two; the player and an AI
                                        GameMaster.Instance.NumPlayers = 2;

                                        // Flag the game as single player
                                        GameMaster.Instance.IsSinglePlayer = true;

                                        // Enable the AI
                                        GameMaster.Instance.IsAIEnabled = true;

                                        // Transition into the colours menu state
                                        menuState = MenuState.Colors;
                                    } // end if
                                    break;
                                } // end case Solo
                            //Multi - Multiplayer game
                            case MenuState.Multi:
                                {
                                    // Clear buttons if not cleared yet
                                    // Hide menu if not cleared yet
                                    if (isMenuDisplayed)
                                    {
                                        DisableMainMenu();
                                    } // end if

                                    // Display maps for selection
                                    if (!isMapsDisplayed)
                                    {
                                        EnableMapSelection();
                                    } // end if

                                    // Only continue once a map has been chosen
                                    if (mapSelection != "nothing")
                                    {
                                        // Pick player amount
                                        guideText.text = "Please select a map...Done!\nPlease enter the number of players [2-4]";

                                        #region Menu Data Adding Stuff

                                        // Set number of players
                                        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
                                        {
                                            GameMaster.Instance.NumPlayers = 2;
                                        } //end if
                                        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
                                        {
                                            GameMaster.Instance.NumPlayers = 3;
                                        } //end else if
                                        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
                                        {
                                            GameMaster.Instance.NumPlayers = 4;
                                        } //end else if
                                        #endregion
                                        
                                        // Note: No need to repeat code if it's the same and not dependent on things inside such circumstances- Damien
                                        // Note: I screwed this up previously, but it still shouldn't have to be repeated so do a simple conditional check
                                        if (GameMaster.Instance.NumPlayers >= 2 && GameMaster.Instance.NumPlayers <= 4)
                                        {
                                            // Flag the game as multiplayer
                                            GameMaster.Instance.IsSinglePlayer = false;
                                            
                                            // Diable the AI
                                            GameMaster.Instance.IsAIEnabled = false;
                                            
                                            // Transition into the colours menu state
                                            menuState = MenuState.Colors;
                                        }
                                    } // end if
                                    break;
                                } // end case Multi
                            // Colors - Selection of player colours
                            case MenuState.Colors:
                                {
                                    // Clear buttons if not cleared yet
                                    // Hide map select if not cleared yet
                                    if (isMapsDisplayed)
                                    {
                                        DisableMapSelection();
                                    } // end if

                                    // Display colours
                                    if (!isColorsDisplayed)
                                    {
                                        EnableColors();
                                        guideText.text = "Please choose a name and color.";

                                        // Check if the game is multiplayer
                                        if (!GameMaster.Instance.IsSinglePlayer)
                                        {
                                            // Check which colour lines should be displayed.
                                            if (GameMaster.Instance.NumPlayers > 1 && GameMaster.Instance.NumPlayers <= 2)
                                            {
                                                // Enable player 2
                                                colorSet.transform.GetChild(1).gameObject.SetActive(true);
                                            } // end if
                                            else if (GameMaster.Instance.NumPlayers > 2 && GameMaster.Instance.NumPlayers <= 3)
                                            {
                                                // Enable player 2-3
                                                colorSet.transform.GetChild(1).gameObject.SetActive(true);
                                                colorSet.transform.GetChild(2).gameObject.SetActive(true);
                                            } // end else if
                                            else if (GameMaster.Instance.NumPlayers > 3 && GameMaster.Instance.NumPlayers <= 4)
                                            {
                                                // Enable player 2-4
                                                colorSet.transform.GetChild(1).gameObject.SetActive(true);
                                                colorSet.transform.GetChild(2).gameObject.SetActive(true);
                                                colorSet.transform.GetChild(3).gameObject.SetActive(true);
                                            } // end else if
                                        } // end if !GameMaster.Instance.IsSinglePlayer
                                    } // end if
                                    break;
                                } // end case Colors
							// Characters - Display the character selections
							case MenuState.Characters:
								{
									// Hide colors if not cleared yet
									if(isColorsDisplayed)
									{
										DisableColors();
									} //end if

									// Display characters if not shown yet
									if(!isCharactersDisplayed)
									{
										EnableCharacters();
									} //end if
									break;
								} //end case Characters
							// Options - Display audio options
							case MenuState.Options:
								{
									// Hide menu if not cleared yet
									if (isMenuDisplayed)
									{
										DisableMainMenu();
									} // end if
					
									// Display options if not shown yet
									if (!isOptionsDisplayed)
									{
										EnableOptions();
									} // end if
									break;
								} //end case Options
                            // Credits - Display source citations
                            case MenuState.Credits:
                                {
                                    // Hide menu if not cleared yet
                                    if (isMenuDisplayed)
                                    {
                                        DisableMainMenu();
                                    } // end if
								
									// Display credits if not shown yet
                                    if (!isCreditsDisplayed)
                                    {
                                        EnableCredits();
                                    } // end if
                                    break;
                                } // end case Credits
                            // Help - Displays the instructions for the game
                            case MenuState.Help:
                                {
                                    // Clear buttons and display instructions if not cleared yet
                                    if (isMenuDisplayed)
                                    {
                                        DisableMainMenu();
                                    } // end if

                                    if (!isInstructionsDisplayed)
                                    {
                                        EnableInstructions();
                                    } // end if
                                    break;
                                } // end case Help
                            // Quit - Change program to End to wrap up any loose ends
                            case MenuState.Quit:
                                {
                                    // Hide menu if not cleared yet
                                    if (isMenuDisplayed)
                                    {
                                        DisableMainMenu();
                                    } // end if

                                    // Move to end of program
                                    programState = OverallState.End;
                                    break;
                                } // end case Quit
                        } // end switch menuState
                        break;
                    } // end case Menu
				// Game
                case OverallState.Game:
                    {
                        // GAMEPLAY ENTRY POINT
                        // Reset states
                        programState = OverallState.Menu;
                        menuState = MenuState.Home;

                        // Play correct background music
                        if (mapSelection == "area01")
                        {
                            // Set the BattleMap
							GameMaster.Instance.BattleMap = BattleMap.area01;

                            AudioManager.Instance.PlayDesert();
                        } // end if
                        else if (mapSelection == "area02")
                        {
                            // Set the BattleMap
							GameMaster.Instance.BattleMap = BattleMap.area02;
                            
                            AudioManager.Instance.PlayEuro();
                        } // end else if
                        else if (mapSelection == "area03")
                        {
                            // Set the BattleMap
							GameMaster.Instance.BattleMap = BattleMap.area03;
                            
                            AudioManager.Instance.PlayMetro();
                        } // end else if
                        else if (mapSelection == "area04")
                        {
                            // Set the BattleMap
							GameMaster.Instance.BattleMap = BattleMap.area04;
                            
                            AudioManager.Instance.PlaySnow();
                        } // end else if 
                        else
                        {
                            AudioManager.Instance.PlayMenu();
                        } // end else

                        // This is a new game
                        GameMaster.Instance.IsNew = true;

                        // Tell the GameMaster to load selected level
                        GameMaster.Instance.LoadLevel(mapSelection);
                        break;
                    } // end case Game
				// End
                case OverallState.End:
                    {
                        // END ENTRY POINT - WATCH OUT UNIVERSE!
                        // Wrap up any loose ends here since the program is now exiting.

                        // Destroy Audio Manager
                        if (AudioManager.Instance != null)
                        {
                            Destroy(AudioManager.Instance.gameObject);
                        } // end if

                        // Reset state machine to initial
                        menuState = MenuState.Home;
                        programState = OverallState.Intro;

                        // Reset objected to initial point
                        EnableTitle();
                        DisableMainMenu();

                        // Reset time holder to initial
                        timeHolder = Time.time + 3.0f;

                        // Exit program if possible
                        Application.Quit();

                        break;
                    } // end case End
				// DEFAULT - Catches any exceptions end prints errored state
                default:
                    {
                        Debug.LogErrorFormat("Error - No program state {0} found.", programState);
                        break;
                    } // end case default
            } // end switch programState
		} // end Update

        // Note: Function names always start with a capital letter - Damien
        
        // State machine functions
		// Program state
		public int GetState()
		{
			return (int)programState;
		} // end GetState
		
		// Menu state
		public int GetMenu()
		{
			return (int)menuState;
		} // end GetMenu

		// Menu state settings
        // Enable the intro screen
		void EnableTitle()
		{
			introSet.SetActive (true);
		} // end EnableTitle

        // Disable the intro screen
        void DisableTitle()
		{
			introSet.SetActive (false);
		} // end DisableTitle

        // Enable the main menu screen
        void EnableMainMenu()
		{
			menuSet.SetActive (true);
			backButton.SetActive (false);
			isMenuDisplayed = true;
		} // end EnableMainMenu

        // Disable the main menu screen
        void DisableMainMenu()
		{
			menuSet.SetActive (false);
			backButton.SetActive (true);
			isMenuDisplayed = false;
		} // end DisableMainMenu

        // Enable the level select screen
        void EnableMapSelection()
		{
			mapSet.SetActive (true);
			guideText.gameObject.SetActive(true);
			isMapsDisplayed = true;
		} // end EnableMapSelection

        // Disable the level select screen
        void DisableMapSelection()
		{
			mapSet.SetActive (false);
			guideText.gameObject.SetActive (false);
			isMapsDisplayed = false;
		} // end DisableMapSelection

        // Enable the instructions screen
        void EnableInstructions()
		{
			instructionsSet.SetActive (true);
			isInstructionsDisplayed = true;
		} // end EnableInstructions

        // Disable the instructions screen
        void DisableInstructions()
		{
			instructionsSet.SetActive (false);
			isInstructionsDisplayed = false;
			instructionsProgress = 1;
			instructionsSet.transform.GetChild(0).gameObject.SetActive(true);
			instructionsSet.transform.GetChild(1).gameObject.SetActive(false);
			instructionsSet.transform.GetChild(2).gameObject.SetActive(false);
		} // end DisableInstructions

        // Enable the credits screen
        void EnableCredits()
		{
			creditsSet.SetActive (true);
			isCreditsDisplayed = true;
		} // end EnableCredits

        // Disable the credits screen
        void DisableCredits()
		{
			creditsSet.SetActive (false);
			isCreditsDisplayed = false;
		} // end DisableCredits

		// Enable the options screen
		void EnableOptions()
		{
			optionsSet.SetActive (true);
			isOptionsDisplayed = true;
		} // end EnableOptions
		
		// Disable the options screen
		void DisableOptions()
		{
			optionsSet.SetActive (false);
			isOptionsDisplayed = false;
		} // end DisableOptions

        // Enable the colours screen
        void EnableColors()
        {
            colorSet.SetActive(true);
            guideText.gameObject.SetActive(true);
            continueButton.SetActive(true);
            isColorsDisplayed = true;
        } // end EnableColors

        // Disable the colours screen
        void DisableColors()
        {
            colorSet.SetActive(false);
            guideText.gameObject.SetActive(false);
            continueButton.SetActive(false);
            isColorsDisplayed = false;
            colorSet.transform.GetChild(1).gameObject.SetActive(false);
            colorSet.transform.GetChild(2).gameObject.SetActive(false);
            colorSet.transform.GetChild(3).gameObject.SetActive(false);
        } // end DisableColors

		// Enable the characters screen
		void EnableCharacters()
		{
			charactersSet.SetActive (true);
			guideText.gameObject.SetActive (true);
			guideText.text = "Choose a character.";
			isCharactersDisplayed = true;
		} //end EnableCharacters

		// Disable the characters screen
		void DisableCharacters()
		{
			charactersSet.SetActive (false);
			guideText.gameObject.SetActive (false);
			isCharactersDisplayed = false;
			playerSpriteNum = -1;
		} //end Disable Characters

		// Menu Button functions
        // Solo button
		public void SoloGame()
		{
			menuState = MenuState.Solo;
		} // end SoloGame

        // Multi button
        public void MultiGame()
		{
			menuState = MenuState.Multi;
		} // end MultiGame

        // Help button
        public void Help()
		{
			menuState = MenuState.Help;
		} // end Help

        // Credits button
        public void Credits()
		{
			menuState = MenuState.Credits;
		} // end Credits

		// Options button
		public void Options()
		{
			menuState = MenuState.Options;
		} //end Options

        // Quit button
        public void QuitGame()
		{
			menuState = MenuState.Quit;
		} // end QuitGame

        // Back button
        public void Back()
		{
			if(menuState == MenuState.Credits)
			{
				DisableCredits();
				menuState = MenuState.Home;
			} // end if
			else if(menuState == MenuState.Help)
			{
				DisableInstructions();
				menuState = MenuState.Home;
			} // end else if
			else if(menuState == MenuState.Solo || menuState == MenuState.Multi)
			{
				DisableMapSelection();
				mapSelection = "nothing";
				guideText.text = "Please select a map";
				menuState = MenuState.Home;
			} // end else if
            else if (menuState == MenuState.Colors)
            {
                DisableColors();
                mapSelection = "nothing";
                guideText.text = "Please select a map";
                GameMaster.Instance.NumPlayers = 0;
                menuState = MenuState.Home;
            } // end else if
			else if(menuState == MenuState.Characters)
			{
				DisableCharacters();
				menuState = MenuState.Colors;
			} //end else if
			else if(menuState == MenuState.Options)
			{
				DisableOptions();
				menuState = MenuState.Home;
			} //end else if
			else
			{
				menuState = MenuState.Home;
			} // end else
		} // end Back

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

		// Desert map button
        public void DesertMap()
		{
			mapSelection = "area01";
		} // end DesertMap

        // Euro map button
        public void EuroMap()
		{
			mapSelection = "area02";
		} // end EuroMap

        // Metro map button
        public void MetroMap()
		{
			mapSelection = "area03";
		} // end MetroMap

        // Snow map button
        public void SnowMap()
		{
			mapSelection = "area04";
		} // end SnowMap

		// Music mute toggle
		public void MuteMusic()
		{
            if (Application.isPlaying)
            {
                AudioManager.Instance.MuteMusic();
            } // end if
		} //end MuteMusic

		// Music volume slider
		public void AdjustMusic(float vol)
		{
			if (Application.isPlaying)
            {
                AudioManager.Instance.MusicVolume(vol);
            } // end if
		} //end AdjustMusic

		// SFX mute toggle
		public void MuteSFX()
		{
            if (Application.isPlaying)
            {
                AudioManager.Instance.MuteSFX();
            } // end if
		} //end MuteSFX

		// SFX volume slider
		public void AdjustSFX(float vol)
		{
            if (Application.isPlaying)
            {
                AudioManager.Instance.SFXVolume(vol);
            } // end if
		} //end AdjustSFX

        // Colours continue button
        public void ColorsContinue()
        {
            // Get the input fields
            InputField playerOneInput = colorSet.transform.GetChild(0).GetChild(0).GetChild(2).GetComponent<InputField>();
            InputField playerTwoInput = colorSet.transform.GetChild(1).GetChild(0).GetChild(2).GetComponent<InputField>();
            InputField playerThreeInput = colorSet.transform.GetChild(2).GetChild(0).GetChild(2).GetComponent<InputField>();
            InputField playerFourInput = colorSet.transform.GetChild(3).GetChild(0).GetChild(2).GetComponent<InputField>();
            
            // Get the names trimmed of any whitespace
            // Player 1
            // Name
			string playerOneName = playerOneInput.text.Trim();
            if (!string.IsNullOrEmpty(playerOneName))
            {
                // Player 1's name isn't empty so set it
                GameMaster.Instance.SetPlayerName(0, playerOneName);
            } // end if
            else
            {
                // Otherwise, Player 1's name is empty so set a default
                GameMaster.Instance.SetPlayerName(0, "Player 1");
            } // end else

			// Sprite
			if(GameMaster.Instance.GetPlayerSprite(0) == -1)
			{
				GameMaster.Instance.SetPlayerSprite(0, 1);
			} //end if

            // Player 2
			// Name
            string playerTwoName = playerTwoInput.text.Trim();
            if (!string.IsNullOrEmpty(playerTwoName))
            {
                // Player 1's name isn't empty so set it
                GameMaster.Instance.SetPlayerName(1, playerTwoName);
            } // end if
            else
            {
                // Otherwise, Player 2's name is empty so set a default
                GameMaster.Instance.SetPlayerName(1, "Player 2");
            } // end else

			// Sprite
			if(GameMaster.Instance.GetPlayerSprite(1) == -1)
			{
				GameMaster.Instance.SetPlayerSprite(1, 2);
			} //end if

            // Player 3
            // Name
			string playerThreeName = playerThreeInput.text.Trim();
            if (!string.IsNullOrEmpty(playerThreeName))
            {
                // Player 1's name isn't empty so set it
                GameMaster.Instance.SetPlayerName(2, playerThreeName);
            } // end if
            else
            {
                // Otherwise, Player 3's name is empty so set a default
                GameMaster.Instance.SetPlayerName(2, "Player 3");
            } // end else

			// Sprite
			if(GameMaster.Instance.GetPlayerSprite(2) == -1)
			{
				GameMaster.Instance.SetPlayerSprite(2, 3);
			} //end if
            
			// Player 4
            // Name
			string playerFourName = playerFourInput.text.Trim();
            if (!string.IsNullOrEmpty(playerFourName))
            {
                // Player 1's name isn't empty so set it
                GameMaster.Instance.SetPlayerName(3, playerFourName);
            } // end if
            else
            {
                // Otherwise, Player 4's name is empty so set a default
                GameMaster.Instance.SetPlayerName(3, "Player 4");
            } // end else

			// Sprite
			if(GameMaster.Instance.GetPlayerSprite(3) == -1)
			{
				GameMaster.Instance.SetPlayerSprite(3, 4);
			} //end if

            // Display loading text
            guideText.text = "Loading, please wait.";

            // Transition into game state
            programState = OverallState.Game;
        } // end ColorsContinue

        public void ParseColorButton(string input)
        {
            // Result string
            string[] result = input.Split('|');
            
            // The player's number
            int playerNum = Convert.ToInt32(result[0]);

            // The player's colour
            int playerColor = Convert.ToInt32(result[1]);

            // Finally, set the colour
            GameMaster.Instance.SetPlayerColor(playerNum, (InterfaceColors)playerColor);
        } // end ParseColorButton

		public void SelectCharacter(int pNum)
		{
			playerSpriteNum = pNum;
			DisableColors ();
			EnableCharacters ();
			menuState = MenuState.Characters;
		} //end SelectCharacter

		public void ParseSpriteButton(int spriteNum)
		{
			GameMaster.Instance.SetPlayerSprite (playerSpriteNum, spriteNum);
			guideText.text = "Sprite " + spriteNum + " chosen.";
		} //end ParseSpriteButton
    } // end BrentsStateMachine
} //end GSP
