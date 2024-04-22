using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Camera cam;

    // debug
    [SerializeField] TextMeshProUGUI debugRaycast;

    Pose placementPose;

    GameObject[] points = new GameObject[3];
    GameObject selectedObject;
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


        SelectionPointLogic();

        //for (int i = 0; i < Input.touchCount; ++i)
        //{
        //    if (Input.GetTouch(i).phase == TouchPhase.Began)
        //    {
        //        //add timer so if touching for 0.5 second then move point that was touched
        //        PlaceAtCenter();
        //    }
        //}
    }

    void SelectionPointLogic()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            debugRaycast.text = "are we even in?";

            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                List<ARRaycastHit> arHit = new List<ARRaycastHit>();
                debugRaycast.text = arHit.Count().ToString();
                if (arRaycastManager.Raycast(Input.GetTouch(0).position, arHit, TrackableType.AllTypes))
                {
                    SelectObject(arHit[0].trackable.gameObject);
                }
            }
        }
    }

    void SelectObject(GameObject obj)
    {
        DeselectObject();

        selectedObject = obj;

        Renderer renderer = selectedObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
    }

    void DeselectObject()
    {
        if (selectedObject != null)
        {
            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green;
            }

            selectedObject = null;
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
