using UnityEngine;

public class ShelfGenerator : MonoBehaviour
{
    public GameObject ShelfPrefab;
    public GameObject FloorPrefab;
    
    public GameObject[] GenerateShelves(int shelvesHorizontal, int shelvesVertical)
    {
        int numShelves = shelvesHorizontal * shelvesVertical;
        GameObject[] shelfPositions = new GameObject[numShelves];

        float shelfWidth = ShelfPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float shelfHeight = ShelfPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        float floorWidth = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float floorHeight = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.y - (2 * WallGenerator.WALL_THICKNESS);

        float spacingX = (floorWidth - (shelvesHorizontal * shelfWidth)) 
            / (shelvesHorizontal - 1 != 0 ? shelvesHorizontal - 1 : 2);
        float spacingY = (floorHeight - (shelvesVertical * shelfHeight)) 
            / (shelvesVertical - 1 != 0 ? shelvesVertical - 1 : 2);

        float startX = FloorPrefab.transform.position.x - (floorWidth / 2f) + (shelfWidth / 2f);
        float startY = FloorPrefab.transform.position.y - (floorHeight / 2f) + (shelfHeight / 2f);

        // generate shelves
        int index = 0;
        for (int x = 0; x < shelvesHorizontal; x++)
        {
            for (int y = 0; y < shelvesVertical; y++)
            {
                Vector3 position = new Vector3(startX + (x * (shelfWidth + spacingX)), startY + (y * (shelfHeight + spacingY)), 0);
                GameObject shelf = GameObject.Instantiate(ShelfPrefab, position, Quaternion.identity);
                shelfPositions[index++] = shelf;
            }
        }

        return shelfPositions;
    }



}
