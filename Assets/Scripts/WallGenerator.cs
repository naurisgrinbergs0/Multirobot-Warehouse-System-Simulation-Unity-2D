using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    public GameObject WallPrefab;
    public GameObject FloorPrefab;
    public float wallThickness = 0.5f;

    public GameObject[] GenerateWalls()
    {
        GameObject[] wallGameObjects = new GameObject[4];

        float floorWidth = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float floorHeight = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;
        float floorCenterX = FloorPrefab.GetComponent<SpriteRenderer>().bounds.center.x;
        float floorCenterY = FloorPrefab.GetComponent<SpriteRenderer>().bounds.center.y;

        List<float[]> positionAndScaleList = new List<float[]>() { 
            new float[]{ floorCenterX, floorCenterY + floorHeight / 2f - wallThickness / 2f, floorWidth, wallThickness } // top
            , new float[]{ floorCenterX, floorCenterY - floorHeight / 2f + wallThickness / 2f, floorWidth, wallThickness } // bottom
            , new float[]{ floorCenterX - floorWidth / 2f + wallThickness / 2f, floorCenterY, wallThickness, floorHeight } // left
            , new float[]{ floorCenterX + floorWidth / 2f - wallThickness / 2f, floorCenterY, wallThickness, floorHeight } // right
        };

        int index = 0;
        foreach (float[] posAndScale in positionAndScaleList)
        {
            Vector3 position = new Vector3(posAndScale[0], posAndScale[1], 0);
            GameObject wall = GameObject.Instantiate(WallPrefab, position, Quaternion.identity);
            wall.transform.localScale = new Vector3(posAndScale[2], posAndScale[3], 1f);
            wallGameObjects[index++] = wall;
        }

        return wallGameObjects;
    }



}
