﻿/*******************************************************************************
 *
 *  File Name: ResourceUtility.cs
 *
 *  Description: Contains utility functions for getting and removing resources
 *               from the new inventory system
 *
 *******************************************************************************/
using GSP.Core;
using GSP.Items.Inventories;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
    
namespace GSP.Items
{
    /*******************************************************************************
     *
     * Name: ResourceUtility
     * 
     * Description: Utility script for getting and removing resources from the new
     *              inventory system.
     * 
     *******************************************************************************/
    public class ResourceUtility
    {
        // Get all the resources
        public static List<Resource> GetResources()
        {
            // The list to return; returns empty list if the inventory doesn't exist
            List<Resource> resources = new List<Resource>();

            // Get the inventory script
            Inventory inventory = GameObject.Find("Canvas").transform.Find("Inventory").GetComponent<Inventory>();

            // Make sure the inventory exists
            if (inventory != null)
            {
                // Get all the items in the inventory
                List<Item> inventoryItems = inventory.Items;

                // Find all the resources
                resources = inventoryItems.FindAll(tempItem => tempItem is Resource).Select(item => (Resource)item).ToList();
            } // end if

            // Return the list of resources
            return resources;
        } // end GetResources

        // Removes all the resources
        public static void RemoveResources()
        {
            // Get the inventory script
            Inventory inventory = GameObject.Find("Canvas").transform.Find("Inventory").GetComponent<Inventory>();

            // Make sure the inventory exists
            if (inventory != null)
            {
                // Get all the resources
                List<Resource> resources = new List<Resource>();
                resources = GetResources();

                // Loop over each resource and remove it
                foreach (var resource in resources)
                {
                    inventory.Remove(GameMaster.Instance.Turn, resource);
                } // end foreach
            } // end if
        } // end RemoveResources

        // Get all the resources of a given type
        public static List<Resource> GetResourcesByType(ResourceType resourceType)
        {
            // The list to return; returns empty list if the inventory doesn't exist
            List<Resource> resources = new List<Resource>();
            
            // Get the inventory script
            Inventory inventory = GameObject.Find("Canvas").transform.Find("Inventory").GetComponent<Inventory>();

            // Make sure the inventory exists
            if (inventory != null)
            {
                // Get all the items in the inventory
                List<Item> inventoryItems = inventory.Items;

                // Find all the resources of the given type
                resources = inventoryItems.FindAll(tempItem => tempItem.Type == resourceType.ToString()).Select(item => (Resource)item).ToList();
            } // end if

            // Return the list of resources
            return resources;
        } // end RemoveResources

        // Remove all the resources of a given type
        public static void RemoveResourcesByType(ResourceType resourceType)
        {
            // Get the inventory script
            Inventory inventory = GameObject.Find("Canvas").transform.Find("Inventory").GetComponent<Inventory>();

            // Make sure the inventory exists
            if (inventory != null)
            {
                // Get all the resources
                 List<Resource> resources = GetResourcesByType(resourceType);

                // Loop over each resource and remove it
                foreach (var resource in resources)
                {
                    inventory.Remove(GameMaster.Instance.Turn, resource);
                } // end foreach
            } // end if
        } // end RemoveResources

        // Remove a single resource
        public static void RemoveResource(Resource resource)
        {
            // Get the inventory script
            Inventory inventory = GameObject.Find("Canvas").transform.Find("Inventory").GetComponent<Inventory>();

            // Make sure the inventory exists
            if (inventory != null)
            {
                // Remove the resource
                inventory.Remove(GameMaster.Instance.Turn, resource);
            } // end if
        } // end RemoveResource
    } // end ResourceUtility
} // end GSP.Items
