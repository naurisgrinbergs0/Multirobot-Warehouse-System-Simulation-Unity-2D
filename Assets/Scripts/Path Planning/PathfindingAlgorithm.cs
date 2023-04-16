using Assets.Scripts.Map;
using Assets.Scripts.Robot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Assets.Scripts.Metrics;

namespace Assets.Scripts.Path_Planning
{
    public abstract class PathfindingAlgorithm
    {
        public MapBase map;
        public List<Tuple<RobotBase, List<Vector2>>> paths = new List<Tuple<RobotBase, List<Vector2>>>();

        public MonoBehaviour coroutineProvider;

        public PathfindingAlgorithm(MapBase map)
        {
            this.map = map;
        }

        public virtual void FindPaths(List<RobotBase> robots, Action callback = null)
        {
            // set robot positions to the start of the first trip
            foreach(RobotBase rb in robots)
                if (rb.trips.Count > 0)
                    rb.position = rb.trips.First().fromLinkedTransform.position;
        }
    }
}
