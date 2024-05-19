using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacePoint : MonoBehaviour
{
    [SerializeField] GameObject pointPrefab;
    [SerializeField] GameObject labelPrefab;
    [SerializeField] GameObject placementIndicator;
    [SerializeField] GameObject projectedIndicator;
    [SerializeField] ARPlaneManager arPlaneManager;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] LineRenderer lineRenderer;
    [Space]
    [SerializeField] float pointSelectionThreshold = 10.0f;

    // debug
    [SerializeField] TextMeshProUGUI debugText;

    public bool showDetectedPlanes = false;
    public bool showLineLabels = true;
    public bool showAngleLabels = false;
    public bool showAreaLabel = false;
    public bool enforcedRightAngle = false;

    Pose placementPose;
    GameObject[] points = new GameObject[3];
    GameObject selectedPoint;
    Label[] labels = new Label[3 + 3 + 1];
    int lastReplacedIndex;

    [SerializeField] HelperRenderer[] helperRenderers = new HelperRenderer[4];

    void Start()
    {
        for (var i = 0; i < labels.Length; ++i)
        {
            labels[i] = Instantiate(labelPrefab).GetComponent<Label>();
        }
    }

    void Update()
    {
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)); // insert point of click
        var hits = new List<ARRaycastHit>();
        arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            placementPose = hits[0].pose;
            var cameraForward = Camera.main.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);

            if (enforcedRightAngle && AreOnlyTwoPointsPlaced(out int indexA, out int indexB))
            {
                Vector3 projectedPosition = CalculateRightAnglePoint(
                    points[indexA].transform.position,
                    points[indexB].transform.position,
                    placementPose.position);

                projectedIndicator.SetActive(true);
                projectedIndicator.transform.position = projectedPosition;
            }
            else
            {
                projectedIndicator.SetActive(false);
            }
        }
        else
        {
            placementIndicator.SetActive(false);
            projectedIndicator.SetActive(false);
        }

        // Objects (planes and labels) visibility

        foreach (var plane in arPlaneManager.trackables)
        {
            plane.gameObject.SetActive(showDetectedPlanes);
        }

        labels[0].gameObject.SetActive(showLineLabels);
        labels[1].gameObject.SetActive(showLineLabels);
        labels[2].gameObject.SetActive(showLineLabels);

        labels[3].gameObject.SetActive(showAngleLabels);
        labels[4].gameObject.SetActive(showAngleLabels);
        labels[5].gameObject.SetActive(showAngleLabels);

        labels[6].gameObject.SetActive(showAreaLabel);

        helperRenderers[0].gameObject.SetActive(showAngleLabels);
        helperRenderers[1].gameObject.SetActive(showAngleLabels);
        helperRenderers[2].gameObject.SetActive(showAngleLabels);

        helperRenderers[3].gameObject.SetActive(showAreaLabel);
    }

    //
    // Button pressed to switch to the "90 Degress Mode"
    //
    public void EnforceRightAngleToggle()
    {
        if (!enforcedRightAngle && (2 == CountPoints()))
        {
            enforcedRightAngle = true;
        }
        else
        {
            enforcedRightAngle = false;
        }
    }

    //
    // Button pressed to place a new point in the center of the cursor
    //
    public void PlaceAtCenter()
    {
        int currIndex;

        // Possibility to change the position of the last selected point
        if (null != selectedPoint)
        {
            currIndex = Array.IndexOf(points, selectedPoint);

            // Moved point must be re-selected
            DeselectPoint();
        }
        else
        {
            currIndex = UpdateAndGetIndexToReplace();
        }

        // Remove an existing point before spawning a new object
        if (points[currIndex] != null)
        {
            Destroy(points[currIndex]);
        }

        // Determine the final point position
        Vector3 positionToPlace;
        if (enforcedRightAngle)
        {
            positionToPlace = projectedIndicator.transform.position;
            enforcedRightAngle = false;
        }
        else
        {
            positionToPlace = placementPose.position;
        }

        points[currIndex] = Instantiate(pointPrefab, positionToPlace, placementPose.rotation);

        UpdateLabelsArray();
        UpdatePointToPointLines();
    }

    //
    // Button pressed to select the closest point to the cursor
    //
    public void SelectPoint()
    {
        var currentCount = CountPoints();
        if (currentCount == 0)
        {
            debugText.text = "no points";
            return;
        }

        var closestDistance = Mathf.Infinity;
        GameObject closestPoint = null;
        foreach (GameObject point in points.Where(e => e != null))
        {
            var distanceToPoint = (point.transform.position - placementIndicator.transform.position).sqrMagnitude;
            if (distanceToPoint < closestDistance)
            {
                closestDistance = distanceToPoint;
                closestPoint = point;
            }
        }

        if (closestPoint != null)
        {
            debugText.text = "point should be selected, index of point: " + Array.IndexOf(points, closestPoint);
        }

        if (closestDistance <= pointSelectionThreshold)
        {
            SelectPoint(closestPoint);
            enforcedRightAngle = false;
        }
    }

    //
    // Button pressed to remove a selected point
    //
    public void RemoveSelectedPoint()
    {
        if (null != selectedPoint)
        {
            points[Array.IndexOf(points, selectedPoint)] = null;
            Destroy(selectedPoint);
            selectedPoint = null;
            enforcedRightAngle = false;
        }

        UpdateLabelsArray();
        UpdatePointToPointLines();
    }

    void UpdateLabelsArray()
    {
        HideAllLabels();

        var currentCount = CountPoints();
        if (currentCount == 3)
        {
            // Labels for Lines
            labels[0].SetLinePoints(points[0], points[1]);
            labels[1].SetLinePoints(points[1], points[2]);
            labels[2].SetLinePoints(points[2], points[0]);

            // Labels for angles
            labels[3].SetAnglePoints(points[0], points[1], points[2]);
            labels[4].SetAnglePoints(points[1], points[2], points[0]);
            labels[5].SetAnglePoints(points[2], points[0], points[1]);

            // Label for area
            labels[6].SetAreaPoints(points[0], points[1], points[2]);

            // Renderers for angles
            helperRenderers[0].RenderAngle(points[0], points[1], points[2]);
            helperRenderers[1].RenderAngle(points[1], points[2], points[0]);
            helperRenderers[2].RenderAngle(points[2], points[0], points[1]);

            // Renderer for area
            helperRenderers[3].RenderArea(points[0], points[1], points[2]);
        }
        else if (AreOnlyTwoPointsPlaced(out int indexA, out int indexB))
        {
            labels[0].SetLinePoints(points[indexA], points[indexB]);
        }
    }

    void HideAllLabels()
    {
        for (var i = 0; i < labels.Length; ++i)
        {
            labels[i].DeactivateLabel();
        }

        for (var i = 0; i < helperRenderers.Length; ++i)
        {
            helperRenderers[i].DeactivateRenderer();
        }
    }

    //
    // Returns index of a new point when nothing is selected on the screen
    //
    int UpdateAndGetIndexToReplace()
    {
        var len = points.Length;
        var currentCount = CountPoints();
        if (currentCount < len)
        {
            lastReplacedIndex = Array.FindIndex(points,e => e == null);
            return lastReplacedIndex;
        }
        lastReplacedIndex++;
        lastReplacedIndex %= len;
        return lastReplacedIndex;
    }

    void UpdatePointToPointLines()
    {
        int index = 0;
        lineRenderer.positionCount = CountPoints();
        foreach (GameObject point in points)
        {
            if (point != null)
            {
                lineRenderer.SetPosition(index++, point.transform.position);
            }
        }
        debugText.text = $"{index} final index";
    }

    //
    // Marks given point as selected
    //
    void SelectPoint(GameObject point)
    {
        DeselectPoint();

        selectedPoint = point;

        Renderer renderer = selectedPoint.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = UnityEngine.Color.red;
        }
    }

    void DeselectPoint()
    {
        if (selectedPoint != null)
        {
            Renderer renderer = selectedPoint.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = UnityEngine.Color.green;
            }

            selectedPoint = null;
        }
    }

    int CountPoints()
    {
        return points.Count(e => e != null);
    }

    bool AreOnlyTwoPointsPlaced(out int indexA, out int indexB)
    {
        if (2 != CountPoints())
        {
            indexA = (-1);
            indexB = (-1);
            return false;
        }

        int emptyPointIndex = Array.FindIndex(points, e => (e == null));
        indexA = emptyPointIndex > 0 ? 0 : 1;
        indexB = emptyPointIndex > 1 ? 1 : 2;

        return true;
    }

    Vector3 CalculateRightAnglePoint(
        Vector3 pointA,
        Vector3 pointB,
        Vector3 referencePoint)
    {
        // Determine which point is closer to the reference point
        Vector3 closestPoint = (Vector3.Distance(pointA, referencePoint) < Vector3.Distance(pointB, referencePoint)) ? pointA : pointB;
        Vector3 farthestPoint = (closestPoint == pointA) ? pointB : pointA;

        // Calculate the normal to the plane defined by the three points
        Vector3 vecCA = closestPoint - referencePoint;
        Vector3 vecCB = farthestPoint - referencePoint;
        Vector3 normal = Vector3.Cross(vecCA, vecCB).normalized;

        // Calculate the direction of the line perpendicular to AB on the plane
        Vector3 vecAB = farthestPoint - closestPoint;
        Vector3 perpendicular = Vector3.Cross(vecAB, normal).normalized;

        // Project the reference point onto the perpendicular line
        Vector3 direction = referencePoint - closestPoint;
        Vector3 projection = Vector3.Project(direction, perpendicular);

        return closestPoint + projection;
    }
}
