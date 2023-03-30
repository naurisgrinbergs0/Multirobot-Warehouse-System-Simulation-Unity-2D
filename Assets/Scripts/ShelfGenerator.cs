using UnityEngine;

public class ShelfGenerator : MonoBehaviour
{
    public GameObject ShelfPrefab;
    public GameObject FloorPrefab;
    public int ShelvesHorizontal = 5;
    public int ShelvesVertical = 5;

    public GameObject[] GenerateShelves()
    {
        int numShelves = ShelvesHorizontal * ShelvesVertical;
        GameObject[] shelfPositions = new GameObject[numShelves];

        float shelfWidth = ShelfPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float shelfHeight = ShelfPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        float floorWidth = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float floorHeight = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        float spacingX = (floorWidth - (ShelvesHorizontal * shelfWidth)) / (ShelvesHorizontal - 1 != 0 ? ShelvesHorizontal - 1 : 2);
        float spacingY = (floorHeight - (ShelvesVertical * shelfHeight)) / (ShelvesVertical - 1 != 0 ? ShelvesVertical - 1 : 2);

        float startX = FloorPrefab.transform.position.x - (floorWidth / 2f) + (shelfWidth / 2f);
        float startY = FloorPrefab.transform.position.y - (floorHeight / 2f) + (shelfHeight / 2f);

        int index = 0;
        for (int x = 0; x < ShelvesHorizontal; x++)
        {
            for (int y = 0; y < ShelvesVertical; y++)
            {
                Vector3 position = new Vector3(startX + (x * (shelfWidth + spacingX)), startY + (y * (shelfHeight + spacingY)), 0);
                GameObject shelf = GameObject.Instantiate(ShelfPrefab, position, Quaternion.identity);
                shelfPositions[index++] = shelf;
            }
        }

        return shelfPositions;
    }



}
