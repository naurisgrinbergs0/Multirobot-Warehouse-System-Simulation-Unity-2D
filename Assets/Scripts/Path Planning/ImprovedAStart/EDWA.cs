using UnityEngine;
using System.Collections.Generic;

public class EDWA
{
    public float MaxLinearSpeed { get; set; }
    public float MaxAngularSpeed { get; set; }
    public float LinearAcceleration { get; set; }
    public float AngularAcceleration { get; set; }
    public float TimeStep { get; set; }

    public EDWA(float maxLinearSpeed, float maxAngularSpeed, float linearAcceleration, float angularAcceleration, float timeStep)
    {
        MaxLinearSpeed = maxLinearSpeed;
        MaxAngularSpeed = maxAngularSpeed;
        LinearAcceleration = linearAcceleration;
        AngularAcceleration = angularAcceleration;
        TimeStep = timeStep;
    }

    //public Vector2 GetVelocityCommand(Vector2 currentPosition, float currentRotation, Vector2 currentVelocity
    //    , Vector2 targetPosition, List<Transform> obstacles)
    //{
    //    float bestScore = float.NegativeInfinity;
    //    Vector2 bestVelocity = currentVelocity;
    //    Vector2 currentDirection = new Vector2(Mathf.Cos(currentRotation), Mathf.Sin(currentRotation));

    //    for (float v = -MaxLinearSpeed; v <= MaxLinearSpeed; v += LinearAcceleration * TimeStep)
    //    {
    //        for (float omega = -MaxAngularSpeed; omega <= MaxAngularSpeed; omega += AngularAcceleration * TimeStep)
    //        {
    //            Vector2 newVelocity = new Vector2(v * currentDirection.x - omega * currentDirection.y, v * currentDirection.y + omega * currentDirection.x);

    //            if (IsCollisionFree(newVelocity, currentPosition, obstacles))
    //            {
    //                float score = CalculateScore(newVelocity, targetPosition, currentPosition);
    //                if (score > bestScore)
    //                {
    //                    bestScore = score;
    //                    bestVelocity = newVelocity;
    //                }
    //            }
    //        }
    //    }

    //    return bestVelocity;
    //}

    public Vector2 GetVelocityCommand(Vector2 currentPosition, float currentRotation, Vector2 currentVelocity,
        Vector2 targetPosition, List<Transform> obstacles)
    {
        float bestScore = float.NegativeInfinity;
        Vector2 bestVelocity = currentVelocity;
        Vector2 currentDirection = new Vector2(Mathf.Cos(currentRotation), Mathf.Sin(currentRotation));
        float[] speedConstraints = new float[] { -MaxLinearSpeed, MaxLinearSpeed, -MaxAngularSpeed, MaxAngularSpeed };

        foreach (float v in SampleSpeeds(speedConstraints, LinearAcceleration, TimeStep))
        {
            foreach (float omega in SampleSpeeds(speedConstraints, AngularAcceleration, TimeStep))
            {
                Vector2 newVelocity = currentDirection * v + new Vector2(-currentDirection.y, currentDirection.x) * omega;

                if (IsCollisionFree(newVelocity, currentPosition, obstacles))
                {
                    float score = CalculateScore(newVelocity, targetPosition, currentPosition);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestVelocity = newVelocity;
                    }
                }
            }
        }

        return bestVelocity;
    }

    private IEnumerable<float> SampleSpeeds(float[] speedConstraints, float acceleration, float timeStep)
    {
        for (float speed = speedConstraints[0]; speed <= speedConstraints[1]; speed += acceleration * timeStep)
        {
            yield return speed;
        }

        for (float speed = speedConstraints[2]; speed <= speedConstraints[3]; speed += acceleration * timeStep)
        {
            yield return speed;
        }
    }




    private bool IsCollisionFree(Vector2 newVelocity, Vector2 currentPosition, List<Transform> obstacles)
    {
        // Calculate the next position using the new velocity
        Vector2 nextPosition = currentPosition + newVelocity;

        // Check for collisions with obstacles
        foreach (Transform obstacle in obstacles)
        {
            //if (obstacleCollider != null && robotCollider.IsTouching(obstacleCollider))
            if (CircleIntersectsRectangle(nextPosition, RobotGenerator.ROBOT_SIZE / 2f, obstacle))
            {
                return false;
            }
        }

        // No collisions detected
        return true;
    }

    
    bool CircleIntersectsRectangle(Vector2 circleCenter, float circleRadius, Transform rectangle)
    {
        Bounds b = rectangle.gameObject.GetComponent<SpriteRenderer>().bounds;

        // Find the closest point in the rectangle to the circle center
        float closestX = Mathf.Clamp(circleCenter.x, b.min.x, b.max.x);
        float closestY = Mathf.Clamp(circleCenter.y, b.min.y, b.max.y);

        // Calculate the distance between the circle center and the closest point
        float deltaX = circleCenter.x - closestX;
        float deltaY = circleCenter.y - closestY;

        // If the distance is less than the circle radius, the circle and rectangle intersect
        float distanceSquared = deltaX * deltaX + deltaY * deltaY;
        return distanceSquared < circleRadius * circleRadius;
    }


    private float CalculateScore(Vector2 newVelocity, Vector2 targetPosition, Vector2 currentPosition)
    {
        float distanceWeight = 2.0f;
        float velocityAlignment = Vector2.Dot(newVelocity.normalized, (targetPosition - currentPosition).normalized);
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        return velocityAlignment - distanceWeight * distanceToTarget;
    }

}
