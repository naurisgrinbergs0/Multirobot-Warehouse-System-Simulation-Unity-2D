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
        public GameObject robotPathPrefab;

        public bool drawGraphics = true;

        public void DrawPath(List<Vector2> path, RobotBase robot)
        {
            if (drawGraphics)
            {
                if(robot.robotPathGameObject == null)
                    robot.robotPathGameObject = GameObject.Instantiate(robotPathPrefab);
                LineRenderer lineRenderer = robot.robotPathGameObject.GetComponent<LineRenderer>();
                //lineRenderer.positionCount = 0;

                // Set the position count of the LineRenderer to the number of points in the path
                lineRenderer.positionCount = path.Count;

                // Set the positions of the LineRenderer to the points in the path
                for (int i = 0; i < path.Count; i++)
                {
                    Vector2 point = new Vector2(path[i].x, path[i].y);
                    lineRenderer.SetPosition(i, point);
                }

                // Customize the look of the LineRenderer
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.material.color = robot.color;
            }
        }
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
