/*******************************************************************************
 *
 *  File Name: MapEvent.cs
 *
 *  Description: The logic for the events that happen on the maps
 *
 *******************************************************************************/
using GSP.Char;
using GSP.Core;
using GSP.Entities;
using GSP.Entities.Friendlies;
using GSP.Entities.Hostiles;
using GSP.Entities.Neutrals;
using GSP.Items;
using GSP.Items.Inventories;
using GSP.Tiles;
using System;
using UnityEngine;

namespace GSP
{
    /*******************************************************************************
     *
     * Name: MapEvent
     * 
     * Description: Deals with the logic behind the different map's events.
     * 
     *******************************************************************************/
    public class MapEvent : MonoBehaviour
	{
		GameObject player;          // The player's GameObject reference
		Player playerScript;        // The player's Character script component reference
        Merchant playerMerchant;    // The player's merchant Entity for convienience
        PlayerInventory inventory;        // The inventory script

		//NOTE!!
		//SIZE must be the last item in the enum so that anything based
		//on the length of the enum can be used as normal. It is best to
		//add items to the left of SIZE but after the current 2nd to last
		//item in the enum. For instance if the list was {Enemy, Ally, Size}
		//you should enter the new item between Ally and Size
		
		// The enumeration for normal tile events
		enum NormalTile {Enemy, Ally, Item, Nothing, Size};
		
		// The enumeration for the resource tile events
		enum ResourceTile {Wool, Wood, Fish, Ore, Size};
		
		//Event chances
		//NOTE: These should add up to less than 100 so that 
		//there is a chance nothing occurs. Minimum chance of
		//one or else there will be problems
        int enemyChance = 25;   // The minimum chance for the MapEvent to an enemy
        int allyChance = 55;    // The minimum chance for the MapEvent to an ally
        int itemChance = 15;    // The minimum chance for the MapEvent to an item
		
		string guiResult; // The MapEvent summary

        //Calls map event and returns string
		public string DetermineEvent(int playerNum, Die die)
		{
			// Set the player GameObject and its Player script
            player = GameMaster.Instance.GetPlayerObject(playerNum);
            playerScript = GameMaster.Instance.GetPlayerScript(playerNum);

            // Set the Merchant entity for convienience
            playerMerchant = (Merchant)playerScript.Entity;

            // Get the inventory script
            inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>();
			
			// Get the player's position
			Vector3 tmp = player.transform.localPosition;

            // Get the tile manager's reference
            TileManager tileManager = GameObject.Find("TileManager").GetComponent<TileManager>();

            // Get the resource type of the tile
            ResourceType resourceTileType = tileManager.GetResourceType(tmp);
            // Get the market type of the tile
            MarketType marketTileType = tileManager.GetMarketType(tmp);
			
			// Was the tile type a non-resource and non-market?
            if (resourceTileType == ResourceType.None && marketTileType == MarketType.None)
			{
				// Roll a die to get a number from 1-100
                if (die == null)
                {
                    Debug.LogError("ME: die is null!");
                }
                int dieResult = die.Roll (1, 100);

                // Check for an enemy
				if(dieResult < enemyChance)
				{
					return ResolveFight(die);
				} // end if
				// Check for an ally
                else if (dieResult < allyChance + enemyChance && dieResult >= enemyChance)
				{
					return "Ally";
				} // end else if
				// Check for an item
                else if(dieResult < itemChance + allyChance + enemyChance && dieResult >= allyChance + enemyChance)
				{
					return ResolveItem(die);
				} // end else if
				else
				{
					// The MapEvent was nothing
					guiResult = "No map event occured.";
					return "Nothing";
				} // end else
			} //end if
			// Otherwise, the tile type is a resource or market
			else
			{
                // Check if the tile is not a market
                if (marketTileType == MarketType.None)
                {
                    // Get the resource from the database
                    Item temp = ItemDatabase.Instance.Items.Find(resource => resource.Type == resourceTileType.ToString());

                    // Pick up the resource
                    playerMerchant.PickupResource((Resource)temp, 1);

                    // Declare what was landed on
                    guiResult = "You got a resource:\n" + temp.Name;

                    // Play found for what was landed on
                    if (temp.Name == "Fish")
                    {
                        // Play fish sound
                        AudioManager.Instance.PlayFish();
                    } // end if
                    else if (temp.Name == "Wood")
                    {
                        // Play wood sound
                        AudioManager.Instance.PlayWood();
                    } // end else if
                    else if (temp.Name == "Wool")
                    {
                        // Play wool sound
                        AudioManager.Instance.PlayShear();
                    } // end else if
                    else
                    {
                        // Play ore sound
                        AudioManager.Instance.PlayMine();
                    } // end else
                    
                    return guiResult;
                } // end if
                // Otherwise the tile is a market
                else
                {
                    // Check which type of market the player is one
                    if (marketTileType == MarketType.Market)
                    {
                        // The player landed on regular market
                        return "Market";
                    } // end if
                    else
                    {
                        // The player landed on the end game village
                        return "Village";
                    } // end else
                } // end else
			} // end else
		} // end DetermineEvent

