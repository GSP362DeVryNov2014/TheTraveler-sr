/*******************************************************************************
 *
 *  File Name: Battle.cs
 *
 *  Description: The battle conducted in the battle scene
 *
 *******************************************************************************/
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using GSP.Core;
using GSP.Char;
using GSP.Entities;
using GSP.Entities.Interfaces;
using GSP.Entities.Hostiles;
using GSP.Entities.Neutrals;
using GSP.Items;
using GSP.Items.Inventories;

namespace GSP
{
	/*******************************************************************************
     *
     * Name: Battle
     * 
     * Description: Allows player to take turns attacking an enemy during a 
     * MapEvent Enemy.
     * 
     *******************************************************************************/
	public class Battle : MonoBehaviour 
	{
		// IDamageable objects
		IDamageable player;				// Damageable Player object
		Merchant playerMerchant;		// Player Merchant object
		int playerAttack;				// Original player attack
		int playerDefense;				// Original player defense
		IDamageable enemy;				// Damageable Enemy object
		int enemyID;					// ID for reference
		Bandit enemyEntity;				// Bandit object

		// HUD Objects
		Text playerName;				// Player's name
		Text enemyName;					// Enemy's name - Usually fixed
		Text playerHealth;				// Remaining player health
		Text enemyHealth;				// Remaining enemy health
		Text fightBoxText;				// Box of turn by turn blows
		RectTransform playerBar;		// Visual of player's health
		RectTransform enemyBar;			// Visual of enemy's health

		// Fight box flavor
		int linesOfText;				// How many lines the fight box has
		List<string> verbs;				// Different fighting verbs
		string randomVerb;					// The random verb to be used from the list

		// Fight controls
		bool playerTurn;				// Whether or not it's the player's turn
		int playerNum;					// Number of the player for easy reference
		Die die;						// The die to roll for damage mods
		int damage;						// Damage dealt during battle

