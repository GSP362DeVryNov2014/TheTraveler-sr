/*******************************************************************************
 *
 *  File Name: GameMaster.cs
 *
 *  Description: Manages the data transfers and general wrapper for dealing
 *               with the entities of the game
 *
 *******************************************************************************/
using GSP.Char;
using GSP.Char.Allies;
using GSP.Entities;
using GSP.Entities.Friendlies;
using GSP.Items;
using GSP.Items.Inventories;
using GSP.Tiles;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace GSP.Core
{
    /*******************************************************************************
     *
     * Name: GameMaster
     * 
     * Description: Oversees the data and such between scene loads and overall.
     * 
     *******************************************************************************/
    public class GameMaster : BaseSingleton<GameMaster>
    {
        string playerFilePath;       // The player's save file prefix
        string highScoreFilePath;    // The highscores save file
        string resourceFilePath;     // The resource postion's save file
        string saveFileExt;          // The save file's extension

        // The first tile
        Vector3 startingPos = new Vector3(GSP.Tiles.TileUtils.MinWidth, GSP.Tiles.TileUtils.MinHeight, -1.6f);

        readonly int maxPlayers = 4; // Max number of players
        int turn;                    // Who's turn it is
        int numPlayers;              // The current number of players

        BattleMap battleMap;    // The map used for the battle scene
        bool isNew;             // Whether the game is new or loaded

        bool isSinglePlayer;    // Whether the game is a single player game
        bool isAIEnabled;       // Whether the AI is enabled for single player games

        HighScoreTable highScoreTable;  // The scores table reference
        
        // The variables here are through dictionaries; The key is the player number
        Dictionary<int, string> playerNames;            // The list of the players' names
        Dictionary<int, InterfaceColors> playerColors;  // The list of the players' colours
		Dictionary<int, int> playerSprite;				// The list of the players' sprites
        Dictionary<int, GameObject> playerObjs;         // The list of players' GameObject's
        Dictionary<int, Char.Player> players;           // The list of players' Player scripts references

        List<int> enemyIdentifiers;     // The list of enemy IDs
        List<int> tempAllyIdentifiers;  // A temporary list of identifiers for newly created allies

        // Initialise the dictionaries and other values
        public override void Awake()
        {
            // Call the parent's Awake() first
            base.Awake();

            // Set the object's name
            gameObject.name = "GameMaster";
            
            // Create the dictionaries
            playerNames = new Dictionary<int, string>();
            playerColors = new Dictionary<int, InterfaceColors>();
			playerSprite = new Dictionary<int, int> ();
            playerObjs = new Dictionary<int, GameObject>();
            players = new Dictionary<int, Char.Player>();

            // Create the lists
            enemyIdentifiers = new List<int>();
            tempAllyIdentifiers = new List<int>();

            // Set the save file information strings
            playerFilePath = Application.persistentDataPath + "/player";
            highScoreFilePath = Application.persistentDataPath + "/highscores";
            resourceFilePath = Application.persistentDataPath + "/resources";
            saveFileExt = ".sav";
        } // end Awake

        // Fill the dictionaries and initialise the values
        void Start()
        {
            // Loop over the players to fill the dictionaries with deafult values
            for (int index = 0; index < MaxPlayers; index++)
            {
                playerNames.Add(index, string.Empty);
                playerColors.Add(index, InterfaceColors.Black);
				playerSprite.Add(index, -1);
                playerObjs.Add(index, null);
                players.Add(index, null);
            } // end for

            // Initialise the number of players to zero
            numPlayers = 0;

            // Initialise the current turn to zero
            turn = 0;
        } // end Start

        // Resets the containers
        void ResetCollections()
        {
            // Loop over the players to reset the dictionaries to deafult values
            for (int index = 0; index < MaxPlayers; index++)
            {
                playerObjs[index] = null;
                players[index] = null;
            } // end for

            // Clear the lists
            enemyIdentifiers.Clear();
            tempAllyIdentifiers.Clear();
        } // end ResetCollections

        // Resets the turn to default; only use this if leaving the game and going to the menu
        public void ResetTurn()
        {
            turn = 0;
        } // end ResetTurn

        // Removes an enemy ID from the enemyIdentifiers list
        public void RemoveEnemyIdentifier(int ID)
        {
            int position; // The position in the list the ID is

            // Get the position in the list the ID is
            position = enemyIdentifiers.IndexOf(ID);

            // Check if the ID is valid
            if (position == -1)
            {
                // Simply return
                return;
            } // end if

            // Remove the ID at the found index
            enemyIdentifiers.RemoveAt(position);
        } // end RemoveEnemyIdentifier

        // Clears the ally identifiers list
        public void ClearAllyIdentifiers()
        {
            tempAllyIdentifiers.Clear();
        } // end ClearAllyIdentifiers

        // Gets the player's name with the given key
        public string GetPlayerName(int playerNum)
        {
            return playerNames[playerNum];
        } // end GetPlayerName

        // Sets the player's name with the given key
        public void SetPlayerName(int playerNum, string playerName)
        {
            playerNames[playerNum] = playerName;

            // Check if the GameObject exists
            if (playerObjs[playerNum] != null)
            {
                playerObjs[playerNum].name = playerName;
            } // end if

            // Check if player script exists
            Player player;
            if ((player = Instance.GetPlayerScript(playerNum)) != null)
            {
                // Set the merchant's name as well
                player.Entity.Name = playerName;
            } // end if
        } // end SetPlayerName

        // Gets the player's colour with the given key
        public InterfaceColors GetPlayerColor(int playerNum)
        {
            return playerColors[playerNum];
        } // end GetPlayerColor

        // Sets the player's colour with the given key
        public void SetPlayerColor(int playerNum, InterfaceColors playerColor)
        {
            playerColors[playerNum] = playerColor;
        } // end SetPlayerColor

		// Gets the player's sprite with the given key
		public int GetPlayerSprite(int playerNum)
		{
			return playerSprite [playerNum];
		} //end GetPlayerSprite

		// Sets the player's sprite with the given key
		public void SetPlayerSprite(int playerNum, int pSprite)
		{
			playerSprite [playerNum] = pSprite;
		} //end SetPlayerSprite

		// Gets the player's GameObject's animator - not settable outside SetPlayerSprite
		public Animator GetPlayerAnimator(int playerNum)
		{
			return playerObjs [playerNum].GetComponent<Animator> ();
		} //end GetPlayerAnimator

        // Gets the player's GameObject
        public GameObject GetPlayerObject(int playerNum)
        {
            return playerObjs[playerNum];
        } // end GetPlayerObject

        // Gets the player's scripts
        public Player GetPlayerScript(int playerNum)
        {
            return players[playerNum];
        } // end GetPlayerScript

        #region Create Characters

        #region Create Players

        // Create a new player
        public void CreatePlayer(int playerNum)
        {
            Vector3 startPos = startingPos; // The starting position
            int entID = -1;                 // The ID of the created entity

            // Get the starting position
            startPos.y = -(playerNum * TileUtils.PlayerMoveDistance);

            // Create the player GameObject
            GameObject obj = Instantiate(PrefabReference.prefabPlayer) as GameObject;
            obj.transform.localScale = Vector3.one;

            // Check if the player is AI
            if (Instance.IsSinglePlayer && playerNum > 0)
            {
                // Give the player a name and colour
                GiveRandomNameAndColor(playerNum);

                // Set the player to be an AI
                obj.GetComponent<Player>().IsAI = true;
            } // end if

            // Name the player in the editor for convienience
            obj.name = GetPlayerName(playerNum);

            // Create the merchant entity
            Entities.EntityManager.Instance.Generator.CreateEntity(out entID, Entities.EntityType.Merchant, obj, playerNum);

            // Set the Entity's position thus setting the GameObject's position
            Entities.EntityManager.Instance.GetEntity(entID).Position = startPos;

            // Set the game obj for the given player
            playerObjs[playerNum] = obj;

            // Set the player script for the given player
            players[playerNum] = playerObjs[playerNum].GetComponent<Char.Player>();

            // Give the player script the ID for the merchant
            players[playerNum].GetMerchant(entID);
        } // end CreatePlayer

        // Gives the player with the given player num, a random name and color
        void GiveRandomNameAndColor(int playerNum)
        {
            // For now just give the player a standard name
            string name = "Player " + (playerNum + 1);
            
            // Set the name for the player
            Instance.SetPlayerName(playerNum, name);

            // Create a new die object to get a random colour
            Die die = new Die();
            die.Reseed(System.Environment.TickCount);

            // Get a random colour; subtract one to get a number between zero and seven
            int randColor = die.Roll(1, 8) - 1;

            // Set the colour for the player
            Instance.SetPlayerColor(playerNum, (InterfaceColors)randColor);

			// Get a random sprite
			int randSprite = die.Roll (1, 8);
			Debug.Log (randSprite);

			// Set the sprite for the player
			Instance.SetPlayerSprite (playerNum, randSprite);
        } // end GiveRandomNameAndColor

        // Create new players
        public void CreatePlayers()
        {
            // Get the keys from the player name dictionary
            var playerKeys = new List<int>(playerNames.Keys);
            
            // Loop over the dictionary to create each player
            foreach (var player in playerKeys)
            {
                // Check if the player is playing
                if (player < numPlayers)
                {
                    // Create the current player
                    CreatePlayer(player);
                } // end if
            } // end foreach
        } // end CreatePlayers

        // Creates a player from a save file
        void CreatePlayerFromSave(int playerNum, int merchantId, bool isDataOnly = false)
        {
            // Create the player GameObject
            GameObject obj = Instantiate(PrefabReference.prefabPlayer) as GameObject;
            obj.transform.localScale = Vector3.one;

            // Check if the player is AI
            if (Instance.IsSinglePlayer && playerNum > 0)
            {
                // Set the player to be an AI
                obj.GetComponent<Player>().IsAI = true;
            } // end if

            // Name the player in the editor for convienience
            obj.name = GetPlayerName(playerNum);

            // Set the game obj for the given player
            playerObjs[playerNum] = obj;

            // Set the player script for the given player
            players[playerNum] = playerObjs[playerNum].GetComponent<Char.Player>();

            // Give the player script the ID for the merchant
            players[playerNum].GetMerchant(merchantId);

            // Update the GameObject reference
            players[playerNum].UpdateGameObject(obj);

            // Update the entity's script references
            players[playerNum].UpdateScriptReferences();

            if (isDataOnly)
            {
                // If in data-only mode, destroy the sprite renderer
                Destroy(GetPlayerObject(playerNum).GetComponent<SpriteRenderer>());
            } // end if
        } // end CreatePlayer

        #endregion

        #region Create Enemies

        // Create a new enemy
        public void CreateEnemy(Entities.HostileType enemyType, string enemyName)
        {
            // Create the enemy GameObject
            GameObject obj = Instantiate(PrefabReference.prefabEnemy) as GameObject;

            int entID = -1;  // The ID of the created enemy

            // Switch over the enemy types
            switch (enemyType)
            {
                case Entities.HostileType.Bandit:
                    {
                        // Make sure the ID is -1 before starting
                        entID = -1;

                        // Add the bandit enemy script
                        var script = obj.AddComponent<Char.Enemies.BanditMB>();

                        // Name the GameObject in the editor for convienience sake
                        obj.name = enemyName;

                        // Create the enemy entity
                        Entities.EntityManager.Instance.Generator.CreateEntity(out entID, Entities.EntityType.Bandit, obj);

                        // Name the enemy entity
                        Entities.EntityManager.Instance.GetEntity(entID).Name = enemyName;

                        // Add the ID to the enemyIdentifiers list
                        enemyIdentifiers.Add(entID);

                        // Give the enemy script the ID for the enemy
                        script.GetEnemy(entID);

                        break;
                    } // end case Bandit
                case Entities.HostileType.Mimic:
                    {
                        // Make sure the ID is -1 before starting
                        entID = -1;

                        // Add the mimic enemy script
                        var script = obj.AddComponent<Char.Enemies.BanditMB>();

                        // Name the GameObject in the editor for convienience sake
                        obj.name = enemyName;

                        // Create the enemy entity
                        Entities.EntityManager.Instance.Generator.CreateEntity(out entID, Entities.EntityType.Mimic, obj);

                        // Name the enemy entity
                        Entities.EntityManager.Instance.GetEntity(entID).Name = enemyName;
                        
                        // Add the ID to the enemyIdentifiers list
                        enemyIdentifiers.Add(entID);

                        // Give the enemy script the ID for the enemy
                        script.GetEnemy(entID);

                        break;
                    } // end case Mimic
            } // end switch enemyType
        } // end CreateEnemy

        #endregion

        #region Create Allies

        // Create a new ally
        public void CreateAlly(Entities.FriendlyType allyType, string allyName)
        {
            // Create the ally GameObject
            GameObject obj = Instantiate(PrefabReference.prefabAlly) as GameObject;

            int entID = -1;  // The ID of the created ally

            // Switch over the ally types
            switch (allyType)
            {
                case Entities.FriendlyType.Porter:
                    {
                        // Make sure the ID is -1 before starting
                        entID = -1;

                        // Add the porter ally script
                        var script = obj.AddComponent<Char.Allies.PorterMB>();

                        // Name the GameObject in the editor for convienience sake
                        obj.name = allyName;

                        // Create the ally entity
                        Entities.EntityManager.Instance.Generator.CreateEntity(out entID, Entities.EntityType.Porter, obj);

                        // Name the ally entity
                        Entities.EntityManager.Instance.GetEntity(entID).Name = allyName;

                        // Add the ID to the tempAllyIdentifiers list
                        tempAllyIdentifiers.Add(entID);

                        // Give the ally script the ID for the ally
                        script.GetAlly(entID);

                        // Set the ally's number
                        ((Friendly)script.Entity).AllyNumber = GameMaster.Instance.Turn + 7;

                        break;
                    } // end case Porter
                case Entities.FriendlyType.Mercenary:
                    {
                        // Make sure the ID is -1 before starting
                        entID = -1;

                        // Add the mercenary ally script
                        var script = obj.AddComponent<Char.Allies.MercenaryMB>();

                        // Name the GameObject in the editor for convienience sake
                        obj.name = allyName;

                        // Create the ally entity
                        Entities.EntityManager.Instance.Generator.CreateEntity(out entID, Entities.EntityType.Mercenary, obj);

                        // Name the ally entity
                        Entities.EntityManager.Instance.GetEntity(entID).Name = allyName;

                        // Add the ID to the tempAllyIdentifiers list
                        tempAllyIdentifiers.Add(entID);

                        // Give the ally script the ID for the ally
                        script.GetAlly(entID);

                        // Set the ally's number
                        ((Friendly)script.Entity).AllyNumber = GameMaster.Instance.Turn + 7;

                        break;
                    } // end case Mercenary
            } // end allyType
        } // end CreateAlly

        // Create a new ally from a save file
        GameObject CreateAllyFromSave(Entities.FriendlyType allyType, string allyName, int allyId)
        {
            // Create the ally GameObject
            GameObject obj = Instantiate(PrefabReference.prefabAlly) as GameObject;

            // Switch over the ally types
            switch (allyType)
            {
                case Entities.FriendlyType.Porter:
                    {
                        // Add the porter ally script
                        var script = obj.AddComponent<Char.Allies.PorterMB>();

                        // Name the GameObject in the editor for convienience sake
                        obj.name = allyName;

                        // Give the ally script the ID for the ally
                        script.GetAlly(allyId);

                        // Update the GameObject reference
                        script.UpdateGameObject(obj);

                        break;
                    } // end case Porter
                case Entities.FriendlyType.Mercenary:
                    {
                        // Add the mercenary ally script
                        var script = obj.AddComponent<Char.Allies.MercenaryMB>();

                        // Name the GameObject in the editor for convienience sake
                        obj.name = allyName;

                        // Give the ally script the ID for the ally
                        script.GetAlly(allyId);

                        // Update the GameObject reference
                        script.UpdateGameObject(obj);

                        break;
                    } // end case Mercenary
            } // end switch allyType

            // Return the GameObject
            return obj;
        } // end CreateAlly

        #endregion

        #endregion

        #region Save and Load

        #region Players

        // Saves a player with the given key
        public void SavePlayer(int playerNum)
        {
            // The player's personal save path
            string playerSavePath = playerFilePath + playerNum + saveFileExt;
            
            // Create a new binary formatter to save to a binary file
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            // Declare the filestream for the file
            FileStream fileStream;

            // Check if the player's save file exists
            if (File.Exists(playerFilePath))
            {
                // The file exists so open the file
                fileStream = File.Open(playerSavePath, FileMode.Open, FileAccess.ReadWrite);
            } // end if
            else
            {
                // Otherwise the file doesn't exist so create it
                fileStream = File.Create(playerSavePath);
            } // end else

            // Create a new player data instance
            PlayerData playerData = new PlayerData();

            // Create a new ally data instance
            AllyData allyData = new AllyData();

            // Set the player's name
            playerData.Name = Instance.GetPlayerName(playerNum);
            
            // Set the player's colour
            playerData.Color = Instance.GetPlayerColor(playerNum);

            // Set the player's position
            playerData.Position = Instance.GetPlayerScript(playerNum).Entity.Position;

            // Set the player's merchant entity ID
            playerData.MerchantId = Instance.GetPlayerScript(playerNum).Entity.Id;

			// Check if the player has an ally
            if (Instance.GetPlayerScript(playerNum).NumAllies > 0)
            {
                // Get the Ally's GameObject
                GameObject ally = Instance.GetPlayerObject(playerNum).GetComponent<AllyList>()[0];

                // Set the player's ally entity ID; This is hard-coded for the Porter ally
                playerData.AllyId = ally.GetComponent<PorterMB>().Entity.Id;

                // Get the ally's inventory component
                AllyInventory allyInventory = GameObject.Find("Canvas").transform.Find("AllyInventory").
                    GetComponent<AllyInventory>();

                // Loop over the player's inventory to store their item IDs
                for (int index = 0; index < allyInventory.MaxSpace; index++)
                {
                    // Hardcoded for a single ally
                    allyData.AddItemId(allyInventory.GetItem(playerNum + 7, index).Id);
                } // end for
            } // end if
            else
            {
                // Otherwise set the player's ally entity ID to negative one
                playerData.AllyId = -1;
            } // end else

            // Get the Inventory component
			PlayerInventory inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").
                GetComponent<PlayerInventory>();

            // Loop over the player's inventory to store their item IDs
            for (int index = 0; index < (inventory.BonusSlotEnd + 1); index++)
            {
                playerData.AddItemId(inventory.GetItem(playerNum, index).Id);
            } // end for

            // Add the ally data to the player data
            playerData.AllyData = allyData;

            // Now write the data to the file
            binaryFormatter.Serialize(fileStream, playerData);

            // Finally, close the file stream
            fileStream.Close();
        } // end Save Player

        // Saves all players
        public void SavePlayers()
        {
            // Loop over the dictionary to save each player; We use the player name dictionary here
            foreach (var player in playerNames)
            {
                // Check if the player is playing
                if (player.Key < numPlayers)
                {
                    // Save the current player
                    Instance.SavePlayer(player.Key);
                } // end if
            } // end foreach
        } // end SavePlayers

        // Loads a player with the given key
        public void LoadPlayer(int playerNum, bool isDataOnly = false)
        {
            // The player's personal save path
            string playerSavePath = playerFilePath + playerNum + saveFileExt;
            
            // Make sure the player's save file exists before trying to load it
            if (File.Exists(playerSavePath))
            {
                // Create a new binary formatter to load the binary file
                BinaryFormatter binaryFormater = new BinaryFormatter();

                // Open a file stream while opening the file
                FileStream fileStream = File.Open(playerSavePath, FileMode.Open);

                // Create a player data instance from the file
                PlayerData playerData = (PlayerData)binaryFormater.Deserialize(fileStream);

                // Create the ally's data from the player's data
                AllyData allyData = playerData.AllyData;

                // Now close the file stream
                fileStream.Close();

                // Create the player
                Instance.CreatePlayerFromSave(playerNum, playerData.MerchantId, isDataOnly);

                // Set the player's position
                Instance.GetPlayerScript(playerNum).Position = playerData.Position;

                // Get the ally's Inventory component
                AllyInventory allyInventory = GameObject.Find("Canvas").transform.Find("AllyInventory").
                    GetComponent<AllyInventory>();

                // Create the list of items for the player; hard coded for a single ally
                allyInventory.CreateAllyItemList(playerNum + 7);

                // Check if the player had an ally
                if (playerData.AllyId >= 0)
                {
                    // Get the ally's FriendlyType
                    Friendly ally = (Friendly)EntityManager.Instance.GetEntity(playerData.AllyId);

                    // The player had an ally so create it for them
                    GameObject allyObj = Instance.CreateAllyFromSave(ally.FriendlyType, ally.Name, playerData.AllyId);

                    // Add the add to the player
                    var list = Instance.GetPlayerObject(playerNum).GetComponent<AllyList>();
                    list.AddAlly(allyObj);
                    
                    // Check if the ally data's list has items
                    if(allyData.IdsCount > 0)
                    {   
                        // Loop over the player's inventory to restore it
                        for (int index = 0; index < allyInventory.MaxSpace; index++)
                        {
                            allyInventory.AddItemFromSave(playerNum + 7, allyData.GetItemId(index), index);
                        } // end for
                    } // end if
                }

                // Get the Inventory component
                PlayerInventory inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").
                    GetComponent<PlayerInventory>();

                // Create the list of items for the player
                inventory.CreatePlayerItemList(playerNum);

                // Loop over the player's inventory to restore it
                for (int index = 0; index < (inventory.BonusSlotEnd + 1); index++)
                {
                    inventory.AddItemFromSave(playerNum, playerData.GetItemId(index), index);
                } // end for
            } // end if
        } // end LoadPlayer

        // Loads all players
        public void LoadPlayers(bool isDataOnly = false)
        {
            // Loop over the dictionary to load each player; We use the player name dictionary here
            foreach (var player in playerNames)
            {
                // Check if the player is playing
                if (player.Key < numPlayers)
                {
                    // Load the current player
                    Instance.LoadPlayer(player.Key, isDataOnly);
                } // end if
            } // end foreach
        } // end LoadPlayers

        #endregion

        #region High Scores

        // Save the highscore table
        public void SaveHighScores()
        {
            // The full high scores save path
            string highScoresSavePath = highScoreFilePath + saveFileExt;

            // Create a new binary formatter to save to a binary file
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            // Declare the filestream for the file
            FileStream fileStream;

            // Check if the player's save file exists
            if (File.Exists(highScoresSavePath))
            {
                // The file exists so open the file
                fileStream = File.Open(highScoresSavePath, FileMode.Open, FileAccess.ReadWrite);
            } // end if
            else
            {
                // Otherwise the file doesn't exist so create it
                fileStream = File.Create(highScoresSavePath);
            } // end else

            // Create a new high scores instance
            HighScores highScores = new HighScores();

            // Loop over the table MaxScores times
            for (int index = 0; index < highScoreTable.MaxScores; index++)
            {
                // Get the entry
                var entry = highScoreTable.GetScore(index);

                // Add it to the HighScores instance
                highScores.AddName(entry.First);
                highScores.AddScore(entry.Second);
            } // end for
            
            // Now write the data to the file
            binaryFormatter.Serialize(fileStream, highScores);

            // Finally, close the file stream
            fileStream.Close();
        } // end SaveHighScores

        // Load the highscore table
        public void LoadHighScores()
        {
            // The full high scores save path
            string highScoresSavePath = highScoreFilePath + saveFileExt;

            // The file stream for loading the scores
            FileStream fileStream;

            // Make sure the high score's save file exists before trying to load it
            if (File.Exists(highScoresSavePath))
            {
                // Create a new binary formatter to load the binary file
                BinaryFormatter binaryFormater = new BinaryFormatter();

                // Open a file stream while opening the file
                fileStream = File.Open(highScoresSavePath, FileMode.Open);

                // Create a high scores instance from the file
                HighScores highScores = (HighScores)binaryFormater.Deserialize(fileStream);

                // Now close the file stream
                fileStream.Close();

                // Create a new scores reference if needed
                if (highScoreTable == null)
                {
                    HighScoreTable table = new HighScoreTable();

                    // Loop over the HighScores instance MaxScores times
                    for (int index = 0; index < table.MaxScores; index++)
                    {
                        // Get the name and score of the entry
                        string name = highScores.GetName(index);
                        int score = highScores.GetScore(index);

                        // Add it to the table
                        table.AddScoreFromSave(name, score);
                    } // end for

                    // Set the scores reference to the table
                    highScoreTable = table;
                } // end if
                else
                {
                    // Loop over the HighScores instance MaxScores times
                    for (int index = 0; index < highScoreTable.MaxScores; index++)
                    {
                        // Get the name and score of the entry
                        string name = highScores.GetName(index);
                        int score = highScores.GetScore(index);

                        // Add it to the table
                        highScoreTable.AddScoreFromSave(name, score);
                    } // end for
                } // end else
            } // end if
            else
            {
                // Otherwise the file doesn't exist so create it
                fileStream = File.Create(highScoresSavePath);
                // Close the filestream
                fileStream.Close();

                // Create a new scores reference
                HighScoreTable table = new HighScoreTable();

                // Set the scores reference to the table
                highScoreTable = table;

                // Fill the table
                highScoreTable.FillTable();
            } // end else
        } // end LoadHighScores

        #endregion

        #region Resources

        // Save the resource positions
        public void SaveResources()
        {
            // The full resource position's save path
            string resourcesSavePath = resourceFilePath + saveFileExt;

            // Create a new binary formatter to save to a binary file
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            // Declare the filestream for the file
            FileStream fileStream;

            // Check if the player's save file exists
            if (File.Exists(resourcesSavePath))
            {
                // The file exists so open the file
                fileStream = File.Open(resourcesSavePath, FileMode.Open, FileAccess.ReadWrite);
            } // end if
            else
            {
                // Otherwise the file doesn't exist so create it
                fileStream = File.Create(resourcesSavePath);
            } // end else

            // Create a new resource positions instance
            ResourcePositionList resourcePostions = new ResourcePositionList();

            // Get a reference to the tile manager
            TileManager tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();

            // Get the resource positions from the TileDictionary
            List<Vector2> positions = tileManager.ResourcePositions;

            // Loop over the positions
            for (int index = 0; index < positions.Count; index++)
            {
                // Get the position
                Vector3 pos = positions[index];

                // Add it to the resource positions instance
                resourcePostions.AddPosition(pos);
            } // end for

            // Now write the data to the file
            binaryFormatter.Serialize(fileStream, resourcePostions);

            // Finally, close the file stream
            fileStream.Close();
        } // end SaveResources

        // Load the resource positions
        public List<Vector2> LoadResources()
        {
            // The full resource position's save path
            string resourcesSavePath = resourceFilePath + saveFileExt;

            // Create a new list of vector2
            List<Vector2> positions = new List<Vector2>();

            // Make sure the high score's save file exists before trying to load it
            if (File.Exists(resourcesSavePath))
            {
                // Create a new binary formatter to load the binary file
                BinaryFormatter binaryFormater = new BinaryFormatter();

                // Open a file stream while opening the file
                FileStream fileStream = File.Open(resourcesSavePath, FileMode.Open);

                // Create a resource positions instance from the file
                ResourcePositionList resourcePostions = (ResourcePositionList)binaryFormater.Deserialize(fileStream);

                // Now close the file stream
                fileStream.Close();

                // Loop over the resource positions and add them to the list
                for (int index = 0; index < resourcePostions.Count; index++)
                {
                    // Get and add the position
                    positions.Add(resourcePostions.GetPosition(index));
                } // end for
            } // end if

            // Return the positions list
            return positions;
        } // end LoadResources

        #endregion

        #region Levels

        // Loads a level by its index
        public void LoadLevel(int level)
        {
            // Reset the collections first
            Instance.ResetCollections();

            // Get the Invenory GameObject
            GameObject inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").gameObject;
            // Check if it exists
            if (inventory != null)
            {
                // Clean the inventory
                inventory.GetComponent<PlayerInventory>().Clean();
            } // end if

            // Then load the level
            Application.LoadLevel(level);
        } // end LoadLevel

        // Loads a level by its name
        public void LoadLevel(string level)
        {
            // Get the Invenory GameObject
            var inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory");
            // Check if it exists
            if (inventory != null)
            {
                // Clean the inventory
                inventory.GetComponent<PlayerInventory>().Clean();
            } // end if
            
            // Reset the collections first
            Instance.ResetCollections();

            // Then load the level
            Application.LoadLevel(level);
        } // end LoadLevel

        #endregion

        #endregion

        // Switches turns through the number of players and returns the current turn
        public int NextTurn()
        {
            // Its now the next players turn
            turn++;

            // Loop back to player 1 if we reached the end of the players turns
            if (turn >= numPlayers)
            {
                // The player's list is zero-index based so setting to zero is player 1.
                turn = 0;
            }

            // Return the turn
            return turn;
        } // end NextTurn
        
        // Gets who's turn it is
        public int Turn
        {
            get { return turn; }
        } // end Turn

        // Gets the max number of players allowed
        public int MaxPlayers
        {
            get { return maxPlayers; }
        } // end MaxPlayers

        // Gets the current number of players
        public int NumPlayers
        {
            get { return numPlayers; }
            set { numPlayers = Utility.ClampInt(value, 1, MaxPlayers); }
        } // end NumPlayers

        // Gets and Sets the type of map used for the battle scene
        public BattleMap BattleMap
        {
            get { return battleMap; }
            set { battleMap = value; }
        } // end BattleMap

        // Gets and Sets whether the game is new or loaded
        public bool IsNew
        {
            get { return isNew; }
            set { isNew = value; }
        } // end IsNew

        // Gets and Sets whether the game is single player
        public bool IsSinglePlayer
        {
            get { return isSinglePlayer; }
            set { isSinglePlayer = value; }
        } // end IsSinglePlayer

        // Gets and Sets whether the AI is enabled
        public bool IsAIEnabled
        {
            get { return isAIEnabled; }
            set { isAIEnabled = value; }
        } // end IsAIEnabled

        // Gets the highscore table
        public HighScoreTable ScoresTable
        {
            get { return highScoreTable; }
        } // end ScoresTable

        // Gets a copy of the enemyIdentifiers list
        public List<int> EnemyIdentifiers
        {
            get
            {
                // Create a copy of the list
                List<int> tmp = new List<int>(enemyIdentifiers);

                // Return the copy
                return tmp;
            } // end get
        } // end EnemyIdentifiers

        // Gets a copy of the tempAllyIdentifiers list
        public List<int> TempAllyIdentifiers
        {
            get
            {
                // Create a copy of the list
                List<int> tmp = new List<int>(tempAllyIdentifiers);

                // Return the copy
                return tmp;
            } // end get
        } // end AllyIdentifiers
    } // end GameMaster
} // end GSP.Core
