/*******************************************************************************
 *
 *  File Name: TileManager.cs
 *
 *  Description: Deals with the logic of the tiles
 *
 *******************************************************************************/
using GSP.Core;
using GSP.Items;
using System;
using UnityEngine;
using X_UniTMX;
using System.Collections.Generic;

namespace GSP.Tiles
{
    /*******************************************************************************
     *
     * Name: TileManager
     * 
     * Description: Manages all the tiles in the game.
     * 
     *******************************************************************************/
    public class TileManager : MonoBehaviour
	{
        Map tiledMap;                       // The map that was loaded for the game
        List<Vector2> resourcePositions;    // List of positions of resources on the map
        List<Vector2> marketPositions;      // List of positions of markets on the map

        public TextAsset mapFile;           // The map file the TileManager is managing, set through the editor
        public Material defaultMaterial;    // The default material for the sprites, set through the editor

    	// Used for initialisation
        void Awake()
        {
            // Initialise the lists
            resourcePositions = new List<Vector2>();
            marketPositions = new List<Vector2>();
        } // end Awake
        
        // Sets the dimensions of the Tiled map; This should only be called once when initialising the map
		void SetDimensions(int size, int tilesWide, int tilesHigh)
		{
            // Update the map values of the TileUtils class
            TileUtils.Update(size, tilesWide, tilesHigh);
		} // end SetDimensions

		// Generates a map for the game's level.
		public void GenerateMap()
		{
            // Create a new map and supply a callback for when the map is loaded.
            new Map(mapFile, string.Empty, OnMapLoaded);
		} // end GenerateAndAddTiles

        // Callback for when the map has been loaded.
        void OnMapLoaded(Map map)
        {
            // Set the loaded map
            tiledMap = map;

            /*
             * Generate the map with the following options:
             * 
             * TileManager's GameObject is the parent in the hierarchy
             * The default material is the default sprite's material
             */
            tiledMap.Generate(this.gameObject, defaultMaterial);

            // Generate any tile collisions
            tiledMap.GenerateTileCollisions();

            MapObjectLayer resourcesObjectLayer;    // The object layer for resources

            // Check if the map has a resources object layer
            if ((resourcesObjectLayer = tiledMap.GetObjectLayer("Resources")) != null)
            {
                // Get the list of MapObjects on the resources layer
                List<MapObject> resourceObjects = resourcesObjectLayer.Objects;

                // Check if the game is new
                if (GameMaster.Instance.IsNew)
                {
                    // The game is new so add all the resources to the list
                    foreach (var resource in resourceObjects)
                    {
                        resourcePositions.Add(ToMap(resource.Bounds));
                    } // end foreach

                    // Finally, save the resource positions list
                    GameMaster.Instance.SaveResources();
                } // end if
                else
                {
                    // The game isn't new so load all the saved resources
                    resourcePositions = GameMaster.Instance.LoadResources();
                } // end else
                
                // Loop through the objects to check if they're on the resources list
                foreach (var resource in resourceObjects)
                {
                    // Make sure the resource is in the list
                    if (resourcePositions.Contains(ToMap(resource.Bounds)))
                    {
                        // Generate the prefab for the resource
                        tiledMap.GeneratePrefab(resource, Vector2.zero, null, false, true);
                    } // end if
                } // end foreach
            } // end if

            // Check if the map has a markets object layer
            if (tiledMap.GetObjectLayer("Markets") != null)
            {
                // Get the list of MapObjects on the markets layer
                List<MapObject> marketObjects = tiledMap.GetObjectLayer("Markets").Objects;

                // Add all the markets to the list
                foreach (var market in marketObjects)
                {
                    marketPositions.Add(ToMap(market.Bounds));
                } // end foreach
                
                // Generate the colliders from this layer
                tiledMap.GenerateCollidersFromLayer("Markets");
                // Generate the prefabs from this layer
                tiledMap.GeneratePrefabsFromLayer("Markets", Vector2.zero, false, true);
            } // end if

            // Finally, Update the TileUtils static class
            SetDimensions(tiledMap.MapRenderParameter.TileHeight, tiledMap.MapRenderParameter.Width,
                tiledMap.MapRenderParameter.Height);
        } // end OnMapLoaded

