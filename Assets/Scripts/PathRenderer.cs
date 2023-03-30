using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public void DrawPath(List<AStar.Node> path, TileMap tileMap)
    {
        // Set the position count of the LineRenderer to the number of points in the path
        lineRenderer.positionCount = path.Count;

        // Set the positions of the LineRenderer to the points in the path
        for (int i = 0; i < path.Count; i++)
        {
            float[] pos = tileMap.TileToXY(path[i].x, path[i].y);
            Vector3 point = new Vector3(pos[0], pos[1], 0f);
            lineRenderer.SetPosition(i, point);
        }

        // Customize the look of the LineRenderer
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material.color = Color.blue;
    }

    public void ClearPath()
    {
        // Set the position count of the LineRenderer to 0 to clear the path
        lineRenderer.positionCount = 0;
    }
}
