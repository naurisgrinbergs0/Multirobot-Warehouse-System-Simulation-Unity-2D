
//using Assets.Scripts;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using UnityEngine;

//public class ImprovedACO
//{
//    private const float PHEROMONE_INTENSITY = 110f;
//    private const float PHEROMONE_EVAPORATION_RATE = 0.3f;
//    public const float ALPHA = 1f;
//    public const float BETA = 8f;


//    public class Robot
//    {
//        public GameObject robotGameObject;
//        public List<Trip> trips;
//        public PathRenderer pathRenderer;

//        public Robot(GameObject robot, List<Trip> trips)
//        {
//            this.robotGameObject = robot;
//            this.trips = trips;
//        }
//    }

//    // Define the number of ants and iterations for the algorithm
//    private const int NumAnts = 10;
//    private const int MaxIterations = 100;

//    // Define the pheromone matrix
//    private float[,] pheromones;

//    // Define the map and robot list
//    public TileMap map;
//    private List<Robot> robots;

//    public ImprovedACO(Transform[] shelves, Transform[] walls, Transform floor, float tileSize, GameObject tilePrefab, GameObject tileGoalPrefab)
//    {
//        map = new TileMap(floor, shelves, walls, tileSize, tilePrefab, tileGoalPrefab);

//        // Initialize the pheromone matrix
//        int numTiles = map.tiles.GetLength(0) * map.tiles.GetLength(1);
//        pheromones = new float[numTiles, numTiles];
//        for (int i = 0; i < numTiles; i++)
//        {
//            for (int j = 0; j < numTiles; j++)
//            {
//                pheromones[i, j] = 1.0f / numTiles;
//            }
//        }
//    }

//    public void StartTravelling(List<Robot> robots, Entry coroutineProvider)
//    {
//        int[] tripTiles = map.GetTripTiles(robots.First().trips.First());
//        //float[] tripStartXY = map.TileToXY(tripTiles[0], tripTiles[1]);
//        //float[] tripEndXY = map.TileToXY(tripTiles[2], tripTiles[3]);
//        //map.DrawTile(tripTiles[0], tripTiles[1]);
//        //map.DrawGoalTile();

//        //var p = FindPaths(robots);

//    }


//    public void Train(List<GameObject> shelves, GameObject load, GameObject unload)
//    {
//        List<int[]> allGoalTiles = new List<int[]>();

//        foreach (GameObject shelf in shelves)
//            allGoalTiles.Add(map.GetShelfTile(shelf));

//        //allGoalTiles.Add();

//        //TrainMatrix();
//    }

//    //public Dictionary<int[], List<Vector2>> TrainMatrix(List<Robot> robots)
//    //{
//    //    // Initialize the ants and best paths
//    //    List<Ant> ants = new List<Ant>();
//    //    Dictionary<int[], List<int>> bestPaths = new Dictionary<int[], List<int>>();
//    //    float[] bestPathLengths = new float[robots.Count];

//    //    // Perform the ACO algorithm
//    //    for (int iteration = 0; iteration < MaxIterations; iteration++)
//    //    {
//    //        // Create the ants
//    //        ants.Clear();
//    //        for (int i = 0; i < NumAnts; i++)
//    //            ants.Add(new Ant());

//    //        // Move the ants
//    //        Parallel.ForEach(ants, ant =>
//    //        {
//    //            int[] startTile = startTiles[ant.Id % startTiles.Count];
//    //            ant.SetStartTile(startTile);

//    //            float shortestPathLength = float.MaxValue;
//    //            List<int> shortestPath = null;

//    //            foreach (int[] endTile in endTiles)
//    //            {
//    //                ant.SetEndTile(endTile);

//    //                while (!ant.AtEnd())
//    //                {
//    //                    // Get the next tile to move to
//    //                    int nextTile = ant.NextTile(map, pheromones);

//    //                    // Update the pheromone matrix
//    //                    float deltaPheromone = 1.0f / ant.PathLength(map);
//    //                    lock (pheromones) // Ensure thread safety when updating pheromones
//    //                    {
//    //                        pheromones[ant.CurrentTile(), nextTile] += deltaPheromone;
//    //                        pheromones[nextTile, ant.CurrentTile()] += deltaPheromone;
//    //                    }

