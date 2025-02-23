using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
public class RuleManager : MonoBehaviour
{
    public bool simActive = false;
    public GameObject button;
    public GameObject RadSlider;
    public GameObject PointsSlider;
    public GameObject MoveSpeedSlider;
    public GameObject TriangleRuleToggle;
    public GameObject MoveCamToggle;

    PlacePoints placePoints;
    private List<Point> IL = new List<Point>();

    private GameObject thirdPointVis;
    private bool thirdPointVisPresent = false;
    private Point selectedPoint; // Reference to the currently selected point

    public Camera mainCamera; // Reference to the main camera
    public float padding = 2f; // Padding around the points
    public float smoothSpeed = 0.1f; // Speed for camera adjustments

    public LineRenderer LR1;
    public LineRenderer LR2;
    public LineRenderer LR3;
    public LineRenderer LR4;

    [Header("Simulation Settings")]
    public float moveSpeed = 1f;
    public bool moveCamera;
    public bool SimulateTriangleRule = true;

    [Header("Point Prefab")]
    public GameObject pointPrefab;

    public void StartSim()
    {
        placePoints = GetComponent<PlacePoints>();
        IL = placePoints.instanceList;

        button.SetActive(false);
        RadSlider.SetActive(false);
        PointsSlider.SetActive(false);
        MoveSpeedSlider.SetActive(false);
        TriangleRuleToggle.SetActive(false);
        MoveCamToggle.SetActive(false);
        PickTargets();
        simActive = true;
    }

    private void FixedUpdate()
    {
        if (simActive)
        {
            if (SimulateTriangleRule)
            {
                TriangleRule();
            }
            else
            {
                MiddleRule();
            }
        }

        // Smoothly move each point toward its target position
        foreach (Point point in IL)
        {
            SmoothMovePoint(point);
        }



        // Update visualization for the selected point
        if (thirdPointVisPresent && selectedPoint != null)
        {
            UpdateVisualization(selectedPoint);
        }

        ApplySeparation();


        if (moveCamera) AdjustCameraToFitPoints();
    }

    void HideVisualisation()
    {
        // Destroy the visualization circle if it exists
        if (thirdPointVisPresent)
        {
            Destroy(thirdPointVis);
            thirdPointVisPresent = false;
        }

        // Disable all LineRenderers
        LR1.enabled = false;
        LR2.enabled = false;
        LR3.enabled = false;
        LR4.enabled = false;

        // Reset all points to black color
        foreach (Point point in IL)
        {
            point.SetColor(Color.black);
        }

        // Clear the selected point
        selectedPoint = null;
    }



    void SmoothMovePoint(Point point)
    {
        // SmoothDamp provides smooth movement with deceleration
        point.PointGO.transform.position = Vector3.SmoothDamp(
            point.PointGO.transform.position,  // Current position
            point.TargetPos,                   // Target position
            ref point.CurrentVelocity,         // Reference to velocity
            1f                               // Smoothing time
        );
    }

    void ApplySeparation()
    {
        float minimumDistance = 1f; // Minimum allowed distance between points
        float separationStrength = 0.1f; // How strong the separation force should be

        for (int i = 0; i < IL.Count; i++)
        {
            for (int j = i + 1; j < IL.Count; j++)
            {
                Point pointA = IL[i];
                Point pointB = IL[j];

                Vector3 positionA = pointA.PointGO.transform.position;
                Vector3 positionB = pointB.PointGO.transform.position;

                float distance = Vector3.Distance(positionA, positionB);
                if (distance < minimumDistance)
                {
                    // Calculate separation direction
                    Vector3 direction = (positionA - positionB).normalized;

                    // Calculate the amount to push each point
                    float overlap = minimumDistance - distance;
                    Vector3 separationForce = direction * overlap * separationStrength;

                    // Apply the separation force
                    pointA.PointGO.transform.position += separationForce / 2f; // Half force to A
                    pointB.PointGO.transform.position -= separationForce / 2f; // Half force to B
                }
            }
        }
    }



    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                // Now we need to find the corresponding Point that was clicked
                GameObject clickedGO = hit.collider.gameObject;
                Point clickedPoint = IL.Find(p => p.PointGO == clickedGO); // Find the Point from IL list that matches the clicked GameObject

                if (clickedPoint != null)
                {
                    // Call DrawVisualisation with the clicked Point
                    DeselectAllPoints();
                    DrawVisualisation(clickedPoint);
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right-click
        {
            // Hide the visualization and reset all point colors
            HideVisualisation();
        }
    }


