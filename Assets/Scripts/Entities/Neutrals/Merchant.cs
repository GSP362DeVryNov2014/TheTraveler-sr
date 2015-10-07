/*******************************************************************************
 *
 *  File Name: Merchant.cs
 *
 *  Description: This is entity the player will be playing as
 *
 *******************************************************************************/
using GSP.Char;
using GSP.Core;
using GSP.Entities.Interfaces;
using GSP.Items;
using GSP.Items.Inventories;
using GSP.Tiles;
using System.Collections.Generic;
using UnityEngine;

namespace GSP.Entities.Neutrals
{
    /*******************************************************************************
     *
     * Name: Merchant
     * 
     * Description: The Merchant class is the playable class.
     * 
     *******************************************************************************/
    public class Merchant : Entity, IInventory, IEquipment, IDamageable
    {
        #region IInventory Variables

        int maxWeight;		                // The maximum weight the entity can hold
        int currency; 		                // The amount of currency the entity is holding
        List<Resource> resources;           // The list of resources
        PlayerInventory inventory;          // The inventory of the player
        ResourceUtility resourceUtility;    // The resource utility functions

        #endregion

        #region IEquipment Variables

        int defencePower;       // The defence of the entity (from armor)
        int attackPower;	    // The attack of the entity (from weapons)

        List<Bonus> bonuses;    // The bonuses picked up (Inventory and Weight mods)
        Armor equippedArmor;    // The piece of armor that is being worn.
        Weapon equippedWeapon;  // The weapon that is being wielded.

        #endregion

        #region IDamageable Variables

        int health;     // The current health the entity has
        int maxHealth;  // THe maximum health the entity has
        bool isDead;    // Whether the entity is dead

        #endregion

        InterfaceColors color;          // The colour of the Merchant
        List<Sprite> charSprites;		// The Sprite's for the Character
        SpriteRenderer spriteRenderer;  // SpriteRenderer component of the Character

        AllyList allyScript;    // The ally script object

        Vector3 target;   // The target the merchant is going after

        int playerNum;  // The merchant's player number

        // Creates a Merchant entity
        public Merchant(int ID, GameObject gameObject, InterfaceColors playerCoulours, string playerName) :
            base(ID, gameObject)
        {
            // Set the entity's type to Merchant
            Type = EntityType.Merchant;

            // Set Merchant's name
            Name = playerName;

            // Set the Merchant's colour
            color = playerCoulours;

            // Initialise the target to null
            target = Vector3.zero;

            // Initialise the player number
            playerNum = -1;

            // Create a new list of Sprite's
            charSprites = new List<Sprite>();
            // Get the GameObject's SpriteRenderer
            spriteRenderer = GameObj.GetComponent<SpriteRenderer>();

            // Get the GameObject's ally script
            allyScript = GameObj.GetComponent<AllyList>();

            #region IInventory Variable Initialisation

            // The default for now - hard coded values
            maxWeight = 300;
            
            // The entity starts with no currency
            currency = 0;

            // Get the ResourceList component reference
            resources = new List<Resource>();

            // Get the inventory script
            inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>();

            // Create a new ResourceUltity object
            resourceUtility = new ResourceUtility();

            #endregion

            #region IEquipment Variable Initialisation

            // The entity isn't wearing any armour, wielding any weapon, or has any bonuses
            equippedArmor = null;
            equippedWeapon = null;
            bonuses = new List<Bonus>();
            attackPower = 0;
            defencePower = 0;

            #endregion

            #region IDamageable Variable Initialisation

            // The default hard coded values for now.
            health = 50;
            maxHealth = 50;
            isDead = false;

            #endregion

        }

        // Updates the script refereneces that depended upon the GameObject
        public void UpdateScriptReferences()
        {
            // Update the sprite renderer
            spriteRenderer = GameObj.GetComponent<SpriteRenderer>();

            // Update the ally script
            allyScript = GameObj.GetComponent<AllyList>();

			// Update the inventory reference
            inventory = GameObject.Find("Canvas").transform.Find("PlayerInventory").GetComponent<PlayerInventory>();
        } // end UpdateScriptReferences

        // Setup the Merchant's Sprite set. This is an array of Sprites that will be used for the Merchant
        public void SetCharacterSprites(int playerNumber)
        {
            // A temporary Sprite array; Make sure the playerNumber is within the proper range of one to MaxPlayers
            Sprite[] tmp = UnityEngine.Resources.LoadAll<Sprite>("player" + Utility.ClampInt(playerNumber, 1, 8));

            // Add the idle sprites for each direction
            charSprites.Add(tmp[1]);
            charSprites.Add(tmp[4]);
            charSprites.Add(tmp[7]);
            charSprites.Add(tmp[10]);
        } // end SetCharacterSprites

