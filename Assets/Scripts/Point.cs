using UnityEngine;
using System.Collections.Generic;

public class Point
{
    public List<Point> TargetPoints { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 TargetPos { get; set; }
    public GameObject PointGO { get; set; } // This will be the reference to the GameObject
    public Vector3 CurrentVelocity; // Store velocity for SmoothDamp

    // Constructor with parameters for TargetPoints, Position, and the GameObject reference
    public Point(List<Point> targetPoints, Vector3 position, GameObject pointGO, Vector3 targetPos)
    {
        TargetPoints = targetPoints;
        TargetPos = targetPos;
        Position = position;
        PointGO = pointGO;
    }

    // Method to change the color of the point's sprite
    public void SetColor(Color color)
    {
        if (PointGO != null)
        {
            SpriteRenderer renderer = PointGO.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = color;
            }
        }
    }

    public void MoveToPoint(float moveSpeed)
    {
        // If moveSpeed is zero, don't move
        if (moveSpeed == 0)
        {
            return;
        }

        // Limit the maximum speed
        float maxSpeed = moveSpeed;
        Vector3 currentPos = PointGO.transform.position;
        Vector3 direction = (TargetPos - currentPos).normalized;

        // Move towards the target position with maxSpeed
        PointGO.transform.position = Vector3.MoveTowards(currentPos, TargetPos, maxSpeed * Time.deltaTime);
    }

}
