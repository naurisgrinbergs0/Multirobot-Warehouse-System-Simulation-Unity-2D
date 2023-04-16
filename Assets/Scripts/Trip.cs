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

            bool isLastShelf = false;
            bool isLastUnloadZone = false;
            bool isLastLoadZone = false;
            bool isLastCargoTrip = false;
            Transform from = robot;
            Transform to = null;

            for (int i = 0; i < numberOfTrips; i++)
            {
                if(i != 0)
                    from = to;

                // if last 'to' was shelf
                if (isLastShelf)
                {
                    to = isLastCargoTrip
                        ? Random.Range(0, 2) == 0
                            ? shelfTransforms.Where(st => st != from).ToArray()[Random.Range(0, shelfTransforms.Length - 1)]
                            : zoneLoadTransform
                        : Random.Range(0, 2) == 0
                            ? shelfTransforms.Where(st => st != from).ToArray()[Random.Range(0, shelfTransforms.Length - 1)]
                            : zoneUnloadTransform
                            ;
                }
                // if last 'to' was load zone
                else if (isLastLoadZone)
                {
                    to = shelfTransforms[Random.Range(0, shelfTransforms.Length)];
                }
                // if last 'to' was unload zone or last 'to' is null
                else
                {
                    to = Random.Range(0, 2) == 0
                            ? shelfTransforms[Random.Range(0, shelfTransforms.Length)]
                            : zoneLoadTransform
                            ;
                }

                tripList.Add(new Trip(fromLinkedTransform: from, toLinkedTransform: to, isCargoTrip: i % 2 == 1));

                // store last trip settings
                isLastUnloadZone = to == zoneUnloadTransform;
                isLastLoadZone = to == zoneLoadTransform;
                isLastShelf = !isLastUnloadZone && !isLastLoadZone;
                isLastCargoTrip = i % 2 == 1;
            }

            return tripList;

        }
    }


}
