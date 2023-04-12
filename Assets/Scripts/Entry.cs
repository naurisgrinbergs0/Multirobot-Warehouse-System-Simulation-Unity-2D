using Assets.Scripts;
using Assets.Scripts.Map;
using Assets.Scripts.Path_Planning;
using Assets.Scripts.Robot;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public enum PathFindingAlgorithmEnum
    {
        AStar, ImporvedAStar, RERAPF, abc
    };
    public enum SimulationMode
    {
        Graphics, Metrics, MetricsAndGraphics
    }

    public PathFindingAlgorithmEnum pathFindingAlgorithmEnum = PathFindingAlgorithmEnum.RERAPF;
    public SimulationMode simulationMode = SimulationMode.Graphics;
    public int numberOfTrips = 1;

    public Transform floorGameTransform;
    public Transform shelfFloorTransform;
    public Transform robotFloorTransform;

    public Transform loadZoneTransform;
    public Transform unloadZoneTransform;

    public GameObject robotCargoPrefab;
    public GameObject robotPathPrefab;
    public GameObject robotPrefab;

    public GameObject tilePrefab;
    public GameObject goalTilePrefab;

    private Metrics metrics;
    private CSVExporter csvExporter;
    List<RobotBase> robots;

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
            case PathFindingAlgorithmEnum.ImporvedAStar:
                {
                    map = new TileMap(floorGameTransform, shelfGameObjects, wallGameObjects, tileSize);
                    ((TileMap)map).DrawObstructedTiles(tilePrefab);
                    break;
                }
        }
        map.robotPrefab = robotPrefab;
        map.robotPathPrefab = robotPathPrefab;
        map.robotCargoPrefab = robotCargoPrefab;
        map.drawGraphics = simulationMode == SimulationMode.Graphics || simulationMode == SimulationMode.MetricsAndGraphics;


        // make trips for robots
        int seed = System.Environment.TickCount;
        Random.InitState(seed);
        robots = new List<RobotBase>();
        foreach (Transform rt in robotGameObjects)
        {
            List<Trip> trips = Trip.GenerateTripList(rt, shelfGameObjects, loadZoneTransform, unloadZoneTransform, numberOfTrips);
            RobotBase robot = null;

            Color c = Random.ColorHSV();
            c.a = 0.3f;

            switch (pathFindingAlgorithmEnum)
            {
                case PathFindingAlgorithmEnum.RERAPF:
                    {
                        robot = new RobotRERAPF(trips, rt, c, (TileMap)map);
                        robot.trips.ForEach((Trip t) => {
                            int[] tt = ((TileMap)map).GetTripTiles(t);
                            float[] xy1 = ((TileMap)map).TileToXY(tt[0], tt[1]);
                            float[] xy2 = ((TileMap)map).TileToXY(tt[2], tt[3]);
                            t.from = new Vector2(xy1[0], xy1[1]);
                            t.to = new Vector2(xy2[0], xy2[1]);
                        });
                        break;
                    }
                case PathFindingAlgorithmEnum.ImporvedAStar:
                    {
                        robot = new RobotImprovedAStar(trips, rt, c);
                        robot.trips.ForEach((Trip t) => {
                            int[] tt = ((TileMap)map).GetTripTiles(t);
                            float[] xy1 = ((TileMap)map).TileToXY(tt[0], tt[1]);
                            float[] xy2 = ((TileMap)map).TileToXY(tt[2], tt[3]);
                            t.from = new Vector2(xy1[0], xy1[1]);
                            t.to = new Vector2(xy2[0], xy2[1]);
                        });
                        break;
                    }
            }
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
            case PathFindingAlgorithmEnum.ImporvedAStar:
                {
                    pathFindingAlgorithm = new ImprovedAStar((TileMap)map);
                    break;
                }
        }
        pathFindingAlgorithm.coroutineProvider = this;

        // set up csv exporter
        csvExporter = new CSVExporter("C://temp/Metrics.csv");
        csvExporter.CreateTableIfNotExists(new[] { "Algorithm", "NumOfRobots", "Efficiency", "UsedMemory", "Optimality", "Smoothness" });

        // set up metrics or simulate right away
        if (simulationMode == SimulationMode.Metrics || simulationMode == SimulationMode.MetricsAndGraphics)
        {
            // set up metrics
            metrics = new Metrics(pathFindingAlgorithm);
            metrics.measurement = Metrics.Measurement.MemoryUsage;
            metrics.callback = SaveMemoryUsage;
            metrics.map = map;
            metrics.robots = robots;
            pathFindingAlgorithm.metrics = metrics;
        }
        else if (simulationMode == SimulationMode.Graphics)
        {
            // simulate
            pathFindingAlgorithm.FindPaths(robots);
        }
    }

    private void Update()
    {
        // calculate every measurement one by one
        if (metrics != null && metrics.measurement != Metrics.Measurement.None && !metrics.calculationInProgress)
        {
            ResetState();
            metrics.StartCalculation();
        }
    }


    private void ResetState()
    {
        // reset trips
        for(int i = 0; i < metrics.robots.Count; i++)
        {
            metrics.robots[i].tripIndex = 0;
            metrics.robots[i].robotTransform.position = (Vector3)metrics.robots[i].trips.First().from;
        }
    }


    private void SaveMemoryUsage()
    {
        metrics.StopCalculation();
        // store value
        Debug.Log($"Mem: {metrics.GetMemoryUsage()}");
        metrics.measurement = Metrics.Measurement.Efficiency;
        metrics.callback = SaveEfficiency;
    }

    private void SaveEfficiency()
    {
        metrics.StopCalculation();
        // store value
        Debug.Log($"Eff: {metrics.GetEfficiency()}");
        metrics.measurement = Metrics.Measurement.AverageSmoothness;
        metrics.callback = SaveAverageSmoothness;
    }

    private void SaveAverageSmoothness()
    {
        metrics.StopCalculation();
        // store value
        Debug.Log($"Smoo: {metrics.GetAverageSmoothness()}");
        metrics.measurement = Metrics.Measurement.None;
        SaveMetricsToFile();
    }

    private void SaveMetricsToFile()
    {
        csvExporter.AddRecord(new string[] {
            System.Enum.GetName(typeof(PathFindingAlgorithmEnum), pathFindingAlgorithmEnum), robots.Count.ToString()
            , metrics.GetEfficiency().ToString(), metrics.GetMemoryUsage().ToString()
            ,  metrics.GetAverageOptimality().ToString(), metrics.GetAverageSmoothness().ToString()
        });
    }
}
