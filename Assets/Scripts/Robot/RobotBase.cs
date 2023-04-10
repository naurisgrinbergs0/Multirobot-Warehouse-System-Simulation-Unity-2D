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
        public int tripIndex = 0;
        public List<Trip> trips;
        public Vector2 position;
        public Transform robotTransform;
        public GameObject robotPathGameObject;
        public Color color = Color.red;

        public RobotBase(List<Trip> trips, Transform robotTransform, Color color)
        {
            this.trips = trips;
            this.robotTransform = robotTransform;
            this.color = color;
        }
    }
}