        // Sets the Merchant's Sprite to the given index
        void SetSprite(int index)
        {
            spriteRenderer.sprite = charSprites[index];
        } // end SetSprite

        // Gets the Merchant's Sprite for the given index
        // Note: The direction is North, East, South, West
        public Sprite GetSprite(int index)
        {
            // Ensure a valid index was given; clamp it
            return charSprites[Utility.ClampInt(index, 0, (charSprites.Count - 1))];
        } // end GetSprite

        // Faces the Merchant in a given direction; This changes the Merchant's Sprite to match this
        public void Face(FacingDirection facingDirection)
        {
            // Get the box collider of the character
            var boxCollider = GameObj.GetComponent<BoxCollider2D>();

            // Set the BoxCollider2D component size smaller to fix for the scaling fix
            boxCollider.size = new Vector2(0.18f, 0.2f);

            // Switch over the selection
            switch (facingDirection)
            {
                case FacingDirection.North:
                    {
                        // Change the Character's Sprite to face the north direction
                        SetSprite(3);

                        // Using the GameObject's transform, scale to fix the small Sprite issue
                        //GameObj.transform.localScale = new Vector3(1.66f, 1.56f, 1.0f);
                        break;
                    } // end case North
                case FacingDirection.East:
                    {
                        // Change the Character's Sprite to face the east direction
                        SetSprite(2);

                        // Using the GameObject's transform, scale to fix the small Sprite issue
                        //GameObj.transform.localScale = new Vector3(1.41f, 1.56f, 1.0f);
                        break;
                    } // end case East
                case FacingDirection.South:
                    {
                        // Change the Character's Sprite to face the south direction
                        SetSprite(0);

                        // Using the GameObject's transform, scale to fix the small Sprite issue
                        //GameObj.transform.localScale = new Vector3(1.66f, 1.56f, 1.0f);
                        break;
                    } // end case South
                case FacingDirection.West:
                    {
                        // Change the Character's Sprite to face the west direction
                        SetSprite(1);

                        // Using the GameObject's transform, scale to fix the small Sprite issue
                        //GameObj.transform.localScale = new Vector3(1.40f, 1.56f, 1.0f);
                        break;
                    } // end case West
            } // end switch facingDirection
        } // end Face

        // Gets an ally GameObject
        public GameObject GetAlly(int allyNumber)
        {
            return allyScript.GetObject(allyNumber);
        } // end GetAlly

        // Gets the Merchant's colour
        public InterfaceColors Color
        {
            get { return color; }
        } // end Color

        // Gets the number of allies the Merchant has
        public int NumAllies
        {
            get { return allyScript.NumAllies; }
        } // end NumAllies

        // Gets and Sets the merchant's target
        public Vector3 Target
        {
            get { return target; }
            set { target = value; }
        } // end Target

        // Gets and Sets the Merchants player number
        public int PlayerNumber
        {
            get { return playerNum; }
            set { playerNum = value; }
        } // end PlayerNumber

        #region IInventory Members

        // Picks up a resource for an entity adding it to their ResourceList
        public bool PickupResource(Resource resource, int amount, bool isFromMap = true)
        {
            // Check if picking up this resource will put the entity overweight
            if ((TotalWeight + resource.Weight) * amount <= MaxWeight)
            {
                // Attempt to add the resource to the inventory
                if (inventory.AddItem(0, PlayerNumber, resource.Id, SlotType.Inventory))
                {
                    // Update the inventory's stats
                    inventory.SetStats(this);

                    // Check if the resource is from the map
                    if (isFromMap)
                    {
                        // Get the resource's position
                        Vector3 tmp = GameObj.transform.localPosition;
                        // Change the z to make tiles work
                        tmp.z = -0.01f;
                        // Remove the resource from the map
                        TileDictionary.RemoveResource(TileManager.ToPixels(tmp));
                    } // end if

                    // Return success
                    return true;
                } // end if
                else
                {
                    Debug.Log("Pickup failed. Max inventory capacity reached.");

                    // Return failure
                    return false;
                } // end else
            } // end if
            else
            {
                Debug.Log("Pickup failed. Max inventory weight reached.");

                // Return failure
                return false;
            } // end else
        } // end PickupResource

