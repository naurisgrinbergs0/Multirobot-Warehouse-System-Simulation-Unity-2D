using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Robot
{
    public abstract class RobotBase
    {
        public List<Trip> trips;
        public Vector2 position;
        public Transform robotTransform;
        public Color color = Color.red;
    }
}