        // NOTE: Hard-coded for now to work with only 1 enemy type; it works for now. :P
        // Resolves a fight when the enemy MapEvent spawns
        public string ResolveFight(Die die)
		{
			GameMaster.Instance.SavePlayers ();
            GameMaster.Instance.SaveResources();
			GameMaster.Instance.LoadLevel ("BattleScene");
			return "Enemy fought";
		} // end ResolveFight

        // NOTE: Hard-coded for now to work with only 1 ally type; it works for now. :P
        // Resolves an ally when the ally MapEvent spawns
        public string ResolveAlly(string guiAcceptResult)
		{
			// Create the ally
            GameMaster.Instance.CreateAlly(FriendlyType.Porter, "Porter");

            // Get the allyID from the temporary list of ally IDs
			// Since there is only one ally there should only be a single ID
            int allyID = GameMaster.Instance.TempAllyIdentifiers[0];

			// Check if the player accepts the ally
            if (guiAcceptResult == "YES")
			{
                bool addResult; // The result of trying to add the ally
                
                // Get the enemy entity
                Porter allyEntity = (Porter)EntityManager.Instance.GetEntity(allyID);

                // Get the player's AllyList component
                AllyList playerAllyScript = player.GetComponent<AllyList>();
                // Add the ally to the player's ally list
				addResult = playerAllyScript.AddAlly(allyEntity.GameObj);

                // Check if the add was successful
                if (addResult)
                {
                    // Set the ally was accepted
                    guiResult = "Ally was added.";
                } // end if
                else
                {
                    // Otherwise, the ally failed to add because the limit was reached
                    guiResult = "Ally limit reached.\nAdd denied.";

                    // Remove the ally entity
                    EntityManager.Instance.RemoveEntity(allyID);
                } // end else

                // Clear the TempAllyIdentifiers now
                GameMaster.Instance.ClearAllyIdentifiers();

                // Return the result
				return guiResult;
			}

			//Check if the player declines the ally
            else if (guiAcceptResult == "NO")
			{
				// Remove the ally entity
                EntityManager.Instance.RemoveEntity(allyID);

                // Clear the TempAllyIdentifiers now
                GameMaster.Instance.ClearAllyIdentifiers();

				// Set and return declined
				guiResult = "Ally was declined.";
				return guiResult;
			} // end else if

			// Otherwise wait for answer
			return "No choice was made.";
		} // end ResolveAlly

        // Resolves an item when the item MapEvent spawns
        public string ResolveItem(Die die)
		{
			// String to return for display
			string result = string.Empty;
			
			// Determine what item was found
			int itemType = die.Roll(1, 3);

            // The item to add to the player's inventory
            Item item = null;

            // Switch ove the itemType
            switch (itemType)
            {
                // A weapon item
                case 1:
                    {
                        // Pick an item from the weapons enumeration
                        int itemNumber = die.Roll(1, (int)WeaponType.Size) - 1;

                        // Find the item in the database
                        item = ItemDatabase.Instance.Items.Find(tempItem => tempItem.Type == ((WeaponType)itemNumber).ToString());
                        break;
                    } // end case 1
                // Am armour item
                case 2:
                    {
                        // Pick an item from the armor enumeration
                        int itemNumber = die.Roll(1, (int)ArmorType.Size) - 1;

                        // Find the item in the database
                        item = ItemDatabase.Instance.Items.Find(tempItem => tempItem.Type == ((ArmorType)itemNumber).ToString());
                        break;
                    } // end case 2
                // A bonus item (inventory/weight
                case 3:
                    {
                        // Pick an item from the inventory enumeration
                        int itemNumber = die.Roll(1, (int)BonusType.Size) - 1;

                        // Find the item in the database
                        item = ItemDatabase.Instance.Items.Find(tempItem => tempItem.Type == ((BonusType)itemNumber).ToString());
                        break;
                    } // end case 3
                // An invalid item
                default:
                    {
                        result = "non-existant item. Nothing given.";
                        break;
                    } // end case default
            } // end switch itemType

            if (item != null)
            {
                // Return the resulting item's name
                result = item.Name;
                
                // Add the item to the player's inventory
                inventory.AddItem(0, GameMaster.Instance.Turn, item.Id, SlotType.Inventory);
            } // end if
			
			// Set and return the result
            guiResult = "You got \n" + result;
            return guiResult;
		} // end ResolveItem

		// Gets the result string
        public string GetResultString()
		{
			return guiResult;
		} // end GetResultString
	} // end MapEvent
} // end GSP