		// Use this for initialization
		void Start () 
		{
			// Init control
			playerTurn = true;
			playerNum = GameMaster.Instance.Turn;
			die = new Die ();
			die.Reseed (Environment.TickCount);
			damage = 0;

			// Init IDamageable objects
			// Create the enemy
			GameMaster.Instance.CreateEnemy(HostileType.Bandit, "Bandit");
			
			// Get the enemyID from the list of enemy IDs; since this a 1v1 fight there should only be a single ID
			enemyID = GameMaster.Instance.EnemyIdentifiers[0];
			
			// Get the enemy entity
			enemyEntity = (Bandit)EntityManager.Instance.GetEntity(enemyID);
			
			// Set the stats of the enemy
			enemyEntity.AttackPower = die.Roll(1, 9) + 5;
			enemyEntity.DefencePower = die.Roll(1, 9) + 5;

			// Set sprite of enemy
			if(enemyEntity.AttackPower > enemyEntity.DefencePower)
			{
				GameObject.Find ("Battler2Sprite").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Bandit1");
			} //end if
			else
			{
				GameObject.Find ("Battler2Sprite").GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Bandit2");
			}

			// Set the enemy
			enemy = (IDamageable) enemyEntity;

			// Get the player's merchant
			GameMaster.Instance.LoadPlayers ();
			playerMerchant = (Merchant)GameMaster.Instance.GetPlayerScript(playerNum).Entity;
			playerAttack = playerMerchant.AttackPower;
			playerDefense = playerMerchant.DefencePower;
			GameObject.Find ("Battler1Sprite").GetComponent<Image> ().sprite = playerMerchant.GetSprite (2);

			// Set the player
			player = (IDamageable)playerMerchant;

			// Set background
			if(GameMaster.Instance.BattleMap == BattleMap.area01)
			{
				GameObject.Find("Background").GetComponent<Image>().sprite = Resources.Load<Sprite> ("BattleBackgrounds/Battle_Desert");
			} //end if
			else if(GameMaster.Instance.BattleMap == BattleMap.area02)
			{
				GameObject.Find("Background").GetComponent<Image>().sprite = Resources.Load<Sprite> ("BattleBackgrounds/Battle_Euro");
			} //end else if
			else if(GameMaster.Instance.BattleMap == BattleMap.area03)
			{
				GameObject.Find("Background").GetComponent<Image>().sprite = Resources.Load<Sprite> ("BattleBackgrounds/Battle_Metro");
			} //end if
			else
			{
				GameObject.Find("Background").GetComponent<Image>().sprite = Resources.Load<Sprite> ("BattleBackgrounds/Battle_Snow");
			} //end else

			// Init and set HUD objects
            GameObject.Find("Canvas").transform.Find("PlayerInventory").gameObject.SetActive(false);
            GameObject.Find("Canvas").transform.Find("AllyInventory").gameObject.SetActive(false);
			GameObject.Find("Canvas").transform.Find("Tooltip").gameObject.SetActive(false);
			playerName = GameObject.Find ("Battler1Name").GetComponent<Text> ();
			playerName.text = GameMaster.Instance.GetPlayerName (playerNum);
			enemyName = GameObject.Find ("Battler2Name").GetComponent<Text> ();
			GameObject.Find ("Battler1Attack").GetComponent<Text> ().text = playerMerchant.AttackPower.ToString ();
			GameObject.Find ("Battler2Attack").GetComponent<Text> ().text = enemyEntity.AttackPower.ToString ();
			GameObject.Find ("Battler1Defense").GetComponent<Text> ().text = playerMerchant.DefencePower.ToString ();
			GameObject.Find ("Battler2Defense").GetComponent<Text> ().text = enemyEntity.DefencePower.ToString ();
			playerHealth = GameObject.Find ("Battler1Health").GetComponent<Text> ();
			playerHealth.text = player.Health.ToString ();
			enemyHealth = GameObject.Find ("Battler2Health").GetComponent<Text> ();
			enemyHealth.text = enemy.Health.ToString ();
			playerBar = GameObject.Find ("Battler1HealthLeft").GetComponent<RectTransform> ();
			enemyBar = GameObject.Find ("Battler2HealthLeft").GetComponent<RectTransform> ();
			fightBoxText = GameObject.Find ("FightText").GetComponent<Text> ();

			// Init fight box and flavor
			fightBoxText.text = "The battlers square off!";
			linesOfText = 1;
			verbs = new List<string>();
			verbs.Add ("attacked");
			verbs.Add ("retaliated against");
			verbs.Add ("hammered");
			verbs.Add ("struck");
			verbs.Add ("lashed at");

            // Check if the player is an AI
            if (playerMerchant.GameObj.GetComponent<Player>().IsAI)
            {
                // Disable the fight buttons
                GameObject.Find("AttackButtons/HeadButton").GetComponent<Button>().interactable = false;
                GameObject.Find("AttackButtons/TorsoButton").GetComponent<Button>().interactable = false;
                GameObject.Find("AttackButtons/FeintButton").GetComponent<Button>().interactable = false;
            }
		}//end Start
	
		void Update()
		{
			if(playerTurn && !playerMerchant.GameObj.GetComponent<Player>().IsAI)
			{
				if(Input.GetKeyDown(KeyCode.Alpha1))
				{
					Head ();
				} //end if 1 key
				if(Input.GetKeyDown(KeyCode.Alpha2))
				{
					Torso();
				} //end if 2 key
				if(Input.GetKeyDown(KeyCode.Alpha3))
				{
					Feint();
				} //end if 3 key
			} //end if playerTurn && !IsAI
		} //end Update

