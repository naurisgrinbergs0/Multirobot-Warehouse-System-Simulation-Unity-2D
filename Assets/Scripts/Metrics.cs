using Assets.Scripts.Map;
using Assets.Scripts.Path_Planning;
using Assets.Scripts.Robot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Assets.Scripts
{
    public class Metrics
    {
        public Metrics(PathfindingAlgorithm algorithm)
        {
            this.algorithm = algorithm;
        }

        private PathfindingAlgorithm algorithm;
        private List<RobotBase> robots;
        private MapBase map;
        private MonoBehaviour coroutineProvider;

        public void SetRobots(List<RobotBase> robots)
        {
            this.robots = robots;
        }
        public void SetMap(MapBase map)
        {
            this.map = map;
        }
        public void SetCoroutineProvider(MonoBehaviour coroutineProvider)
        {
            this.coroutineProvider = coroutineProvider;
        }



        public long CalculateMemoryUsage()
        {
            // Collect garbage before measuring memory usage
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Start the process
            var process = Process.GetCurrentProcess();

            // Call the method and measure memory usage
            algorithm.FindPaths(robots, coroutineProvider);

            // Get the memory usage in bytes
            return process.PrivateMemorySize64;
        }

        public double CalculateEficiency()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Run the path planning algorithm
            algorithm.FindPaths(robots, coroutineProvider);

            stopwatch.Stop();

            // get efficiency in milliseconds
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        public float CalculateOptimality()
        {
            /*
            // calculate shortest path possible & get it's length
            AStar astar = new AStar();
            List<Vector2> shortestPath = astar.FindPath(robots, map);
            float shortestPathLength = 0f;
            for (int i = 0; i < shortestPath.Count - 1; i++)
                shortestPathLength += Vector2.Distance(shortestPath[i], shortestPath[i + 1]);

            // calculate current path possible & get it's length
            List<Vector2> currentPath = algorithm(robots, map);
            float currentPathLength = 0f;
            for (int i = 0; i < currentPath.Count - 1; i++)
                currentPathLength += Vector2.Distance(currentPath[i], currentPath[i + 1]);

            // compare the shortest path length to the algorithm's path length [0 - 1]
            return shortestPathLength / currentPathLength;*/
            return 0;
        }

        public float CalculateSmoothness()
        {
            algorithm.FindPaths(robots, coroutineProvider);

            float avgSmoothness = 0f;

            foreach (Tuple<RobotBase, List<Vector2>> path in algorithm.paths)
            {
                float smoothness = 0f;

                for (int i = 0; i < path.Item2.Count - 2; i++)
                {
                    Vector2 p0 = path.Item2[i];
                    Vector2 p1 = path.Item2[i + 1];
                    Vector2 p2 = path.Item2[i + 2];

                    float diff = (p1 - p0).sqrMagnitude + (p2 - p1).sqrMagnitude;
                    float length = (p2 - p0).sqrMagnitude;

                    if (length > 0f)
                    {
                        smoothness += diff / length;
                    }
                }
                avgSmoothness += smoothness;
            }

            return avgSmoothness / algorithm.paths.Count;
        }

    }
}
