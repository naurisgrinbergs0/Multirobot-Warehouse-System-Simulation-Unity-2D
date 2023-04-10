using Assets.Scripts.Map;
using Assets.Scripts.Path_Planning;
using Assets.Scripts.Robot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Assets.Scripts
{
    public class Metrics
    {
        public enum Measurement
        {
            Efficiency, MemoryUsage, AverageOptimality, AverageSmoothness, None
        }

        public Metrics(PathfindingAlgorithm algorithm)
        {
            this.algorithm = algorithm;
        }

        public Measurement measurement;
        public Action callback;

        private PathfindingAlgorithm algorithm;
        public List<RobotBase> robots;
        public MapBase map;


        #region Measurements

        private Stopwatch stopwatch;
        private double? efficiency = null;
        public double? GetEfficiency()
        {
            return efficiency;
        }

        private long? memory = null;
        private long memoryStart;
        public double? GetMemoryUsage()
        {
            return memory;
        }

        private float? averageSmoothness = null;
        public float? GetAverageSmoothness()
        {
            return averageSmoothness;
        }

        private AStar astar;
        private float? averageOptimality = null;
        public float? GetAverageOptimality()
        {
            return averageOptimality;
        }

        public bool calculationInProgress = false;

        #endregion Measurements


        public void StartCalculation()
        {
            calculationInProgress = true;
            switch (measurement)
            {
                default:
                case Measurement.Efficiency:
                    StartEficiencyCalculation();
                    break;
                case Measurement.MemoryUsage:
                    StartMemoryUsageCalculation();
                    break;
                case Measurement.AverageSmoothness:
                    StartAverageSmoothnessCalculation();
                    break;
            }
        }

        public void StopCalculation()
        {
            switch (measurement)
            {
                default:
                case Measurement.Efficiency:
                    StopEfficiencyCalculation();
                    break;
                case Measurement.MemoryUsage:
                    StopMemoryUsageCalculation();
                    break;
                case Measurement.AverageSmoothness:
                    StopAverageSmoothnessCalculation();
                    break;
            }
            calculationInProgress = false;
        }

        private void StartMemoryUsageCalculation()
        {
            // Collect garbage before measuring memory usage
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            memoryStart = Profiler.GetTotalAllocatedMemoryLong();

            // Run the pathplanning algo
            algorithm.FindPaths(robots);
        }
        public void StopMemoryUsageCalculation()
        {
            memory = Profiler.GetTotalAllocatedMemoryLong() - memoryStart;
        }

        private void StartEficiencyCalculation()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            // Run the path planning algorithm
            algorithm.FindPaths(robots);
        }
        public void StopEfficiencyCalculation()
        {
            stopwatch.Stop();
            efficiency = stopwatch.Elapsed.TotalSeconds;
        }

        private void StartAverageOptimalityCalculation()
        {
            // run shortes path calculation algorithm
            astar = new AStar((TileMap)map);
            astar.FindPaths(robots); // find shortest paths - use A*
            algorithm.FindPaths(robots); // find paths - use current algorithm
        }
        private void StopAverageOptimalityCalculation()
        {
            float averageOptimality = 0;

            // go through each robot's path
            for (int n = 0; n < astar.paths.Count; n++)
            {
                float shortestPathLength = 0f;
                for (int i = 0; i < astar.paths[n].Item2.Count - 1; i++)
                    shortestPathLength += Vector2.Distance(astar.paths[n].Item2[i], astar.paths[n].Item2[i + 1]);

                float currentPathLength = 0f;
                for (int i = 0; i < algorithm.paths[n].Item2.Count - 1; i++)
                    currentPathLength += Vector2.Distance(algorithm.paths[n].Item2[i], algorithm.paths[n].Item2[i + 1]);

                // compare the shortest path length to the algorithm's path length [0 - 1]
                averageOptimality += shortestPathLength / currentPathLength;
            }

            this.averageOptimality = averageOptimality / robots.Count;
        }

        private void StartAverageSmoothnessCalculation()
        {
            algorithm.FindPaths(robots);
        }
        private void StopAverageSmoothnessCalculation()
        {
            float smoothnessSum = 0f;

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
                        smoothness += diff / length;
                }
                smoothnessSum += smoothness;
            }

            this.averageSmoothness = smoothnessSum / algorithm.paths.Count;
        }
    }
}