		void Fight()
		{
			// Play a sword sound
			AudioManager.Instance.PlaySword ();

			// Determine whether it's player's turn, and update with result
			if (playerTurn) 
			{
				fightBoxText.text += "\n" + playerName.text + " " + randomVerb + " " +
					enemyName.text + " for " + damage;
				enemy.TakeDamage (damage);
				enemyHealth.text = enemy.Health.ToString ();
				float test = (float)enemy.Health / (float)enemy.MaxHealth;
				enemyBar.localScale = new Vector3 (test, 1f, 1f);
			} //end if
			else
			{
				fightBoxText.text += "\n" + enemyName.text + " " + randomVerb + " " +
					playerName.text + " for " + damage;
				player.TakeDamage (damage);
				playerHealth.text = player.Health.ToString ();
				float test = (float)player.Health / (float)player.MaxHealth;
				playerBar.localScale = new Vector3 (test, 1f, 1f);
			} //end else

			// Update turn and fight box
			playerTurn = !playerTurn;
			linesOfText++;
			if(linesOfText > 4)
			{
				fightBoxText.transform.position = new Vector3(
					fightBoxText.transform.position.x,
					fightBoxText.transform.position.y + 26);
			} //end if

			// Check if the player lost the fight
			if(player.IsDead)
			{
				// Stop the fight repeating
				CancelInvoke();

				// Set up inventory
				GameObject.Find("Canvas").transform.Find("Tooltip").gameObject.SetActive(true);
                GameObject.Find("Canvas").transform.Find("PlayerInventory").gameObject.SetActive(true);
                GameObject.Find("Canvas").transform.Find("AllyInventory").gameObject.SetActive(true);
                PlayerInventory inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>();

				// The player lost the fight, remove its resources or its weapon
				fightBoxText.text += "\n" + enemyName.text + " wins!";
				if(playerMerchant.TotalWeight <= 0)
				{
					// Check if the player has a weapon
					if (playerMerchant.EquippedWeapon != null)
					{
						// The player has no resources so remove its weapon
						fightBoxText.text += "\nAs a result, you lost your " + playerMerchant.EquippedWeapon.Name;
						playerAttack -= playerMerchant.EquippedWeapon.AttackValue;	
						playerMerchant.UnequipWeapon(playerMerchant.EquippedWeapon);					
						inventory.Remove(playerNum, inventory.WeaponSlot);
					} // end if EquippedWeapon != null
					else
					{
						fightBoxText.text += "...but\n you weren't worth mugging.";
					} //end else EquippedWeapon == null
				} // end if TotalWeight <= 0
				// Otherwise, the player has resources so remove a random resource
				else
				{
					/* Choose a resource;
                     * Choose a number between one and the integer value of Resource.None
                     * Then subtract one to get a random number between zero and the integer value before Resource.None
                     * Example: Die roll of one to four; random number is between zero and three
                     */
                    int resourceNumber = die.Roll(1, (int)ResourceType.None) - 1;
                    Debug.LogFormat("Resource number is {0}", resourceNumber);

					// Check if the player has the resource; Don't display the message if they don't have the resource
                    if (playerMerchant.ResourceUtility.GetResourcesByType((ResourceType)resourceNumber, GameMaster.Instance.Turn).Count != 0)
                    {
                        // Remove the resources by list
                        playerMerchant.ResourceUtility.RemoveResourcesByType((ResourceType)resourceNumber, GameMaster.Instance.Turn);
                        fightBoxText.text += " \nAs a result, you lost all your " + Enum.GetName(typeof(ResourceType), resourceNumber);
                    } // end if Count != 0
				} // end else TotalWeight > 0
				// Disable inventory and tooltip
                GameObject.Find("Canvas").transform.Find("PlayerInventory").gameObject.SetActive(false);
				GameObject.Find("Canvas").transform.Find("Tooltip").gameObject.SetActive(false);

				// Update the fightbox position
				fightBoxText.transform.position = new Vector3(
					fightBoxText.transform.position.x,
					fightBoxText.transform.position.y + 40);

				// Return to game after 3 seconds
				playerTurn = false;
				Invoke ("EndFight", 3.0f);
			} // end if player IsDead
			// Check if enemy lost
			else if(enemy.IsDead)
			{
				// Stop fight repeating
				CancelInvoke();

				// Update fight text box
				fightBoxText.text += "\n" + playerName.text + " wins!";

				// Update the fightbox position
				fightBoxText.transform.position = new Vector3(
					fightBoxText.transform.position.x,
					fightBoxText.transform.position.y + 26);

				// Return to game after 3 seconds
				playerTurn = false;
				Invoke ("EndFight", 3.0f);
			} //end else if
			//Otherwise, let the enemy fight
			else if(!playerTurn)
			{
				Invoke("EnemyFight", 0.5f);
			} //end else if
		} //end Fight

