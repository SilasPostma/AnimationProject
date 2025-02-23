using System.Collections.Generic;
using UnityEngine;

public class PlacePoints : MonoBehaviour
{
    [Header("Circle Parameters")]
    [Range(1, 50)]
    public int points;
    [Range(3.0f, 20.0f)]
    public float radius;

    private int pointsToCheck;
    private float radToCheck;

    [Header("Point Prefab")]
    public GameObject pointPrefab;

    public readonly List<Point> instanceList = new List<Point>();

    RuleManager ruleManager;

    private void Start()
    {
        ruleManager = GetComponent<RuleManager>();


        pointsToCheck = points;
        radToCheck = radius;

        UpdatePoints();
    }

    private void FixedUpdate()
    {
        if ((points != pointsToCheck || Mathf.Abs(radius - radToCheck) > Mathf.Epsilon) && !ruleManager.simActive)
        {
            UpdatePoints();
        }
    }

    private List<Point> CirclePoints(int pointAmount, float radius)
    {
        float thetaInc = 360f / pointAmount;
        float theta = 0f;

        var circlePoints = new List<Point>();

        for (int i = 0; i < pointAmount; i++)
        {
            // Calculate coordinates for point
            float xCor = Mathf.Cos(theta * Mathf.Deg2Rad) * radius;
            float yCor = Mathf.Sin(theta * Mathf.Deg2Rad) * radius;
            theta += thetaInc;

            // Add a new Point instance to the list
            var newPoint = new Point(
                new List<Point> { null, null }, // Placeholder for target points
                new Vector3(xCor, yCor, 0f),
                pointPrefab,
                Vector3.zero
            );

            circlePoints.Add(newPoint);
        }

        return circlePoints;
    }

    private void UpdatePoints()
    {
        ClearInstances();

        // Generate new points and place them
        var newPoints = CirclePoints(points, radius);
        PointPlacer(newPoints);

        // Update the cache values
        pointsToCheck = points;
        radToCheck = radius;
    }

    private void ClearInstances()
    {
        foreach (var point in instanceList)
        {
            if (point.PointGO != null)
            {
                Destroy(point.PointGO);
            }
        }

        instanceList.Clear();
    }

    private void PointPlacer(List<Point> circlePoints)
    {
        foreach (var point in circlePoints)
        {
            // Instantiate the prefab and set its position
            var instance = Instantiate(pointPrefab, point.Position, Quaternion.identity);
            point.PointGO = instance; // Update the GameObject in the Point instance
            instanceList.Add(point);
        }
    }
}
