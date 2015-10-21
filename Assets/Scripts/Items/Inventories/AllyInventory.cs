using GSP.Char.Allies;
/*******************************************************************************
 *
 *  File Name: AllyInventory.cs
 *
 *  Description: Contains the logic of the new inventory system for allies.
 *               This is like the player's inventory system.
 *
 *******************************************************************************/
using GSP.Core;
using GSP.Entities.Friendlies;
using GSP.Entities.Neutrals;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSP.Items.Inventories
{
    /*******************************************************************************
     *
     * Name: AllyInventory
     * 
     * Description: The logic for the new inventory system for allies.
     * 
     *******************************************************************************/
    public class AllyInventory : Inventory<AllySlot>
    {
        int numInventorySlotsCreate;    // The number of inventory slots to create

        Transform bottomGrid;       // The Inentory's Bottom Panel
        Transform statusLeft;       // The Inventory's Top/StatusLeft Panel
        Transform statusRight;       // The Inventory's Top/StatusRight Panel

        // Use this for initialisation
        protected override void Awake()
        {
            // Call the parent's Awake() first
            base.Awake();

            // Set the ally inventory's transform references
            SetReferences();

            // Initialise the number of slots to create
            numInventorySlotsCreate = 28;
        } // end Awake

        // Use this for initialisation
        protected override void Start()
        {
            // Call the parent's Start() first
            base.Start();

            // Create the inventory slots
            CreateSlots(2, numInventorySlotsCreate, SlotType.Ally, bottomGrid, "AllySlot ");
        } // end Start

        // Runs each frame; used to update the tooltip's position
        protected override void Update()
        {
            // Call the parent's Update() first
            base.Update();
        } // end Update

        // Sets the references used for the ally's inventory such as the transforms
        void SetReferences()
        {
            // Get Inventory's Bottom panel
            bottomGrid = GameObject.Find("AllyInventory/Bottom").transform;

            // Get the Inventory's Top/StatusLeft panel
            statusLeft = GameObject.Find("AllyInventory/Top/StatusLeft").transform;

            // Get the Inventory's Top/StatusRight panel
            statusRight = GameObject.Find("AllyInventory/Top/StatusRight").transform;
        } // end SetReferences

        // Creates the list of items for the ally
        public void CreateAllyItemList(int allyNum)
        {
            // Check if the list needs to be created
            if (GetItems(allyNum).Count == 0)
            {
                // Create the player's item list
                CreateItemList(allyNum, numInventorySlotsCreate);
            } // end if
        } // end CreateAllyItemList

        // Gets the first empty slot of the given SlotType
        public override int FindFreeSlot(int slotKey, int key, SlotType slotType)
        {
            // Find the next free slot using the parent's calculations
            int freeSlot = base.FindFreeSlot(slotKey, key, slotType);

            // Check if we found a free slot
            if (freeSlot < 0)
            {
                // No free slots available so return negative one
                return -1;
            } // end if

            // Return the first empty slot while clamping the slot to inventory range
            return Utility.ClampInt(freeSlot, 0, (numInventorySlotsCreate - 1));
        } // end FindFreeSlot

        // Sets the player's stats for the status panels
        public override void SetStats(Merchant player)
        {
            if (statusLeft == null && statusRight == null)
            {
                // Set the references for the transforms
                SetReferences();
            } // end if
            
            // Hard coding it for now to accept the first ally of the player
            // that happens to be porter
            Porter ally = (Porter)player.GetAlly(0).GetComponent<PorterMB>().Entity;

            if (statusLeft != null)
            {
                // Set the ally's name
                statusLeft.GetChild(0).GetChild(1).GetComponent<Text>().text = ally.Name;

                // Set the ally's type
                statusLeft.GetChild(1).GetChild(1).GetComponent<Text>().text = ally.FriendlyType.ToString();
            } // end if

            if (statusRight != null)
            {
                // Set the Weight status line's value
                statusRight.GetChild(0).GetChild(1).GetComponent<Text>().text = ally.TotalWeight.ToString() + "/" + ally.MaxWeight.ToString();

                // Set the Profit status line's value
                statusRight.GetChild(1).GetChild(1).GetComponent<Text>().text = ally.TotalWorth.ToString();
            } // end if
        } // end SetStats

        // Sets the inventory up for the current player
        public override void SetPlayer(int playerNum)
        {
            // Call the parent's SetPlayer() first
            base.SetPlayer(playerNum);

            // Set the player's stats
            SetStats((Merchant)GameMaster.Instance.GetPlayerScript(playerNum).Entity);
        } // end SetPlayer

        // Gets the max space for the inventory
        public int MaxSpace
        {
            get { return numInventorySlotsCreate; }
        } // end MaxSpace
    } // end AllyInventory
} // end GSP.Items.Inventories