    void TriangleRule()
    {
        foreach (Point point in IL)
        {
            Vector3 target1 = point.TargetPoints[0].PointGO.transform.position;
            Vector3 target2 = point.TargetPoints[1].PointGO.transform.position;
            point.TargetPos = ClampPointToCameraView(CalculateEquilateralThirdPoint(target1, target2, point.PointGO.transform.position));
            point.MoveToPoint(moveSpeed);
        }
    }

    void MiddleRule()
    {
        foreach (Point point in IL)
        {
            Vector3 target1 = point.TargetPoints[0].PointGO.transform.position;
            Vector3 target2 = point.TargetPoints[1].PointGO.transform.position;
            point.TargetPos = ClampPointToCameraView(CalculateMiddle(target1, target2));
            point.MoveToPoint(moveSpeed);
        }
    }

    Vector3 CalculateEquilateralThirdPoint(Vector3 point1, Vector3 point2, Vector3 selfPoint)
    {
        float distBetweenTargets = Vector3.Distance(point1, point2);

        // Midpoint between the two target points
        Vector3 midpoint = (point1 + point2) / 2;

        // Direction from point1 to point2
        Vector3 direction = point2 - point1;

        // Rotate the direction by 60 degrees to get the third point
        Vector3 rotatedDirection = Quaternion.Euler(0, 0, 90) * direction * Mathf.Sqrt(3) * 0.5f;

        // Choose the closest position to selfPoint
        Vector3 thirdPoint = (Vector3.Distance(selfPoint, midpoint + rotatedDirection) < Vector3.Distance(selfPoint, midpoint - rotatedDirection))
            ? midpoint + rotatedDirection
            : midpoint - rotatedDirection;

        return thirdPoint;
    }

    Vector3 CalculateMiddle(Vector3 point1, Vector3 point2)
    {
        return (point1 + point2) / 2;
    }

    void PickTargets()
    {
        System.Random random = new System.Random();
        foreach (Point point in IL)
        {
            // Pick first target
            Point targetOne;
            do
            {
                targetOne = IL[random.Next(0, IL.Count)];
            } while (targetOne == point); // Ensure the point doesn't pick itself as target

            // Pick second target
            Point targetTwo;
            do
            {
                targetTwo = IL[random.Next(0, IL.Count)];
            } while (targetTwo == point || targetTwo == targetOne); // Ensure the point doesn't pick itself or the first target
            point.TargetPoints = new List<Point> { targetOne, targetTwo };
        }
    }


    void DrawVisualisation(Point clickedPoint)
    {
        // Change the color of the clicked point to red
        clickedPoint.SetColor(Color.red);

        // Update the visualization (grey circle and line renderer)
        UpdateVisualization(clickedPoint);

        // Track the selected point for continuous updates
        selectedPoint = clickedPoint;

        // Ensure the blue lines are visible for the selected point
        if (clickedPoint.TargetPoints != null)
        {
            // Enable and update the LineRenderers for the targets
            LR2.enabled = true;
            LR3.enabled = true;

            // Set the color of the target points to blue
            foreach (Point target in clickedPoint.TargetPoints)
            {
                target.SetColor(Color.blue); // Set the target points to blue

                // Update the LineRenderer positions for the target points
                LR2.SetPosition(0, clickedPoint.PointGO.transform.position);
                LR2.SetPosition(1, target.PointGO.transform.position);

                LR3.SetPosition(0, clickedPoint.PointGO.transform.position);
                LR3.SetPosition(1, target.PointGO.transform.position);
            }

            // Ensure LR4 is enabled and updated as well
            LR4.enabled = true;
            LR4.SetPosition(0, clickedPoint.PointGO.transform.position);
            LR4.SetPosition(1, clickedPoint.TargetPos);
        }
        else
        {
            // If no target points exist, ensure that the lines are disabled
            LR2.enabled = false;
            LR3.enabled = false;
            LR4.enabled = false; // Disable LR4 as well
        }
    }



