using UnityEngine;

public class RobotGenerator : MonoBehaviour
{
    public GameObject RobotPrefab;
    public GameObject FloorPrefab;
    public int NumRobots = 5;
    public static float ROBOT_SIZE = 1f;

    public GameObject[] GenerateRobots()
    {
        GameObject[] robotPositions = new GameObject[NumRobots];

        float floorHeight = FloorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

        float startX = FloorPrefab.transform.position.x;
        float startY = FloorPrefab.transform.position.y - (floorHeight / 2f) + (ROBOT_SIZE / 2f);

        float spacing = (floorHeight - (NumRobots * ROBOT_SIZE)) / (NumRobots + 1);

        for (int i = 0; i < NumRobots; i++)
        {
            Vector3 position = new Vector3(startX, startY + ((i + 1) * spacing) + (i * ROBOT_SIZE), 0);
            GameObject robot = GameObject.Instantiate(RobotPrefab, position, Quaternion.Euler(0, 0, 90));
            //robot.GetComponent<SpriteRenderer>().color = Color.red;
            robot.GetComponent<SpriteRenderer>().size = new Vector2(ROBOT_SIZE, ROBOT_SIZE);
            robotPositions[i] = robot;
        }

        return robotPositions;
    }

}