		void EndFight()
		{
			// Remove the enemy entity
			EntityManager.Instance.RemoveEntity(enemyID);
			
			// Remove the entity from the list of identifiers.
			GameMaster.Instance.RemoveEnemyIdentifier(enemyID);

			// Restore player health and stats
			player.ResetHealth ();
			playerMerchant.AttackPower = playerAttack;
			playerMerchant.DefencePower = playerDefense;

			// Update to the next player
			GameMaster.Instance.NextTurn();

			// Save players
			GameMaster.Instance.SavePlayers ();

			// Return to game
			GameMaster.Instance.LoadLevel (GameMaster.Instance.BattleMap.ToString ());
		} //end EndFight

		// Enemy fighting function
		void EnemyFight()
		{
			int dieRoll = die.Roll (1, 3);
			switch (dieRoll) 
			{
				case 1:
				{
					// Battle characters
					Fight fighter = new Fight();
					damage = fighter.CharacterFightHead<Bandit>(playerMerchant, playerTurn, die);
					if(playerMerchant.DefencePower > 0)
					{
						playerMerchant.DefencePower -= 1;
					} //end if
					GameObject.Find ("Battler1Defense").GetComponent<Text> ().text = playerMerchant.DefencePower.ToString ();

					// Generate fight box verb
					randomVerb = verbs [Random.Range (0, verbs.Count)];
				
					// Process fight
					Invoke ("Fight", 0.0f);
					break;
				} //end case Head
				case 2:
				{
					// Battle characters
					Fight fighter = new Fight();
					damage = fighter.CharacterFightTorso<Bandit>(playerMerchant, playerTurn, die);
				
					// Generate fight box verb
					randomVerb = verbs [Random.Range (0, verbs.Count)];
				
					// Process fight
					Invoke ("Fight", 0.0f);
					break;
				} //end case Torso
				case 3:
				{
					// Battle characters
					Fight fighter = new Fight();
					damage = fighter.CharacterFightFeint<Bandit>(playerMerchant, playerTurn, die);
					playerMerchant.AttackPower *= 2;
					GameObject.Find ("Battler1Attack").GetComponent<Text> ().text = playerMerchant.AttackPower.ToString ();

					// Generate fight box verb
					randomVerb = "feinted at";
				
					// Process fight
					Invoke ("Fight", 0.0f);
					break;
				} //end case Feint
				default:
				{
					break;
				} //end case default
			} //end switch
		} //end EnemyFight

		// Function for Head button - Lower opponent defense, lower damage
		public void Head()
		{
			if(playerTurn)
			{
				// Battle characters
				Fight fighter = new Fight();
				damage = fighter.CharacterFightHead<Bandit>(playerMerchant, playerTurn, die);
				if(enemyEntity.DefencePower > 0)
				{
					enemyEntity.DefencePower -= 1;
				} // end if
				GameObject.Find ("Battler2Defense").GetComponent<Text> ().text = enemyEntity.DefencePower.ToString ();

				// Generate fight box verb
				randomVerb = verbs [Random.Range (0, verbs.Count)];

				// Process fight
				Invoke ("Fight", 0.0f);
			} //end if
		} //end Head

		// Function for Torso button - Regular damage
		public void Torso()
		{
			if (playerTurn) 
			{
				// Battle characters
				Fight fighter = new Fight ();
				damage = fighter.CharacterFightTorso<Bandit> (playerMerchant, playerTurn, die);

				// Generate fight box verb
				randomVerb = verbs [Random.Range (0, verbs.Count)];

				// Process fight
				Invoke ("Fight", 0.0f);
			} //end if
		} //end Torso

		// Function for Feint button - Doubles opponent attack, chance at double damage
		public void Feint()
		{
			if (playerTurn) 
			{
				// Battle characters
				Fight fighter = new Fight ();
				damage = fighter.CharacterFightFeint<Bandit> (playerMerchant, playerTurn, die);
				enemyEntity.AttackPower *= 2;
				GameObject.Find ("Battler2Attack").GetComponent<Text> ().text = enemyEntity.AttackPower.ToString ();

				// Generate fight box verb
				randomVerb = "feinted at";

				// Process fight
				Invoke ("Fight", 0.0f);
			} //end if
		} //end Feint
	} //end Battle class
} //end namespace GSP