        // Gets the resource type of a given resource on the map
        public ResourceType GetResourceType(string resourceName)
        {
            MapObjectLayer resourcesObjectLayer;    // The object layer for resources
            
            // The resource type used to return the resource's type
            ResourceType resourceType = ResourceType.None;

            // Check if the map has a resources object layer
            if ((resourcesObjectLayer = tiledMap.GetObjectLayer("Resources")) != null)
            {
                // Get the list of MapObjects on the resources layer
                List<MapObject> resourceObjects = resourcesObjectLayer.Objects;

                // Find the object that matches the given name
                MapObject mapResource = resourceObjects.Find(resource => resource.Name == resourceName);

                // Check if we found anything
                if (mapResource != null)
                {
                    // Check if the resource has a resource type property
                    if (mapResource.HasProperty("restype"))
                    {
                        // Get the resource type property
                        string type = mapResource.GetPropertyAsString("restype");

                        // Try to parse the ResourceType from the Type
                        try
                        {
                            resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), type);

                            // Check if the result is defined
                            if (!Enum.IsDefined(typeof(ResourceType), resourceType))
                            {
                                // It's not defined so return None
                                Debug.LogErrorFormat("'{0}' is not defined within the ResourceType enum.", resourceType.ToString());
                                resourceType = ResourceType.None;
                            } // end if
                        } // end try
                        catch (ArgumentException)
                        {
                            // We couldn't parse so return None
                            Debug.LogErrorFormat("Parsing the Type '{0}' as a ResourceType failed.", type);
                            resourceType = ResourceType.None;
                        } // end catch
                    } // end if
                    else
                    {
                        // Otherwise, the resource doesn't have a resource type property so return none
                        resourceType = ResourceType.None;
                    } // end else
                } // end if
                else
                {
                    // Otherwise, nothing was found so return none
                    resourceType = ResourceType.None;
                } // end else
            }