        // Sells a resource for an entity removing it from their ResourceList
        public void SellResource(Resource resource, int amount)
        {
            // A temporary list to hold the resources
            List<Resource> tmpResources = new List<Resource>();

            // The counter for the for loop below
            int count = 0;

            // Get all the resources of the given resource's type
            tmpResources = ResourceUtility.GetResourcesByType(resource.ResourceType, PlayerNumber);

            // Check if the returned number of resources is fewer than amount
            if (tmpResources.Count < amount)
            {
                // Set the counter to the number of resources found
                count = tmpResources.Count;
            } // end if
            else
            {
                // Set the counter to amount
                count = amount;
            } // end else

            // Loop over the list until we reach count
            for (int index = 0; index < count; index++)
            {
                // Credit the entity for the resource
                currency += tmpResources[index].Worth;

                // Remove the resource from the inventory
                ResourceUtility.RemoveResource(tmpResources[index], PlayerNumber);
            } // end for
        } // end SellResource

        // Sells all resources for an entity clearing their ResourceList
        public void SellResources()
        {
            // Credit the entity for the resources they are holding
            currency += TotalWorth;

            // Remove all the resources now
            ResourceUtility.RemoveResources(PlayerNumber);
        } // end SellResources

        // Transfers currency from the entity to another entity
        public void TransferCurrency<TInventoryEntity>(TInventoryEntity other, int amount) where TInventoryEntity : IInventory
        {
            // The clamped amount between zero and the entity's currency amount
            int transferAmount = Utility.ClampInt(amount, 0, currency);

            // Add the amount of currency to the other Character
            other.Currency += transferAmount;

            // Subtract the amount of currency from the Character this is attached to
            currency -= transferAmount;
        } // end TransferCurrency

        // Transfers a resource from the entity to another entity
        public bool TransferResource<TInventoryEntity>(TInventoryEntity other, Resource resource) where TInventoryEntity : IInventory
        {
            // Check if the resource object exists
            if (resource == null)
            {
                // The resource object is invalid so return failure
                return false;
            } // end if

            // Have the other entity pickup the resource and test if it's a success
            if (other.PickupResource(resource, 1, false))
            {
                // The pickup succeeded so remove the resource from the entity's inventory
                ResourceUtility.RemoveResource(resource, PlayerNumber);
                
                // Return success
                return true;
            } // end if
            else
            {
                // The pickup failed for the other entity so return failure
                Debug.Log("Transfer failed.");
                return false;
            }
        } // end TransferResource

        // Gets the ResourceUtility object
        public ResourceUtility ResourceUtility
        {
            get { return resourceUtility; }
        } // end ResourceUtility
        
        // Gets the list of resources of the entity
        public List<Resource> Resources
        {
            get
            {
                // Get the list of resources in the player's inventory
                resources = ResourceUtility.GetResources(PlayerNumber);

                // Create a temporary list based on the list of resources
                List<Resource> tempList = resources;
                
                // Return the temporary list
                return tempList;
            } // end get
        } // end Resources

        // Gets the TotalWeight of the entity's resources
        public int TotalWeight
        {
            get
            {
                // The total weight of all the resources in the player's inventory
                int totalWeight = 0;

                // Get all the resources
                List<Resource> allResources = Resources;

				// Make sure there are resources
				if(allResources.Count > 0)
				{
               		 // Get the total weight
              		  foreach (Resource resource in allResources)
               		 {
                  		  totalWeight += resource.Weight;
               		 } // end foreach
				} // end if
                // Return the total weight
                return totalWeight;
            } // end get
        } // end TotalWeight

        // Gets the TotalValue of the entity's resources
        public int TotalWorth
        {
            get
            {
                // The total weight of all the resources in the player's inventory
                int totalWorth = 0;

                // Get all the resources
                List<Resource> allResources = Resources;

                // Make sure there are resources
                if (allResources.Count > 0)
                {
                    // Get the total worth
                    foreach (Resource resource in allResources)
                    {
                        totalWorth += resource.Worth;
                    } // end foreach
                } // end if

                // Return the total worth
                return totalWorth;
            } // end get
        } // end TotalWorth

        // Gets and Sets the MaxWeight of the entity
        public int MaxWeight
        {
            get { return maxWeight; }
            set { maxWeight = Utility.ZeroClampInt(value); }
        } // end MaxWeight

        // Gets and Sets the MaxInventorySpace of the entity
        // This is the number of regular inventory slots
        public int MaxInventorySpace
        {
            get { return inventory.WeaponSlot; }
        } // end MaxInventorySpace

        // Gets and Sets the Currency of the entity
        public int Currency
        {
            get { return currency; }
            set { currency = Utility.ZeroClampInt(value); }
        } // end Currency

        #endregion

        #region IEquipment Members

