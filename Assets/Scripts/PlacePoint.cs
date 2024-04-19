using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacePoint : MonoBehaviour
{
    [SerializeField] GameObject pointPrefab;
    [SerializeField] GameObject labelPrefab;
    [SerializeField] GameObject placementIndicator;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] Camera cam;

    Pose placementPose;

    GameObject[] points = new GameObject[3];
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

        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                //add timer so if touching for 0.5 second then move point that was touched
                PlaceAtCenter();
            }
        }
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
}
