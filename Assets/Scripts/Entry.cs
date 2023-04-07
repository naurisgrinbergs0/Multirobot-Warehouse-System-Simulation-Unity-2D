using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Path_Planning;
using Assets.Scripts.Robot;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public Transform floorGameTransform;
    public Transform shelfFloorTransform;
    public Transform robotFloorTransform;

    public Transform loadZoneTransform;
    public Transform unloadZoneTransform;

    public GameObject pathPrefab;
    public GameObject robotPrefab;

    public GameObject tilePrefab;
    public GameObject goalTilePrefab;

    public enum PathFindingAlgorithmEnum
    {
        AStar,
        ImporvedAStar,
        RERAPF,
        abc
    };
    public PathFindingAlgorithmEnum pathFindingAlgorithmEnum = PathFindingAlgorithmEnum.RERAPF;

    private void Start()
    {
        Transform[] wallGameObjects = floorGameTransform.GetComponent<WallGenerator>().GenerateWalls().Select(go => go.transform).ToArray();
        Transform[] shelfGameObjects = shelfFloorTransform.GetComponent<ShelfGenerator>().GenerateShelves().Select(go => go.transform).ToArray();
        Transform[] robotGameObjects = robotFloorTransform.GetComponent<RobotGenerator>().GenerateRobots().Select(go => go.transform).ToArray();

        // determine the size of the tiles based on the smaller of the robot and shelf sizes
        float tileSize = Mathf.Min(robotGameObjects[0].GetComponent<SpriteRenderer>().bounds.size.x / 2f
            , robotGameObjects[0].GetComponent<SpriteRenderer>().bounds.size.y / 2f);

        // make a map
        MapBase map = null;
        switch (pathFindingAlgorithmEnum)
        {
            case PathFindingAlgorithmEnum.RERAPF:
                {
                    map = new TileMap(floorGameTransform, shelfGameObjects, wallGameObjects, tileSize);
                    ((TileMap)map).DrawObstructedTiles(tilePrefab);
                    break;
                }
        }
        map.robotPrefab = robotPrefab;
        map.pathPrefab = pathPrefab;


        // make trips for robots
        List<RobotBase> robots = new List<RobotBase>();
        foreach (Transform rt in robotGameObjects)
        {
            List<Trip> trips = Trip.GenerateTripList(rt, shelfGameObjects, loadZoneTransform, unloadZoneTransform, 1);
            RobotBase robot = null;

            switch (pathFindingAlgorithmEnum)
            {
                case PathFindingAlgorithmEnum.RERAPF:
                    {
                        robot = new RobotRERAPF(trips, (TileMap)map);
                        robot.color = Random.ColorHSV();
                        robot.color.a = 0.3f;
                        break;
                    }
            }

            robot.robotTransform = rt;
            robots.Add(robot);
        }

        // set up algorithm
        PathfindingAlgorithm pathFindingAlgorithm = null;
        switch (pathFindingAlgorithmEnum)
        {
            default:
            case PathFindingAlgorithmEnum.RERAPF:
                {
                    pathFindingAlgorithm = new RERAPF((TileMap)map);
                    break;
                }
        }

        // measure metrics
        Metrics metrics = new Metrics(pathFindingAlgorithm);
        metrics.SetMap(map);
        metrics.SetRobots(robots);
        metrics.SetCoroutineProvider(this);
        var efficiencyMillis = metrics.CalculateEficiency();

        //// Execute algorithm again and draw paths
        //pathFindingAlgorithm.FindPaths(robots, this);
    }

}