    void UpdateVisualization(Point clickedPoint)
    {
        Vector3 updatedPosition;

        // Determine the target position based on the active rule
        if (SimulateTriangleRule)
        {
            updatedPosition = ClampPointToCameraView(CalculateEquilateralThirdPoint(
                clickedPoint.TargetPoints[0].PointGO.transform.position,
                clickedPoint.TargetPoints[1].PointGO.transform.position,
                clickedPoint.PointGO.transform.position
            ));

            LR1.enabled = true;
            LR2.enabled = true;
            LR3.enabled = true;

            // Update the LineRenderer positions
            LR1.SetPosition(0, clickedPoint.TargetPoints[0].PointGO.transform.position);
            LR1.SetPosition(1, clickedPoint.TargetPoints[1].PointGO.transform.position);

            LR2.SetPosition(0, clickedPoint.TargetPoints[0].PointGO.transform.position);
            LR2.SetPosition(1, clickedPoint.TargetPos);

            LR3.SetPosition(0, clickedPoint.TargetPoints[1].PointGO.transform.position);
            LR3.SetPosition(1, clickedPoint.TargetPos);

            LR4.SetPosition(0, clickedPoint.PointGO.transform.position);
            LR4.SetPosition(1, clickedPoint.TargetPos);
        }
        else
        {
            updatedPosition = ClampPointToCameraView(CalculateMiddle(
                clickedPoint.TargetPoints[0].PointGO.transform.position,
                clickedPoint.TargetPoints[1].PointGO.transform.position
            ));

            LR1.enabled = false;

            LR2.SetPosition(0, clickedPoint.TargetPoints[0].PointGO.transform.position);
            LR2.SetPosition(1, clickedPoint.TargetPos);

            LR3.SetPosition(0, clickedPoint.TargetPoints[1].PointGO.transform.position);
            LR3.SetPosition(1, clickedPoint.TargetPos);

            LR4.SetPosition(0, clickedPoint.PointGO.transform.position);
            LR4.SetPosition(1, clickedPoint.TargetPos);
        }

        // Create or update the grey circle (thirdPointVis)
        if (!thirdPointVisPresent)
        {
            thirdPointVis = Instantiate(pointPrefab, updatedPosition, Quaternion.identity);
            SpriteRenderer spriteRenderer = thirdPointVis.GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color32(220, 220, 220, 255);

            // Set the sorting order to render underneath points
            spriteRenderer.sortingOrder = -1; // Ensure it's below other objects with default sortingOrder of 0
            thirdPointVisPresent = true;
        }
        else
        {
            thirdPointVis.transform.position = updatedPosition;

            // Update the sorting order in case it's not set correctly
            SpriteRenderer spriteRenderer = thirdPointVis.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = -1;
            }
        }
    }


    void DeselectAllPoints()
    {
        // Disable LineRenderers to hide the lines
        LR2.enabled = false;
        LR3.enabled = false;

        Destroy(thirdPointVis); // Remove the visualisation circle
        thirdPointVisPresent = false; // Reset the flag
        selectedPoint = null; // Clear the selected point

        // Reset point colors to black
        foreach (Point point in IL)
        {
            point.SetColor(Color.black);
        }
    }



    void AdjustCameraToFitPoints()
    {
        if (IL.Count == 0) return;

        // Calculate the average position of all points
        Vector3 averagePosition = Vector3.zero;
        foreach (Point point in IL)
        {
            averagePosition += point.PointGO.transform.position;
        }
        averagePosition /= IL.Count;

        // Adjust the camera position to the average position (keeping the current z)
        Vector3 targetPosition = new Vector3(averagePosition.x, averagePosition.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, smoothSpeed);

        // Calculate the maximum distance from the average position for all points
        float maxDistance = 0f;
        foreach (Point point in IL)
        {
            float distance = Vector3.Distance(averagePosition, point.PointGO.transform.position);
            if (distance > maxDistance) maxDistance = distance;
        }
    }

    Vector3 ClampPointToCameraView(Vector3 pointPosition)
    {
        // Get the camera's orthographic bounds
        float verticalSize = Camera.main.orthographicSize - padding;
        float horizontalSize = verticalSize * Camera.main.aspect - padding;

        // Camera's center position
        Vector3 cameraCenter = Camera.main.transform.position;

        // Calculate the boundaries
        float minX = cameraCenter.x - horizontalSize;
        float maxX = cameraCenter.x + horizontalSize;
        float minY = cameraCenter.y - verticalSize;
        float maxY = cameraCenter.y + verticalSize;

        // Clamp the point's position to the camera bounds
        pointPosition.x = Mathf.Clamp(pointPosition.x, minX, maxX);
        pointPosition.y = Mathf.Clamp(pointPosition.y, minY, maxY);

        return pointPosition;
    }

}