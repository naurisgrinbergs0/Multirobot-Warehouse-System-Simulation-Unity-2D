using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Path_Planning;
using Assets.Scripts.Robot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImprovedAStar : PathfindingAlgorithm
{
    public ImprovedAStar(TileMap map) : base(map)
    {
    }


    public class Node
    {
        public int xTile;
        public int yTile;
        public float f;
        public float g;
        public float h;
        public Node parent;

        public Node(int x, int y)
        {
            this.xTile = x;
            this.yTile = y;
        }
    }


    private List<Node> FindPath(int startX, int startY, int goalX, int goalY)
    {
        int width = ((TileMap)map).tiles.GetLength(0);
        int height = ((TileMap)map).tiles.GetLength(1);

        Node[,] nodes = new Node[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                nodes[x, y] = new Node(x, y);

        List<Node> openList = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        Node startNode = nodes[startX, startY];
        Node goalNode = nodes[goalX, goalY];

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = openList[0];
            for (int i = 1; i < openList.Count; i++)
                if (openList[i].f < current.f || openList[i].f == current.f && openList[i].h < current.h)
                    current = openList[i];

            openList.Remove(current);
            closedSet.Add(current);

            if (current == goalNode)
                return GetPath(goalNode);

            foreach (Node neighbor in GetNeighbors(current, nodes, width, height, ((TileMap)map).tiles))
            {
                if (closedSet.Contains(neighbor))
                    continue;

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
                        openList.Add(neighbor);
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

        for (int x = node.xTile - 1; x <= node.xTile + 1; x++)
        {
            for (int y = node.yTile - 1; y <= node.yTile + 1; y++)
            {
                if (x == node.xTile && y == node.yTile)
                    continue;

                if (x < 0 || x >= width || y < 0 || y >= height)
                    continue;

                if (map[x, y] == 1) // obstructed tile
                    continue;

                Node neighbor = nodes[x, y];
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private static float GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.xTile - b.xTile);
        int dy = Mathf.Abs(a.yTile - b.yTile);
        return Mathf.Sqrt(dx * dx + dy * dy);
    }



    public override void FindPaths(List<RobotBase> robots, Action callback = null)
    {
        base.FindPaths(robots);

        coroutineProvider.StartCoroutine(MoveRobots(robots.Select(r => (RobotImprovedAStar) r).ToList(), callback));
    }


    private IEnumerator MoveRobots(List<RobotImprovedAStar> robots, Action callback)
    {
        // loop while any unfinished trips left to any robot
        while (robots.Where(r => r.tripIndex < r.trips.Count).Count() > 0)
        {
            // go through each robot that has not finished the trip
            foreach (RobotImprovedAStar robot in robots.Where(r => r.tripIndex < r.trips.Count))
            {
                Trip trip = robot.trips[robot.tripIndex];

                int[] tripTiles = ((TileMap)map).GetTripTiles(trip);

                List<Node> path = FindPath(tripTiles[0], tripTiles[1], tripTiles[2], tripTiles[3]);

                // set up initial EDWA params
                EDWA edwa = new EDWA(2, 2f, 1, 0.1f, 1f);

                // draw path
                map.DrawPath(path.Select((Node n) => {
                    float[] xy = ((TileMap)map).TileToXY(n.xTile, n.yTile);
                    return new Vector2(xy[0], xy[1]);
                }).ToList(), robot); // problema

                Vector2 currentVelocity = new Vector2();

                // Set the look-ahead distance
                float lookAheadDistance = 1f;

                // Loop through each node in the path
                for (int i = 0; i < path.Count; i++)
                {
                    int lookAheadIndex = i;
                    Vector2 currPos = new Vector2(robot.position.x, robot.position.y);

                    while (lookAheadIndex + 1 < path.Count)
                    {
                        float[] lookAheadPos = ((TileMap)map).TileToXY(path[lookAheadIndex + 1].xTile, path[lookAheadIndex + 1].yTile);
                        Vector2 nextLookAheadPosition = new Vector2(lookAheadPos[0], lookAheadPos[1]);
                        if (Vector2.Distance(currPos, nextLookAheadPosition) < lookAheadDistance)
                            lookAheadIndex++;
                        else
                            break;
                    }

                    // Update target position to the new lookahead point
                    float[] pos = ((TileMap)map).TileToXY(path[lookAheadIndex].xTile, path[lookAheadIndex].yTile);
                    Vector2 targetPosition = new Vector2(pos[0], pos[1]);

                    int cntr = 0;
                    // Move the robot towards the target position
                    while (Vector2.Distance(robot.position, targetPosition) > 0.02f)
                    {
                        Vector2 currentPosition = new Vector2(robot.position.x, robot.position.y);
                        float currentOrientation = GetAngle(robot);

                        currentVelocity = edwa.GetVelocityCommand(currentPosition, currentOrientation
                            , currentVelocity, targetPosition, ((TileMap)map).walls.Union(((TileMap)map).shelves)
                            .Union(robots.Select(r => r.robotTransform)).ToList());

                        robot.position += currentVelocity * 0.008f;
                        float rotationAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
                        robot.angle = rotationAngle;

                        cntr++;
                        if (cntr > 10)
                        {
                            map.DrawRobot(robot, trip.isCargoTrip);
                            cntr = 0;
                        }

                        yield return null;
                    }
                }
                robot.tripIndex++;
            }
            map.DrawDelay();
            yield return null;
        }
        if (callback != null)
            callback.Invoke();
    }

    private float GetAngle(RobotImprovedAStar robot)
    {
        // Convert the angle to radians
        float angleRadians = robot.angle * Mathf.Deg2Rad;

        return angleRadians;
    }
}
