using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacePoint : MonoBehaviour
{
    [SerializeField] GameObject pointPrefab;
    [SerializeField] GameObject labelPrefab;
    [SerializeField] GameObject placementIndicator;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Camera cam;
    [Space]
    [SerializeField] float pointSelectionThreshold = 10.0f;

    // debug
    [SerializeField] TextMeshProUGUI debugText;

    Pose placementPose;

    GameObject[] points = new GameObject[3];
    GameObject selectedPoint;
    Label[] labels = new Label[3];
    int lastReplacedIndex;

    void Update()
    {
        var screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f)); // insert point of click
        var hits = new List<ARRaycastHit>();
        arRaycastManager.Raycast(screenCenter, hits, TrackableType.Planes);
        if (hits.Count > 0)
        {
            placementPose = hits[0].pose;
            var cameraForward = cam.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }

        //for (int i = 0; i < Input.touchCount; ++i)
        //{
        //    if (Input.GetTouch(i).phase == TouchPhase.Began)
        //    {
        //        //add timer so if touching for 0.5 second then move point that was touched
        //        PlaceAtCenter();
        //    }
        //}
    }

    public void PlaceAtCenter()
    {
        var currIndex = UpdateAndGetIndexToReplace();

        if (points[currIndex] != null)
        {
            Destroy(points[currIndex]);
        }
        points[currIndex] = Instantiate(pointPrefab, placementPose.position, placementPose.rotation);

        UpdateLabelsArray();
        UpdatePointToPointLines();
    }

    public void SelectPoint()
    {
        var currentCount = points.Count(e => e != null);
        if (currentCount == 0 )
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
            debugText.text = "point is should be selected, index of point: " + Array.IndexOf(points, closestPoint);
        }

        if (closestDistance <= pointSelectionThreshold)
        {
            SelectPoint(closestPoint);
        }
    }

    public void RemoveSelectedPoint()
    {
        UpdateAndGetIndexToReplace();

        points[Array.IndexOf(points, selectedPoint)] = null;
        Destroy(selectedPoint);

        UpdateLabelsArray();
        UpdatePointsArray();
    }

    void UpdateLabelsArray()
    {
        var currentCount = points.Count(e => e != null);
        ClearAllLabels();
        if (currentCount == 3)
        {
            labels[0] = Instantiate(labelPrefab).GetComponent<Label>();
            labels[0].SetPoints(points[0], points[1]);
            labels[1] = Instantiate(labelPrefab).GetComponent<Label>();
            labels[1].SetPoints(points[1], points[2]);
            labels[2] = Instantiate(labelPrefab).GetComponent<Label>();
            labels[2].SetPoints(points[2], points[0]);
        }
        else if (currentCount == 2)
        {
            labels[0] = Instantiate(labelPrefab).GetComponent<Label>();
            labels[0].SetPoints(points[0], points[1]);
        }
    }

    void ClearAllLabels()
    {
        for (var i = 0; i < labels.Length; ++i)
        {
            if (labels[i] == null) continue;

            Destroy(labels[i].gameObject);
            labels[i] = null;
        }
    }

    int UpdateAndGetIndexToReplace()
    {
        var len = points.Length;
        var currentCount = points.Count(e => e != null);
        if (currentCount < len)
        {
            lastReplacedIndex = currentCount;
            return currentCount;
        }
        lastReplacedIndex++;
        lastReplacedIndex %= len;
        return lastReplacedIndex;
    }

    void UpdatePointsArray()
    {
        return;
    }

    void UpdatePointToPointLines()
    {
        var index = 0;
        GameObject lastPoint = null;
        foreach (GameObject point in points)
        {
            if (point == null)
            {
                if (lastPoint)
                {
                    lineRenderer.SetPosition(index++, lastPoint.transform.position);
                }
                continue;
            }

            lastPoint = point;
            lineRenderer.SetPosition(index++, point.transform.position);
        }
    }

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
}