            // Return the resource type of the given resource on the map
            return resourceType;
        } // end GetResourceType

        // Gets the resource type of a given resource on the map
        public ResourceType GetResourceType(Vector2 key)
        {
            // First, get the map object at the specified location
            MapObject resource = GetObject(1, key);

            // Make sure the object exists
            if (resource != null)
            {
                // Get the object's resource type by its name now
                return GetResourceType(resource.Name);
            } // end if
            else
            {
                // Otherwise the object doesn't exist so return none
                return ResourceType.None;
            } // end else
        } // end GetResourceType

        // Gets the market type of a given market on the map
        public MarketType GetMarketType(string marketName)
        {
            MapObjectLayer marketsObjectLayer;    // The object layer for markets

            // The market type used to return the market's type
            MarketType marketType = MarketType.None;

            // Check if the map has a markets object layer
            if ((marketsObjectLayer = tiledMap.GetObjectLayer("Markets")) != null)
            {
                // Get the list of MapObjects on the markets layer
                List<MapObject> marketObjects = marketsObjectLayer.Objects;

                // Find the object that matches the given name
                MapObject mapMarket = marketObjects.Find(market => market.Name == marketName);

                // Check if we found anything
                if (mapMarket != null)
                {
                    // Check if the market has a restype property
                    if (mapMarket.HasProperty("restype"))
                    {
                        // Get the restype property
                        string type = mapMarket.GetPropertyAsString("restype");

                        // Try to parse the MarketType from the type
                        try
                        {
                            marketType = (MarketType)Enum.Parse(typeof(MarketType), type);

                            // Check if the result is defined
                            if (!Enum.IsDefined(typeof(MarketType), marketType))
                            {
                                // It's not defined so return None
                                Debug.LogErrorFormat("'{0}' is not defined within the MarketType enum.", marketType.ToString());
                                marketType = MarketType.None;
                            } // end if
                        } // end try
                        catch (ArgumentException)
                        {
                            // We couldn't parse so return None
                            Debug.LogErrorFormat("Parsing the Type '{0}' as a MarketType failed.", type);
                            marketType = MarketType.None;
                        } // end catch
                    } // end if
                    else
                    {
                        // Otherwise, the resource doesn't have a restype property so return none
                        marketType = MarketType.None;
                    } // end else
                } // end if
                else
                {
                    // Otherwise, nothing was found so return none
                    marketType = MarketType.None;
                } // end else
            }

            // Return the market type of the given market on the map
            return marketType;
        } // end GetResourceType

        // Gets the market type of a given resource on the map
        public MarketType GetMarketType(Vector2 key)
        {
            // First, get the map object at the specified location
            MapObject market = GetObject(2, key);

            // Make sure the object exists
            if (market != null)
            {
                // Get the object's market type by its name now
                return GetMarketType(market.Name);
            } // end if
            else
            {
                // Otherwise the object doesn't exist so return none
                return MarketType.None;
            } // end else
        } // end GetMarketType

        // Gets if a key exists in the list
        bool EntryExists(int list, Vector2 key)
        {
            // Switch over the list parameter
            switch (list)
            {
                // Resource list
                case 1:
                    {
                        return resourcePositions.Contains(key);
                    } // end case 1
                // Market list
                case 2:
                    {
                        return marketPositions.Contains(key);
                    } // end case 2
                default:
                    {
                        return false;
                    } // end default case
            } // end switch
        } // end EntryExists
        
        // Gets a map object's game object at the given coordinates
        MapObject GetObject(int list, Vector2 key)
        {
            // Holds the returned value
            MapObject mapObject = null;
            
            // Check if the entry exists
            if (EntryExists(list, key))
            {
                // The key exists so retrieve the object

                MapObjectLayer objectLayer;    // The object layer

                // Switch over the list parameter
                switch (list)
                {
                    // Resource list
                    case 1:
                        {
                            // Check if the map has a resources object layer
                            if ((objectLayer = tiledMap.GetObjectLayer("Resources")) != null)
                            {
                                // Get the list of MapObjects on the resources layer
                                List<MapObject> resourceObjects = objectLayer.Objects;

                                // Find the object that matches the given position
                                MapObject mapResource = resourceObjects.Find(resource => resource.Bounds.x == key.x && resource.Bounds.y == Math.Abs(key.y));

                                // Check if we found anything
                                if (mapResource != null)
                                {
                                    // Set the map object to the found resource
                                    mapObject = mapResource;
                                } // end if
                                else
                                {
                                    // Otherwise, nothing was found so return null
                                    mapObject = null;
                                } // end else
                            } // end if

                            break;
                        } // end case 1
                    // Market list
                    case 2:
                        {
                            // Check if the map has a markets object layer
                            if ((objectLayer = tiledMap.GetObjectLayer("Markets")) != null)
                            {
                                // Get the list of MapObjects on the markets layer
                                List<MapObject> marketObjects = objectLayer.Objects;

                                // Find the object that matches the given position
                                MapObject mapMarket = marketObjects.Find(market => market.Bounds.x == key.x && market.Bounds.y == Math.Abs(key.y));

                                // Check if we found anything
                                if (mapMarket != null)
                                {
                                    // Set the map object to the found resource
                                    mapObject = mapMarket;
                                } // end if
                                else
                                {
                                    // Otherwise, nothing was found so return null
                                    mapObject = null;
                                } // end else
                            } // end if

                            break;
                        } // end case 2
                    default:
                        {
                            mapObject = null;
                            break;
                        } // end default case
                } // end switch
            } // end if
            else
            {
                // Otherwise the key doesn't exist so return null
                mapObject = null;
            } // end else

            // Return the map object
            return mapObject;
        } // end GetObject

        // Gets a resource's game object
        public GameObject GetResource(Vector2 key)
        {
            // Retrieve the map object first
            MapObject mapObject = GetObject(1, key);

            // Make sure the object isn't null
            if (mapObject != null)
            {
                // Return the map object's game object
                return mapObject.LinkedGameObject;
            } // end if
            else
            {
                // Otherwise, simply return null
                return null;
            } // end else
        } // end GetResource

        // Gets a market's game object
        public GameObject GetMarket(Vector2 key)
        {
            // Retrieve the map object first
            MapObject mapObject = GetObject(2, key);

            // Make sure the object isn't null
            if (mapObject != null)
            {
                // Return the map object's game object
                return mapObject.LinkedGameObject;
            } // end if
            else
            {
                // Otherwise, simply return null
                return null;
            } // end else
        } // end GetMarket

        // Removes a resource from the list and the map
        public void RemoveResource(Vector2 key)
        {
            // Check to make sure the entry exists
            if (EntryExists(1, key))
            {
                // If the entry exists, it means the correct key format was entered.
                
                // First destroy the game object
                Destroy(GetResource(key));

                // Finally, remove it from the list
                resourcePositions.Remove(key);
            } // end if
        } // end RemoveResource

        // Converts the map objects' bounding box positions to the map's version
        Vector2 ToMap(Rect bounds)
        {
            // Create a new vector based upon the bound's x and y
            Vector2 tmp = new Vector2(bounds.x, bounds.y);
            tmp.y *= -1.0f;

            // Return the converted vector
            return tmp;
        } // end ToMap

        // Gets the resource's positions
        public List<Vector2> ResourcePositions
        {
            get
            {
                // Create a temp list based upon the resource positions
                List<Vector2> tempPositions = resourcePositions;

                // Return the temp list
                return tempPositions;
            } // end get
        } // end ResourcePositions

        // Gets the market's positions
        public List<Vector2> MarketPositions
        {
            get
            {
                // Create a temp list based upon the market positions
                List<Vector2> tempPositions = marketPositions;

                // Return the temp list
                return tempPositions;
            } // end get
        } // end MarketPositions
	} // end TileManager
} // end GSP.Tiles
