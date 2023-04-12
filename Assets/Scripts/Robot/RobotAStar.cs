using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Robot
{
    public class RobotAStar : RobotBase
    {
        public RobotAStar(List<Trip> trips, Transform robotTransform, Color color)
            : base(trips, robotTransform, color) 
        {
        }
    }
}
