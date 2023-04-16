using Assets.Scripts.Path_Planning;
using Assets.Scripts.Robot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public class TileMap : MapBase
    {
        public int[,] tiles;
        public float tileSize;

        private const float marginAroundObstacles = 0.5f;

        public TileMap(Transform floor, Transform[] shelves, Transform[] walls, float tileSize)
            : base(floor, shelves, walls)
        {
            this.tileSize = tileSize;
            tiles = GenerateMap();
        }

        private int[,] GenerateMap()
        {
            // get the number of tiles in each direction for both floors
            int widthTiles = Mathf.RoundToInt(floor.GetComponent<SpriteRenderer>().bounds.size.x / tileSize);
            int heightTiles = Mathf.RoundToInt(floor.GetComponent<SpriteRenderer>().bounds.size.y / tileSize);

            // initialize the map with all free tiles
            int[,] map = new int[widthTiles, heightTiles];
            for (int x = 0; x < map.GetLength(0); x++)
                for (int y = 0; y < map.GetLength(1); y++)
                    map[x, y] = 0; // 0 represents a free tile

            // define the margin width
            float margin = tileSize * marginAroundObstacles;

            // iterate over all the tiles and mark obstructed tiles
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    float[] tileCenter = TileToXY(x, y);

                    // check if the tile intersects with any shelf with a margin around it
                    bool intersectsObstacle = false;
                    foreach (Transform shelf in shelves)
                    {
                        Bounds shelfBounds = shelf.GetComponent<SpriteRenderer>().bounds;
                        shelfBounds.Expand(margin); // expand the bounds by the margin width
                        if (shelfBounds.Intersects(new Bounds(new Vector3(tileCenter[0], tileCenter[1]), new Vector3(tileSize, tileSize))))
                        {
                            intersectsObstacle = true;
                            break;
                        }
                    }

                    // check if the tile intersects with any wall with a margin around it
                    foreach (Transform wall in walls)
                    {
                        Bounds wallBounds = wall.GetComponent<SpriteRenderer>().bounds;
                        wallBounds.Expand(margin); // expand the bounds by the margin width
                        if (wallBounds.Intersects(new Bounds(new Vector3(tileCenter[0], tileCenter[1]), new Vector3(tileSize, tileSize))))
                        {
                            intersectsObstacle = true;
                            break;
                        }
                    }

                    // set the tile as obstructed if it intersects with a shelf
                    map[x, y] = intersectsObstacle ? 1 : 0;
                }
            }

            return map;
        }


        #region Calculations

        public int[] XYToTile(float x, float y)
        {
            // convert xy position to tile position
            Vector3 floorLowerLeftCorner = floor.transform.position - floor.GetComponent<Renderer>().bounds.extents;
            return new int[] { (int)((x - floorLowerLeftCorner.x) / tileSize), (int)((y - floorLowerLeftCorner.y) / tileSize) };
        }

        public float[] TileToXY(int tileX, int tileY)
        {
            // convert tile position to xy position
            Vector3 floorLowerLeftCorner = floor.transform.position - floor.GetComponent<Renderer>().bounds.extents;
            return new float[] { floorLowerLeftCorner.x + tileX * tileSize + tileSize / 2
                , floorLowerLeftCorner.y + tileY * tileSize + tileSize / 2};
        }

        public int[] GetShelfTile(Transform shelf)
        {
            // take center tile within the shelf
            float x = shelf.GetComponent<Renderer>().bounds.center.x;
            float y = shelf.GetComponent<Renderer>().bounds.center.y;

            int[] tile = XYToTile(x, y);

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
            if (trip.fromLinkedTransform.CompareTag("Shelf"))
                tileStart = GetShelfTile(trip.fromLinkedTransform);
            else if (trip.fromLinkedTransform.CompareTag("ZoneLoad") || trip.fromLinkedTransform.CompareTag("ZoneUnload")
                || trip.fromLinkedTransform.CompareTag("Robot"))
                tileStart = XYToTile(trip.fromLinkedTransform.GetComponent<Renderer>().bounds.center.x
                    , trip.fromLinkedTransform.GetComponent<Renderer>().bounds.center.y);

            // to
            if (trip.toLinkedTransform.CompareTag("Shelf"))
                tileEnd = GetShelfTile(trip.toLinkedTransform);
            else if (trip.toLinkedTransform.CompareTag("ZoneLoad") || trip.toLinkedTransform.CompareTag("ZoneUnload"))
                tileEnd = XYToTile(trip.toLinkedTransform.GetComponent<Renderer>().bounds.center.x
                    , trip.toLinkedTransform.GetComponent<Renderer>().bounds.center.y);

            return new[] { tileStart[0], tileStart[1], tileEnd[0], tileEnd[1] };
        }

        #endregion Calculations



        #region Drawing

        public void DrawObstructedTiles(GameObject tilePrefab/* = null*/)
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
                for (int y = 0; y < tiles.GetLength(1); y++)
                    if (tiles[x, y] == 1) // check if the tile is obstructed
                        DrawTile(x, y, tilePrefab: /*tilePrefab == null ? this.tilePrefab :*/ tilePrefab);
        }

        public GameObject DrawGoalTile(int tileX, int tileY, GameObject tilePrefab/* = null*/)
        {
            return DrawTile(tileX, tileY, /*tilePrefab == null ? this.tileGoalPrefab :*/ tilePrefab, tileSize * 1.5f, tileSize * 1.5f);
        }

        public GameObject DrawTile(int tileX, int tileY, GameObject tilePrefab/* = null*/, float sizeX = 0, float sizeY = 0)
        {
            // calculate the position of the tile
            float[] pos = TileToXY(tileX, tileY);

            // create the tile game object
            GameObject tileObj = GameObject.Instantiate(/*tilePrefab == null ? this.tilePrefab :*/ tilePrefab
                , new Vector3(pos[0], pos[1], 0f), Quaternion.identity);
            //tileObj.transform.localScale = new Vector3(tileSize, tileSize, 1f);
            tileObj.GetComponent<SpriteRenderer>().size = new Vector2(sizeX == 0 ? tileSize : sizeX, sizeY == 0 ? tileSize : sizeY);
            return tileObj;
        }

        #endregion Drawing
    }
}
