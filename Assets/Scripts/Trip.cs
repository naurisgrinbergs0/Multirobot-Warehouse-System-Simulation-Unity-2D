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
        public Vector2? from;
        public Vector2? to;
        public Transform fromLinkedTransform;
        public Transform toLinkedTransform;
        public bool isCargoTrip;

        public Trip(Vector2? from = null, Vector2? to = null
            , Transform fromLinkedTransform = null, Transform toLinkedTransform = null, bool isCargoTrip = false)
        {
            this.from = from;
            this.to = to;
            this.fromLinkedTransform = fromLinkedTransform;
            this.toLinkedTransform = toLinkedTransform;
            this.isCargoTrip = isCargoTrip;
        }


        public static List<Trip> GenerateTripList(Transform robot, Transform[] shelfTransforms, Transform zoneLoadTransform,
            Transform zoneUnloadTransform, int numberOfTrips)
        {
            List<Trip> tripList = new List<Trip>();
            Transform lastShelf = null;
            Transform lastUnloadZone = null;

            // Add first trip from robot to a shelf
            Transform from = robot;
            Transform to = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
            Trip firstTrip = new Trip(fromLinkedTransform: from, toLinkedTransform: to, isCargoTrip: false);
            tripList.Add(firstTrip);
            lastShelf = to;
            from = to;

            for (int i = 0; i < numberOfTrips - 1; i++)
            {
                // Determine the trip type
                bool isCargoTrip = (i + 1) % 2 == 0;
                Transform nextFrom = null;
                Transform nextTo = null;

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

                Trip nextTrip = new Trip(fromLinkedTransform: from, toLinkedTransform: nextTo, isCargoTrip: isCargoTrip);
                tripList.Add(nextTrip);
                from = nextTo;
            }

            return tripList;
        }


    }


}
