using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImprovedAStar
{
    public TileMap map;
    private List<Transform> walls;
    private List<Transform> shelves;

    public ImprovedAStar(Transform[] shelves, Transform[] walls, Transform floor, float tileSize
        , GameObject tilePrefab, GameObject tileGoalPrefab)
    {
        this.walls = walls.ToList();
        this.shelves = shelves.ToList();
        map = new TileMap(floor, shelves, walls, tileSize, tilePrefab, tileGoalPrefab);
    }



    public class Node
    {
        public int x;
        public int y;
        public float f;
        public float g;
        public float h;
        public Node parent;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Robot
    {
        public GameObject robotGameObject;
        public List<Trip> trips;
        public PathRenderer pathRenderer;

        public Robot(GameObject robot, List<Trip> trips)
        {
            this.robotGameObject = robot;
            this.trips = trips;
        }
    }


    private List<Node> FindPath(int startX, int startY, int goalX, int goalY)
    {
        int width = map.tiles.GetLength(0);
        int height = map.tiles.GetLength(1);

        Node[,] nodes = new Node[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                nodes[x, y] = new Node(x, y);
            }
        }

        List<Node> openList = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        Node startNode = nodes[startX, startY];
        Node goalNode = nodes[goalX, goalY];

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].f < current.f || openList[i].f == current.f && openList[i].h < current.h)
                {
                    current = openList[i];
                }
            }

            openList.Remove(current);
            closedSet.Add(current);

            if (current == goalNode)
            {
                return GetPath(goalNode);
            }

            foreach (Node neighbor in GetNeighbors(current, nodes, width, height, map.tiles))
            {
                if (closedSet.Contains(neighbor))
                {
                    continue;
                }

                float tentativeGCost = current.g + GetDistance(current, neighbor);
                float r = GetDistance(neighbor, goalNode);
                float R = GetDistance(startNode, goalNode);

                if (!openList.Contains(neighbor) || tentativeGCost < neighbor.g)
                {
                    neighbor.g = tentativeGCost;
                    neighbor.h = GetDistance(neighbor, goalNode);
                    neighbor.f = neighbor.g + (1 + r / R) * neighbor.h; // Use improved cost function
                    neighbor.parent = current;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }







    private static List<Node> GetPath(Node goalNode)
    {
        List<Node> path = new List<Node>();
        Node current = goalNode;

        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private static List<Node> GetNeighbors(Node node, Node[,] nodes, int width, int height, int[,] map)
    {
        List<Node> neighbors = new List<Node>();

        for (int x = node.x - 1; x <= node.x + 1; x++)
        {
            for (int y = node.y - 1; y <= node.y + 1; y++)
            {
                if (x == node.x && y == node.y)
                {
                    continue;
                }

                if (x < 0 || x >= width || y < 0 || y >= height)
                {
                    continue;
                }

                if (map[x, y] == 1) // obstructed tile
                {
                    continue;
                }

                Node neighbor = nodes[x, y];
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private static float GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }



    public void StartTravelling(List<Robot> robots, Entry coroutineProvider)
    {
        coroutineProvider.StartCoroutine(MoveRobots(robots, 4, coroutineProvider));
    }


    private IEnumerator MoveRobots(List<Robot> robots, float moveSpeed, Entry coroutineProvider)
    {
        // Create a coroutine for each robot
        List<IEnumerator> coroutines = new List<IEnumerator>();
        foreach (Robot robot in robots)
        {
            IEnumerator coroutine = MoveRobot(robot, robots, coroutineProvider.pathPrefab);
            coroutines.Add(coroutine);
            coroutineProvider.StartCoroutine(coroutine);
        }

        // Wait for all coroutines to finish
        yield return new WaitForSeconds(0.1f);

        bool finished = false;
        while (!finished)
        {
            finished = true;

            // Check if any robot still has an unfinished path
            foreach (Robot robot in robots)
            {
                if (robot.trips.Count > 0)
                {
                    finished = false;
                    break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator MoveRobot(Robot robot, List<Robot> robots, GameObject pathPrefab)
    {
        // Loop through each trip in the list
        while (robot.trips.Count > 0)
        {
            Trip trip = robot.trips.First();

            int[] tileStart = null; int[] tileEnd = null;
            // from
            if (trip.from.CompareTag("Shelf"))
                tileStart = map.GetShelfTile(trip.from);
            else if (trip.from.CompareTag("ZoneLoad") || trip.from.CompareTag("ZoneUnload")
                || trip.from.CompareTag("Robot"))
                tileStart = map.XYToTile(trip.from.GetComponent<Renderer>().bounds.center.x
                    , trip.from.GetComponent<Renderer>().bounds.center.y);

            // to
            if (trip.to.CompareTag("Shelf"))
                tileEnd = map.GetShelfTile(trip.to);
            else if (trip.to.CompareTag("ZoneLoad") || trip.to.CompareTag("ZoneUnload"))
                tileEnd = map.XYToTile(trip.to.GetComponent<Renderer>().bounds.center.x
                    , trip.to.GetComponent<Renderer>().bounds.center.y);

            List<Node> path = FindPath(tileStart[0], tileStart[1], tileEnd[0], tileEnd[1]);

            // set up initial EDWA params
            //EDWA edwa = new EDWA(1, 0.3f, 1, 0.2f, 1f);
            EDWA edwa = new EDWA(1, 1f, 2, 0.1f, 1f);




            // draw path
            GameObject pathInstance = GameObject.Instantiate(pathPrefab);
            PathRenderer pr = pathInstance.GetComponent<PathRenderer>();
            pr.DrawPath(path, map);

            Vector2 currentVelocity = new Vector2();

            // Set the look-ahead distance
            float lookAheadDistance = 1.0f;

            // Loop through each node in the path
            for (int i = 0; i < path.Count; i++)
            {
                // Get the position of the current node
                //float[] pos = map.TileToXY(path[i].x, path[i].y);
                //Vector2 targetPosition = new Vector2(pos[0], pos[1]);

                int lookAheadIndex = i;
                Vector2 currPos = new Vector2(robot.robotGameObject.transform.position.x, robot.robotGameObject.transform.position.y);

                while (lookAheadIndex + 1 < path.Count)
                {
                    float[] lookAheadPos = map.TileToXY(path[lookAheadIndex + 1].x, path[lookAheadIndex + 1].y);
                    Vector2 nextLookAheadPosition = new Vector2(lookAheadPos[0], lookAheadPos[1]);
                    if (Vector2.Distance(currPos, nextLookAheadPosition) < lookAheadDistance)
                    {
                        lookAheadIndex++;
                    }
                    else
                    {
                        break;
                    }
                }

                // Update target position to the new lookahead point
                float[] pos = map.TileToXY(path[lookAheadIndex].x, path[lookAheadIndex].y);
                Vector2 targetPosition = new Vector2(pos[0], pos[1]);





                var gtt = map.XYToTile(targetPosition.x, targetPosition.y);
                GameObject gt = map.DrawGoalTile(gtt[0], gtt[1]);

                // Move the robot towards the target position
                while (Vector2.Distance(robot.robotGameObject.transform.position, targetPosition) > 0.02f)
                {
                    Vector2 currentPosition = new Vector2(robot.robotGameObject.transform.position.x
                        , robot.robotGameObject.transform.position.y);
                    float currentOrientation = GetAngle(robot);

                    currentVelocity = edwa.GetVelocityCommand(currentPosition, currentOrientation
                        , currentVelocity, targetPosition, walls.Union(shelves).Union(robots.Select(r => r.robotGameObject.transform)).ToList());

                    robot.robotGameObject.transform.position += (Vector3)(currentVelocity * Time.deltaTime);
                    float rotationAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                    robot.robotGameObject.transform.rotation = Quaternion.Euler(0, 0, rotationAngle);

                    yield return null;
                }

                GameObject.DestroyImmediate(gt);

                // Pause for a short time at each node in the path
                yield return null;// return new WaitForSeconds(0.1f);
            }


            robot.trips.RemoveAt(0);
            GameObject.DestroyImmediate(pathInstance);
        }
    }


    private float GetAngle(Robot robot)
    {
        // Get the forward direction of the robot
        Vector2 forwardDirection = robot.robotGameObject.transform.up; // Assuming the robot's forward direction is aligned with its up axis

        // Calculate the angle between the forward direction and the positive X-axis in degrees
        float angleDegrees = Vector2.Angle(Vector2.right, forwardDirection);

        // Check if the angle should be negative (clockwise) based on the orientation of the robot
        if (Vector3.Cross(forwardDirection, Vector2.right).z < 0)
        {
            angleDegrees = -angleDegrees;
        }

        // Convert the angle to radians
        return angleDegrees * Mathf.Deg2Rad;
    }
}
