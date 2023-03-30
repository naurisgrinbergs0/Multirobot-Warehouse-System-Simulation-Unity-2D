using UnityEngine;

public class RobotGenerator : MonoBehaviour
{
    public GameObject RobotPrefab;
    public GameObject FloorPrefab;
    public int NumRobots = 5;

    public GameObject[] GenerateRobots()
    {
        GameObject[] robotPositions = new GameObject[NumRobots];

        float robotHeight = RobotPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        float floorWidth = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        float floorHeight = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        float startX = FloorPrefab.transform.position.x;
        float startY = FloorPrefab.transform.position.y - (floorHeight / 2f) + (robotHeight / 2f);

        float spacing = (floorHeight - (NumRobots * robotHeight)) / (NumRobots + 1);

        for (int i = 0; i < NumRobots; i++)
        {
            Vector3 position = new Vector3(startX, startY + ((i + 1) * spacing) + (i * robotHeight), 0);
            GameObject robot = GameObject.Instantiate(RobotPrefab, position, Quaternion.Euler(0, 0, 90));
            robotPositions[i] = robot;
        }

        return robotPositions;
    }

}
