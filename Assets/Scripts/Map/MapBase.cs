using Assets.Scripts.Robot;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Map
{
    public abstract class MapBase
    {
        public Transform floor;
        public Transform[] shelves;
        public Transform[] walls;

        public GameObject robotPrefab;
        public GameObject pathPrefab;

        public bool drawGraphics = true;

        public abstract void DrawPath(List<Vector2> path, Color color);
        public void DrawRobot(RobotBase robot)
        {
            if (drawGraphics)
            {
                Thread.Sleep(50);
                robot.robotTransform.position = robot.position;
                Color c = new Color(robot.color.r, robot.color.g, robot.color.b, 0.6f);
                robot.robotTransform.gameObject.GetComponent<SpriteRenderer>().color = c;
            }
        }
    }
}
