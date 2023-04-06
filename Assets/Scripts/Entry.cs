using Assets.Scripts;
using Assets.Scripts.Path_Planning;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject shelfFloorPrefab;
    public GameObject robotFloorPrefab;

    public GameObject loadZonePrefab;
    public GameObject unloadZonePrefab;

    public GameObject pathPrefab;

    public GameObject tilePrefab;
    public GameObject goalTilePrefab;

    public enum PathFindingAlgorithm
    {
        AStar,
        ImporvedAStar,
        RERAPF,
        abc
    };
    public PathFindingAlgorithm pathFindingAlgorithm = PathFindingAlgorithm.AStar;

    private void Start()
    {
        GameObject[] wallGameObjects = floorPrefab.GetComponent<WallGenerator>().GenerateWalls();
        GameObject[] shelfGameObjects = shelfFloorPrefab.GetComponent<ShelfGenerator>().GenerateShelves();
        GameObject[] robotGameObjects = robotFloorPrefab.GetComponent<RobotGenerator>().GenerateRobots();

        // Determine the size of the tiles based on the smaller of the robot and shelf sizes
        float tileSize = Mathf.Min(robotGameObjects[0].GetComponent<SpriteRenderer>().bounds.size.x / 2f
            , robotGameObjects[0].GetComponent<SpriteRenderer>().bounds.size.y / 2f);


        switch (pathFindingAlgorithm)
        {
            default:
            case PathFindingAlgorithm.AStar:
                {
                    AStar astar = new AStar(shelfGameObjects.Select(sgo => sgo.transform).ToArray()
                        , wallGameObjects.Select(wgo => wgo.transform).ToArray(), floorPrefab.transform, tileSize, tilePrefab
                        , goalTilePrefab);

                    astar.map.DrawObstructedTiles();

                    // make trips for robots
                    List<AStar.Robot> robots = new List<AStar.Robot>();
                    foreach (GameObject rgo in robotGameObjects)
                    {
                        List<Trip> trips = Trip.GenerateTripList(rgo, shelfGameObjects, loadZonePrefab, unloadZonePrefab, 5);
                        AStar.Robot robot = new AStar.Robot(rgo, trips);

                        robots.Add(robot);
                    }
                    astar.StartTravelling(robots, this);
                    break;
                }
            case PathFindingAlgorithm.ImporvedAStar:
                {
                    ImprovedAStar improvedAStar = new ImprovedAStar(shelfGameObjects.Select(sgo => sgo.transform).ToArray()
                        , wallGameObjects.Select(wgo => wgo.transform).ToArray(), floorPrefab.transform, tileSize, tilePrefab
                        , goalTilePrefab);

                    improvedAStar.map.DrawObstructedTiles();

                    // make trips for robots
                    List<ImprovedAStar.Robot> robots = new List<ImprovedAStar.Robot>();
                    foreach (GameObject rgo in robotGameObjects)
                    {
                        List<Trip> trips = Trip.GenerateTripList(rgo, shelfGameObjects, loadZonePrefab, unloadZonePrefab, 5);
                        ImprovedAStar.Robot robot = new ImprovedAStar.Robot(rgo, trips);

                        robots.Add(robot);
                    }
                    improvedAStar.StartTravelling(robots, this);
                    break;
                }
            case PathFindingAlgorithm.RERAPF:
                {
                    RERAPF rerapf = new RERAPF(shelfGameObjects.Select(sgo => sgo.transform).ToArray()
                        , wallGameObjects.Select(wgo => wgo.transform).ToArray(), floorPrefab.transform, tileSize, tilePrefab
                        , goalTilePrefab);

                    rerapf.map.DrawObstructedTiles();

                    // make trips for robots
                    List<RERAPF.Robot> robots = new List<RERAPF.Robot>();
                    foreach (GameObject rgo in robotGameObjects)
                    {
                        List<Trip> trips = Trip.GenerateTripList(rgo, shelfGameObjects, loadZonePrefab, unloadZonePrefab, 5);
                        RERAPF.Robot robot = new RERAPF.Robot(rgo, trips);

                        robots.Add(robot);
                    }
                    rerapf.StartTravelling(robots, this);
                    break;
                }
            case PathFindingAlgorithm.abc:
                {
                    ImprovedACO improvedACO = new ImprovedACO(shelfGameObjects.Select(sgo => sgo.transform).ToArray()
                        , wallGameObjects.Select(wgo => wgo.transform).ToArray(), floorPrefab.transform, tileSize, tilePrefab
                        , goalTilePrefab);

                    improvedACO.map.DrawObstructedTiles();

                    // make trips for robots
                    List<ImprovedACO.Robot> robots = new List<ImprovedACO.Robot>();
                    foreach (GameObject rgo in robotGameObjects)
                    {
                        List<Trip> trips = Trip.GenerateTripList(rgo, shelfGameObjects, loadZonePrefab, unloadZonePrefab, 5);
                        ImprovedACO.Robot robot = new ImprovedACO.Robot(rgo, trips);

                        robots.Add(robot);
                    }
                    improvedACO.StartTravelling(robots, this);
                    break;
                }
        }
    }

}