//    //                    // Move to the next tile
//    //                    ant.MoveToTile(nextTile);
//    //                }

//    //                // Check if this ant found a better path
//    //                float pathLength = ant.PathLength(map);
//    //                if (pathLength < shortestPathLength)
//    //                {
//    //                    shortestPath = new List<int>(ant.Path());
//    //                    shortestPathLength = pathLength;
//    //                }

//    //                // Reset the ant for the next iteration
//    //                ant.Reset();
//    //            }

//    //            // Update the best paths and pheromone matrix based on the shortest path for this ant
//    //            int index = ant.Id % endTiles.Count;
//    //            lock (bestPaths) // Ensure thread safety when updating bestPaths
//    //            {
//    //                if (shortestPathLength < bestPathLengths[index])
//    //                {
//    //                    bestPaths[endTiles[index]] = shortestPath;
//    //                    bestPathLengths[index] = shortestPathLength;

//    //                    for (int i = 0; i < shortestPath.Count - 1; i++)
//    //                    {
//    //                        int row = shortestPath[i];
//    //                        int col = shortestPath[i + 1];

//    //                        pheromones[row, col] += PHEROMONE_INTENSITY / shortestPathLength;
//    //                        pheromones[col, row] = pheromones[row, col];
//    //                    }
//    //                }
//    //            }
//    //        });

//    //        // Evaporate the pheromone trails
//    //        for (int i = 0; i < map.tiles.GetLength(0); i++)
//    //            for (int j = 0; j < map.tiles.GetLength(1); j++)
//    //                pheromones[i, j] *= (1 - PHEROMONE_EVAPORATION_RATE);
//    //    }

//    //    // Convert the best path indices to a list of vectors
//    //    List<Vector2> path = new List<Vector2>();
//    //    foreach (int tileIndex in shortestPath)
//    //    {
//    //        Vector2 tilePos = GetPositionFromIndex(tileIndex, map);
//    //        path.Add(tilePos);
//    //    }
//    //    return path;
//    //}


//    public static Vector2 GetPositionFromIndex(int index, TileMap map)
//    {
//        int row = index / map.tiles.GetLength(0);
//        int col = index % map.tiles.GetLength(1);
//        return new Vector2(col, row);
//    }

//    public static int GetIndexFromTile(int[] tile, TileMap map)
//    {
//        int xIndex = tile[0];
//        int yIndex = tile[1];

//        // Check if the indices are within the bounds of the tiles array
//        if (xIndex < 0 || xIndex >= map.tiles.GetLength(0) || yIndex < 0 || yIndex >= map.tiles.GetLength(1))
//        {
//            // If the indices are out of bounds, return -1 to indicate an invalid index
//            return -1;
//        }

//        // Return the index of the cell
//        return xIndex * map.tiles.GetLength(0) + yIndex;
//    }



//    // Helper function to get neighboring tiles
//    public static List<int> GetNeighbors(int tile, TileMap map)
//    {
//        List<int> neighbors = new List<int>();

//        int col = tile / map.tiles.GetLength(0);
//        int row = tile % map.tiles.GetLength(1);


//        // Check left neighbour
//        if (col - 1 > -1 && map.tiles[col - 1, row] == 0)
//            neighbors.Add(row + (col - 1) * map.tiles.GetLength(0));

//        // Check right neighbour
//        if (col + 1 < map.tiles.GetLength(0) && map.tiles[col + 1, row] == 0)
//            neighbors.Add(row + (col + 1) * map.tiles.GetLength(0));

//        // Check top neighbour
//        if (row + 1 < map.tiles.GetLength(1) && map.tiles[col, row + 1] == 0)
//            neighbors.Add((row + 1) + col * map.tiles.GetLength(0));

//        // Check bottom neighbour
//        if (row - 1 > -1 && map.tiles[col, row - 1] == 0)
//            neighbors.Add((row - 1) + col * map.tiles.GetLength(0));

//        return neighbors;
//    }
//}
