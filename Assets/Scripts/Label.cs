using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Label : MonoBehaviour
{
    [SerializeField] TextMeshPro textMesh;

    GameObject a, b;

    public void SetPoints(GameObject a, GameObject b)
    {
        this.a = a;
        this.b = b;
    }

    void Update()
    {
        // if (a == null || b == null)
        // {
        //     textMesh.text = "null!";
        //     textMesh.color = Color.red;
        // }
        var aVec = a.transform.position;
        var bVec = b.transform.position;
        transform.position = (aVec + bVec) / 2;
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(Vector3.up, 180f);
        var distance = Vector3.Distance(aVec, bVec);
        textMesh.text = distance>100f ? $"{distance:0.00} m" : $"{distance*100:0} cm";
    }
}
