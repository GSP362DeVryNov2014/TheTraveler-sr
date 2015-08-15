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
using GSP.Tiles;
using System;
using System.Collections.Generic;
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
        int allyChance = 15;    // The minimum chance for the MapEvent to an ally
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
			
			// Get the tile at the player's position
			Vector3 tmp = player.transform.localPosition;

			// Fix the z-axis; change by Damien to get the tiles to work again.
			tmp.z = -0.01f;
            Tile currentTile = TileDictionary.GetTile(TileManager.ToPixels(tmp));
			
			// Was a tile found?
			if(currentTile == null)
			{
				// No tile found so return "This is not a valid \ntile. No event occured.";
				guiResult = "This is not a valid \ntile. No event occured.";
				return "Nothing";
			} // end if
			// Otherwise, was the tile a non-resource?
			else if (currentTile.ResourceType == ResourceType.None) 
			{
				// Roll a die to get a number from 1-100
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
			// Otherwise, the tile is a resource
			else
			{
                // Create a temp resource
                Resource temp = GameMaster.Instance.CreateResource(currentTile.ResourceType);
				
				// Pick up the resource
                playerMerchant.PickupResource(temp, 1);
				
				// Declare what was landed on
				guiResult = "You got a resource:\n" + temp.Name;

                // Play found for what was landed on
                if(temp.Name == "Fish")
                {
                    // Play fish sound
					AudioManager.Instance.PlayFish();
                } // end if
                else if(temp.Name == "Wood")
                {
                    // Play wood sound
					AudioManager.Instance.PlayWood();
                } // end else if
                else if(temp.Name == "Wool")
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
			} // end else
		} // end DetermineEvent

        // NOTE: Hard-coded for now to work with only 1 enemy type; it works for now. :P
        // Resolves a fight when the enemy MapEvent spawns
        public string ResolveFight(Die die)
		{
			// Create the enemy
            GameMaster.Instance.CreateEnemy(HostileType.Bandit, "Bandit");

            // Get the enemyID from the list of enemy IDs; since this a 1v1 fight there should only be a single ID
            int enemyID = GameMaster.Instance.EnemyIdentifiers[0];
            
            // Get the enemy entity
            Bandit enemyEntity = (Bandit)EntityManager.Instance.GetEntity(enemyID);
			
			// Set the stats of the enemy
			enemyEntity.AttackPower = die.Roll(1, 9);
			enemyEntity.DefencePower = die.Roll(1, 9);
			
			//TODO: Brent: Add Battle Scene
			
			//Battle characters
			Fight fighter = new Fight();
			string result = fighter.CharacterFight<Bandit>(playerScript);
			
			// Check if the player lost the fight
			if(result.Contains("Enemy wins"))
			{
                // The player lost the fight, remove its resources or its weapon
				if(playerMerchant.TotalWeight <= 0)
				{
                    // Check if the player has a weapon
                    if (playerMerchant.EquippedWeapon != null)
                    {
                        // The player has no resources so remove its weapon
                        result += "\nAs a result, you lost your " + playerMerchant.EquippedWeapon.Name;
                        playerMerchant.UnequipWeapon(playerMerchant.EquippedWeapon);
                    } // end if playerMerchant.EquippedWeapon != null
					else
					{
						result += "...but\n you weren't worth mugging.";
					} //end else
				} // end if playerMerchant.TotalWeight <= 0
				// Otherwise, the player has resources so remove a random resource
				else
				{
					// Get the player's resource list script
					ResourceList tempList = player.GetComponent<ResourceList>();

                    /* Choose a resource;
                     * Choose a number between one and the integer value of Resource.None
                     * Then subtract one to get a random number between zero and the integer value before Resource.None
                     * Example: Die roll of one to four; random number is between zero and three
                     */
                    int resourceNumber = die.Roll(1, (int)ResourceType.None) - 1;
                    Debug.LogFormat("Resource number is {0}", resourceNumber);

					// Get the list of resources of that type
                    List<Resource> resList = tempList.GetResourcesByType(Enum.GetName(typeof(ResourceType), resourceNumber));

					// Check if the player has the resource; Don't display the message if they don't have the resource
                    if (resList.Count != 0)
                    {
                        // Remove the resources by list
                        tempList.RemoveResources(resList);
                        result += " \nAs a result, you lost all your " + Enum.GetName(typeof(ResourceType), resourceNumber);
                    } // end if resList.Count != 0
				} // end else
			} // end if

			// Remove the enemy entity
            EntityManager.Instance.RemoveEntity(enemyID);

            // Remove the entity from the list of identifiers.
            GameMaster.Instance.RemoveEnemyIdentifier(enemyID);

			// Set the summary and return it
			guiResult = result;
			return guiResult;
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
			string result;
			
			// Determine what item was found
			int itemType = die.Roll(1, 3);

            // Switch ove the itemType
            switch (itemType)
            {
                // A weapon item
                case 1:
                    {
                        // Pick an item from the weapons enumeration
                        int itemNumber = die.Roll(1, (int)WeaponType.Size) - 1;

                        // Assign the chosen number as the item
                        result = Enum.GetName(typeof(WeaponType), itemNumber);

                        // Equip the weapon on the player
                        playerMerchant.EquipWeapon(GameMaster.Instance.CreateWeapon((WeaponType)itemNumber));
                        break;
                    } // end case 1
                // Am armour item
                case 2:
                    {
                        // Pick an item from the armor enumeration
                        int itemNumber = die.Roll(1, (int)ArmorType.Size) - 1;

                        // Assign the chosen number as the item
                        result = Enum.GetName(typeof(ArmorType), itemNumber);

                        // Equip the armour on the player
                        playerMerchant.EquipArmor(GameMaster.Instance.CreateArmor((ArmorType)itemNumber));
                        break;
                    } // end case 2
                // A bonus item (inventory/weight
                case 3:
                    {
                        // Pick an item from the inventory enumeration
                        int itemNumber = die.Roll(1, (int)BonusType.Size) - 1;

                        // Assign the chosen number as the item
                        result = Enum.GetName(typeof(BonusType), itemNumber);

                        // Equip the bonus item on the player
                        playerMerchant.EquipBonus(GameMaster.Instance.CreateBonus((BonusType)itemNumber));
                        break;
                    } // end case 3
                // An invalid item
                default:
                    {
                        result = "non-existant item. Nothing given.";
                        break;
                    } // end case default
            } // end switch itemType
			
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
