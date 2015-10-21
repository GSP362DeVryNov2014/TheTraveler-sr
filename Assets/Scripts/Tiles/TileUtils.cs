/*******************************************************************************
 *
 *  File Name: TileUtils.cs
 *
 *  Description: Utility class to replace the static functionality of the old
 *               TileManager.
 *
 *******************************************************************************/
using UnityEngine;

namespace GSP.Tiles
{
    /*******************************************************************************
     *
     * Name: TileUtils
     * 
     * Description: Replaces the static functionality of the old TileManager.
     * 
     *******************************************************************************/
    public static class TileUtils
    {
        // These will be the same that is used in Tiled when creating the map
        static int tileSize = 0;		// The size of each tile
        static int numTilesWide = 0;	// The number of tiles in width
        static int numTilesHigh = 0;   // The number of tile in height

        // Update the values of the static variables
        public static void Update(int size, int numWide, int numHigh)
        {
            // Update the static variables
            TileSize = size;
            NumTilesWide = numWide;
            NumTilesHigh = numHigh;
        } // end Update

        // Resets the values of the variables to their initial states. This is used when switching scenes.
        public static void Reset()
        {
            // Reset everything to its initial state
            tileSize = 0;
            numTilesWide = 0;
            numTilesHigh = 0;
        } // end Reset

        // Gets the tile's Size
        public static int TileSize
        {
            get { return tileSize; }
            private set { TileUtils.tileSize = value; }
        } // end TileSize

        // Gets the number of tiles in width
        public static int NumTilesWide
        {
            get { return numTilesWide; }
            private set { TileUtils.numTilesWide = value; }
        } // end NumTilesWide

        // Gets the number of tiles in width
        public static int NumTilesHigh
        {
            get { return numTilesHigh; }
            private set { TileUtils.numTilesHigh = value; }
        } // end NumTilesHigh

        // Gets the max height you can place a tile
        public static int MaxHeight
        {
            // The lowest part on the map is one less than the number of tiles high
            // Since we're in the fourth quadrant, the y value is negative
            get { return ((NumTilesHigh - 1) * -1); }
        } // end MaxHeight

        // Gets the max width you can place a tile
        public static int MaxWidth
        {
            // The farthest right you can get on the map is one less than the number of tiles wide
            get { return (NumTilesWide - 1); }
        } // end MaxHeight

        // Gets the min height you can place a tile
        public static int MinHeight
        {
            get { return 0; }
        } // end MinWidth

        // Gets the min width you can place a tile
        public static int MinWidth
        {
            get { return 0; }
        } // end MaxWidth

        // Gets the map's size in pixels
        public static Vector2 MapSizePixels
        {
            get { return new Vector2(NumTilesWide * TileSize, NumTilesHigh * TileSize); }
        } // end MapSizePixels

        // Gets the map's size in tiles
        public static Vector2 MapSizeTiles
        {
            get { return new Vector2(NumTilesWide, NumTilesHigh); }
        } // end MapSizeTiles

        // Gets the standard move distance for a player
        public static float PlayerMoveDistance
        {
            // The movement distance is now just one
            get { return 1.0f; }
        } // end PlayerMoveDistance
    } // end TileUtils
} // end GSP.Tiles
