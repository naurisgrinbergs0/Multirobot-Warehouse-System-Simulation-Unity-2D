using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Robot
{
    public class RobotRERAPF : RobotBase
    {
        public class ExploredTile
        {
            public int[] tile;
            float? startStaticPotential = null;
            float prevStaticPotential;

            public float? GetStartStaticPotential() { return startStaticPotential; }
            public float GetPrevStaticPotential() { return prevStaticPotential; }
            public void UpdateStaticPotential(float pot) { if (startStaticPotential == null) startStaticPotential = pot; prevStaticPotential = pot; }
            public ExploredTile(int x, int y, float staticPotential)
            {
                tile = new int[] { x, y };
                UpdateStaticPotential(staticPotential);
            }
        }

        public void AddExploredTile(int x, int y, float staticPotential)
        {
            exploredTiles.Add(new ExploredTile(x, y, staticPotential));
        }

        //public PathRenderer pathRenderer;
        public List<ExploredTile> exploredTiles = new List<ExploredTile>();
        public int[] currentTile;

        public RobotRERAPF(List<Trip> trips, Map.TileMap map)
        {
            this.trips = trips;

            // set current position
            currentTile = map.XYToTile(trips.First().from.position.x, trips.First().from.position.y);
            var pos = map.TileToXY(currentTile[0], currentTile[1]);
            position = new Vector2(pos[0], pos[1]);
        }
    }
}
