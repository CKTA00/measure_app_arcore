using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HelperRenderer : MonoBehaviour
{
    // Number of segments to approximate the arc
    [SerializeField] int segmentCount = 20;

    // Radius of the arc
    [SerializeField] float radius = 1.0f;

    MeshFilter meshFilter;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        // Reset position to [0,0,0]
        transform.position = new Vector3();
    }

    public void DeactivateRenderer()
    {
        meshFilter.mesh = null;
    }

    public void RenderAngle(GameObject a, GameObject b, GameObject c)
    {
        meshFilter.mesh = CreateCircleSectorMesh(
            a.transform.position, b.transform.position, c.transform.position,
            segmentCount, radius);
    }

    public void RenderArea(GameObject a, GameObject b, GameObject c)
    {
        meshFilter.mesh = CreateTriangleMesh(
            a.transform.position, b.transform.position, c.transform.position);
    }

    Mesh CreateTriangleMesh(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = new Vector3[] { point1, point2, point3 };
        mesh.triangles = new int[] { 0, 1, 2 };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Mesh CreateCircleSectorMesh(
        Vector3 center, Vector3 edge1, Vector3 edge2,
        int segments, float radius)
    {
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        Vector3 dir1 = (edge1 - center).normalized;
        Vector3 dir2 = (edge2 - center).normalized;

        vertices[0] = center;

        // Create vertices for the arc
        for (int i = 0; i <= segments; i++)
        {
            float t = (float) i / segments;
            vertices[i + 1] = Vector3.Slerp(dir1, dir2, t) * radius + center;
        }

        // Calculate triangles (triangle fan)
        for (int i = 0; i < segments; i++)
        {
            triangles[3 * i + 0] = 0;
            triangles[3 * i + 1] = i + 1;
            triangles[3 * i + 2] = i + 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
