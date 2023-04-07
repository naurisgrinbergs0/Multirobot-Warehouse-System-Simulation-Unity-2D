using Assets.Scripts.Map;
using Assets.Scripts.Robot;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Path_Planning
{
    public abstract class PathfindingAlgorithm
    {
        public MapBase map;
        public List<Tuple<RobotBase, List<Vector2>>> paths = new List<Tuple<RobotBase, List<Vector2>>>();
        public bool isFinished = false;

        public abstract void FindPaths(List<RobotBase> robots, MonoBehaviour coroutineProvider);
    }
}
