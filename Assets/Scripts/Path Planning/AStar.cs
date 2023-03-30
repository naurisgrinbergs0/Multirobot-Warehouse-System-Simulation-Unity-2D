using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    public TileMap map;

    public AStar(Transform[] shelves, Transform[] walls, Transform floor, float tileSize, GameObject tilePrefab, GameObject tileGoalPrefab)
    {
        map = new TileMap(floor, shelves, walls, tileSize, tilePrefab, tileGoalPrefab);
    }



    public class Node
    {
        public int x;
        public int y;
        public int f;
        public int g;
        public int h;
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
        public int tripNum;
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

                int tentativeGCost = current.g + GetDistance(current, neighbor);
                if (!openList.Contains(neighbor) || tentativeGCost < neighbor.g)
                {
                    neighbor.g = tentativeGCost;
                    neighbor.h = GetDistance(neighbor, goalNode);
                    neighbor.f = neighbor.g + neighbor.h;
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

    private static int GetDistance(Node a, Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
            IEnumerator coroutine = MoveRobot(robot, moveSpeed, coroutineProvider.pathPrefab);
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
                if (robot.tripNum < robot.trips.Count)
                {
                    finished = false;
                    break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator MoveRobot(Robot robot, float moveSpeed, GameObject pathPrefab)
    {
        // Loop through each trip in the list
        while (robot.tripNum < robot.trips.Count)
        {
            Trip trip = robot.trips[robot.tripNum];

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


            // draw path
            GameObject pathInstance = GameObject.Instantiate(pathPrefab);
            PathRenderer pr = pathInstance.GetComponent<PathRenderer>();
            pr.DrawPath(path, map);


            // Loop through each node in the path
            for (int i = 0; i < path.Count; i++)
            {
                // Get the position of the current node
                float[] pos = map.TileToXY(path[i].x, path[i].y);
                Vector3 targetPosition = new Vector3(pos[0], pos[1], robot.robotGameObject.transform.position.z);

                // Move the robot towards the target position
                while (Vector3.Distance(robot.robotGameObject.transform.position, targetPosition) > 0.01f)
                {
                    robot.robotGameObject.transform.position = Vector3.MoveTowards(robot.robotGameObject.transform.position
                        , targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                // Pause for a short time at each node in the path
                yield return new WaitForSeconds(0.1f);
            }
            robot.tripNum++;
            GameObject.DestroyImmediate(pathInstance);
        }
    }
}
