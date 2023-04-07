using Assets.Scripts.Map;
using Assets.Scripts.Robot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Path_Planning
{
    class RERAPF : PathfindingAlgorithm
    {
        private const float ROBOT_REPULSION_FACTOR = 120f;
        private const float OBSTACLE_REPULSION_FACTOR = 5f;
        private const float GOAL_ATTRACTION_FACTOR = 10f;

        private const float EXCITATION_FACTOR = 5f;
        private const float RELAXATION_FACTOR = 0.5f;

        private const float OBSTACLE_INFLUENCE_RADIUS = 2f;
        private const float ROBOT_INFLUENCE_RADIUS = 3f;

        public RERAPF(TileMap map)
        {
            this.map = map;
        }


        public override void FindPaths(List<RobotBase> robots, MonoBehaviour coroutineProvider)
        {
            paths.AddRange(robots.Select(r => new Tuple<RobotBase, List<Vector2>>(r, new List<Vector2>())));

            coroutineProvider.StartCoroutine(MoveRobots(robots.Select(r => (RobotRERAPF)r).ToList(), coroutineProvider));
        }


        private IEnumerator MoveRobots(List<RobotRERAPF> robots, MonoBehaviour coroutineProvider)
        {
            // Create a coroutine for each robot
            List<IEnumerator> coroutines = new List<IEnumerator>();
            foreach (RobotRERAPF robot in robots)
            {
                IEnumerator coroutine = MoveRobot(robot, robots);
                coroutines.Add(coroutine);
                coroutineProvider.StartCoroutine(coroutine);
            }

            bool finished = false;
            while (!finished)
            {
                finished = true;

                // Check if any robot still has an unfinished path
                foreach (RobotRERAPF robot in robots)
                {
                    if (robot.trips.Count > 0)
                    {
                        finished = false;
                        break;
                    }
                }

                yield return null;
            }
            isFinished = true;
        }


        public IEnumerator MoveRobot(RobotRERAPF robot, List<RobotRERAPF> robots)
        {
            // loop until robot has reached the goal
            //bool newGoal = true;
            while (robot.trips.Count() > 0)
            {
                // if goal is not reached
                int[] tripTiles = ((TileMap)map).GetTripTiles(robot.trips.First());
                /*
                if (newGoal)
                {
                    if (robot.goalTileGameObject)
                        UnityEngine.Object.Destroy(robot.goalTileGameObject);
                    robot.goalTileGameObject = map.DrawGoalTile(tripTiles[2], tripTiles[3]);
                }
                newGoal = false;
                */
                if (robot.currentTile[0] != tripTiles[2] || robot.currentTile[1] != tripTiles[3])
                {
                    // artificial potential field pathfinding
                    int[] nextTile = FindNextTile(robot, tripTiles, robots);

                    if (nextTile == null)
                        break;

                    robot.currentTile = nextTile;

                    float[] pos = ((TileMap)map).TileToXY(nextTile[0], nextTile[1]);

                    // Move the robot to the target position
                    robot.position = new Vector2(pos[0], pos[1]);

                    // if goal reached - remove trip & delete visited tiles
                    if (nextTile[0] == tripTiles[2] && nextTile[1] == tripTiles[3])
                    {
                        robot.trips.RemoveAt(0);
                        robot.exploredTiles.Clear();
                        //newGoal = true;
                    }
                    map.DrawRobot(robot);
                    paths.Where(p => p.Item1 == robot).First().Item2.Add(robot.position);
                    map.DrawPath(paths.Where(p => p.Item1 == robot).First().Item2, robot.color);
                }
                yield return null;
            }
            yield return null;
        }




        private int[] FindNextTile(RobotRERAPF robot, int[] tripTiles, List<RobotRERAPF> robots)
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
                    int x = robot.currentTile[0] + i;
                    int y = robot.currentTile[1] + j;

                    // skip the tiles outside of the map
                    if (x < 0 || x >= ((TileMap)map).tiles.GetLength(0) || y < 0 || y >= ((TileMap)map).tiles.GetLength(1))
                    {
                        apf[i + 1, j + 1] = float.PositiveInfinity;
                        continue;
                    }
                    // skip obstacle tiles
                    if (((TileMap)map).tiles[x, y] == 1)
                    {
                        apf[i + 1, j + 1] = float.PositiveInfinity;
                        continue;
                    }

                    List<RobotRERAPF.ExploredTile> exploredTileArr = robot.exploredTiles.Where(ep => ep.tile[0] == x && ep.tile[1] == y).ToList();
                    RobotRERAPF.ExploredTile exploredTile = null;
                    if (exploredTileArr.Count > 0)
                        exploredTile = exploredTileArr[0];

                    // THIS BASICALLY RECALCULATES PREVIOUS STATIC POTENTIAL FOR EVERY EXPLORED TILE
                    // THIS MAY BE FAULTY AND FOR NOW DOES NOT SEEM TO BE INCLUDED IN THE PSEUDO-CODE
                    foreach(RobotRERAPF.ExploredTile et in robot.exploredTiles)
                        et.UpdateStaticPotential(Relaxation(et));

                    float staticPotential = 0, dynamicPotential = 0;
                    // if tile has not been visited
                    if (exploredTile == null)
                    {
                        // calculate static component potential (goal & obstacle components)
                        staticPotential = CalculateGoalPotential(x, y, tripTiles[2], tripTiles[3])
                            + CalculateObstaclePotential(x, y, ((TileMap)map).tiles);
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
                    dynamicPotential = CalculateRobotPotential(x, y, robots.Where(r => r != robot).ToList());

                    // add static & dynamic potentials together
                    apf[i + 1, j + 1] = staticPotential + dynamicPotential;
                }
            }

            // find the neighboring tile with the lowest potential
            int[] nextTile = new int[] { robot.currentTile[0], robot.currentTile[1] };
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
                        nextTile[0] = robot.currentTile[0] + i;
                        nextTile[1] = robot.currentTile[1] + j;
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

        private static float CalculateRobotPotential(int xCurr, int yCurr, List<RobotRERAPF> robots)
        {
            float robotPotential = 0;

            // Loop through each robot in the map
            foreach (RobotRERAPF r in robots)
            {
                // Calculate the distance to the robot
                float distanceToRobot = Vector2.Distance(new Vector2(xCurr, yCurr), r.position);

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


        private static float Excitation(RobotRERAPF.ExploredTile exploredTile)
        {
            return EXCITATION_FACTOR * exploredTile.GetPrevStaticPotential();
        }

        private static float Relaxation(RobotRERAPF.ExploredTile exploredTile)
        {
            if (exploredTile.GetStartStaticPotential() == null)
                throw new Exception("Start static potential is null! Cannot complete relaxation value calculation!");
            return (1 - RELAXATION_FACTOR) * exploredTile.GetPrevStaticPotential() + EXCITATION_FACTOR * (float)exploredTile.GetStartStaticPotential();
        }

    }
}
