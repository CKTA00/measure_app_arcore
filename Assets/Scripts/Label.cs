using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Label : MonoBehaviour
{
    [SerializeField] TextMeshPro textMesh;

    GameObject a, b, c;

    public void DeactivateLabel()
    {
        textMesh.text = string.Empty;

        a = null;
        b = null;
        c = null;
    }

    public void SetLinePoints(GameObject a, GameObject b)
    {
        this.a = a;
        this.b = b;

        RepositionAndCalculateDistance();
    }

    public void SetAnglePoints(GameObject a, GameObject b, GameObject c)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        RepositionAndCalculateAngle();
    }

    public void SetAreaPoints(GameObject a, GameObject b, GameObject c)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        RepositionAndCalculateArea();
    }

    void Start()
    {
        textMesh.text = string.Empty;
    }

    void RepositionAndCalculateDistance()
    {
        Vector3 aVec = a.transform.position;
        Vector3 bVec = b.transform.position;

        transform.position = (aVec + bVec) / 2;

        float distance = Vector3.Distance(aVec, bVec);

        textMesh.text = (distance >= 1) ? $"{distance:0.00} m" : $"{(distance * 100):0} cm";
    }

    void RepositionAndCalculateAngle()
    {
        Vector3 aVec = a.transform.position;
        Vector3 bVec = b.transform.position;
        Vector3 cVec = c.transform.position;

        Vector3 centroid = (aVec + bVec + cVec) / 3;
        transform.position = (0.75f * aVec) + (0.25f * centroid);

        float angle = Vector3.Angle((bVec - aVec), (cVec - aVec));

        textMesh.text = $"{angle:0} °";
    }

    void RepositionAndCalculateArea()
    {
        Vector3 aVec = a.transform.position;
        Vector3 bVec = b.transform.position;
        Vector3 cVec = c.transform.position;

        transform.position = (aVec + bVec + cVec) / 3;

        float area = 0.5f * Vector3.Cross((bVec - aVec), (cVec - aVec)).magnitude;

        textMesh.text = (area >= 1) ? $"{area:0.00} m²" : $"{(area * 100 * 100):0} cm²";
    }

    void Update()
    {
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(Vector3.up, 180f);
    }
}
