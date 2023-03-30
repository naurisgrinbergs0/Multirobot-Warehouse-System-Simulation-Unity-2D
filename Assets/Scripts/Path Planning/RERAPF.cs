using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Path_Planning
{
    class RERAPF
    {
        private const float ROBOT_REPULSION_FACTOR = 1f;
        private const float OBSTACLE_REPULSION_FACTOR = 0.1f;
        private const float GOAL_ATTRACTION_FACTOR = 10f;

        private const float EXCITATION_FACTOR = 5f;
        private const float RELAXATION_FACTOR = 0.5f;

        private const float OBSTACLE_INFLUENCE_RADIUS = 5f;
        private const float ROBOT_INFLUENCE_RADIUS = 5f;

        public TileMap map;

        public RERAPF(Transform[] shelves, Transform[] walls, Transform floor, float tileSize, GameObject tilePrefab)
        {
            map = new TileMap(floor, shelves, walls, tileSize, tilePrefab);
        }


        public class Robot
        {
            public class ExploredTile
            {
                public int[] tile;
                float? startStaticPotential = null;
                float prevStaticPotential;
                
                public float? GetStartStaticPotential() { return startStaticPotential; }
                public float GetPrevStaticPotential() { return prevStaticPotential; }
                public void UpdateStaticPotential(float pot) { if (startStaticPotential == null) startStaticPotential = pot; prevStaticPotential = pot; }
                public ExploredTile(int x, int y, float staticPotential)
                {
                    tile = new int[]{ x, y };
                    UpdateStaticPotential(staticPotential);
                }
            }

            public void AddExploredTile(int x, int y, float staticPotential)
            {
                exploredTiles.Add(new ExploredTile(x, y, staticPotential));
            }

            public GameObject robotGameObject;
            public List<Trip> trips;
            public PathRenderer pathRenderer;
            public List<ExploredTile> exploredTiles = new List<ExploredTile>();
            public int[] currentPosition;

            public Robot(GameObject robot, List<Trip> trips)
            {
                this.robotGameObject = robot;
                this.trips = trips;
            }
        }



        public void StartTravelling(List<Robot> robots, Entry coroutineProvider)
        {
            // set current position of each robot
            foreach (Robot r in robots)
            {
                r.currentPosition = map.XYToTile(r.robotGameObject.GetComponent<Renderer>().bounds.center.x
                    , r.robotGameObject.GetComponent<Renderer>().bounds.center.y);
            }

            coroutineProvider.StartCoroutine(MoveRobots(robots, 2f, coroutineProvider));
        }


        private IEnumerator MoveRobots(List<Robot> robots, float moveSpeed, Entry coroutineProvider)
        {
            // Create a coroutine for each robot
            List<IEnumerator> coroutines = new List<IEnumerator>();
            foreach (Robot robot in robots)
            {
                IEnumerator coroutine = MoveRobot(robot, moveSpeed, robots);
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


        public IEnumerator MoveRobot(Robot robot, float speed, List<Robot> robots)
        {
            // loop until robot has reached the goal
            while (robot.trips.Count() > 0)
            {
                // if goal is not reached
                int[] tripTiles = map.GetTripTiles(robot.trips.First());
                map.DrawTile(tripTiles[2], tripTiles[3]);
                if (robot.currentPosition[0] != tripTiles[2] || robot.currentPosition[1] != tripTiles[3])
                {
                    // artificial potential field pathfinding
                    int[] nextTile = FindNextTile(robot, tripTiles, robots);

                    if (nextTile == null)
                        break;

                    robot.currentPosition = nextTile;

                    float[] pos = map.TileToXY(nextTile[0], nextTile[1]);

                    // Get the position of the current node
                    Vector3 targetPosition = new Vector3(pos[0], pos[1], robot.robotGameObject.transform.position.z);

                    // Move the robot towards the target position
                    while (Vector3.Distance(robot.robotGameObject.transform.position, targetPosition) > 0.01f)
                    {
                        robot.robotGameObject.transform.position = Vector3.MoveTowards(robot.robotGameObject.transform.position
                            , targetPosition, speed * Time.deltaTime);
                        yield return null;
                    }

                    // if goal reached - remove trip & delete visited tiles
                    if(nextTile[0] == tripTiles[2] && nextTile[1] == tripTiles[3])
                    {
                        robot.trips.RemoveAt(0);
                        robot.exploredTiles.Clear();
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }




        private int[] FindNextTile(Robot robot, int[] tripTiles, List<Robot> robots)
        {
            // if no trips planned - exit
            if (robot.trips.Count == 0)
                return null;

            // calculate artificial potential field for each neighboring tile
            float[,] apf = new float[3, 3];
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    int x = robot.currentPosition[0] + i;
                    int y = robot.currentPosition[1] + j;

                    // skip the tiles outside of the map
                    if (x < 0 || x >= map.tiles.GetLength(0) || y < 0 || y >= map.tiles.GetLength(1))
                    {
                        apf[i + 1, j + 1] = float.PositiveInfinity;
                        continue;
                    }
                    // skip obstacle tiles
                    if (map.tiles[x, y] == 1)
                    {
                        apf[i + 1, j + 1] = float.PositiveInfinity;
                        continue;
                    }

                    List<Robot.ExploredTile> exploredTileArr = robot.exploredTiles.Where(ep => ep.tile[0] == x && ep.tile[1] == y).ToList();
                    Robot.ExploredTile exploredTile = null;
                    if (exploredTileArr.Count > 0)
                        exploredTile = exploredTileArr[0];

                    float staticPotential = 0, dynamicPotential = 0;
                    // if tile has not been visited
                    if (exploredTile == null)
                    {
                        // calculate static component potential (goal & obstacle components)
                        staticPotential = CalculateGoalPotential(x, y, tripTiles[2], tripTiles[3])
                            + CalculateObstaclePotential(x, y, map.tiles);
                        // add tile to explored tile list
                        robot.AddExploredTile(x, y, staticPotential);
                    }
                    // if this is current tile
                    else if (i == 0 && j == 0)
                    {
                        // calculate static component potential (goal & obstacle components)
                        // use excitation formula
                        staticPotential = Excitation(exploredTile);
                        exploredTile.UpdateStaticPotential(staticPotential);
                    }
                    // if tile has been visited
                    else
                    {
                        // calculate static component potential (goal & obstacle components)
                        // use relaxation formula
                        staticPotential = Relaxation(exploredTile);
                        exploredTile.UpdateStaticPotential(staticPotential);
                    }
                    dynamicPotential = CalculateRobotPotential(x, y, robots);

                    // add static & dynamic potentials together
                    apf[i + 1, j + 1] = staticPotential + dynamicPotential;
                }
            }

            // find the neighboring tile with the lowest potential
            int[] nextTile = new int[] { robot.currentPosition[0], robot.currentPosition[1] };
            float lowestPotential = float.MaxValue;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    // skip the current tile
                    if (i == 0 && j == 0)
                    {
                        continue;
                    }

                    float potential = apf[i + 1, j + 1];
                    if (potential < lowestPotential)
                    {
                        nextTile[0] = robot.currentPosition[0] + i;
                        nextTile[1] = robot.currentPosition[1] + j;
                        lowestPotential = potential;
                    }
                }
            }

            return nextTile;
        }

        private static float CalculateGoalPotential(int xCurr, int yCurr, int xGoal, int yGoal)
        {
            float distanceToGoal = Mathf.Sqrt(Mathf.Pow(xCurr - xGoal, 2) + Mathf.Pow(yCurr - yGoal, 2));

            // calculate goal attractive potential
            return distanceToGoal * GOAL_ATTRACTION_FACTOR;
        }

        private static float CalculateObstaclePotential(int xCurr, int yCurr, int[,] tiles)
        {
            float obstaclePotential = 0;

            // Loop through each tile in the map
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    // If the tile is obstructed
                    if (tiles[i, j] == 1)
                    {
                        // Calculate the distance to the obstacle
                        float distanceToObstacle = Vector2.Distance(new Vector2(xCurr, yCurr), new Vector2(i, j));

                        // If the distance is within the influence radius
                        if (distanceToObstacle < OBSTACLE_INFLUENCE_RADIUS)
                        {
                            // Calculate the obstacle potential
                            float obstacleInfluence = 1 - distanceToObstacle / OBSTACLE_INFLUENCE_RADIUS;
                            obstaclePotential += obstacleInfluence * (1 / distanceToObstacle - 1 / OBSTACLE_INFLUENCE_RADIUS);
                        }
                    }
                }
            }

            return obstaclePotential * OBSTACLE_REPULSION_FACTOR;
        }

        private static float CalculateRobotPotential(int xCurr, int yCurr, List<Robot> robots)
        {
            float robotPotential = 0;

            // Loop through each robot in the map
            foreach (Robot r in robots)
            {
                // Calculate the distance to the robot
                float distanceToRobot = Vector2.Distance(new Vector2(xCurr, yCurr), new Vector2(r.currentPosition[0], r.currentPosition[1]));

                // If the distance is within the influence radius
                if (distanceToRobot < ROBOT_INFLUENCE_RADIUS)
                {
                    // Calculate the robot potential
                    float robotInfluence = 1 - distanceToRobot / ROBOT_INFLUENCE_RADIUS;
                    robotPotential += robotInfluence * (1 / distanceToRobot - 1 / ROBOT_INFLUENCE_RADIUS);
                }
            }

            return robotPotential * ROBOT_REPULSION_FACTOR;
        }


        private static float Excitation(Robot.ExploredTile exploredTile)
        {
            return EXCITATION_FACTOR * exploredTile.GetPrevStaticPotential();
        }

        private static float Relaxation(Robot.ExploredTile exploredTile)
        {
            if (exploredTile.GetStartStaticPotential() == null)
                throw new Exception("Start static potential is null! Cannot complete relaxation value calculation!");
            return (1 - RELAXATION_FACTOR) * exploredTile.GetPrevStaticPotential() + EXCITATION_FACTOR * (float)exploredTile.GetStartStaticPotential();
        }

    }
}
