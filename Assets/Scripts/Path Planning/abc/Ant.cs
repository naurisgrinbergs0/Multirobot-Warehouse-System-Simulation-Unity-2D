using Assets.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class Ant
{
    // Fields
    private int currentTile;
    private int endTile;
    private List<int> path;
    private HashSet<int> visited;

    // Constructor
    public Ant()
    {
        
    }

    public void SetStartTile(int startTile)
    {
        currentTile = startTile;
        this.path = new List<int> { startTile };
        this.visited = new HashSet<int> { startTile };
    }
    public void SetEndTile(int endTile)
    {
        this.endTile = endTile;
    }

    // Methods
    public int CurrentTile()
    {
        return currentTile;
    }

    public List<int> Path()
    {
        return path;
    }

    public bool AtEnd()
    {
        return currentTile == endTile;
    }

    public void MoveToTile(int nextTile)
    {
        currentTile = nextTile;
        path.Add(nextTile);
        visited.Add(nextTile);
    }

    public int NextTile(TileMap map, float[,] pheromones)
    {
        // Get the valid neighbor tiles
        List<int> neighbors = ImprovedACO.GetNeighbors(currentTile, map);
        List<int> unvisitedNeighbours = new List<int>(neighbors);
        unvisitedNeighbours.RemoveAll(x => visited.Contains(x));

        // Calculate the probability of choosing each neighbor tile
        List<float> probabilities = new List<float>();
        float totalPheromones = 0;
        foreach (int neighbor in unvisitedNeighbours)
        {
            float pheromone = pheromones[currentTile, neighbor];
            float distance = Vector2.Distance(ImprovedACO.GetPositionFromIndex(currentTile, map),
                                               ImprovedACO.GetPositionFromIndex(neighbor, map));
            float probability = Mathf.Pow(pheromone, ImprovedACO.ALPHA) * Mathf.Pow(1 / distance, ImprovedACO.BETA);
            probabilities.Add(probability);
            totalPheromones += probability;
        }

        // Choose a neighbor tile based on the probabilities
        float randomValue = UnityEngine.Random.Range(0f, totalPheromones);
        for (int i = 0; i < unvisitedNeighbours.Count; i++)
        {
            randomValue -= probabilities[i];
            if (randomValue <= 0)
            {
                return unvisitedNeighbours[i];
            }
        }

        // If no neighbor was chosen, choose a random one
        return neighbors[UnityEngine.Random.Range(0, neighbors.Count)];
    }

    public float PathLength(TileMap map)
    {
        float length = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector2.Distance(ImprovedACO.GetPositionFromIndex(path[i], map)
                , ImprovedACO.GetPositionFromIndex(path[i + 1], map));
        }
        return length;
    }


    public void Reset()
    {
        path.Clear();
        visited.Clear();
    }

}
