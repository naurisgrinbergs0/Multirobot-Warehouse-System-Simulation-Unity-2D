//using Assets.Scripts;
//using Assets.Scripts.Map;
//using Assets.Scripts.Path_Planning;
//using Assets.Scripts.Robot;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class AStar : PathfindingAlgorithm
//{
//    public AStar(TileMap map) : base(map)
//    {
//    }



//    public class Node
//    {
//        public int xTile;
//        public int yTile;
//        public float f;
//        public float g;
//        public float h;
//        public Node parent;

//        public Node(int xTile, int yTile)
//        {
//            this.xTile = xTile;
//            this.yTile = yTile;
//        }
//    }



//    public override void FindPaths(List<RobotBase> robots)
//    {
//        coroutineProvider.StartCoroutine(MoveRobots(robots.Select(r => (RobotAStar)r).ToList()));
//    }

//    private IEnumerator MoveRobots(List<RobotAStar> robots)
//    {
//        // Create a coroutine for each robot
//        List<IEnumerator> coroutines = new List<IEnumerator>();
//        foreach (RobotAStar robot in robots)
//        {
//            IEnumerator coroutine = MoveRobot(robot);
//            coroutines.Add(coroutine);
//            coroutineProvider.StartCoroutine(coroutine);
//        }

//        bool finished = false;
//        while (!finished)
//        {
//            finished = true;

//            // Check if any robot still has an unfinished path
//            foreach (RobotAStar robot in robots)
//            {
//                if (robot.trips.Count > 0)
//                {
//                    finished = false;
//                    break;
//                }
//            }

//            yield return null;
//        }
//    }

//    private IEnumerator MoveRobot(RobotAStar robot)
//    {
//        // Loop through each trip in the list
//        while (robot.trips.Count > 0)
//        {
//            Trip trip = robot.trips.First();

//            int[] tripTiles = ((TileMap)map).GetTripTiles(trip);

//            List<Node> path = FindPath(tripTiles[0], tripTiles[1], tripTiles[2], tripTiles[3]);


//            // draw path
//            map.DrawPath(path.Select((Node n) => {
//                float[] xy = ((TileMap)map).TileToXY(n.xTile, n.yTile);
//                return new Vector2(xy[0], xy[1]); 
//            }).ToList(), robot); // problema


//            // Loop through each node in the path
//            for (int i = 0; i < path.Count; i++)
//            {
//                // Get the position of the current node
//                float[] pos = ((TileMap)map).TileToXY(path[i].xTile, path[i].yTile);
//                Vector2 targetPosition = new Vector2(pos[0], pos[1]);

//                // Move the robot to the target position
//                robot.position = targetPosition;

//                map.DrawRobot(robot, trip.isCargoTrip);
//                // Pause for a short time at each node in the path
//                yield return null;
//            }
//            robot.trips.RemoveAt(0);
//        }
//    }




//    private List<Node> FindPath(int startX, int startY, int goalX, int goalY)
//    {
            //int width = ((TileMap)map).tiles.GetLength(0);
            //int height = ((TileMap)map).tiles.GetLength(1);

            //Node[,] nodes = new Node[width, height];
            //for (int x = 0; x < width; x++)
            //    for (int y = 0; y < height; y++)
            //        nodes[x, y] = new Node(x, y);

            //List<Node> openList = new List<Node>();
            //HashSet<Node> closedSet = new HashSet<Node>();

            //Node startNode = nodes[startX, startY];
            //Node goalNode = nodes[goalX, goalY];

            //openList.Add(startNode);

            //while (openList.Count > 0)
            //{
            //    Node current = openList[0];
            //    for (int i = 1; i < openList.Count; i++)
            //        if (openList[i].f < current.f || openList[i].f == current.f && openList[i].h < current.h)
            //            current = openList[i];

            //    openList.Remove(current);
            //    closedSet.Add(current);

            //    if (current == goalNode)
            //        return GetPath(goalNode);

            //    foreach (Node neighbor in GetNeighbors(current, nodes, width, height, ((TileMap)map).tiles))
            //    {
            //        if (closedSet.Contains(neighbor))
            //            continue;

            //        float tentativeGCost = current.g + GetDistance(current, neighbor);
            //        if (!openList.Contains(neighbor) || tentativeGCost < neighbor.g)
            //        {
            //            neighbor.g = tentativeGCost;
            //            neighbor.h = GetDistance(neighbor, goalNode);
            //            neighbor.f = neighbor.g + neighbor.h;
            //            neighbor.parent = current;

            //            if (!openList.Contains(neighbor))
            //                openList.Add(neighbor);
            //        }
            //    }
            //}

            //return null;
//    }

//    private static List<Node> GetPath(Node goalNode)
//    {
//        List<Node> path = new List<Node>();
//        Node current = goalNode;

//        while (current != null)
//        {
//            path.Add(current);
//            current = current.parent;
//        }

//        path.Reverse();
//        return path;
//    }

//    private static List<Node> GetNeighbors(Node node, Node[,] nodes, int width, int height, int[,] map)
//    {
//        List<Node> neighbors = new List<Node>();

//        for (int x = node.xTile - 1; x <= node.xTile + 1; x++)
//        {
//            for (int y = node.yTile - 1; y <= node.yTile + 1; y++)
//            {
//                if (x == node.xTile && y == node.yTile)
//                    continue;

//                if (x < 0 || x >= width || y < 0 || y >= height)
//                    continue;

//                if (map[x, y] == 1) // obstructed tile
//                    continue;

//                Node neighbor = nodes[x, y];
//                neighbors.Add(neighbor);
//            }
//        }

//        return neighbors;
//    }

//    private static float GetDistance(Node a, Node b)
//    {
//        return Mathf.Abs((float)a.xTile - (float)b.xTile) + Mathf.Abs((float)a.yTile - (float)b.yTile);
//    }
//}
