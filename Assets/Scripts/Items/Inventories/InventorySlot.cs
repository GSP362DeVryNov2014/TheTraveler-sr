/*******************************************************************************
 *
 *  File Name: InventorySlot.cs
 *
 *  Description: Contains the logic of a slot. The Inventory system is filled
 *               with slots. Their functionality is fairly minimal.
 *
 *******************************************************************************/
using GSP.Char.Allies;
using GSP.Core;
using GSP.Entities.Friendlies;
using GSP.Entities.Neutrals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GSP.Items.Inventories
{
    /*******************************************************************************
     *
     * Name: InventorySlot
     * 
     * Description: The functionality of each slot in the inventory. This is done
     *              through Unity's EventSystems interfaces.
     * 
     *******************************************************************************/
    public class InventorySlot : Slot<PlayerInventory, Market, AllyInventory, RecycleBin>, 
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region IPointerEnterHandler Members

        // Event triggers when the mouse enters the slot's space
        public void OnPointerEnter(PointerEventData pointerEventData)
        {
            // Check if there is an item in the slot
            if (mainInventory.GetItem(PlayerNumber, SlotId).Name != string.Empty)
            {
                // Show the tooltip window while hovering over an item
                mainInventory.ShowTooltip(mainInventory.GetItem(PlayerNumber, SlotId));
            } // end if
        } // end OnPointerEnter

        #endregion

        #region IPointerExitHandler Members

        // Event triggers when the mouse leaves the slot's space
        public void OnPointerExit(PointerEventData pointerEventData)
        {
            // Check if there is an item in the slot
            if (mainInventory.GetItem(PlayerNumber, SlotId).Name != string.Empty)
            {
                // Show the tooltip window while not hovering over an item
                mainInventory.ShowTooltip(null, false);
            } // end if
        } // end OnPointerEnter

        #endregion

        #region IPointerDownHandler Members

        // Event triggers when a mouse button is pressed in the slot's space
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            // Leave empty for now; used to detect mouse so it can detect mouse up
        } // end OnPointerDown

        #endregion

        #region IPointerUpHandler Members

        // Event triggers when a mouse button is released in the slot's space
        public void OnPointerUp(PointerEventData pointerEventData)
        {
            // Check if the button was the right mouse
            if (pointerEventData.pointerId == -2)
            {
                // Check if there is an item in the slot
                if (mainInventory.GetItem(PlayerNumber, SlotId).Name != string.Empty)
                {
                    // Get the item that was right clicked
                    Item item = mainInventory.GetItem(PlayerNumber, SlotId);

                    // Check if the market exists and is in buy mode
                    if (subInventoryOne != null && subInventoryOne is Market && subInventoryOne.IsOpen)
                    {
                        if (((Market)(object)subInventoryOne).Action == MarketAction.Buy)
                        {
                            // Handle selling to the market
                            SellToMarket(item);
                        } // end if
                        else
                        {
                            // Otherwise, handle any equipment
                            HandleEquipment(item);
                        } // end else
                    } // end if
                    // Check if the ally inventory exists
                    else if (subInventoryTwo != null && subInventoryTwo is AllyInventory && subInventoryTwo.IsOpen)
                    {
                        // Check if the item is a resource
                        if (item is Resource)
                        {
                            // Handle the transferring to the ally's inventory
                            TradeToAlly(item);
                        } // end if
                        else
                        {
                            // Otherwise, handle any equipment
                            HandleEquipment(item);
                        } // end else
                    } // end else if
                    // Check if the recycle inventory exists
                    else if (subInventoryThree != null && subInventoryThree is RecycleBin && subInventoryThree.IsOpen)
                    {
                        // Handle the adding of items to the recycle bin
                        HandleRecycle(item);
                    } // end else if
                    else
                    {
                        Debug.Log("handle equipment");
                        // Otherwise, handle any equipment
                        HandleEquipment(item);
                    } // end else
                } // end if inventory.GetItem(PlayerNumber, SlotId).Name != string.Empty
            } // end if
        } // end OnPointerUp

        void HandleEquipment(Item item)
        {
            // Check if the item is a piece of equipment
            if (item is Equipment)
            {
                // Make sure we're not right clicking the equipped equipment
                if (SlotId < mainInventory.WeaponSlot)
                {
                    // The item is a piece of equipment so equip it
                    mainInventory.EquipItem(PlayerNumber, (Equipment)item);
                } // end if SlotId < inventory.WeaponSlot
                else
                {
                    // Check if the item is a bonus item and that we're right clicking it from the bonus slot range.
                    if (item is Bonus && (SlotId >= mainInventory.BonusSlotBegin && SlotId < mainInventory.BonusSlotEnd))
                    {
                        int freeSlot;   // The first slot that is free

                        // Check if there's space for the item
                        if ((freeSlot = mainInventory.FindFreeSlot(0, PlayerNumber, SlotType.Inventory)) >= 0)
                        {
                            // Swap the bonus item with the item at the free slot
                            mainInventory.SwapItem(PlayerNumber, item, mainInventory.GetItem(PlayerNumber, freeSlot));

                            // Unequip the bonus item
                            Merchant playerMerchant = (Merchant)GameMaster.Instance.GetPlayerScript(PlayerNumber).Entity;
                            playerMerchant.UnequipBonus((Bonus)item);

                            // Disable the tooltip
                            mainInventory.ShowTooltip(null, false);

                            // Update the inventory's stats
                            mainInventory.SetStats(playerMerchant);
                        } // end if (freeSlot = inventory.FindFreeSlot(PlayerNumber, SlotType.Inventory)) >= 0
                    } // end if
                } // end else
            } // end if
        } // end HandleEquipment

        // Sell an item to the market
        void SellToMarket(Item item)
        {
            // Make sure we don't sell equipped items
            if (SlotId < mainInventory.WeaponSlot)
            {
                // Add it to the market's inventory
                if (subInventoryOne.AddItem(1, 5, item.Id, SlotType.Market))
                {
                    // Now remove it from the player's inventory
                    mainInventory.Remove(PlayerNumber, item);
                } // end if subInventoryOne.AddItem(1, 5, item.Id, SlotType.Market)
            } // end if
        } // end SellToMarket

        // Trades an item to an ally
        void TradeToAlly(Item item)
        {
            // Get the player's merchant
            Merchant playerMerchant = (Merchant)GameMaster.Instance.GetPlayerScript(PlayerNumber).Entity;

            // Get the ally, this is hardcoded for the port ally
            Porter ally = (Porter)playerMerchant.GetAlly(0).GetComponent<PorterMB>().Entity;

            // Transfer the resource to the ally
            if (playerMerchant.TransferResource<Porter>(ally, (Resource)item))
            {
                // Update the player's stats
                mainInventory.SetStats(playerMerchant);
            } // end if
        } // end TradeToAlly

        // Adds items to the recycle bin
        void HandleRecycle(Item item)
        {
            // Make sure we're not right clicking the equipped equipment
            if (SlotId < mainInventory.WeaponSlot)
            {
                // Add it to the recycle bin's inventory
                if (subInventoryThree.AddItem(3, 6, item.Id, SlotType.Recycle))
                {
                    // Now remove it from the player's inventory
                    mainInventory.Remove(PlayerNumber, item);
                } // end if subInventoryThree.AddItem(3, 6, item.Id, SlotType.Recycle)
            } // end if
        } // end HandleRecycle

        #endregion
    } // end InventorySlot
} // end GSP.Items.Inventories
