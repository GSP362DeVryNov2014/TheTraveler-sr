using GSP.Core;
/*******************************************************************************
 *
 *  File Name: Player.cs
 *
 *  Description: Wrapper for the players
 *
 *******************************************************************************/
using GSP.Entities;
using GSP.Entities.Neutrals;
using GSP.Items;
using GSP.Items.Inventories;
using GSP.Tiles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GSP.Char
{
    /*******************************************************************************
     *
     * Name: Player
     * 
     * Description: The wrapper to the merchant entity. This is what is placed in
     *              the scene.
     * 
     *******************************************************************************/
    public class Player : MonoBehaviour
	{
        Merchant merchant;  // Each Player needs a Merchant GameObject
        bool isAI;          // Whether the player is a player or an AI

        #region AI Variables

        GameplayStateMachine GPSM;	// Active GPSM of the game
        GUIMovement GUIM;			// Active movement of the game
        int turnPhase;				// Moves AI along through turn
        bool isProcessing;			// Whether AI is already doing something
        bool isEnd;                 // Whether it's the end of the game

        #endregion

        // Called before Start()
        void Awake()
        {
            // Initialise the player to be a player and not an AI
            isAI = false;
        } // end Awake
        
        // Use this for initialisation
        void Start()
        {
            #region AI Variable Initialization

            // Initialize GPSM reference
            GPSM = GameObject.Find("Canvas").GetComponent<GameplayStateMachine>();

            // Initialize GUIM reference
            GUIM = GameObject.Find("Canvas").GetComponent<GUIMovement>();

            // Initialize turn phase to 0
            turnPhase = 0;

            // Initailize isProcessing to false
            isProcessing = false;

            // It's not the end of the game
            isEnd = false;

            #endregion
        } // end Start

        // Runs every frame; Used to direct AI's actions
        void Update()
        {
            #region AI Functionality

            if (merchant != null && IsAI)
            {
                // Otherwise, Check if it's the AI's turn
                if (GameMaster.Instance.Turn == merchant.PlayerNumber)
                {
                    // Roll Dice phase
                    if (turnPhase == 0)
                    {
                        // Only run once per turn
                        if (!isProcessing)
                        {
                            // Check if we are on the battle scene
                            if (Application.loadedLevelName == "BattleScene")
                            {
                                // Set isProcessing to true
                                isProcessing = true;

                                // Set the turnPhase to 2 so we can battle
                                turnPhase = 2;
                            } // end if
                            else
                            {
                                // Check if we're in the RollDice State
                                if (GPSM.GetState() == 1)
                                {
                                    // Set isProcessing to true
                                    isProcessing = true;

                                    // Otherwise, Only roll die once
                                    PressAction();
                                } // end if
                            } // end else
                        } //end if !isProcessing
                    } //end if
                    // Select Path to Take phase
                    else if (turnPhase == 1)
                    {
                        // Only run once the movement class has been initialised for the player
                        if (GPSM.IsMovementInitialized)
                        {
                            // Only run once per turn
                            if (!isProcessing)
                            {
                                // Set isProcessing to true
                                isProcessing = true;

                                // Only set target once
                                SetTarget();
                            } //end if

                            // Keep calling Move until Move ends itself
                            Move();
                        } // end if
                    } //end else if
                    // Do Action phase
                    else if (turnPhase == 2)
                    {
                        // Determine what scene we are in
                        if (Application.loadedLevelName == "BattleScene")
                        {
                            // If in battle scene, keep battling until BattleScene ends itself
                            Battle();
                        } //end if
                        else
                        {
                            // Only run once per turn
                            if (!isProcessing)
                            {
                                // Set isProcessing to true
                                isProcessing = true;
                                
                                // Only do the below events once
                                // Accept or Decline ally if prompted
                                if (GameObject.Find("Canvas").transform.Find("Accept").gameObject.activeInHierarchy)
                                {
                                    Ally();
                                } //end if Accept.activeInHierarchy
                                else
                                {
                                    Item();
                                } //end else
                            } //end if !isProcessing
                        } //end else scene != "BattleScene"
                    } //end else if
                    // Go to the next player's turn
                    else if (turnPhase == 3)
                    {
                        // Only run phase three if it's not the end of the game
                        if (!isEnd)
                        {
                            // Only run once per turn
                            if (!isProcessing)
                            {
                                // Set isProcessing to true
                                isProcessing = true;

                                // Only go to the next player once
                                PressAction();
                            } //end if !isProcessing

                            // Reset the variables
                            Reset();
                        } // end if
                    } //end else if
                } // end if
            } // end else

            #endregion
        } //end Update

        public void GetMerchant(int ID)
        {
            // Get the Merchant's reference
            merchant = (Merchant)EntityManager.Instance.GetEntity(ID);
        } // end GetMerchant

        // Updates the GameObject reference on the entity
        public void UpdateGameObject(GameObject obj)
        {
            merchant.UpdateGameObject(obj);
        } // end UpdateGameObject

        // Updates the script references on the entity
        public void UpdateScriptReferences()
        {
            merchant.UpdateScriptReferences();
        } // end UpdateScriptReferences
        
        // Allows for collision on the market place to end the game
        void OnCollisionEnter2D(Collision2D coll)
        {
			Debug.LogFormat("GameObject being collided with: {0}", coll.gameObject.name);
            // Layer 8 is the "Market"
            if (coll.gameObject.layer == 8)
            {
                // It's now the end of the game
                isEnd = true;
                
                // End the game by calling EndGame()
                GPSM.EndGame();
            }
        } // end OnCollisionEnter2D
        
        // Destroy the GameObject this script is attached to
		public void DestroyGO()
		{
			Destroy(this.gameObject);
        } // end DestroyGO

        // Gets the player's entity
        // This is used to get the entity to do things with the implemented interfaces; just cast
        // back to the proper type first
        public Entity Entity
        {
            get { return merchant; }
        } // end Entity

        // Gets and Sets whether tha player is an AI
        public bool IsAI
        {
            get { return isAI; }
            set { isAI = value; }
        } // end IsAI

        #region Wrapper for the merchant class

        // Setup the character's sprite set; This is an array of sprites that will be used for the character
        public void SetCharacterSprites(int playerNumber)
        {
            merchant.SetCharacterSprites(playerNumber);
        } // end SetCharacterSprites

        // Faces the merchant in a given direction; This changes the merchant's sprite to match this
		//TODO: This is depreciated. All uses have been moved to animator controllers.
        public void Face(FacingDirection facingDirection)
        {
            merchant.Face(facingDirection);
        } // end Face

        // Gets the Merchant's Name
        public string Name
        {
            get { return merchant.Name; }
        } // end Name

        // Gets the Merchant's Colour
        public InterfaceColors Color
        {
            get { return merchant.Color; }
        } // end Color

        // Gets the Merchant's number of allies
        public int NumAllies
        {
            get { return merchant.NumAllies; }
        } // end NumAllies

        // Gets and Sets the Merchant's Position
        public Vector3 Position
        {
            get { return merchant.Position; }
            set { merchant.Position = value; }
        }

        #endregion

        #region AI Functions

        // Reset variables for the AI
        void Reset()
        {
            turnPhase = 0;
            isProcessing = false;
        } //end Reset

        // Rolls the die or starts the turn
        void PressAction()
        {
            GPSM.ActionButton();
            turnPhase++;
            isProcessing = false;
        } //end RollDie

        // Sets the target to move towards
        void SetTarget()
        {
            // If a target does not exist, create one
            if (merchant.Target == Vector3.zero)
            {
                // Value of the target resource
                int resourceValue = 0;

                // Get all objects within move distance sphere on layer 9  (resource layer)
                float radius = GPSM.GetDist() * TileManager.PlayerMoveDistance - 0.31f;
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(merchant.Position, radius, 
                    1 << LayerMask.NameToLayer("Resources"));

                // Determine highest value if possible
                for (int index = 0; index < hitColliders.Length; index++)
                {
                    // Get ResourceType of resource hit
                    ResourceType resourceType = hitColliders[index].gameObject.GetComponent<ResourceTile>().Type;

                    // Get worth/value of the resource
                    Resource resource = (Resource)ItemDatabase.Instance.Items.Find(tempItem =>
                        tempItem.Type == resourceType.ToString());

                    // If the value is higher than current target, replace it as the new target
                    if (resourceValue < resource.Worth)
                    {
                        merchant.Target = hitColliders[index].transform.position;
                        resourceValue = resource.Worth;
                    }
                } //end for

                // Verify there is a target, otherwise set it to a village
                if (merchant.Target == Vector3.zero)
                {
                    // Set target depending on the map
                    if (GameMaster.Instance.BattleMap == BattleMap.area01)
                    {
                        merchant.Target = new Vector3(12.48f, -3.52f, -0.01f);
                    } //end if
                    else if (GameMaster.Instance.BattleMap == BattleMap.area02)
                    {
                        merchant.Target = new Vector3(12.48f, -3.52f, -0.01f);
                    } //end else if
                    else if (GameMaster.Instance.BattleMap == BattleMap.area03)
                    {
                        merchant.Target = new Vector3(9.28f, -0.32f, -0.01f);
                    } //end else if
                    else
                    {
                        merchant.Target = new Vector3(12.48f, -3.52f, -0.01f);
                    } //end else
                } //end if
            } //end if
            else
            {
                // Make sure target still exists
                if (TileDictionary.ResourcePositions.Contains(merchant.Target))
                {
                    return;
                } //end if
                // If not, pick a new target
                else
                {
                    // Value of the target resource
                    int resourceValue = 0;

                    // Get all objects within move distance sphere on layer 9  (resource layer)
                    float radius = GPSM.GetDist() * TileManager.PlayerMoveDistance - 0.31f;
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(merchant.Position, radius,
                        1 << LayerMask.NameToLayer("Resources"));

                    // Determine highest value if possible
                    for (int index = 0; index < hitColliders.Length; index++)
                    {
                        // Get ResourceType of resource hit
                        ResourceType resourceType = hitColliders[index].gameObject.GetComponent<ResourceTile>().Type;

                        // Get worth/value of the resource
                        Resource resource = (Resource)ItemDatabase.Instance.Items.Find(tempItem =>
                            tempItem.Type == resourceType.ToString());

                        // If the value is higher than current target, replace it as the new target
                        if (resourceValue < resource.Worth)
                        {
                            merchant.Target = hitColliders[index].transform.position;
                            resourceValue = resource.Worth;
                        };
                    } //end for

                    // Verify there is a target, otherwise set it to a village
                    if (merchant.Target == Vector3.zero)
                    {
                        // Set target depending on the map
                        if (GameMaster.Instance.BattleMap == BattleMap.area01)
                        {
                            merchant.Target = new Vector3(12.48f, -3.52f, -0.01f);
                        } //end if
                        else if (GameMaster.Instance.BattleMap == BattleMap.area02)
                        {
                            merchant.Target = new Vector3(12.48f, -3.52f, -0.01f);
                        } //end else if
                        else if (GameMaster.Instance.BattleMap == BattleMap.area03)
                        {
                            merchant.Target = new Vector3(9.28f, -0.32f, -0.01f);
                        } //end else if
                        else
                        {
                            merchant.Target = new Vector3(12.48f, -3.52f, -0.01f);
                        } //end else
                    } //end if
                } //end else
            } //end else
        } //end SetTarget

        // Moves the AI towards the target
        void Move()
        {
            // Move while there are still moves to make
            if (GPSM.GetDist() > 0)
            {
                float x = merchant.Position.x - merchant.Target.x;
                float y = merchant.Position.y - merchant.Target.y;

                // Move horizontally if not aligned with target
                if (Utility.TruncateFloat(x) != 0)
                {
                    if (x > 0)
                    {
                        GUIM.MoveLeft();
                    } //end if
                    else
                    {
                        GUIM.MoveRight();
                    } //end else
                } //end if
                // Move vertically if not aligned with target 
                else if (Utility.TruncateFloat(y) != 0)
                {
                    if (y > 0)
                    {
                        GUIM.MoveDown();
                    } //end if
                    else
                    {
                        GUIM.MoveUp();
                    } //end else
                } //end else if
                // End turn if aligned with target
                else
                {
                    merchant.Target = Vector3.zero;
                    GPSM.ActionButton();
                    turnPhase++;
                    isProcessing = false;
                }
            } //end if
            // End turn when out of moves
            else
            {
                GPSM.ActionButton();
                turnPhase++;
                isProcessing = false;
            } //end else
        } //end Move

        // Determines what attack AI will use to defeat enemy
        void Battle()
        {
            // Get active battle component
            Battle battle = GameObject.Find("Canvas").GetComponent<Battle>();

            // Determine what attack to use
            if (merchant.Health > 40)
            {
                battle.Head();
            } //end if
            else if ((merchant.Health < 40) && (merchant.Health > 15))
            {
                battle.Torso();
            } //end else if
            else
            {
                battle.Feint();
            } //end else
        } //end Battle

        // Determines whether AI will accept ally or not
        void Ally()
        {
            // If AI has an ally, decline ally
            if (merchant.NumAllies != 0)
            {
                GPSM.No();
            } //end if
            // If AI has no allies, accept ally
            else
            {
                GPSM.Yes();
            } //end else

            turnPhase++;
            isProcessing = false;
        } //end Ally

        // Determines if a better item is in inventory
        void Item()
        {
            // Get AI's inventory and all items in it
            PlayerInventory inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>();
            List<Item> equipment = inventory.GetItems(GameMaster.Instance.Turn).
                FindAll(tempItem => tempItem is Equipment);

            // Loop through item list
            foreach (Item item in equipment)
            {
                if (item is Weapon)
                {
                    // Cast the item to a weapon
                    Weapon weapon = (Weapon)item;

                    // Compare with equipped
                    if (weapon.AttackValue > merchant.AttackPower)
                    {
                        inventory.EquipItem(GameMaster.Instance.Turn, (Equipment)weapon);
                    } //end if weapon.AttackValue > merchant.AttackPower
                } //end if item is Weapon
                else if (item is Armor)
                {
                    // Cast the item to an armor
                    Armor armor = (Armor)item;

                    // Compare with equipped
                    if (armor.DefenceValue > merchant.DefencePower)
                    {
                        inventory.EquipItem(GameMaster.Instance.Turn, (Equipment)armor);
                    } //end if
                } //end else if
            } //end foreach

            turnPhase++;
            isProcessing = false;
        } //end Item

        #endregion
    } // end Player
} // end GSP.Char
