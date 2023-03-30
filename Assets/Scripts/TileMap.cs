using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class TileMap
    {
        public int[,] tiles;
        Transform floor;
        Transform[] shelves;
        Transform[] walls;
        public float tileSize;
        public GameObject tilePrefab;
        public GameObject tileGoalPrefab;

        private const float marginAroundObstacles = 1f;

        public TileMap(Transform floor, Transform[] shelves, Transform[] walls, float tileSize, GameObject tilePrefab, GameObject tileGoalPrefab)
        {
            this.floor = floor;
            this.shelves = shelves;
            this.walls = walls;
            this.tileSize = tileSize;
            this.tilePrefab = tilePrefab;
            this.tileGoalPrefab = tileGoalPrefab;
            tiles = GenerateMap();
        }

        private int[,] GenerateMap()
        {
            // Get the number of tiles in each direction for both floors
            int widthTiles = Mathf.RoundToInt(floor.GetComponent<SpriteRenderer>().bounds.size.x / tileSize);
            int heightTiles = Mathf.RoundToInt(floor.GetComponent<SpriteRenderer>().bounds.size.y / tileSize);

            // Initialize the map with all free tiles
            int[,] map = new int[widthTiles, heightTiles];
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = 0; // 0 represents a free tile
                }
            }

            // Define the margin width
            float margin = tileSize * marginAroundObstacles;

            // Iterate over all the tiles and mark obstructed tiles
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    float[] tileCenter = TileToXY(x, y);

                    // Check if the tile intersects with any shelf with a margin around it
                    bool intersectsObstacle = false;
                    foreach (Transform shelf in shelves)
                    {
                        Bounds shelfBounds = shelf.GetComponent<SpriteRenderer>().bounds;
                        shelfBounds.Expand(margin); // Expand the bounds by the margin width
                        if (shelfBounds.Intersects(new Bounds(new Vector3(tileCenter[0], tileCenter[1]), new Vector3(tileSize, tileSize))))
                        {
                            intersectsObstacle = true;
                            break;
                        }
                    }

                    // Check if the tile intersects with any wall with a margin around it
                    foreach (Transform wall in walls)
                    {
                        Bounds wallBounds = wall.GetComponent<SpriteRenderer>().bounds;
                        wallBounds.Expand(margin); // Expand the bounds by the margin width
                        if (wallBounds.Intersects(new Bounds(new Vector3(tileCenter[0], tileCenter[1]), new Vector3(tileSize, tileSize))))
                        {
                            intersectsObstacle = true;
                            break;
                        }
                    }

                    // Set the tile as obstructed if it intersects with a shelf
                    map[x, y] = intersectsObstacle ? 1 : 0;
                }
            }

            return map;
        }


        #region Calculations

        public int[] XYToTile(float x, float y)
        {
            Vector3 floorLowerLeftCorner = floor.transform.position - floor.GetComponent<Renderer>().bounds.extents;
            return new int[] { (int)((x - floorLowerLeftCorner.x) / tileSize), (int)((y - floorLowerLeftCorner.y) / tileSize) };
        }

        public float[] TileToXY(int tileX, int tileY)
        {
            Vector3 floorLowerLeftCorner = floor.transform.position - floor.GetComponent<Renderer>().bounds.extents;
            return new float[] { floorLowerLeftCorner.x + tileX * tileSize + tileSize / 2
                , floorLowerLeftCorner.y + tileY * tileSize + tileSize / 2};
        }

        public int[] GetShelfTile(GameObject shelf)
        {
            // take center tile within the shelf
            float x = shelf.GetComponent<Renderer>().bounds.center.x;
            float y = shelf.GetComponent<Renderer>().bounds.center.y;

            int[] tile = XYToTile(x, y);

            // SKETCHY - BASICALLY GETS CLOSEST UNOBSTRUCTED LINE TO THE RIGHT
            int i = tile[0];
            while (tiles[i, tile[1]] == 1)
                i++;
            tile[0] = i;
            return tile;
        }

        public int[] GetTripTiles(Trip trip)
        {
            int[] tileStart = null; int[] tileEnd = null;
            // from
            if (trip.from.CompareTag("Shelf"))
                tileStart = GetShelfTile(trip.from);
            else if (trip.from.CompareTag("ZoneLoad") || trip.from.CompareTag("ZoneUnload")
                || trip.from.CompareTag("Robot"))
                tileStart = XYToTile(trip.from.GetComponent<Renderer>().bounds.center.x
                    , trip.from.GetComponent<Renderer>().bounds.center.y);

            // to
            if (trip.to.CompareTag("Shelf"))
                tileEnd = GetShelfTile(trip.to);
            else if (trip.to.CompareTag("ZoneLoad") || trip.to.CompareTag("ZoneUnload"))
                tileEnd = XYToTile(trip.to.GetComponent<Renderer>().bounds.center.x
                    , trip.to.GetComponent<Renderer>().bounds.center.y);

            return new[] { tileStart[0], tileStart[1], tileEnd[0], tileEnd[1] };
        }

        #endregion Calculations



        #region Drawing

        public void DrawObstructedTiles(GameObject tilePrefab = null)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
                for (int y = 0; y < tiles.GetLength(1); y++)
                    if (tiles[x, y] == 1) // check if the tile is obstructed
                        DrawTile(x, y, tilePrefab: tilePrefab == null ? this.tilePrefab : tilePrefab);
        }

        public GameObject DrawGoalTile(int tileX, int tileY, GameObject tilePrefab = null)
        {
            return DrawTile(tileX, tileY, tileSize * 1.5f, tileSize * 1.5f, tilePrefab == null ? this.tileGoalPrefab : tilePrefab);
        }

        public GameObject DrawTile(int tileX, int tileY, float sizeX = 0, float sizeY = 0, GameObject tilePrefab = null)
        {
            // Calculate the position of the tile
            float[] pos = TileToXY(tileX, tileY);

            // Create the tile game object
            GameObject tileObj = GameObject.Instantiate(tilePrefab == null ? this.tilePrefab : tilePrefab
                , new Vector3(pos[0], pos[1], 0f), Quaternion.identity);
            //tileObj.transform.localScale = new Vector3(tileSize, tileSize, 1f);
            tileObj.GetComponent<SpriteRenderer>().size = new Vector2(sizeX == 0 ? tileSize : sizeX, sizeY == 0 ? tileSize : sizeY);
            return tileObj;
        }

        #endregion Drawing
    }
}
