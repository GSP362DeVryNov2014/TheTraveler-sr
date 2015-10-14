/*******************************************************************************
 *
 *  File Name: Inventory.cs
 *
 *  Description: Contains the logic of the new inventory system. This is more
 *               functional that the old system, but it's still pretty minimal.
 *
 *******************************************************************************/
using GSP.Core;
using GSP.Entities.Neutrals;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GSP.Items.Inventories
{
    /*******************************************************************************
     *
     * Name: Inventory
     * 
     * Description: The logic for the new inventory system
     * 
     *******************************************************************************/
    /*
     * Items Dictionary Keys:
     * 0 - Player 1 Items
     * 1 - Player 2 Items
     * 2 - Player 3 Items
     * 3 - Player 4 Items
     * 4 - Market Sell Items
     * 5 - Market Buy Items
     * 6 - Recycle Bin
     * 7 - Player 1's Ally 1 Items
     * 8 - Player 2's Ally 1 Items
     * 9 - Player 3's Ally 1 Items
     * 10 - Player 4's Ally 1 Items
     */
    /*
     * Slots Dictionary Keys:
     * 0 - Player Inventory
     * 1 - Market Inventory
     * 2 - Ally Inventory
     * 3 - Recycle Bin Inventory
     */
    public abstract class Inventory<TSlotType> : MonoBehaviour, IBaseInventory where TSlotType : class, IBaseSlot
    {
        static Dictionary<int, List<Item>> items;          // The dictionary of the lists of items for the each of the inventory system
        static Dictionary<int, List<GameObject>> slots;   // The list of inventory slots for the inventory
        //List<GameObject> slots;             // The list of inventory slots for the inventory

        bool canShowTooltip;        // Whether the tooltip is show
        GameObject tooltip;         // The tooltip's GameObject
        RectTransform tooltipRect;  // The transform of the tooltip

        bool isOpen;    // Whether the window is open

        // Use this for initialisation
        protected virtual void Awake()
        {
            // Initialise the dictionaries only if necessary
            if (items == null && slots == null)
            {
                items = new Dictionary<int, List<Item>>();
                slots = new Dictionary<int, List<GameObject>>();
            } // end if
            else
            {
                // Otherwise clean the dictionaries.
                Clean();
            } // end else
            
            // Get the tooltip GameObject
            tooltip = GameObject.Find("Canvas").transform.Find("Tooltip").gameObject;
            // Disable the tooltip if not already disabled
            if (tooltip.activeInHierarchy)
            {
                tooltip.SetActive(false);
            } // end if

            // Add all the empty lists for the inventories
            for (int key = 0; key < 11; key++)
            {
                // Check if the key is already in the dictionary
                if (items.ContainsKey(key))
                {
                    // The key exists already so set the value instead of adding a new entry
                    items[key] = new List<Item>();
                } // end if
                else
                {
                    // Otherwise add the key
                    items.Add(key, new List<Item>());
                } // end else
            } // end for

            // Add all the empty lists for the slots
            for (int key = 0; key < 5; key++)
            {
                slots.Add(key, new List<GameObject>());
            } // end for
        } // end Awake

        // Use this for initialisation
        protected virtual void Start()
        {
            // Iniialise the tooltip as not shown
            canShowTooltip = false;

            // Get the reference to the tooltips RectTransform
            tooltipRect = tooltip.GetComponent<RectTransform>();

            // Initialise the inventory as closed
            isOpen = false;
        } // end Start

        // Runs each frame; used to update the tooltip's position
        protected virtual void Update()
        {
            // Only proceed if the tooltip exists
            if (tooltip != null)
            {
                // Check if the tooltip is shown
                if (canShowTooltip)
                {
                    tooltipRect.position = new Vector3((Input.mousePosition.x + 15.0f), (Input.mousePosition.y - 10.0f), 0.0f);
                } // end if canShowTooltip
            } // end if
        } // end Update

        // Cleans the static dictionaries
        public void Clean()
        {
            // Only clear the lists in the items dictionary
            for (int index = 0; index < items.Count; index++)
            {
                items[index].Clear();
            } // end for

            // Clear the slots dictionary completely
            slots.Clear();
        } // end Clean

        // Creates an item list for a given key
        protected void CreateItemList(int key, int numItems)
        {
            for (int index = 0; index < numItems; index++)
            {
                items[key].Add(ItemDatabase.Instance.Items.Find(item => item.Type == "Empty"));
            } // end for
        } // end CreateItemList

        // Empties an item list for a given key
        protected void ClearItemList(int key)
        {
            items[key].Clear();
        } // end ClearItemList

        #region IBaseInventory Members

        // Create the slots for the inventory
        public void CreateSlots(int slotKey, int numSlots, SlotType slotType, Transform parent, string slotName)
        {
            // The slot's index
            int slotIndex = slots[slotKey].Count;            
            // Loop to create the inventory slots
            for (int index = 0; index < numSlots; index++)
            {
                // Declare the slot
                GameObject slot = null;

                // Check if it should be a player inventory slot
                if (typeof(TSlotType) == typeof(InventorySlot))
                {
                    // Create the inventory slot
                    slot = Instantiate(PrefabReference.prefabInventorySlot) as GameObject;
                } // end if
                // Check if it should be a market inventory slot
                else if (typeof(TSlotType) == typeof(MarketSlot))
                {
                    // Create the market slot
                    slot = Instantiate(PrefabReference.prefabMarketSlot) as GameObject;
                } // end else if
                // Check if it should be an ally inventory slot
                else if (typeof(TSlotType) == typeof(AllySlot))
                {
                    // Create the ally slot
                    slot = Instantiate(PrefabReference.prefabAllySlot) as GameObject;
                } // end else if
                // Check if it should be a recycle bin inventory slot
                else if (typeof(TSlotType) == typeof(RecycleSlot))
                {
                    // Create the ally slot
                    slot = Instantiate(PrefabReference.prefabRecycleSlot) as GameObject;
                } // end else if

                // Make sure the slot was created
                if (slot == null)
                {
                    // Throw a null reference exception
                    throw new System.NullReferenceException("Incorrect TSlotType parameter given resulting failure to create the slot.");
                } // end if

                // Parent the slot to the given transform
                slot.transform.SetParent(parent);

                // Name the slot in the editor for convienience
                slot.name = slotName + (slotIndex + 1).ToString() + "(" + slotIndex + ")";

                // Set the slot's type
                slot.GetComponent<TSlotType>().SlotType = slotType;

                // Change the slotId
                slot.GetComponent<TSlotType>().SlotId = slotIndex;

                // Add the slot to the list
                slots[slotKey].Add(slot);

                // Increment the slot index
                slotIndex++;
            } // end for
        } // end CreateSlots

        // Add an item to the inventory for the given player
        public bool AddItem(int slotKey, int key, int itemId, SlotType slotType)
        {
            // Get the list of items from the ItemDatabase
            List<Item> database = ItemDatabase.Instance.Items;

            // Only proceed if the ID exists in the database
            if (database.Exists(item => item.Id == itemId))
            {
                int freeSlot;   // The first slot that is free

                // Check if there's space for the item
                if ((freeSlot = FindFreeSlot(slotKey, key, slotType)) >= 0)
                {
                    // Get the item from the database
                    Item tempItem = database[database.FindIndex(item => item.Id == itemId)];

                    // Place it in the free slot
                    items[key][freeSlot] = tempItem;

                    // Update the stats
                    SetStats((Merchant)GameMaster.Instance.GetPlayerScript(GameMaster.Instance.Turn).Entity);

                    // Return success
                    return true;
                } // end if

                // Otherwise, return failure as there isn't enough space
                Debug.LogFormat("No space for item of Id '{0}' in the inventory.", itemId);
                return false;
            } // end if
            else
            {
                // The item didn't exist in the database to return failure
                Debug.LogErrorFormat("The Id '{0}' does not exist in the ItemDatabase!", itemId);
                return false;
            } // end else
        } // end AddItem

        // Add an item to the inventory for a given player from a save file
        public bool AddItemFromSave(int key, int itemId, int slotNum)
        {
            // Get the list of items from the ItemDatabase
            List<Item> database = ItemDatabase.Instance.Items;

            // Only proceed if the ID exists in the database
            if (database.Exists(item => item.Id == itemId))
            {
                // Get the item from the database
                Item tempItem = database[database.FindIndex(item => item.Id == itemId)];

                // Place it in the given slot
                items[key][slotNum] = tempItem;

                // Return success
                return true;
            } // end if
            else
            {
                // The item didn't exist in the database to return failure
                Debug.LogErrorFormat("The Id '{0}' does not exist in the ItemDatabase!", itemId);
                return false;
            } // end else
        } // end AddItemFromSave

        // Removes an item from the inventory
        public void Remove(int key, int slotNum)
        {
            // Remove the item at the given slot
            items[key][slotNum] = ItemDatabase.Instance.Items.Find(item => item.Type == "Empty");

            // Disable the tooltip
            ShowTooltip(null, false);
        } // end Remove

        // Removes an item from the inventory
        public void Remove(int key, Item item)
        {
            // Find the index of the item
            int index = items[key].FindIndex(tempItem => tempItem.Id == item.Id);

            // Remove the item
            Remove(key, index);
        } // end Remove

        // Swaps an item's place in the inventory with another slot
        public void SwapItem(int key, Item a, Item b)
        {
            int aSlot;  // The slot item a resides in
            int bSlot;  // The slot item b resides in

            // Get the item's indices
            aSlot = items[key].FindIndex(aItem => aItem.Id == a.Id);
            bSlot = items[key].FindIndex(bItem => bItem.Id == b.Id);

            // Now swap the items
            items[key][aSlot] = b;
            items[key][bSlot] = a;
        } // end SwapItem

        // Swaps an item's place in the inventory with another slot
        public void SwapItem(int key, int slotNumA, int slotNumB)
        {
            // Get the items in the slots
            Item aItem = items[key][slotNumA];
            Item bItem = items[key][slotNumB];

            // Swap the slots
            items[key][slotNumA] = bItem;
            items[key][slotNumB] = aItem;
        } // end SwapItem

        // Gets the first empty slot of the given SlotType
        public virtual int FindFreeSlot(int slotKey, int key, SlotType slotType)
        {
            int totalFreeSlot;  // The slot free between the inventory and bonus inventory

            // Find the next free slot
            totalFreeSlot = FindAvailableSlot(slotKey, key, slotType);

            // Check if we found a free slot
            if (totalFreeSlot < 0)
            {
                // No free slots available so return negative one
                return -1;
            } // end if

            // Return the first empty slot
            return totalFreeSlot;
        } // end FindFreeSlot

        // Gets the first empty slot of the given SlotType
        int FindAvailableSlot(int slotKey, int key, SlotType slotType)
        {
            // The next free slot of the given type
            int freeSlot = -1;

            // Loop over the items and slots to determine the next free slot
            for (int index = 0; index < slots[slotKey].Count; index++)
            {
                // Get the current slot's script reference
                TSlotType inventorySlot = slots[slotKey][index].GetComponent<TSlotType>();

                // Check if the slot type matches
                if (inventorySlot.SlotType == slotType)
                {
                    // We have a matching slot type so check if the slot is empty
                    if (items[key][index].Name == string.Empty)
                    {
                        // We have a matching free slot so set the slot to the current index
                        freeSlot = index;

                        // Now break out of the loop
                        break;
                    } // end if items[playerNum][index].Name == string.Empty
                } // end if
            } // end for

            // Return the first empty slot, if any
            return freeSlot;
        } // end FindAvailableSlot

        // Gets an item at the given index
        public Item GetItem(int key, int slotNum)
        {
            return items[key][slotNum];
        } // end GetItem

        // Shows the tooltip window for item information
        public void ShowTooltip(Item item, bool canShow = true)
        {
            // Store the canShow bool for updating the tooltip
            canShowTooltip = canShow;

            // Check if we're showing the tooltip
            if (canShow)
            {
                // Enable the tooltip window
                if (!tooltip.activeInHierarchy)
                {
                    tooltip.SetActive(true);
                } // end if

                // Get the Title Text child
                Text tooltipTitleText = tooltip.transform.GetChild(0).GetChild(0).GetComponent<Text>();

                // Get the Body Text Child
                Text tooltipBodyText = tooltip.transform.GetChild(0).GetChild(1).GetComponent<Text>();

                // Set the tooltip's title text
                tooltipTitleText.text = item.Name;

                // Check if the item is a piece of armour
                if (item is Armor)
                {
                    // Set the tooltip's body text
                    tooltipBodyText.text = "Defence: +" + ((Armor)item).DefenceValue + "\nCost: " + ((Armor)item).CostValue;
                } // end if
                // Check if the item is a weapon
                else if (item is Weapon)
                {
                    // Set the tooltip's body text
                    tooltipBodyText.text = "Attack: +" + ((Weapon)item).AttackValue + "\nCost: " + ((Weapon)item).CostValue;
                } // end else if
                // Check if the item is a bonus
                else if (item is Bonus)
                {
                    string text = "";   // Holds the compiled string

                    // Check if the weight is set
                    if (((Bonus)item).WeightValue > 0)
                    {
                        // Append the weight text
                        text += "Weight: +" + ((Bonus)item).WeightValue + "\n";
                    }

                    // Check if the inventory space variable is set
                    if (((Bonus)item).InventoryValue > 0)
                    {
                        // Append the space text
                        text += "Space: +" + ((Bonus)item).InventoryValue + "\n";
                    }

                    // Append the cost text
                    text += "Cost: " + ((Bonus)item).CostValue;

                    // Set the tooltip's body text
                    tooltipBodyText.text = text;
                } // end else if
                // Otherwise, the item must be a resource
                else
                {
                    // Set the tooltip's body text
                    tooltipBodyText.text = "Weight: -" + ((Resource)item).Weight + "\nWorth: " + ((Resource)item).Worth;
                } // end else
            }
            else
            {
                // Otherwise, disable the tooltip window
                if (tooltip.activeInHierarchy)
                {
                    tooltip.SetActive(false);
                } // end if
            } // end else
        } // end ShowTooltip

        // Gets the items list count for the specified key
        public int GetItemsListCount(int key)
        {
            return items[key].Count;
        } // end GetItemsListCount

        #endregion

        // Gets the items from the inventories
        public List<Item> GetItems(int key)
        {
            // Create a temp list based on the items list
            List<Item> tempItems = items[key];

            // Return the temp items list
            return tempItems;
        } // end GetItems

        // Sets the stats in certain inventories
        public abstract void SetStats(Merchant player);

        // Sets the inventory colour for the inventory
        void SetInventoryColor(InterfaceColors interfaceColor)
        {
            // Get the colour for the player's interface colour
            Color color = Utility.InterfaceColorToColor(interfaceColor);

            // Get the Image component of the inventory and set its colour
            GetComponent<Image>().color = color;

            // Get the Image component of the tooltip and set its colour
            if (tooltip.GetComponent<Image>().color != color)
            {
                tooltip.GetComponent<Image>().color = color;
            } // end if
        } // end SetInventoryColor

        public virtual void SetPlayer(int playerNum)
        {
            // Set the inventory's colour
            SetInventoryColor(GameMaster.Instance.GetPlayerColor(playerNum));
        } // end SetPlayer

        // Gets and Sets whether the inventory window is open
        public bool IsOpen
        {
            get { return isOpen; }
            set { isOpen = value; }
        } // end IsOpen
    } // end Inventory
} // end GSP.Items.Inventories
