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

    public bool runRERAPF = true;
    public bool runImprovedAStar = true;

    public SimulationMode simulationMode = SimulationMode.Graphics;
    public int numberOfTrips = 1;
    public int delayBetweenStepsInMillis = 40;
    public int[] numOfRobots = new[] { 2, 5, 10 };
    public int shelvesHorizontal = 5;
    public int shelvesVertical = 5;
    public float tileSize = 0.5f;

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
    private List<RobotBase> robots = new List<RobotBase>();
    private MapBase map;

    private Transform[] wallGameObjects = new Transform[] { };
    private Transform[] shelfGameObjects = new Transform[] { };
    private Transform[] robotGameObjects = new Transform[] { };

    private List<PathFindingAlgorithmEnum> algorithmEnums = new List<PathFindingAlgorithmEnum>();
    private int currentAlgorithmEnumIndex = 0;
    private int currentNumOfRobotsIndex = 0;
    private bool algorithmRunning = false;
    private bool simulationFinished = false;

    private void Start()
    {
        wallGameObjects = floorGameTransform.GetComponent<WallGenerator>().GenerateWalls().Select(go => go.transform).ToArray();

        if(runRERAPF)
            algorithmEnums.Add(PathFindingAlgorithmEnum.RERAPF);
        if (runImprovedAStar)
            algorithmEnums.Add(PathFindingAlgorithmEnum.ImporvedAStar);

        CreateMap(shelvesHorizontal, shelvesVertical);
        CreateRobots();
    }

    private void Update()
    {
        if (!simulationFinished && !algorithmRunning)
        {
            algorithmRunning = true;
            ExecuteAlgorithm();
        }
    }


    private void CreateMap(int numOfShelvesHorizontal, int numOfShelvesVertical)
    {
        // destroy old shelf objects
        foreach (Transform sgo in shelfGameObjects)
            GameObject.Destroy(sgo.gameObject);

        shelfGameObjects = shelfFloorTransform.GetComponent<ShelfGenerator>()
            .GenerateShelves(numOfShelvesHorizontal, numOfShelvesVertical).Select(go => go.transform).ToArray();

        // make a map
        map = null;
        switch (algorithmEnums[currentAlgorithmEnumIndex])
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
        map.delayMilliseconds = delayBetweenStepsInMillis;
        map.drawGraphics = simulationMode == SimulationMode.Graphics || simulationMode == SimulationMode.MetricsAndGraphics;
    }

    private void CreateRobots()
    {
        // destroy old robot game objects & cargo & paths
        foreach (RobotBase r in robots)
        {
            GameObject.Destroy(r.robotTransform.gameObject);
            GameObject.Destroy(r.robotCargoGameObject);
            GameObject.Destroy(r.robotPathGameObject);
        }

        // create new game objects
        robotGameObjects = robotFloorTransform.GetComponent<RobotGenerator>()
            .GenerateRobots(numOfRobots[currentNumOfRobotsIndex]).Select(go => go.transform).ToArray();

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

            switch (algorithmEnums[currentAlgorithmEnumIndex])
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
    }

    private void ExecuteAlgorithm()
    {
        // set up algorithm
        PathfindingAlgorithm pathFindingAlgorithm = null;
        switch (algorithmEnums[currentAlgorithmEnumIndex])
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
        csvExporter.CreateTableIfNotExists(new[] { "Algorithm", "NumOfRobots", "ExecutionTime"
            , "UsedMemory"/*, "Optimality"*/, "Smoothness" });

        // set up metrics or simulate right away
        if (simulationMode == SimulationMode.Metrics || simulationMode == SimulationMode.MetricsAndGraphics)
        {
            // set up metrics
            metrics = new Metrics(pathFindingAlgorithm);
            metrics.robots = robots;
            metrics.StartCalculation(SaveMetrics);
        }
        else if (simulationMode == SimulationMode.Graphics)
        {
            // simulate
            pathFindingAlgorithm.FindPaths(robots);
        }
    }



    private void SaveMetrics()
    {
        csvExporter.AddRecord(new string[] {
            System.Enum.GetName(typeof(PathFindingAlgorithmEnum), algorithmEnums[currentAlgorithmEnumIndex]), robots.Count.ToString()
            , metrics.GetExecutionTime().ToString(), metrics.GetMemoryUsage().ToString()
            /*, metrics.GetAverageOptimality().ToString()*/, metrics.GetAverageSmoothness().ToString()
        });
        PrepareForNextExecution();
    }

    private void PrepareForNextExecution()
    {
        // progress to next execution starting state
        // first go through each algorithm with the same number of robots
        // then change num of robots
        if (currentAlgorithmEnumIndex == algorithmEnums.Count - 1)
        {
            if (currentNumOfRobotsIndex == numOfRobots.Length - 1)
            {
                simulationFinished = true;
                return;
            }
            currentNumOfRobotsIndex++;
            currentAlgorithmEnumIndex = 0;
            CreateRobots();
            algorithmRunning = false;
            return;
        }
        currentAlgorithmEnumIndex++;

        // reset robot states
        foreach (RobotBase r in robots)
            r.ResetState();

        algorithmRunning = false;
    }
}
