/*******************************************************************************
 *
 *  File Name: Highlight.cs
 *
 *  Description: Used for showing where the player can move
 *
 *******************************************************************************/
using GSP.Tiles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSP
{
    /*******************************************************************************
     *
     * Name: Highlight
     * 
     * Description: Highlight tiles that show the allowed movement space.
     * 
     *******************************************************************************/
    public static class Highlight
	{
		static List<GameObject> highlights = new List<GameObject>();    // The list of highlight GameObject's

		// Generates the highlight GameObject's based on position and allowed travel distance
        public static void GenerateHighlight(Vector3 position, int travelDistance)
		{
			// Initialise the temporary position to a single tile above the player's position
            Vector3 tmpPos = new Vector3(position.x, position.y + TileUtils.PlayerMoveDistance, position.z);

			// Truncate the y value now
            tmpPos.y = Convert.ToInt32(tmpPos.y * 100) / 100.0f;

			// Stretch to the north first
			for (int northIndex = 0; northIndex < travelDistance; northIndex++)
			{
				// Check if we have hit the min height
                if (tmpPos.y > TileUtils.MinHeight)
				{
					// Reached min height so break out of the loop
					break;
				} // end if
				else
				{
					// Otherwise, add a highlight GameObject to the position
                    AddHighlight(tmpPos);
					
					// Advance the position to the next tile
                    tmpPos.y += TileUtils.PlayerMoveDistance;
				} // end else
			} // end for

			// Reset the temporary vector to be a single tile to the right of the player's position
            tmpPos = new Vector3(position.x + TileUtils.PlayerMoveDistance, position.y, position.z);

			// Truncate the x value now
            tmpPos.x = Convert.ToInt32(tmpPos.x * 100) / 100.0f;

			// Stretch to the east next
			for (int eastIndex = 0; eastIndex < travelDistance; eastIndex++)
			{
				// Check if we have hit the max width
                if (tmpPos.x > TileUtils.MaxWidth)
				{
					// Reached min height so break out of the loop
					break;
				} // end if
				else
				{
					// Otherwise, add a highlight GameObject to the position
                    AddHighlight(tmpPos);
					
					// Advance the position to the next tile
                    tmpPos.x += TileUtils.PlayerMoveDistance;
				} // end else
			} // end for

			// Reset the temporary vector to be a single tile below the player's position
            tmpPos = new Vector3(position.x, position.y - TileUtils.PlayerMoveDistance, position.z);
			
			// Truncate the y value now
			tmpPos.y = Convert.ToInt32(tmpPos.y * 100) / 100.0f;
			
			// Stretch to the south next
			for (int southIndex = 0; southIndex < travelDistance; southIndex++)
			{
				// Check if we have hit the min height
                if (tmpPos.y < TileUtils.MaxHeight)
				{
					// Reached min height so break out of the loop
					break;
				} // end if
				else
				{
					// Otherwise, add a highlight GameObject to the position
                    AddHighlight(tmpPos);
					
					// Advance the position to the next tile
                    tmpPos.y -= TileUtils.PlayerMoveDistance;
				} // end else
			} // end for

			// Reset the temporary vector to be a single tile to the left of the player's position
            tmpPos = new Vector3(position.x - TileUtils.PlayerMoveDistance, position.y, position.z);
			
			// Truncate the x value now
            tmpPos.x = Convert.ToInt32(tmpPos.x * 100) / 100.0f;
			
			// Finally, stretch to the west
			for (int westIndex = 0; westIndex < travelDistance; westIndex++)
			{
				// Check if we have hit the max width
                if (tmpPos.x < TileUtils.MinWidth)
				{
					// Reached min height so break out of the loop
					break;
				} // end if
				else
				{
					// Otherwise, add a highlight GameObject to the position
                    AddHighlight(tmpPos);
					
					// Advance the position to the next tile
                    tmpPos.x -= TileUtils.PlayerMoveDistance;
				} // end else
			} // end for
		} // end GenerateHighlight

		// Destroys the highlight GameObject's
		public static void ClearHighlight()
		{
			// Loop over the list and destroy each GameObject.
			foreach (var highlight in highlights)
			{
                GameObject.Destroy(highlight);
			} // end foreach

			// Clear the list now
			highlights.Clear();
		} // end ClearHighlight

		// Adds a highlight GameObject to scene and to the list.
		static void AddHighlight(Vector3 position)
		{
			// Add a highlight GameObject to the scene at the position.
            GameObject highlight = GameObject.Instantiate(PrefabReference.prefabHighlight, position, Quaternion.identity) as GameObject;
			
			// Add the highlight GameObject to the list.
            highlights.Add(highlight);
        } // end AddHighlight
    } // end Highlight
} // end GSP
