/*******************************************************************************
 *
 *  File Name: ResourcePositionList.cs
 *
 *  Description: A clean serialisable class for saving and loading the resource
 *               positions
 *
 *******************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSP.Core
{
    [Serializable]
    /*******************************************************************************
     *
     * Name: ResourcePositionList
     * 
     * Description: Container class for a clean version of the serialisable resource
     *              position.
     * 
     *******************************************************************************/
    public class ResourcePositionList
    {
        List<SerializableVector2> positions; // The positions of the resources

        // Default constructor; This will create an empty object
        public ResourcePositionList()
        {
            // Initialise the lists
            positions = new List<SerializableVector2>();

            // Reset the container; This prevents the initialisation being in 2 places
            Reset();
        } // end ResourcePositionList

        // Resets the contents of the container
        public void Reset()
        {
            // Clear the lists
            positions.Clear();
        } // end Reset

        // Gets the position of a resource at the given index
        public Vector2 GetPosition(int index)
        {
            return positions[index];
        } // end GetPosition

        // Adds the position of a resource to the list
        public void AddPosition(Vector2 position)
        {
            positions.Add(position);
        } // end AddPosition

        // Gets the number of elements in the list
        public int Count
        {
            get { return positions.Count; }
        } // end Count
    } // end ResourcePositionList
} // end GSP.Core
