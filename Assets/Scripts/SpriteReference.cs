/*******************************************************************************
 *
 *  File Name: SpriteReference.cs
 *
 *  Description: Loads and deals with Sprites at runtime
 *
 *******************************************************************************/
using UnityEngine;

namespace GSP
{
    /*******************************************************************************
     *
     * Name: SpriteReference
     * 
     * Description: Manages the references to any srpites loaded from Resources.
     * 
     *******************************************************************************/
    public static class SpriteReference
	{
        /*
         * Inventory Sprites
         */
        // This is the sprite sheet for the armour items
        static Sprite[] armorSpriteSheet = Resources.LoadAll<Sprite>("armour_spritesheet");
        
        //  This is the sprite sheet for the other items
        static Sprite[] itemsSpriteSheet = Resources.LoadAll<Sprite>("Items_spritesheet");

        // This is the sprite sheet for the resource items
        static Sprite[] resourcesSpriteSheet = Resources.LoadAll<Sprite>("resource_spritesheet");

        // This is the reference to the broadsword sprite
        public static Sprite spriteBroadsword = itemsSpriteSheet[51];

        // This is the reference to the mace sprite
        public static Sprite spriteMace = itemsSpriteSheet[27];

        // This is the reference to the spear sprite
        public static Sprite spriteSpear = itemsSpriteSheet[16];

        // This is the reference to the sword sprite
        public static Sprite spriteSword = itemsSpriteSheet[37];

        // This is the reference to the chainlegs sprite
        public static Sprite spriteChainlegs = armorSpriteSheet[17];

        // This is the reference to the chainmail sprite
        public static Sprite spriteChainmail = armorSpriteSheet[15];

        // This is the reference to the plate fullsuit sprite
        public static Sprite spritePlateFullsuit = armorSpriteSheet[19];

        // This is the reference to the platebody sprite
        public static Sprite spritePlatebody = armorSpriteSheet[20];

        // This is the reference to the platelegs sprite
        public static Sprite spritePlatelegs = armorSpriteSheet[22];

        // This is the reference to the rubber boots sprite
        public static Sprite spriteRubberBoots = itemsSpriteSheet[60];

        // This is the reference to the sachel sprite
        public static Sprite spriteSachel = itemsSpriteSheet[49];

        // This is the reference to the robe sprite
        public static Sprite spriteRobe = armorSpriteSheet[5];

        // This is the reference to the skirt sprite
        public static Sprite spriteSkirt = armorSpriteSheet[7];
        
        // This is the reference to the cloth fullsuit sprite
        public static Sprite spriteClothFullsuit = armorSpriteSheet[4];

        // This is the reference to the tunic sprite
        public static Sprite spriteTunic = armorSpriteSheet[10];

        // This is the reference to the leggings sprite
        public static Sprite spriteLeggings = armorSpriteSheet[12];

        // This is the reference to the leather fullsuit sprite
        public static Sprite spriteLeatherFullsuit = armorSpriteSheet[9];

        // This is the reference to the chain fullsuit sprite
        public static Sprite spriteChainFullsuit = armorSpriteSheet[14];

        // This is the reference to the sachel sprite
        public static Sprite spriteBattleaxe = itemsSpriteSheet[26];
        
        // This is the reference to the fish resource sprite
        public static Sprite spriteFishResource = resourcesSpriteSheet[7];

        // This is the reference to the ore resource sprite
        public static Sprite spriteOreResource = resourcesSpriteSheet[4];

        // This is the reference to the wood resource sprite
        public static Sprite spriteWoodResource = resourcesSpriteSheet[5];

        // This is the reference to the wool resource sprite
        public static Sprite spriteWoolResource = resourcesSpriteSheet[6];
	} // end SpriteReference
} // end GSP

