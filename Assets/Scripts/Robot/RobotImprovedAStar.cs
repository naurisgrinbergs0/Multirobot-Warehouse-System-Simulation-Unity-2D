using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Robot
{
    public class RobotImprovedAStar : RobotBase
    {
        public float angle;

        public RobotImprovedAStar(List<Trip> trips, Transform robotTransform, Color color)
            : base(trips, robotTransform, color)
        {
        }
    }
}