        // Equips a piece of armour for an entity
        public void EquipArmor(Armor armor)
        {
            // Check if the merchant is wearing armour
            if (equippedArmor == null)
            {
                // The merchant isn't wearing armor so just equip the given armour
                defencePower += armor.DefenceValue;
                equippedArmor = armor;
            } // end if
            else
            {
                // The merchant is already wearing armour so unequip it first
                UnequipArmor(equippedArmor);

                // Now equip the given armour
                defencePower += armor.DefenceValue;
                equippedArmor = armor;
            } // end else
        } // end EquipArmor

        // Unequips a piece of armour for an entity
        public void UnequipArmor(Armor armor)
        {
            // Only unequip the armour if the merchant is wearing any
            if (equippedArmor != null)
            {
                // Unequip the given armour
                defencePower -= armor.DefenceValue;
                equippedArmor = null;
            } // end if
        } // end UnequipArmor

        // Equips a bonus item
        public void EquipBonus(Bonus bonus)
        {
            // Add the bonus to the list
            bonuses.Add(bonus);

            // Add the stats of the bonus
            maxWeight += bonus.WeightValue;
            //TODO: Damien: Change this later when doing the bonuses
            //maxInventory += bonus.InventoryValue;
        } // end EquipBonus

        // Unequips a bonus item
        public void UnequipBonus(Bonus bonus)
        {
            // Remove the stats of the bonus
            maxWeight -= bonus.WeightValue;
            //TODO: Damien: Change this later when doing the bonuses
            //maxInventory -= bonus.InventoryValue;

            // Remove the bonus from the list
            bonuses.Remove(bonus);
        } // end UnequipBonus

        // Equips a weapon for an entity
        public void EquipWeapon(Weapon weapon)
        {
            // Check if the merchant is wielding a weapon
            if (equippedArmor == null)
            {
                // The merchant isn't wielding a weapon so just equip the given weapon
                attackPower += weapon.AttackValue;
                equippedWeapon = weapon;
            } // end if
            else
            {
                // The merchant is already wielding a weapon so unequip it first
                UnequipWeapon(equippedWeapon);

                // Now wield the given weapon
                attackPower += weapon.AttackValue;
                equippedWeapon = weapon;
            } // end else
        } // end EquipWeapon

        // Unequips a weapon for the entity
        public void UnequipWeapon(Weapon weapon)
        {
            // Only unequip the weapon if the merchant is wielding any
            if (equippedWeapon != null)
            {
                // Unequip the given weapon
                attackPower -= weapon.AttackValue;
                equippedWeapon = null;
            } // end if
        } // end UnequipWeapon

        // Gets and Sets the amount of defense the entity has
        public int DefencePower
        {
            get { return defencePower;}
            set { defencePower = Utility.ZeroClampInt(value);}
        } // end DefencePower

        // Gets the EquippedArmor of the entity
        public Armor EquippedArmor
        {
            get { return equippedArmor;}
        } // end EquippedArmor

        // Gets the bonuses the entity has
        public List<Bonus> Bonuses
        {
            get
            { 
                // Get a temp list
                var tmp = new List<Bonus>(bonuses);
                
                // Return the temp list
                return tmp; 
            } // end get
        } // end Bonuses

        // Gets and Sets the how hard the the entity hits
        public int AttackPower
        {
            get { return attackPower; }
            set { attackPower = Utility.ZeroClampInt(value); }
        } // end AttackPower

        // Gets the EquippedWeapon of the entity
        public Weapon EquippedWeapon
        {
            get { return equippedWeapon; }
        } // end EquippedWeapon

        #endregion

        #region IDamageable Members

        // Causes the entity to take damage; this is call by others
        public void TakeDamage(int damage)
        {
            // Only allow damage if the entity isn't dead
            if (!IsDead)
            {
                // Dish out the damage
                health -= damage;

                // Check if the entity is dead
                if (health <= 0)
                {
                    // The entity is dead
                    isDead = true;
                } // end if health <= 0
            } // end if
        } // end TakeDamage

        // Resets the health of the entity
        public void ResetHealth()
        {
            health = maxHealth;
            isDead = false;
        } // end ResetHealth

        // Gets the current health of the entity
        public int Health
        {
            get { return Utility.ZeroClampInt(health); }
        } // end Health

        // Gets and Sets the maximum health of the entity
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = Utility.ZeroClampInt(value); }
        } // end MaxHealth

        // Gets whether the eneity is dead
        public bool IsDead
        {
            get { return isDead; }
        } // end IsDead

        #endregion
    } // end Merchant
} // end GSP.Entities.Neutrals
