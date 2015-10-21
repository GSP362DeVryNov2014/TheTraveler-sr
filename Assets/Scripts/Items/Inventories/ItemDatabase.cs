/*******************************************************************************
 *
 *  File Name: ItemDatabase.cs
 *
 *  Description: Contains all the items available in the game.
 *
 *******************************************************************************/
using GSP.Core;
using System.Collections.Generic;

namespace GSP.Items.Inventories
{
    /*******************************************************************************
     *
     * Name: ItemDatabase
     * 
     * Description: A singleton storage class. It holds a list of all the available
     *              items in the game. Used in conjunction with the Inventory
     *              system.
     * 
     *******************************************************************************/
    public class ItemDatabase : BaseSingleton<ItemDatabase>
    {
        List<Item> items;   // The list of items

        // Initialise the list and other values
        public override void Awake()
        {
            // Call Awake on the parent first
            base.Awake();

            // Set the object's name
            gameObject.name = "ItemDatabase";

            // Initialise our list
            items = new List<Item>();
        } // end Awake

        // Use this for initialisation
        void Start()
        {
            // Add the items to the list
            items.Add(new EmptyItem());
            items.Add(new Weapon("Broadsword", WeaponType.Broadsword, SpriteReference.spriteBroadsword, 12, 60));
            items.Add(new Weapon("Mace", WeaponType.Mace, SpriteReference.spriteMace, 8, 24));
            items.Add(new Weapon("Spear", WeaponType.Spear, SpriteReference.spriteSpear, 10, 30));
            items.Add(new Weapon("Sword", WeaponType.Sword, SpriteReference.spriteSword, 6, 18));
            items.Add(new Armor("Chainlegs", ArmorType.Chainlegs, SpriteReference.spriteChainlegs, 7, 21));
            items.Add(new Armor("Chainmail", ArmorType.Chainmail, SpriteReference.spriteChainmail, 9, 27));
            items.Add(new Armor("Plate Fullsuit", ArmorType.PlateFullsuit, SpriteReference.spritePlateFullsuit, 22, 110));
            items.Add(new Armor("Platebody", ArmorType.Platebody, SpriteReference.spritePlatebody, 12, 36));
            items.Add(new Armor("Platelegs", ArmorType.Platelegs, SpriteReference.spritePlatelegs, 10, 30));
            items.Add(new Bonus("Rubber Boots", BonusType.RubberBoots, SpriteReference.spriteRubberBoots, 0, 10, 10));
            items.Add(new Bonus("Sachel", BonusType.Sachel, SpriteReference.spriteSachel, 3, 0, 10));
            items.Add(new Resource("Fish", ResourceType.Fish, SpriteReference.spriteFishResource, 20, 5, 65));
            items.Add(new Resource("Ore", ResourceType.Ore, SpriteReference.spriteOreResource, 30, 5, 90));
            items.Add(new Resource("Wood", ResourceType.Wood, SpriteReference.spriteWoodResource, 15, 5, 40));
            items.Add(new Resource("Wool", ResourceType.Wool, SpriteReference.spriteWoolResource, 10, 5, 25));
            items.Add(new Armor("Robe", ArmorType.Robe, SpriteReference.spriteRobe, 3, 9));
            items.Add(new Armor("Skirt", ArmorType.Skirt, SpriteReference.spriteSkirt, 2, 6));
            items.Add(new Armor("Cloth Fullsuit", ArmorType.ClothFullsuit, SpriteReference.spriteClothFullsuit, 5, 25));
            items.Add(new Armor("Tunic", ArmorType.Tunic, SpriteReference.spriteTunic, 6, 18));
            items.Add(new Armor("Leggings", ArmorType.Leggings, SpriteReference.spriteLeggings, 4, 12));
            items.Add(new Armor("Leather Fullsuit", ArmorType.LeatherFullsuit, SpriteReference.spriteLeatherFullsuit, 10, 50));
            items.Add(new Armor("Chain Fullsuit", ArmorType.ChainFullSuit, SpriteReference.spriteChainFullsuit, 16, 80));
            items.Add(new Weapon("Battleaxe", WeaponType.Battleaxe, SpriteReference.spriteBattleaxe, 15, 75));
        } // end Start

        public List<Item> Items
        {
            get
            { 
                // Create a copy of the list
                List<Item> tempItems = new List<Item>(items);

                // Return the copy
                return tempItems;
            } // end get
        } // end Items
    } // end ItemDatabase
} // end GSP.Items.Inventories
