using UnityEngine;

public class WarehouseController : MonoBehaviour
{
    public GameObject robotPrefab;
    public Transform[] robotSpawnPoints;
    public Transform[] shelfPositions;

    private GameObject[] robots;

    void Start()
    {
        // Spawn robots at the designated spawn points
        robots = new GameObject[robotSpawnPoints.Length];
        for (int i = 0; i < robotSpawnPoints.Length; i++)
        {
            GameObject robot = Instantiate(robotPrefab, robotSpawnPoints[i].position, Quaternion.identity);
            robots[i] = robot;

            // Assign each robot a path planning algorithm to follow
            // (implementation of this would depend on the specific algorithm being used)
            PathPlanningAlgorithm algorithm = new PathPlanningAlgorithm();
            robot.GetComponent<RobotController>().SetAlgorithm(algorithm);
        }
    }

    void Update()
    {
        // Update each robot's position and target based on its path planning algorithm
        for (int i = 0; i < robots.Length; i++)
        {
            RobotController robotController = robots[i].GetComponent<RobotController>();
            if (robotController.HasReachedTarget())
            {
                // Choose a new target based on the robot's current position and the location of the shelves
                Vector3 currentPosition = robotController.GetCurrentPosition();
                Vector3 targetShelfPosition = ChooseNewTargetShelf(currentPosition);
                robotController.SetTarget(targetShelfPosition);
            }
            robotController.UpdatePosition();
        }
    }

    private Vector3 ChooseNewTargetShelf(Vector3 currentPosition)
    {
        // Choose the closest shelf that has not been visited yet
        float minDistance = float.MaxValue;
        Transform targetShelf = null;
        foreach (Transform shelf in shelfPositions)
        {
            if (!shelf.GetComponent<ShelfController>().HasBeenVisited())
            {
                float distance = Vector3.Distance(currentPosition, shelf.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetShelf = shelf;
                }
            }
        }

        if (targetShelf != null)
        {
            targetShelf.GetComponent<ShelfController>().SetVisited(true);
            return targetShelf.position;
        }
        else
        {
            // If all shelves have been visited, return to the starting position
            return robotSpawnPoints[0].position;
        }
    }
}

public class RobotController : MonoBehaviour
{
    private PathPlanningAlgorithm algorithm;
    private Vector3 target;

    void Start()
    {
        // Set the initial target to the robot's starting position
        target = transform.position;
    }

    public void SetAlgorithm(PathPlanningAlgorithm algorithm)
    {
        this.algorithm = algorithm;
    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
    }

    public Vector3 GetCurrentPosition()
    {
        return transform.position;
    }

    public bool HasReachedTarget()
    {
        float distance = Vector3.Distance(transform.position, target);
        return distance < 0.1f;
    }

    public void UpdatePosition()
    {
        // Move towards the target using a simple linear interpolation
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 2.0f);
    }
}

public class ShelfController : MonoBehaviour
{
    private bool hasBeenVisited = false;

    public bool HasBeenVisited()
    {
        return hasBeenVisited;
    }

    public void SetVisited(bool visited)
    {
        hasBeenVisited = visited;
    }
}

public class PathPlanningAlgorithm
{
    // Placeholder class for a path planning algorithm implementation
}
