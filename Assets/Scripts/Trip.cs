using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    public class Trip
    {
        public GameObject from;
        public GameObject to;
        public bool isCargoTrip;

        public Trip(GameObject from, GameObject to, bool isCargoTrip)
        {
            this.from = from;
            this.to = to;
            this.isCargoTrip = isCargoTrip;
        }


        public static List<Trip> GenerateTripList(GameObject robot, GameObject[] shelfTransforms, GameObject zoneLoadTransform,
            GameObject zoneUnloadTransform, int numberOfTrips)
        {
            List<Trip> tripList = new List<Trip>();
            GameObject lastShelf = null;
            GameObject lastUnloadZone = null;

            // Add first trip from robot to a shelf
            GameObject from = robot;
            GameObject to = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
            Trip firstTrip = new Trip(from, to, false);
            tripList.Add(firstTrip);
            lastShelf = to;
            from = to;

            for (int i = 0; i < numberOfTrips - 1; i++)
            {
                // Determine the trip type
                bool isCargoTrip = (i + 1) % 2 == 0;
                GameObject nextFrom = null;
                GameObject nextTo = null;

                if (lastShelf == null || Random.Range(0, 2) == 0)
                {
                    // From load zone to shelf
                    isCargoTrip = false;
                    nextFrom = zoneLoadTransform;
                    nextTo = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
                    lastShelf = nextTo;
                }
                else if (lastUnloadZone == null || Random.Range(0, 2) == 0)
                {
                    // From shelf to unload zone
                    nextFrom = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
                    nextTo = zoneUnloadTransform;
                    lastUnloadZone = nextTo;
                }
                else
                {
                    // From shelf to shelf
                    nextFrom = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
                    nextTo = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
                    while (nextTo == nextFrom)
                    {
                        nextTo = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
                    }
                    lastShelf = nextTo;
                }

                Trip nextTrip = new Trip(from, nextTo, isCargoTrip);
                tripList.Add(nextTrip);
                from = nextTo;
            }

            return tripList;
        }


    }


}
